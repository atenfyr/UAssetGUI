using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetGUI
{
    public static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static List<Type> strongRefs = new List<Type>();
        static Program()
        {
            try
            {
                // extract .dll.gz resources and load them on demand
                AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
                {
                    using (var stream = typeof(Program).Assembly.GetManifestResourceStream($"UAssetGUI." + assemblyName.Name + ".dll.gz"))
                    {
                        // if not found, default behavior
                        if (stream == null) return null;

                        string libsPath = Path.Combine(UAGConfig.ConfigFolder, "Libraries");
                        Directory.CreateDirectory(UAGConfig.ConfigFolder);
                        Directory.CreateDirectory(libsPath);

                        string outPath = Path.Combine(libsPath, assemblyName.Name + ".dll");
                        using (FileStream newFileStream = File.Open(outPath, FileMode.Create, FileAccess.Write))
                        {
                            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                gzipStream.CopyTo(newFileStream);
                            }
                        }

                        return Assembly.LoadFrom(outPath);
                    }

                    // if not found, default behavior
                    return null;
                };

                strongRefs.Add(typeof(System.Collections.Immutable.ImmutableArray));
                strongRefs.Add(typeof(System.Reflection.Metadata.MetadataReader));
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
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
                        // UAssetGUI tojson A.umap B.json 23 Outriders
                        case "tojson":
                            UAGConfig.LoadMappings();

                            if (args.Length < 5) break;
                            if (args.Length >= 6) UAGConfig.TryGetMappings(args[5], out selectedMappings);

                            EngineVersion selectedVer = EngineVersion.UNKNOWN;
                            if (int.TryParse(args[4], out int selectedVerRaw)) selectedVer = EngineVersion.VER_UE4_0 + selectedVerRaw;
                            else Enum.TryParse(args[4], out selectedVer);

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
                                jsonDeserializedAsset.FilePath = args[2];
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
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
