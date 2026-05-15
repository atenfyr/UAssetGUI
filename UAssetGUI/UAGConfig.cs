using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetGUI
{
    public interface ILocalizable
    {
        /// <summary>
        /// Method called on initial form load or when language is changed.
        /// </summary>
        public void Localize();
    }

    public struct UAGConfigData
    {
        public string Agent;
        public string Language;
        public string PreferredVersion;
        public string PreferredMappings;
        public string Theme;
        public string MapStructTypeOverride;
        public string FavoriteThing;
        public int DataZoom;
        public bool ChangeValuesOnScroll;
        public bool EnableDynamicTree;
        public bool EnableDiscordRPC;
        public bool DoubleClickToEdit;
        public bool EnableBak;
        public bool RestoreSize;
        public bool EnableUpdateNotice;
        public bool EnablePrettyBytecode;
        public int StartupWidth;
        public int StartupHeight;
        public int CustomSerializationFlags;
        public bool EnableBakJson;
        public bool AllowUntrustedScripts;
        public string RetocExtraCommands;
        public string GameSpecificOverride;

        public UAGConfigData()
        {
            Agent = "UAssetGUI";
            Language = string.Empty;
            PreferredVersion = string.Empty;
            Theme = string.Empty;
            MapStructTypeOverride = string.Empty;
            FavoriteThing = string.Empty;
            DataZoom = 0;
            ChangeValuesOnScroll = true;
            EnableDynamicTree = true;
            EnableDiscordRPC = true;
            DoubleClickToEdit = true;
            EnableBak = true;
            RestoreSize = true;
            EnableUpdateNotice = true;
            EnablePrettyBytecode = true;
            StartupWidth = 1000;
            StartupHeight = 700;
            CustomSerializationFlags = 0;
            EnableBakJson = false;
            AllowUntrustedScripts = false;
            RetocExtraCommands = string.Empty;
            GameSpecificOverride = string.Empty;
        }
    }

    public static class UAGConfig
    {
        public static UAGConfigData Data;
        public static Dictionary<string, string> AllMappings = new Dictionary<string, string>();
        public static List<string> AllScriptIDs = new List<string>();

        public static bool SafeToAccessConfigFolder = false;
        public static bool IsPortable = false;
        public static string ConfigFolder
        {
            get
            {
                if (!SafeToAccessConfigFolder) throw new InvalidOperationException("Attempt to access UAGConfig.ConfigFolder before it is ready");
                return IsPortable ? Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Data") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UAssetGUI");
            }
        }
        public static string MappingsFolder => Path.Combine(ConfigFolder, "Mappings");
        public static string StagingFolder => Path.Combine(ConfigFolder, "Staging");
        public static string ExtractedFolder => Path.Combine(ConfigFolder, "Extracted");
        public static string ScriptsFolder => Path.Combine(ConfigFolder, "Scripts");
        public static string TempFolder => Path.Combine(Path.GetTempPath(), "UAssetGUI", Environment.ProcessId.ToString());

        internal static bool DifferentStagingPerPak = false;

        public static readonly string[] ValidMappingsExtensions = [".usmap", ".jmap", ".jmap.gz"]; // must include dot prefix

        /// <summary>
        /// Load the list of mappings from disk.
        /// </summary>
        /// <param name="pathToFetchNameOf">Path to fetch mappings name.</param>
        /// <returns>If pathToFetchNameOf is specified, the combo box name of the mappings located at the path pathToFetchNameOf, else null.</returns>
        public static string LoadMappings(string pathToFetchNameOf = null)
        {
            Directory.CreateDirectory(ConfigFolder);
            Directory.CreateDirectory(MappingsFolder);

            string fetchedName = null;

            AllMappings.Clear();
            List<string> allMappingFiles = new List<string>();
            int numDifferentExtensions = 0;
            foreach (string ext in ValidMappingsExtensions)
            {
                string[] filesWithExt = Directory.GetFiles(MappingsFolder, "*" + ext, SearchOption.TopDirectoryOnly);
                if (filesWithExt.Length > 0) numDifferentExtensions++;
                allMappingFiles.AddRange(filesWithExt);
            }
            foreach (string mappingPath in allMappingFiles)
            {
                // can't use Path.GetFileNameWithoutExtension because it doesn't properly handle extensions with multiple dots
                string mappingPathNoExtension = Path.GetFileName(mappingPath);
                string fileExtension = ".usmap";
                foreach (string ext in UAGConfig.ValidMappingsExtensions)
                {
                    if (mappingPathNoExtension.EndsWith(ext))
                    {
                        mappingPathNoExtension = mappingPathNoExtension.Substring(0, mappingPathNoExtension.Length - ext.Length);
                        fileExtension = ext;
                        break;
                    }
                }

                string decidedMappingsName = mappingPathNoExtension;
                if (numDifferentExtensions > 1) decidedMappingsName += string.Concat(" [", fileExtension.AsSpan(1), "]"); // append extension to name if multiple types of mappings are present

                AllMappings[decidedMappingsName] = mappingPath;
                if (pathToFetchNameOf != null && Path.GetRelativePath(pathToFetchNameOf, mappingPath) == ".") fetchedName = decidedMappingsName;
            }

            return fetchedName;
        }

        public static string GetStagingDirectory(string pakPath)
        {
            string rootDir = DifferentStagingPerPak ? Path.Combine(StagingFolder, Path.GetFileNameWithoutExtension(pakPath)) : StagingFolder;
            Directory.CreateDirectory(rootDir);
            return rootDir;
        }

        public static string[] GetStagingFiles(string pakPath, out string[] fixedPathsOnDisk)
        {
            string rootDir = GetStagingDirectory(pakPath);

            fixedPathsOnDisk = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories);
            string[] res = new string[fixedPathsOnDisk.Length]; Array.Copy(fixedPathsOnDisk, res, fixedPathsOnDisk.Length);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = res[i].Replace(Path.DirectorySeparatorChar, '/').Substring(rootDir.Length).TrimStart('/');
            }

            return res;
        }

        public static void StageFile(string rawPathOnDisk, string CurrentContainerPath, string newPath = null)
        {
            if (newPath == null) newPath = Path.GetFileName(rawPathOnDisk);
            newPath = newPath.Replace('/', Path.DirectorySeparatorChar);

            var finalPath = DifferentStagingPerPak ? Path.Combine(StagingFolder, Path.GetFileNameWithoutExtension(CurrentContainerPath), newPath) : Path.Combine(StagingFolder, newPath);
            try { Directory.CreateDirectory(Path.GetDirectoryName(finalPath)); } catch { return; } // fail silently if cant make the directory we need

            UAGUtils.DeleteDirectoryQuick(finalPath, true);

            File.Copy(rawPathOnDisk, finalPath, true);
            try { File.Copy(Path.ChangeExtension(rawPathOnDisk, ".uexp"), Path.ChangeExtension(finalPath, ".uexp"), true); } catch { }
            try { File.Copy(Path.ChangeExtension(rawPathOnDisk, ".ubulk"), Path.ChangeExtension(finalPath, ".ubulk"), true); } catch { }
        }

        public static void StageFile(DirectoryTreeItem item, InteropType interopType, string newPath = null)
        {
            // recursive if we were given a directory
            if (!item.IsFile)
            {
                foreach (var child in item.Children) StageFile(child.Value, interopType, newPath == null ? null : Path.Combine(newPath, child.Value.Name));
                return;
            }

            if (newPath == null) newPath = item.FullPath;
            newPath = newPath.Replace('/', Path.DirectorySeparatorChar);

            string outputPath = item.SaveFileToTemp(interopType);
            var finalPath = DifferentStagingPerPak ? Path.Combine(StagingFolder, Path.GetFileNameWithoutExtension(item.ParentForm.CurrentContainerPath), newPath) : Path.Combine(StagingFolder, newPath);
            if (outputPath == null || finalPath == null) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(finalPath)); } catch { return; } // fail silently if cant make the directory we need

            UAGUtils.DeleteDirectoryQuick(finalPath, true); // if we turn a directory into a file, try and get rid of the directory

            UAGUtils.CopyFileQuick(outputPath, finalPath, true);
            UAGUtils.CopyFileQuick(Path.ChangeExtension(outputPath, ".uexp"), Path.ChangeExtension(finalPath, ".uexp"), true);
            UAGUtils.CopyFileQuick(Path.ChangeExtension(outputPath, ".ubulk"), Path.ChangeExtension(finalPath, ".ubulk"), true);
            UAGUtils.DeleteFileQuick(outputPath);
            UAGUtils.DeleteFileQuick(Path.ChangeExtension(outputPath, ".uexp"));
            UAGUtils.DeleteFileQuick(Path.ChangeExtension(outputPath, ".ubulk"));
        }

        public static string ExtractFile(DirectoryTreeItem item, InteropType interopType, FileStream stream2 = null, PakReader reader2 = null)
        {
            var finalPath = Path.Combine(ExtractedFolder, item.FullPath.Replace('/', Path.DirectorySeparatorChar));

            // recursive if we were given a directory
            if (!item.IsFile)
            {
                foreach (var child in item.Children) ExtractFile(child.Value, interopType, stream2, reader2);
                return finalPath;
            }

            return item.SaveFileToTemp(interopType, ExtractedFolder, stream2, reader2);
        }

        public static bool TryGetMappings(string name, out Usmap mappings)
        {
            if (AllMappings.TryGetValue(name, out string value))
            {
                try
                {
                    mappings = new Usmap(value);
                    return true;
                }
                catch
                {
                    UAGUtils.InvokeUI(() =>
                    {
                        MessageBox.Show("Failed to parse mappings: " + name, "Notice");

                        // update list of mappings for good measure
                        foreach (var form in Application.OpenForms)
                        {
                            if (form is Form1 form1)
                            {
                                form1.UpdateMappings(UAGConfig.GetString("Generic.NoMappings"), false);
                            }
                        }
                    });
                }
            }

            mappings = null;
            return false;
        }

        public static void RefreshAllScriptIDs(bool refreshTheme = true)
        {
            Directory.CreateDirectory(ScriptsFolder);
            AllScriptIDs = Directory.GetFiles(ScriptsFolder, "*.cs", SearchOption.TopDirectoryOnly).Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
            if (AllScriptIDs.Count == 0)
            {
                InstallBuiltInScripts();
            }

            UAGUtils.InvokeUI(() =>
            {
                foreach (var form in Application.OpenForms)
                {
                    if (form is Form1 form1)
                    {
                        {
                            List<ToolStripItem> subScriptItems = new List<ToolStripItem>();
                            foreach (string scriptID in AllScriptIDs)
                            {
                                ToolStripMenuItem newItem = new ToolStripMenuItem()
                                {
                                    Name = "executeScriptToolStripMenuItemSubScriptItem_" + scriptID,
                                    Size = form1.executeScriptToolStripMenuItem.Size,
                                    Text = scriptID.Replace('_', ' '),
                                    Tag = scriptID
                                };
                                newItem.Click += form1.executeScriptSubItem_Click;
                                subScriptItems.Add(newItem);
                            }
                            ToolStripMenuItem newItem2 = new ToolStripMenuItem()
                            {
                                Name = "executeScriptToolStripMenuItemSubScriptItem_New",
                                Size = form1.executeScriptToolStripMenuItem.Size,
                                Text = UAGConfig.GetString("Menu.Utils.EditScript.AddNewScript"),
                            };
                            newItem2.Click += form1.executeScriptNewItem_Click;
                            subScriptItems.Add(newItem2);

                            form1.executeScriptToolStripMenuItem.DropDownItems.Clear();
                            form1.executeScriptToolStripMenuItem.DropDownItems.AddRange(subScriptItems.ToArray());
                        }
                        {
                            List<ToolStripItem> subScriptItems = new List<ToolStripItem>();
                            foreach (string scriptID in AllScriptIDs)
                            {
                                ToolStripMenuItem newItem = new ToolStripMenuItem()
                                {
                                    Name = "editScriptToolStripMenuItemSubScriptItem_" + scriptID,
                                    Size = form1.editScriptToolStripMenuItem.Size,
                                    Text = scriptID.Replace('_', ' '),
                                    Tag = scriptID
                                };
                                newItem.Click += form1.editScriptSubItem_Click;
                                subScriptItems.Add(newItem);
                            }
                            ToolStripMenuItem newItem2 = new ToolStripMenuItem()
                            {
                                Name = "editScriptToolStripMenuItemSubScriptItem_New",
                                Size = form1.editScriptToolStripMenuItem.Size,
                                Text = UAGConfig.GetString("Menu.Utils.EditScript.AddNewScript"),
                            };
                            newItem2.Click += form1.executeScriptNewItem_Click;
                            subScriptItems.Add(newItem2);

                            form1.editScriptToolStripMenuItem.DropDownItems.Clear();
                            form1.editScriptToolStripMenuItem.DropDownItems.AddRange(subScriptItems.ToArray());
                        }

                        form1.executeScriptToolStripMenuItem.Enabled = UAGConfig.Data.AllowUntrustedScripts;
                        form1.editScriptToolStripMenuItem.Enabled = UAGConfig.Data.AllowUntrustedScripts;

                        if (refreshTheme) UAGPalette.RefreshTheme(form1);
                    }
                }
            });
        }

        public static string GetScriptTextByID(string id)
        {
            Directory.CreateDirectory(ScriptsFolder);
            try
            {
                return File.ReadAllText(Path.ChangeExtension(Path.Combine(ScriptsFolder, id), "cs"));
            }
            catch
            {
                return null;
            }
        }

        public static string CreateAndReturnPathToScript(string id)
        {
            string newScriptPath = Path.ChangeExtension(Path.Combine(ScriptsFolder, id), "cs");
            if (File.Exists(newScriptPath)) return newScriptPath;
            try
            {
                File.WriteAllText(newScriptPath, Properties.Resources.Hello_world);
                RefreshAllScriptIDs();
                return newScriptPath;
            }
            catch
            {
                return null;
            }
        }

        public static bool InstallBuiltInScripts()
        {
            try
            {
                File.WriteAllText(Path.ChangeExtension(Path.Combine(ScriptsFolder, "Hello_world"), "cs"), Properties.Resources.Hello_world);
                File.WriteAllText(Path.ChangeExtension(Path.Combine(ScriptsFolder, "Set_all_visible_floats_to_100"), "cs"), Properties.Resources.Set_all_visible_floats_to_100);
                File.WriteAllText(Path.ChangeExtension(Path.Combine(ScriptsFolder, "Print_visible_nodes"), "cs"), Properties.Resources.Print_visible_nodes);

                RefreshAllScriptIDs();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static GameSpecificOverride GetOverride()
        {
            if (string.IsNullOrEmpty(UAGConfig.Data.GameSpecificOverride)) return GameSpecificOverride.None;
            return Enum.TryParse(UAGConfig.Data.GameSpecificOverride, out GameSpecificOverride overr) ? overr : GameSpecificOverride.None;
        }

        private static Dictionary<string, string> _LocalizedStringsFallbackCache;
        private static Dictionary<string, string> LocalizedStringsFallback
        {
            get
            {
                if (_LocalizedStringsFallbackCache == null)
                {
                    _LocalizedStringsFallbackCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(Properties.Resources.en_US));
                }
                return _LocalizedStringsFallbackCache;
            }
        }

        private static Dictionary<string, Dictionary<string, string>> _LocalizedStringsCacheCache = null;
        private static Dictionary<string, string> _LocalizedStringsCache = null;
        private static Dictionary<string, string> LocalizedStrings
        {
            get
            {
                if (_LocalizedStringsCache == null)
                {
                    if (_LocalizedStringsCacheCache != null && _LocalizedStringsCacheCache.ContainsKey(UAGConfig.Data.Language))
                    {
                        _LocalizedStringsCache = _LocalizedStringsCacheCache[UAGConfig.Data.Language];
                    }
                    else
                    {
                        _LocalizedStringsCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString((byte[])Properties.Resources.ResourceManager.GetObject(UAGConfig.Data.Language)));
                    }
                }
                return _LocalizedStringsCache;
            }
        }

        private static List<string> _LanguageCodesCache;
        private static List<string> LanguageCodes
        {
            get
            {
                if (_LanguageCodesCache == null)
                {
                    _LanguageCodesCache = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(Properties.Resources.language_codes));
#if DEBUG || DEBUGVERBOSE || DEBUGTRACING
                    try
                    {
                        if (Properties.Resources.ResourceManager.GetObject("DEBUG") != null) _LanguageCodesCache.Add("DEBUG");
                    }
                    catch { }
#endif
                }
                return _LanguageCodesCache;
            }
        }

        private static List<string> _LanguageNamesCache;
        private static List<string> LanguageNames
        {
            get
            {
                if (_LanguageNamesCache == null)
                {
                    _LanguageNamesCache = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(Properties.Resources.language_names));
#if DEBUG || DEBUGVERBOSE || DEBUGTRACING
                    try
                    {
                        if (Properties.Resources.ResourceManager.GetObject("DEBUG") != null) _LanguageNamesCache.Add("DEBUG");
                    }
                    catch { }
#endif
                }
                return _LanguageNamesCache;
            }
        }

        public static void AddCustomLanguageFromJSON(string jsonText)
        {
            if (_LocalizedStringsCacheCache == null) _LocalizedStringsCacheCache = new Dictionary<string, Dictionary<string, string>>();
            _LocalizedStringsCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            _LocalizedStringsCacheCache[_LocalizedStringsCache["Code"]] = _LocalizedStringsCache;
            LanguageNames.Remove(_LocalizedStringsCache["Language"]);
            LanguageCodes.Remove(_LocalizedStringsCache["Code"]);
            LanguageNames.Add(_LocalizedStringsCache["Language"]);
            LanguageCodes.Add(_LocalizedStringsCache["Code"]);
            UAGConfig.Data.Language = _LocalizedStringsCache["Code"];
        }

        public static bool SetLanguage(string code)
        {
            if (UAGConfig.Data.Language == code) return false;
            UAGConfig.Data.Language = code;
            _LocalizedStringsCache = null;
            return true;
        }

        public static string GetString(string key, bool returnNullIfFail = false)
        {
            if (!LocalizedStrings.ContainsKey(key))
            {
                if (!LocalizedStringsFallback.ContainsKey(key))
                {
                    if (returnNullIfFail) return null;
                    throw new InvalidOperationException($"Attempt to load unknown localized string with key {key}");
                }
                return LocalizedStringsFallback[key];
            }
            return LocalizedStrings[key];
        }

        public static IReadOnlyList<string> GetLanguageCodes()
        {
            return LanguageCodes.AsReadOnly();
        }

        public static IReadOnlyList<string> GetLanguageNames()
        {
            return LanguageNames.AsReadOnly();
        }

        public static bool ReadyToSave = true; // only accessed on UI thread

        public static void Save(bool canSilentlyFail = false)
        {
            UAGUtils.InvokeUI(() =>
            {
                if (!ReadyToSave) return;
                Data.Agent = UAGUtils.DisplayVersion;
                SaveCustomFile("config.json", JsonConvert.SerializeObject(Data, Formatting.Indented), null, canSilentlyFail);
            });
        }

        public static string SaveCustomFile(string name, string text, string subFolder = null, bool canSilentlyFail = false)
        {
            string outPath = Path.Combine(ConfigFolder, name);
            if (!string.IsNullOrEmpty(subFolder)) outPath = Path.Combine(ConfigFolder, subFolder, name);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                File.WriteAllText(outPath, text);
            }
            catch (IOException)
            {
                // throw exception only if canSilentlyFail false, else absorb this exception
                if (!canSilentlyFail) throw;
            }

            return outPath;
        }

        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(ConfigFolder);
                Data = JsonConvert.DeserializeObject<UAGConfigData>(File.ReadAllText(Path.Combine(ConfigFolder, "config.json")));
            }
            catch
            {
                Data = new UAGConfigData();
                Save();
            }
            finally
            {
                if (string.IsNullOrEmpty(UAGConfig.Data.Language) || !LanguageCodes.Contains(UAGConfig.Data.Language))
                {
                    UAGConfig.Data.Language = GetLanguageCodes()[0];
                }
            }
        }
    }
}
