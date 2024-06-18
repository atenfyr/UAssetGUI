using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetGUI
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetDefaultFont(new Font(new FontFamily("Microsoft Sans Serif"), 8.25f)); // default font changed in .NET Core 3.0

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                Usmap selectedMappings = null;

                switch (args[1].ToLowerInvariant())
                {
                    // tojson <source> <destination> <engine version> [mappings name]
                    // UAssetGUI tojson A.umap B.json 514 Outriders
                    case "tojson":
                        UAGConfig.LoadMappings();

                        if (args.Length < 5) break;
                        if (args.Length >= 6) UAGConfig.TryGetMappings(args[5], out selectedMappings);

                        EngineVersion selectedVer = EngineVersion.UNKNOWN;
                        if (!Enum.TryParse(args[4], out selectedVer))
                        {
                            if (int.TryParse(args[4], out int selectedVerRaw)) selectedVer = EngineVersion.VER_UE4_0 + selectedVerRaw;
                        }

                        string jsonSerializedAsset = new UAsset(args[2], selectedVer, selectedMappings).SerializeJson(Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(args[3], jsonSerializedAsset);
                        return;
                    // fromjson <source> <destination> [mappings name]
                    // UAssetGUI fromjson B.json A.umap Outriders
                    case "fromjson":
                        UAGConfig.LoadMappings();

                        if (args.Length < 4) break;
                        if (args.Length >= 5) UAGConfig.TryGetMappings(args[4], out selectedMappings);

                        UAsset jsonDeserializedAsset = null;
                        using (var sr = new FileStream(args[2], FileMode.Open))
                        {
                            jsonDeserializedAsset = UAsset.DeserializeJson(sr);
                        }

                        if (jsonDeserializedAsset != null)
                        {
                            jsonDeserializedAsset.Mappings = selectedMappings;
                            jsonDeserializedAsset.Write(args[3]);
                        }
                        return;
                }
            }

            Form1 f1 = new Form1
            {
                Size = new System.Drawing.Size(1000, 700)
            };
            Application.Run(f1);
        }
    }
}
