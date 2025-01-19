using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.Unversioned;

namespace UAssetGUI
{
    public class UAGConfigData
    {
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

        public UAGConfigData()
        {
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
        }
    }

    public static class UAGConfig
    {
        public static UAGConfigData Data;
        public static Dictionary<string, string> AllMappings = new Dictionary<string, string>();
        public readonly static string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UAssetGUI");
        public readonly static string MappingsFolder = Path.Combine(ConfigFolder, "Mappings");
        public readonly static string StagingFolder = Path.Combine(ConfigFolder, "Staging");
        public readonly static string ExtractedFolder = Path.Combine(ConfigFolder, "Extracted");

        internal static bool DifferentStagingPerPak = false;

        public static void LoadMappings()
        {
            Directory.CreateDirectory(ConfigFolder);
            Directory.CreateDirectory(MappingsFolder);

            AllMappings.Clear();
            string[] allMappingFiles = Directory.GetFiles(MappingsFolder, "*.usmap", SearchOption.TopDirectoryOnly);
            foreach (string mappingPath in allMappingFiles)
            {
                AllMappings.Add(Path.GetFileNameWithoutExtension(mappingPath), mappingPath);
            }
        }

        public static string[] GetStagingFiles(string pakPath, out string[] fixedPathsOnDisk)
        {
            string rootDir = DifferentStagingPerPak ? Path.Combine(StagingFolder, Path.GetFileNameWithoutExtension(pakPath)) : StagingFolder;
            Directory.CreateDirectory(rootDir);

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

            try { Directory.Delete(finalPath, true); } catch { } // if we turn a directory into a file, try and get rid of the directory

            File.Copy(rawPathOnDisk, finalPath, true);
            try { File.Copy(Path.ChangeExtension(rawPathOnDisk, ".uexp"), Path.ChangeExtension(finalPath, ".uexp"), true); } catch { }
            try { File.Copy(Path.ChangeExtension(rawPathOnDisk, ".ubulk"), Path.ChangeExtension(finalPath, ".ubulk"), true); } catch { }
        }

        public static void StageFile(DirectoryTreeItem item, string newPath = null)
        {
            // recursive if we were given a directory
            if (!item.IsFile)
            {
                foreach (var child in item.Children) StageFile(child.Value, newPath == null ? null : Path.Combine(newPath, child.Value.Name));
                return;
            }

            if (newPath == null) newPath = item.FullPath;
            newPath = newPath.Replace('/', Path.DirectorySeparatorChar);

            string outputPath = item.SaveFileToTemp();
            var finalPath = DifferentStagingPerPak ? Path.Combine(StagingFolder, Path.GetFileNameWithoutExtension(item.ParentForm.CurrentContainerPath), newPath) : Path.Combine(StagingFolder, newPath);
            if (outputPath == null || finalPath == null) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(finalPath)); } catch { return; } // fail silently if cant make the directory we need

            try { Directory.Delete(finalPath, true); } catch { } // if we turn a directory into a file, try and get rid of the directory

            File.Copy(outputPath, finalPath, true);
            try { File.Copy(Path.ChangeExtension(outputPath, ".uexp"), Path.ChangeExtension(finalPath, ".uexp"), true); } catch { }
            try { File.Copy(Path.ChangeExtension(outputPath, ".ubulk"), Path.ChangeExtension(finalPath, ".ubulk"), true); } catch { }
            try { File.Delete(outputPath); } catch { }
            try { File.Delete(Path.ChangeExtension(outputPath, ".uexp")); } catch { }
            try { File.Delete(Path.ChangeExtension(outputPath, ".ubulk")); } catch { }
        }

        public static string ExtractFile(DirectoryTreeItem item, FileStream stream2 = null, PakReader reader2 = null)
        {
            var finalPath = Path.Combine(ExtractedFolder, item.FullPath.Replace('/', Path.DirectorySeparatorChar));

            // recursive if we were given a directory
            if (!item.IsFile)
            {
                foreach (var child in item.Children) ExtractFile(child.Value, stream2, reader2);
                return finalPath;
            }

            return item.SaveFileToTemp(ExtractedFolder, stream2, reader2);
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
                                form1.UpdateMappings("No mappings", false);
                            }
                        }
                    });
                }
            }

            mappings = null;
            return false;
        }

        public static void Save()
        {
            SaveCustomFile("config.json", JsonConvert.SerializeObject(Data, Formatting.Indented));
        }

        public static string SaveCustomFile(string name, string text, string subFolder = null)
        {
            string outPath = Path.Combine(ConfigFolder, name);
            if (!string.IsNullOrEmpty(subFolder)) outPath = Path.Combine(ConfigFolder, subFolder, name);

            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllText(outPath, text);

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
        }
    }
}
