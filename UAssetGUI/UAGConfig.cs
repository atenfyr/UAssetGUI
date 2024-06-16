using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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
            StartupWidth = 1000;
            StartupHeight = 700;
        }
    }

    public static class UAGConfig
    {
        public static UAGConfigData Data;
        public static Dictionary<string, Usmap> AllMappings = new Dictionary<string, Usmap>();
        public readonly static string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UAssetGUI");
        public readonly static string MappingsFolder = Path.Combine(ConfigFolder, "Mappings");
        public static ISet<string> MappingsToSuppressWarningsFor = new HashSet<string>();

        public static void LoadMappings()
        {
            Directory.CreateDirectory(ConfigFolder);
            Directory.CreateDirectory(MappingsFolder);

            AllMappings.Clear();
            string[] allMappingFiles = Directory.GetFiles(MappingsFolder, "*.usmap", SearchOption.TopDirectoryOnly);
            ISet<string> failedMappings = new HashSet<string>();
            foreach (string mappingPath in allMappingFiles)
            {
                var mappingsName = Path.GetFileNameWithoutExtension(mappingPath);
                try
                {
                    AllMappings.Add(mappingsName, new Usmap(mappingPath));
                }
                catch
                {
                    if (!MappingsToSuppressWarningsFor.Contains(mappingsName))
                    {
                        failedMappings.Add(mappingsName);
                        MappingsToSuppressWarningsFor.Add(mappingsName);
                    }
                }
            }

            if (failedMappings.Count > 0)
            {
                UAGUtils.InvokeUI(() =>
                {
                    MessageBox.Show("Failed to parse " + failedMappings.Count + " mappings:\n\n" + string.Join(",", failedMappings), "Notice");
                });
            }
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
