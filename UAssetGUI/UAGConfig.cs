using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        public bool ChangeValuesOnScroll;
        public int DataZoom;
        public bool EnableDiscordRPC;

        public UAGConfigData()
        {
            PreferredVersion = string.Empty;
            Theme = string.Empty;
            MapStructTypeOverride = string.Empty;
            FavoriteThing = string.Empty;
            ChangeValuesOnScroll = true;
            DataZoom = 0;
            EnableDiscordRPC = true;
        }
    }

    public static class UAGConfig
    {
        public static UAGConfigData Data;
        public static Dictionary<string, Usmap> AllMappings;
        public readonly static string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UAssetGUI");
        public readonly static string MappingsFolder = Path.Combine(ConfigFolder, "Mappings");

        public static void LoadMappings()
        {
            Directory.CreateDirectory(ConfigFolder);
            Directory.CreateDirectory(MappingsFolder);

            AllMappings = new Dictionary<string, Usmap>();
            string[] allMappingFiles = Directory.GetFiles(MappingsFolder, "*.usmap", SearchOption.TopDirectoryOnly);
            foreach (string mappingPath in allMappingFiles)
            {
                try
                {
                    AllMappings.Add(Path.GetFileNameWithoutExtension(mappingPath), new Usmap(mappingPath));
                }
                catch { }
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(ConfigFolder);
            File.WriteAllText(Path.Combine(ConfigFolder, "config.json"), JsonConvert.SerializeObject(Data, Formatting.Indented));
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

            LoadMappings();
        }
    }
}
