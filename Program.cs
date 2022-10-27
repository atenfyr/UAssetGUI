using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

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

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                switch (args[1].ToLowerInvariant())
                {
                    // tojson <source> <destination> <engine version>
                    // UAssetGUI tojson A.umap B.json 514
                    case "tojson":
                        if (args.Length < 5) break;
                        UE4Version selectedVer = UE4Version.UNKNOWN;
                        if (!Enum.TryParse(args[4], out selectedVer))
                        {
                            if (int.TryParse(args[4], out int selectedVerRaw)) selectedVer = UE4Version.VER_UE4_0 + selectedVerRaw;
                        }

                        string jsonSerializedAsset = new UAsset(args[2], selectedVer).SerializeJson(Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(args[3], jsonSerializedAsset);
                        return;
                    // fromjson <source> <destination>
                    // UAssetGUI fromjson B.json A.umap
                    case "fromjson":
                        if (args.Length < 4) break;
                        UAsset jsonDeserializedAsset = null;
                        using (var sr = new FileStream(args[2], FileMode.Open))
                        {
                            jsonDeserializedAsset = UAsset.DeserializeJson(sr);
                        }

                        if (jsonDeserializedAsset != null)
                        {
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
