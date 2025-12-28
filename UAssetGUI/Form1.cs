using DiscordRPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetGUI
{
    public partial class Form1 : Form
    {
        internal EngineVersion ParsingVersion = EngineVersion.UNKNOWN;
        internal Usmap ParsingMappings = null;
        internal DataGridView dataGridView1;
        internal ColorfulTreeView treeView1;
        internal SplitContainer splitContainer1;
        internal MenuStrip menuStrip1;
        internal AC7Decrypt ac7decrypt;

        public TableHandler tableEditor;
        public ByteViewer byteView1;
        public TextBox jsonView;

        private DiscordRpcClient _discordRpc = null;

        public static readonly string UsmapInstructionsNotice = "If you have a .usmap file for this game, go to Utils --> Import Mappings... and select your .usmap file to import.";

        private void DisposeDiscordRpc()
        {
            if (_discordRpc == null || _discordRpc.IsDisposed) return;
            _discordRpc.ClearPresence();
            _discordRpc.Dispose();
        }

        internal DiscordRpcClient DiscordRPC
        {
            get
            {
                if (_discordRpc == null || _discordRpc.IsDisposed)
                {
                    _discordRpc = new DiscordRpcClient("1035701531342811156");
                    _discordRpc.Initialize();
                }
                else if (!_discordRpc.IsInitialized)
                {
                    _discordRpc.Initialize();
                }
                return _discordRpc;
            }
        }

        public string DisplayVersion
        {
            get
            {
                return "UAssetGUI v" + UAGUtils._displayVersion;
            }
        }

        public Form1()
        {
            InitializeComponent();

            UAGUtils.InitializeInvoke(this);

            UAGUtils.InvokeUI(() =>
            {
                try
                {
                    UAGConfig.Load();

                    Assembly assembly = Assembly.GetExecutingAssembly();
                    UAGUtils._displayVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

                    // version suffix based on nature of the build
#if RELEASEX
                    UAGUtils._displayVersion += "x";
#elif DEBUG || DEBUGVERBOSE || DEBUGTRACING
                    UAGUtils._displayVersion += "d";
#endif

                    string gitVersionGUI = string.Empty;
                    using (Stream stream = assembly.GetManifestResourceStream("UAssetGUI.git_commit.txt"))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                if (reader != null) gitVersionGUI = reader.ReadToEnd().Trim();
                            }
                        }
                    }

                    if (!gitVersionGUI.All("0123456789abcdef".Contains)) gitVersionGUI = string.Empty;

                    string gitVersionAPI = string.Empty;
                    using (Stream stream = typeof(PropertyData).Assembly.GetManifestResourceStream("UAssetAPI.git_commit.txt"))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                if (reader != null) gitVersionAPI = reader.ReadToEnd().Trim();
                            }
                        }
                    }

                    if (!gitVersionAPI.All("0123456789abcdef".Contains)) gitVersionAPI = string.Empty;

                    if (!string.IsNullOrEmpty(gitVersionGUI))
                    {
                        UAGUtils._displayVersion += " (" + gitVersionGUI;
                        if (!string.IsNullOrEmpty(gitVersionAPI))
                        {
                            UAGUtils._displayVersion += " - " + gitVersionAPI;
                        }
                        UAGUtils._displayVersion += ")";
                    }

                    this.Text = DisplayVersion;
                    this.AllowDrop = true;
                    dataGridView1.Visible = true;

                    // Extra data viewer
                    byteView1 = new ByteViewer
                    {
                        Dock = DockStyle.Fill,
                        AutoScroll = true,
                        AutoSize = true,
                        Visible = false
                    };
                    splitContainer1.Panel2.Controls.Add(byteView1);

                    jsonView = new TextBox
                    {
                        Dock = DockStyle.Fill,
                        Visible = false,
                        AutoSize = true,
                        Multiline = true,
                        ReadOnly = true,
                        MaxLength = int.MaxValue,
                        ScrollBars = ScrollBars.Both,
                    };
                    splitContainer1.Panel2.Controls.Add(jsonView);

                    jsonView.TextChanged += (object sender, EventArgs e) => { if (tableEditor == null) return; tableEditor.dirtySinceLastLoad = true; SetUnsavedChanges(true); };

                    importBinaryData.Visible = false;
                    exportBinaryData.Visible = false;
                    setBinaryData.Visible = false;

                    // Enable double buffering to look nicer
                    if (!SystemInformation.TerminalServerSession)
                    {
                        Type ourGridType = dataGridView1.GetType();
                        PropertyInfo pi = ourGridType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                        pi.SetValue(dataGridView1, true, null);
                    }

                    // Auto resizing
                    SizeChanged += frm_sizeChanged;
                    FormClosing += frm_closing;

                    // position of ByteViewer buttons depends on splitter location so resize if splitter moves
                    splitContainer1.SplitterMoved += (sender, e) => { ForceResize(); };
                    splitContainer1.SplitterDistance = UAGPalette.InitialSplitterDistance;

                    // Drag-and-drop support
                    DragEnter += new DragEventHandler(frm_DragEnter);
                    DragDrop += new DragEventHandler(frm_DragDrop);

                    dataGridView1.MouseWheel += dataGridView1_MouseWheel;
                    //dataGridView1.EditMode = UAGConfig.Data.DoubleClickToEdit ? DataGridViewEditMode.EditProgrammatically : DataGridViewEditMode.EditOnEnter;

                    menuStrip1.Renderer = new UAGMenuStripRenderer();
                    foreach (ToolStripMenuItem entry in menuStrip1.Items)
                    {
                        entry.DropDownOpened += (sender, args) =>
                        {
                            isDropDownOpened[entry] = true;
                        };
                        entry.DropDownClosed += (sender, args) =>
                        {
                            isDropDownOpened[entry] = false;
                        };
                    }

                    ac7decrypt = new AC7Decrypt();

                    UpdateRPC();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while initializing!\n" + ex.GetType() + ": " + ex.Message + "\n\nUAssetGUI will now close.", "UAssetGUI");
                    Environment.Exit(1); // kill the process
                }
            });
        }

        private static Dictionary<ToolStripItem, bool> isDropDownOpened = new Dictionary<ToolStripItem, bool>();
        public static bool IsDropDownOpened(ToolStripItem item)
        {
            if (!isDropDownOpened.ContainsKey(item)) return false;
            return isDropDownOpened[item];
        }

        private List<string> allMappingsKeys = new List<string>();

        private void OpenFileContainerForm(string path = null)
        {
            var test = new FileContainerForm();
            test.CurrentContainerPath = path;
            test.BaseForm = this;
            test.Show();
        }

        internal void UpdateMappings(string newSelection = null, bool alsoCheckVersion = true)
        {
            UAGConfig.LoadMappings();
            UAGUtils.InvokeUI(() =>
            {
                allMappingsKeys.Clear();
                allMappingsKeys.Add("No mappings");
                allMappingsKeys.AddRange(UAGConfig.AllMappings.Keys.OrderBy(s => s).ToArray());
                comboSpecifyMappings.Items.Clear();
                comboSpecifyMappings.Items.AddRange(allMappingsKeys.ToArray());

                string initialSelection = newSelection == null ? (allMappingsKeys.Contains(UAGConfig.Data.PreferredMappings) ? UAGConfig.Data.PreferredMappings : allMappingsKeys[0]) : newSelection;

                bool success = false;
                for (int i = 0; i < allMappingsKeys.Count; i++)
                {
                    if (allMappingsKeys[i] == initialSelection)
                    {
                        comboSpecifyMappings.SelectedIndex = i;
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    comboSpecifyMappings.SelectedIndex = 0;
                }

                UpdateComboSpecifyMappings(alsoCheckVersion);
            });
        }

        private string[] versionOptionsKeys = new string[]
        {
            "Unknown",
            "4.0",
            "4.1",
            "4.2",
            "4.3",
            "4.4",
            "4.5",
            "4.6",
            "4.7",
            "4.8",
            "4.9",
            "4.10",
            "4.11",
            "4.12",
            "4.13",
            "4.14",
            "4.15",
            "4.16",
            "4.17",
            "4.18",
            "4.19",
            "4.20",
            "4.21",
            "4.22",
            "4.23",
            "4.24",
            "4.25",
            "4.26",
            "4.27",
            "5.0EA",
            "5.0",
            "5.1",
            "5.2",
            "5.3",
            "5.4",
            "5.5",
            "5.6",
            "5.7",
        };

        private EngineVersion[] versionOptionsValues = new EngineVersion[]
        {
            EngineVersion.UNKNOWN,
            EngineVersion.VER_UE4_0,
            EngineVersion.VER_UE4_1,
            EngineVersion.VER_UE4_2,
            EngineVersion.VER_UE4_3,
            EngineVersion.VER_UE4_4,
            EngineVersion.VER_UE4_5,
            EngineVersion.VER_UE4_6,
            EngineVersion.VER_UE4_7,
            EngineVersion.VER_UE4_8,
            EngineVersion.VER_UE4_9,
            EngineVersion.VER_UE4_10,
            EngineVersion.VER_UE4_11,
            EngineVersion.VER_UE4_12,
            EngineVersion.VER_UE4_13,
            EngineVersion.VER_UE4_14,
            EngineVersion.VER_UE4_15,
            EngineVersion.VER_UE4_16,
            EngineVersion.VER_UE4_17,
            EngineVersion.VER_UE4_18,
            EngineVersion.VER_UE4_19,
            EngineVersion.VER_UE4_20,
            EngineVersion.VER_UE4_21,
            EngineVersion.VER_UE4_22,
            EngineVersion.VER_UE4_23,
            EngineVersion.VER_UE4_24,
            EngineVersion.VER_UE4_25,
            EngineVersion.VER_UE4_26,
            EngineVersion.VER_UE4_27,
            EngineVersion.VER_UE5_0EA,
            EngineVersion.VER_UE5_0,
            EngineVersion.VER_UE5_1,
            EngineVersion.VER_UE5_2,
            EngineVersion.VER_UE5_3,
            EngineVersion.VER_UE5_4,
            EngineVersion.VER_UE5_5,
            EngineVersion.VER_UE5_6,
            EngineVersion.VER_UE5_7,
        };

        public static readonly string GitHubRepo = "atenfyr/UAssetGUI";
        private Version latestOnlineVersion = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                // sync size from config
                if (UAGConfig.Data.RestoreSize)
                {
                    this.Size = new Size(UAGConfig.Data.StartupWidth, UAGConfig.Data.StartupHeight);
                }

                UAGPalette.InitializeTheme();
                UAGPalette.RefreshTheme(this);

                // load mappings and update combo box
                UpdateMappings(null, false);

                // update version combo box
                string initialSelection = versionOptionsKeys[0];
                try
                {
                    initialSelection = UAGConfig.Data.PreferredVersion;
                }
                catch
                {
                    initialSelection = versionOptionsKeys[0];
                }

                comboSpecifyVersion.Items.AddRange(versionOptionsKeys);
                comboSpecifyVersion.SelectedIndex = 0;

                for (int i = 0; i < versionOptionsKeys.Length; i++)
                {
                    if (versionOptionsKeys[i] == initialSelection)
                    {
                        comboSpecifyVersion.SelectedIndex = i;
                        break;
                    }
                }

                UpdateComboSpecifyVersion();

                // set text for copy/paste/delete
                this.copyToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.C);
                this.pasteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.V);
                this.deleteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Delete);

                // Fetch the latest version from github
                Task.Run(() =>
                {
                    latestOnlineVersion = GitHubAPI.GetLatestVersionFromGitHub(GitHubRepo);
                }).ContinueWith(res =>
                {
                    if (UAGConfig.Data.EnableUpdateNotice && latestOnlineVersion != null && latestOnlineVersion.IsUAGVersionLower())
                    {
                        DialogResult updateBoxRes = MessageBox.Show("A new version of UAssetGUI (v" + latestOnlineVersion + ") is available to download!\nWould you like to open the webpage in your browser?", "Notice", MessageBoxButtons.YesNo);
                        switch (updateBoxRes)
                        {
                            case DialogResult.Yes:
                                UAGUtils.OpenURL("https://github.com/" + GitHubRepo + "/releases/latest");
                                break;
                            default:
                                break;
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

                UpdateVersionFromMappings();

                // Command line parameter support
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    EngineVersion selectedVer = EngineVersion.UNKNOWN;

                    if (args.Length > 2)
                    {
                        if (int.TryParse(args[2], out int selectedVerRaw)) selectedVer = EngineVersion.VER_UE4_0 + selectedVerRaw;
                        else Enum.TryParse(args[2], out selectedVer);
                    }
                    if (args.Length > 3)
                    {
                        UpdateMappings(args[3]);
                    }

                    if (selectedVer > EngineVersion.UNKNOWN) SetParsingVersion(selectedVer);
                    LoadFileAt(args[1]);
                }
            });
        }

        private ISet<string> unknownTypes = new HashSet<string>();
        private ISet<string> rawStructTypes = new HashSet<string>();
        private int numRawStructs = 0;
        private bool RecordUnknownProperty(PropertyData dat)
        {
            if (dat == null) return false;

            if (dat is UnknownPropertyData unknownDat)
            {
                string serializingType = unknownDat?.SerializingPropertyType?.Value;
                if (!string.IsNullOrEmpty(serializingType))
                {
                    unknownTypes.Add(serializingType);
                    return true;
                }
            }
            if (dat is RawStructPropertyData unknownDat2)
            {
                numRawStructs++;
                string serializingType = unknownDat2?.StructType?.ToString();
                if (!string.IsNullOrEmpty(serializingType))
                {
                    rawStructTypes.Add(serializingType);
                    return true;
                }
            }
            return false;
        }

        private void GetUnknownProperties(PropertyData dat)
        {
            RecordUnknownProperty(dat);

            if (dat is ArrayPropertyData arrDat)
            {
                for (int i = 0; i < arrDat.Value.Length; i++) GetUnknownProperties(arrDat.Value[i]);
            }
            else if (dat is StructPropertyData strucDat)
            {
                for (int i = 0; i < strucDat.Value.Count; i++) GetUnknownProperties(strucDat.Value[i]);
            }
            else if (dat is MapPropertyData mapDat)
            {
                foreach (var entry in mapDat.Value)
                {
                    GetUnknownProperties(entry.Key);
                    GetUnknownProperties(entry.Value);
                }
            }
        }

        public uint GetFileSignature(string path, out byte[] nextBytes)
        {
            byte[] buffer = new byte[4];
            uint res = uint.MaxValue;
            nextBytes = new byte[32];

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Read(buffer, 0, buffer.Length) == buffer.Length) res = BitConverter.ToUInt32(buffer, 0);
                fs.Read(nextBytes, 0, nextBytes.Length);
            }

            return res;
        }

        public void LoadFileAt(string filePath)
        {
            UAGUtils.InvokeUI(() => LoadFileAtInternal(filePath));
        }

        public DateTime LastLoadTimestamp = DateTime.UtcNow;
        private void LoadFileAtInternal(string filePath)
        {
            dataGridView1.Visible = true;
            byteView1.Visible = false;
            jsonView.Visible = false;

            bool didACE7Decrypt = false;
            string jsonTracingPath = null;

            try
            {
                UAsset targetAsset;
                string fileExtension = Path.GetExtension(filePath);
                string savingPath = filePath;
                bool desiredSetUnsavedChanges = false;
                switch (fileExtension)
                {
                    case ".json":
                        savingPath = Path.ChangeExtension(filePath, "uasset");
                        using (var sr = new FileStream(filePath, FileMode.Open))
                        {
                            targetAsset = UAsset.DeserializeJson(sr);
                        }
                        targetAsset.Mappings = ParsingMappings;
                        desiredSetUnsavedChanges = true;
                        break;
                    case ".pak":
                        OpenFileContainerForm(filePath);
                        return;
                    default:
                        MapStructTypeOverrideForm.LoadFromConfig();

                        uint sig = GetFileSignature(filePath, out byte[] nextBytes);

                        uint nextFourBytes = uint.MaxValue;
                        uint ue4CookedHeaderSize = uint.MaxValue;
                        if (nextBytes.Length >= 4) nextFourBytes = BitConverter.ToUInt32(nextBytes.Take(4).ToArray());
                        if (nextBytes.Length >= 24) ue4CookedHeaderSize = BitConverter.ToUInt32(nextBytes.Skip(20).Take(4).ToArray());

                        if (sig == UAsset.ACE7_MAGIC)
                        {
                            // Decrypt file in-situ
                            ac7decrypt.Decrypt(filePath, filePath);
                            didACE7Decrypt = true;
                        }
                        else if (sig != UAsset.UASSET_MAGIC)
                        {
                            // check if opened .usmap
                            if (Path.GetExtension(filePath) == ".usmap")
                            {
                                ImportMappingsFromPathInteractive(filePath);
                            }
                            // check if accidentally opened .uexp
                            else if (Path.GetExtension(filePath) == ".uexp")
                            {
                                MessageBox.Show("Failed to open this file! This is a .uexp file, which cannot be read directly. Please open the respective .uasset file instead.", "Uh oh!");
                            }
                            // check if Zen asset for custom popup
                            // this definitely has potential for false positives, but it will still filter out basically any other file type, if it mattered that much i'd just actually parse the thing
                            else if (Path.GetExtension(filePath) == ".uasset" && (sig == 0 || sig == 1) && nextFourBytes > 40 && nextFourBytes < 1e9) // IsUnversioned reasonable, HeaderSize reasonable
                            {
                                DialogResult messageBoxRes = MessageBox.Show("Failed to open this file! UE5 Zen Loader assets cannot currently be loaded directly into UAssetGUI. You could try to extract traditional cooked assets from IOStore container files by using something like ZenTools by Archengius, or otherwise try software like FModel to read the asset.\n\nWould you like to open the GitHub page for ZenTools?", "Uh oh!", MessageBoxButtons.YesNo);
                                switch (messageBoxRes)
                                {
                                    case DialogResult.Yes:
                                        UAGUtils.OpenURL("https://github.com/Archengius/ZenTools");
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (Path.GetExtension(filePath) == ".uasset" && nextFourBytes == 0 && ue4CookedHeaderSize > 40 && ue4CookedHeaderSize < 1e9) // zero FName, CookedHeaderSize reasonable
                            {
                                DialogResult messageBoxRes = MessageBox.Show("Failed to open this file! UE4 Zen Loader assets cannot currently be loaded directly into UAssetGUI. You could try to extract traditional cooked assets from IOStore container files by using something like ZenTools-UE4, originally by Archengius and developed by Ryn/WistfulHopes, or otherwise try software like FModel to read the asset.\n\nWould you like to open the GitHub page for ZenTools-UE4?", "Uh oh!", MessageBoxButtons.YesNo);
                                switch (messageBoxRes)
                                {
                                    case DialogResult.Yes:
                                        UAGUtils.OpenURL("https://github.com/WistfulHopes/ZenTools-UE4");
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Failed to open this file! File format not recognized", "Uh oh!");
                            }
                            return;
                        }

                        targetAsset = new UAsset(ParsingVersion);
                        targetAsset.FilePath = filePath;
                        targetAsset.Mappings = ParsingMappings;
                        targetAsset.CustomSerializationFlags = (CustomSerializationFlags)UAGConfig.Data.CustomSerializationFlags;
                        if (MapStructTypeOverrideForm.MapStructTypeOverride != null) targetAsset.MapStructTypeOverride = MapStructTypeOverrideForm.MapStructTypeOverride;

                        var strmRaw = targetAsset.PathToStream(filePath);
#if DEBUGTRACING
                        var strm = new UAssetAPI.Trace.TraceStream(strmRaw, filePath);
                        UAssetAPI.Trace.LoggingAspect.Start(strm);
                        targetAsset.Read(new AssetBinaryReader(strm, targetAsset));
                        jsonTracingPath = UAssetAPI.Trace.LoggingAspect.Stop();
#else
                        targetAsset.Read(new AssetBinaryReader(strmRaw, targetAsset));
#endif
                        break;
                }

                currentSavingPath = savingPath;
                SetUnsavedChanges(false);

                tableEditor = new TableHandler(dataGridView1, targetAsset, treeView1, jsonView);

                saveToolStripMenuItem.Enabled = !IsReadOnly();
                saveAsToolStripMenuItem.Enabled = true;
                findToolStripMenuItem.Enabled = true;
                //stageToolStripMenuItem.Enabled = true;

                tableEditor.FillOutTree(!UAGConfig.Data.EnableDynamicTree);
                tableEditor.Load();

                UAGPalette.RefreshTheme(this);

                bool hasDuplicates = false;
                HashSet<string> nameMapRefs = new HashSet<string>();
                foreach (FString x in tableEditor.asset.GetNameMapIndexList())
                {
                    if (nameMapRefs.Contains(x.Value))
                    {
                        hasDuplicates = true;
                        break;
                    }
                    nameMapRefs.Add(x.Value);
                }
                nameMapRefs = null;

                int failedCategoryCount = 0;
                unknownTypes = new HashSet<string>();
                rawStructTypes = new HashSet<string>();
                numRawStructs = 0;
                foreach (Export cat in tableEditor.asset.Exports)
                {
                    if (cat is RawExport) failedCategoryCount++;
                    if (cat is NormalExport usNormal)
                    {
                        foreach (PropertyData dat in usNormal.Data) GetUnknownProperties(dat);
                    }
                }

                bool failedToMaintainBinaryEquality = !string.IsNullOrEmpty(tableEditor.asset.FilePath) && !tableEditor.asset.VerifyBinaryEquality();

#if DEBUGTRACING
                if (jsonTracingPath != null && (failedToMaintainBinaryEquality || failedCategoryCount > 0))
                {
                    // if ser-hex-viewer available (https://github.com/trumank/ser-hex), run that
                    try
                    {
                        Process.Start(new ProcessStartInfo("ser-hex-viewer", "\"" + jsonTracingPath + "\"") { UseShellExecute = false });
                    }
                    catch { }
                }
#endif

                if (didACE7Decrypt)
                {
                    MessageBox.Show("This file uses Ace Combat 7 encryption and was decrypted in-situ.", "Notice");
                }

                if (failedCategoryCount > 0)
                {
                    MessageBox.Show("Failed to parse " + failedCategoryCount + " export" + (failedCategoryCount == 1 ? "" : "s") + "!", "Notice");
                }

                if (hasDuplicates)
                {
                    MessageBox.Show("Encountered duplicate name map entries! Serialized FNames will coalesce to one of the entries in the map and binary equality may not be maintained.", "Notice");
                }

                if (unknownTypes.Count > 0)
                {
                    MessageBox.Show("Encountered " + unknownTypes.Count + " unknown property type" + (unknownTypes.Count == 1 ? "" : "s") + ":\n" + string.Join(", ", unknownTypes) + (failedToMaintainBinaryEquality ? "" : "\n\nThe asset will still parse normally."), "Notice");
                }

                if (rawStructTypes.Count > 0)
                {
                    MessageBox.Show("Encountered " + numRawStructs + " struct" + (numRawStructs == 1 ? "" : "s") + " that could not be parsed, and " + (numRawStructs == 1 ? "was" : "were") + " instead read as an array of bytes. " + (numRawStructs == 1 ? "It has the following type" : "They have the following types") + ":\n" + string.Join(", ", rawStructTypes) + (failedToMaintainBinaryEquality ? "" : "\n\nThe asset will still parse normally."), "Notice");
                }

                if (tableEditor.asset.HasUnversionedProperties && tableEditor.asset.Mappings == null)
                {
                    MessageBox.Show("Failed to parse unversioned properties! Exports cannot be parsed for this asset unless a valid set of mappings is provided. " + UsmapInstructionsNotice, "Notice");
                }

                if (tableEditor.asset.HasUnversionedProperties && failedCategoryCount > 0 && (tableEditor.asset.OtherAssetsFailedToAccess?.Count ?? 0) > 0)
                {
                    string formattedListOfFailedToAccessAssets = string.Join("\n", tableEditor.asset.OtherAssetsFailedToAccess);
                    MessageBox.Show("UAssetAPI attempted to access the following assets, but failed to do so. Some errors may potentially be resolved by giving it access to these assets. You can either include them in the same directory, or reconstruct the game's Content directory tree.\n\n" + formattedListOfFailedToAccessAssets, "Notice");
                }

                if (failedToMaintainBinaryEquality)
                {
                    MessageBox.Show("Failed to maintain binary equality! UAssetAPI may not be able to parse this particular asset correctly, and you may not be able to load this file in-game if modified.", "Uh oh!");
                }

                if (!tableEditor.asset.IsUnversioned)
                {
                    EngineVersion calculatedVer = tableEditor.asset.GetEngineVersion();
                    if (calculatedVer != EngineVersion.UNKNOWN) SetParsingVersion(calculatedVer);
                }
                if (desiredSetUnsavedChanges) SetUnsavedChanges(desiredSetUnsavedChanges);
            }
            catch (Exception ex)
            {
                string formattedListOfFailedToAccessAssets = null;
                if (tableEditor?.asset != null && tableEditor.asset.HasUnversionedProperties && (tableEditor.asset.OtherAssetsFailedToAccess?.Count ?? 0) > 0)
                {
                    formattedListOfFailedToAccessAssets = string.Join("\n", tableEditor.asset.OtherAssetsFailedToAccess);
                }

                //MessageBox.Show(ex.StackTrace);
                currentSavingPath = "";
                SetUnsavedChanges(false);
                tableEditor = null;
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;
                findToolStripMenuItem.Enabled = false;
                //stageToolStripMenuItem.Enabled = false;

                treeView1.Nodes.Clear();
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
                UAGPalette.RefreshTheme(this);

                switch (ex)
                {
                    case IOException _:
                        MessageBox.Show("Failed to open this file! Please make sure the specified engine version is correct.", "Uh oh!");
                        break;
                    case FormatException formatEx:
                        MessageBox.Show("Failed to parse this file!\n" + formatEx.GetType() + ": " + formatEx.Message, "Uh oh!");
                        break;
                    case UnknownEngineVersionException _:
                        MessageBox.Show("Please specify an engine version using the dropdown at the upper-right corner of this window before opening an unversioned asset.", "Uh oh!");
                        break;
                    default:
                        MessageBox.Show("Encountered an unknown error when trying to open this file!\n" + ex.GetType() + ": " + ex.Message, "Uh oh!");
                        break;
                }

                if (formattedListOfFailedToAccessAssets != null)
                {
                    MessageBox.Show("UAssetAPI attempted to access the following assets, but failed to do so. It's possible that this error could be resolved by giving it access to these assets. You can either include them in the same directory, or reconstruct the game's Content directory tree.\n\n" + formattedListOfFailedToAccessAssets, "Notice");
                }
            }
            finally
            {
                LastLoadTimestamp = DateTime.UtcNow;
                UpdateRPC();

                treeView1.Select();
            }
        }

        public bool IsReadOnly()
        {
            int idx = currentSavingPath.Replace(Path.DirectorySeparatorChar, '/').LastIndexOf(ReadOnlyPathKeyword);
            return idx >= 0;
        }

        public bool existsUnsavedChanges = false;
        public static readonly string ReadOnlyPathKeyword = "UAG_read_only/";
        public void SetUnsavedChanges(bool flag)
        {
            existsUnsavedChanges = flag;
            if (string.IsNullOrEmpty(currentSavingPath))
            {
                this.Text = DisplayVersion;
            }
            else
            {
                string formattedCurrentSavingPath = currentSavingPath;
                int idx = currentSavingPath.Replace(Path.DirectorySeparatorChar, '/').LastIndexOf(ReadOnlyPathKeyword);
                if (idx >= 0)
                {
                    formattedCurrentSavingPath = formattedCurrentSavingPath.Substring(idx + ReadOnlyPathKeyword.Length);
                }

                if (existsUnsavedChanges)
                {
                    this.Text = DisplayVersion + " - *" + formattedCurrentSavingPath;
                }
                else
                {
                    this.Text = DisplayVersion + " - " + formattedCurrentSavingPath;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Unreal Assets (*.uasset, *.umap, *.json)|*.uasset;*.umap;*.json|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFileAt(openFileDialog.FileName);
                }
            }
        }

        internal string currentSavingPath = "";

        private bool ForceSave(string path)
        {
            if (tableEditor != null && !string.IsNullOrEmpty(currentSavingPath))
            {
                if (UAGConfig.Data.EnableBak && File.Exists(path)) File.Copy(path, path + ".bak", true);
                if (UAGConfig.Data.EnableBak && File.Exists(Path.ChangeExtension(path, "uexp"))) File.Copy(Path.ChangeExtension(path, "uexp"), Path.ChangeExtension(path, "uexp") + ".bak", true);

                tableEditor.Save(true);

                bool isLooping = true;
                while (isLooping)
                {
                    isLooping = false;
                    try
                    {
                        tableEditor.asset.Write(path);
                        SetUnsavedChanges(false);
                        tableEditor.Load();
                        return true;
                    }
                    catch (NameMapOutOfRangeException ex)
                    {
                        try
                        {
                            tableEditor.asset.AddNameReference(ex.RequiredName);
                            isLooping = true;
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show("Failed to save! " + ex2.Message, "Uh oh!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to save! " + ex.Message, "Uh oh!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to save!", "Uh oh!");
            }
            return false;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor?.asset == null) return;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Unreal Assets (*.uasset, *.umap)|*.uasset;*.umap|UAssetAPI JSON (*.json)|*.json|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (Path.GetExtension(dialog.FileName) == ".json")
                    {
                        // JSON export
                        string jsonSerializedAsset = tableEditor.asset.SerializeJson(Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(dialog.FileName, jsonSerializedAsset);
                    }
                    else
                    {
                        currentSavingPath = dialog.FileName;
                        ForceSave(currentSavingPath);
                    }
                }
                else if (res != DialogResult.Cancel)
                {
                    MessageBox.Show("Failed to save!", "Uh oh!");
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ForceSave(currentSavingPath);
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tableEditor?.ChangeAllExpansionStatus(true);
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tableEditor?.ChangeAllExpansionStatus(false);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (treeView1?.SelectedNode != null && treeView1.SelectedNode is PointingTreeNode pointerNode && pointerNode.Type == PointingTreeNodeType.Kismet) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    if (tableEditor == null || dataGridView1.IsCurrentCellInEditMode) break;
                    copyToolStripMenuItem.PerformClick();
                    return true;
                case Keys.Control | Keys.V:
                    if (dataGridView1.ReadOnly || !dataGridView1.AllowUserToAddRows) break;
                    if (tableEditor == null || dataGridView1.IsCurrentCellInEditMode) break;
                    pasteToolStripMenuItem.PerformClick();
                    return true;
                case Keys.Delete:
                    if (dataGridView1.ReadOnly || !dataGridView1.AllowUserToAddRows) break;
                    if (tableEditor == null || dataGridView1.IsCurrentCellInEditMode) break;
                    deleteToolStripMenuItem.PerformClick();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string CopyIndividual(int rowIndex)
        {
            object objectToCopy = null;

            switch (tableEditor.mode)
            {
                case TableHandlerMode.ExportData:
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Type == PointingTreeNodeType.ByteArray)
                        {
                            string parsedData = BitConverter.ToString(pointerNode.Pointer is RawExport ? ((RawExport)pointerNode.Pointer).Data : ((NormalExport)pointerNode.Pointer).Extras)?.Replace("-", " ");
                            return string.IsNullOrWhiteSpace(parsedData) ? "zero" : parsedData;
                        }
                        else if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            string parsedData = BitConverter.ToString(((StructExport)pointerNode.Pointer).ScriptBytecodeRaw)?.Replace("-", " ");
                            return string.IsNullOrWhiteSpace(parsedData) ? "zero" : parsedData;
                        }
                        else if (pointerNode.Pointer is StructPropertyData copyingDat1)
                        {
                            if (treeView1.Focused) objectToCopy = copyingDat1;
                            if (rowIndex >= 0 && !treeView1.Focused && copyingDat1.Value.Count > rowIndex) objectToCopy = copyingDat1.Value[rowIndex];
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData copyingDat2)
                        {
                            if (treeView1.Focused) objectToCopy = copyingDat2;
                            if (rowIndex >= 0 && !treeView1.Focused && copyingDat2.Value.Length > rowIndex) objectToCopy = copyingDat2.Value[rowIndex];
                        }
                        else if (pointerNode.Pointer is PointingDictionaryEntry copyingDat3)
                        {
                            // don't allow copying the dictionary entry itself
                            if (rowIndex >= 0) objectToCopy = treeView1.ContainsFocus ? null : (rowIndex == 0 ? copyingDat3.Entry.Key : copyingDat3.Entry.Value);
                        }
                        else if (pointerNode.Pointer is PropertyData[] copyingDat4)
                        {
                            if (treeView1.Focused) objectToCopy = copyingDat4;
                            if (rowIndex >= 0 && !treeView1.Focused && copyingDat4.Length > rowIndex) objectToCopy = copyingDat4[rowIndex];
                        }
                        else if (pointerNode.Pointer is Export || (treeView1.Focused && pointerNode.WillCopyWholeExport))
                        {
                            switch (pointerNode.Type)
                            {
                                case PointingTreeNodeType.Normal:
                                    var copyingDat5 = tableEditor.asset.Exports[pointerNode.ExportNum];
                                    if (treeView1.Focused)
                                    {
                                        objectToCopy = copyingDat5;
                                    }
                                    else if (copyingDat5 is NormalExport copyingDat6)
                                    {
                                        if (rowIndex >= 0 && !treeView1.Focused && copyingDat6.Data.Count > rowIndex) objectToCopy = copyingDat6.Data[rowIndex];
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }

            if (objectToCopy != null)
            {
                return tableEditor.asset.SerializeJsonObject(objectToCopy, Newtonsoft.Json.Formatting.None);
            }

            // fallback to copying raw row data
            if (rowIndex >= 0)
            {
                var currentRow = dataGridView1.Rows[rowIndex];
                string[] newClipboardText = new string[currentRow.Cells.Count];
                for (int i = 0; i < currentRow.Cells.Count; i++)
                {
                    newClipboardText[i] = currentRow.Cells[i].Value?.ToString() ?? string.Empty;
                }
                return JsonConvert.SerializeObject(newClipboardText, Formatting.None);
            }

            return null;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor == null) return;

            string fullData = null;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Dictionary<int, string> dataByRowIndex = new Dictionary<int, string>();
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    dataByRowIndex[row.Index] = CopyIndividual(row.Index).Replace("\r", "").Replace("\n", "");
                }
                fullData = string.Join('\n', dataByRowIndex.OrderBy(entry => entry.Key).Select(entry => entry.Value)); // sort values by key ascending
            }
            else
            {
                fullData = CopyIndividual(-1);
            }

            if (fullData != null) Clipboard.SetText(fullData);
        }

        private TreeNode SearchForTreeNode(TreeView node, int expNum)
        {
            foreach (TreeNode entry in node.Nodes)
            {
                TreeNode res = SearchForTreeNode(entry, expNum);
                if (res != null) return res;
            }
            return null;
        }

        private TreeNode SearchForTreeNode(TreeNode node, int expNum)
        {
            foreach (TreeNode entry in node.Nodes)
            {
                if (node is PointingTreeNode pointerNode2 && pointerNode2.ExportNum == expNum) return pointerNode2;

                TreeNode res = SearchForTreeNode(entry, expNum);
                if (res != null) return res;
            }
            return null;
        }

        private void PasteIndividual(int rowIndex, string dat)
        {
            PropertyData deserializedClipboard = null;
            try
            {
                deserializedClipboard = tableEditor.asset.DeserializeJsonObject<PropertyData>(dat);
            }
            catch (Exception)
            {
                // the thing we're trying to paste probably isn't a PropertyData
            }

            switch (tableEditor.mode)
            {
                case TableHandlerMode.ExportData:
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Type == PointingTreeNodeType.ByteArray)
                        {
                            try
                            {
                                if (pointerNode.Pointer is RawExport)
                                {
                                    ((RawExport)pointerNode.Pointer).Data = dat == "zero" ? new byte[0] : UAPUtils.ConvertHexStringToByteArray(dat);
                                }
                                else if (pointerNode.Pointer is NormalExport)
                                {
                                    ((NormalExport)pointerNode.Pointer).Extras = dat== "zero" ? new byte[0] : UAPUtils.ConvertHexStringToByteArray(dat);
                                }
                            }
                            catch (Exception)
                            {
                                // the thing we're trying to paste probably isn't a byte array
                            }

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            try
                            {
                                ((StructExport)pointerNode.Pointer).ScriptBytecodeRaw = dat == "zero" ? new byte[0] : UAPUtils.ConvertHexStringToByteArray(dat);
                            }
                            catch (Exception)
                            {
                                // the thing we're trying to paste probably isn't a byte array
                            }

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Pointer is StructPropertyData copyingDat1 && deserializedClipboard != null)
                        {
                            if (rowIndex < 0) return;
                            copyingDat1.Value.Insert(rowIndex, deserializedClipboard);

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData copyingDat2 && deserializedClipboard != null)
                        {
                            if (rowIndex < 0) return;
                            List<PropertyData> origArr = copyingDat2.Value.ToList();
                            origArr.Insert(rowIndex, deserializedClipboard);
                            copyingDat2.Value = origArr.ToArray();

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Pointer is NormalExport copyingDat3 && deserializedClipboard != null)
                        {
                            if (rowIndex < 0) return;
                            copyingDat3.Data.Insert(rowIndex, deserializedClipboard);

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else
                        {
                            // check if we're pasting a whole export
                            Export deserExport = null;
                            try
                            {
                                deserExport = tableEditor.asset.DeserializeJsonObject<Export>(dat);
                            }
                            catch (Exception)
                            {
                                // the thing we're trying to paste probably isn't an Export
                            }

                            if (deserExport != null)
                            {
                                // add a new export after the current one
                                tableEditor.asset.Exports.Insert(pointerNode.ExportNum + 1, deserExport);

                                if (tableEditor != null)
                                {
                                    SetUnsavedChanges(true);
                                    tableEditor.Save(true);
                                    tableEditor.FillOutTree(!UAGConfig.Data.EnableDynamicTree);

                                    TreeNode newNode = SearchForTreeNode(treeView1, pointerNode.ExportNum + 1);
                                    newNode.EnsureVisible();
                                    newNode.ExpandAll();
                                }
                            }
                        }
                    }
                    break;
            }

            // fallback to pasting raw row data
            if (rowIndex >= 0)
            {
                try
                {
                    string[] rawData = JsonConvert.DeserializeObject<string[]>(dat);
                    dataGridView1.Rows.Insert(rowIndex, rawData);
                    SetUnsavedChanges(true);
                    return;
                }
                catch (Exception)
                {
                    // the thing we're trying to paste probably isn't a string array
                }
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor == null) return;
            int rowIndex = dataGridView1.SelectedCells.Count > 0 ? dataGridView1.SelectedCells[0].RowIndex : -1;

            string[] allDats = Clipboard.GetText().Split('\n');
            foreach (string dat in allDats)
            {
                PasteIndividual(rowIndex, dat);
                rowIndex += 1; // paste after new row
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor == null) return;
            int rowIndex = dataGridView1.SelectedCells.Count > 0 ? dataGridView1.SelectedCells[0].RowIndex : -1;

            switch (tableEditor.mode)
            {
                case TableHandlerMode.ExportData:
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Type == PointingTreeNodeType.ByteArray)
                        {
                            if (pointerNode.Pointer is RawExport)
                            {
                                ((RawExport)pointerNode.Pointer).Data = new byte[0];
                            }
                            else if (pointerNode.Pointer is NormalExport)
                            {
                                ((NormalExport)pointerNode.Pointer).Extras = new byte[0];
                            }

                            return;
                        }
                        else if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            ((StructExport)pointerNode.Pointer).ScriptBytecodeRaw = new byte[0];

                            return;
                        }
                        else if (pointerNode.Pointer is StructPropertyData copyingDat1)
                        {
                            if (rowIndex < 0 || rowIndex >= copyingDat1.Value.Count) return;
                            copyingDat1.Value.RemoveAt(rowIndex);

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData copyingDat2)
                        {
                            if (rowIndex < 0 || rowIndex >= copyingDat2.Value.Length) return;
                            List<PropertyData> origArr = copyingDat2.Value.ToList();
                            origArr.RemoveAt(rowIndex);
                            copyingDat2.Value = origArr.ToArray();

                            SetUnsavedChanges(true);
                            if (tableEditor != null)
                            {
                                tableEditor.Load();
                            }
                            return;
                        }
                        else if (pointerNode.Pointer is Export || (treeView1.Focused && pointerNode.WillCopyWholeExport))
                        {
                            switch (pointerNode.Type)
                            {
                                case PointingTreeNodeType.Normal:
                                    if (treeView1.Focused)
                                    {
                                        DialogResult res = MessageBox.Show("Are you sure you want to delete this export?\nTHIS OPERATION CANNOT BE UNDONE!", DisplayVersion, MessageBoxButtons.OKCancel);
                                        if (res != DialogResult.OK) break;

                                        tableEditor.asset.Exports.RemoveAt(pointerNode.ExportNum);

                                        SetUnsavedChanges(true);
                                        tableEditor.Save(true);
                                        tableEditor.FillOutTree(!UAGConfig.Data.EnableDynamicTree);

                                        TreeNode newNode = SearchForTreeNode(treeView1, pointerNode.ExportNum);
                                        if (newNode != null)
                                        {
                                            newNode.EnsureVisible();
                                            newNode.Expand();
                                        }
                                    }
                                    else if (pointerNode.Pointer is NormalExport copyingDat3)
                                    {
                                        if (rowIndex < 0 || rowIndex >= copyingDat3.Data.Count) return;
                                        copyingDat3.Data.RemoveAt(rowIndex);

                                        SetUnsavedChanges(true);
                                        if (tableEditor != null)
                                        {
                                            tableEditor.Load();
                                        }
                                    }

                                    return;
                            }

                            return;
                        }

                    }
                    break;
            }

            // fallback to just deleting the whole row and refreshing
            if (rowIndex >= 0)
            {
                foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells) cell.Value = null;
                SetUnsavedChanges(true);
                if (tableEditor != null)
                {
                    tableEditor.Save(true);
                }
            }
        }

        private void dataGridEditCell(object sender, EventArgs e)
        {
            if (tableEditor != null && tableEditor.readyToSave)
            {
                tableEditor.Save(false);
            }
        }

        private void dataGridClickCell(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Style != null && dataGridView1.CurrentCell.Style.Font != null && dataGridView1.CurrentCell.Style.Font.Underline == true)
            {
                switch (dataGridView1.CurrentCell.Tag)
                {
                    case "CategoryJump":
                        DataGridViewCell previousCell = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex - 1];
                        if (previousCell == null || previousCell.Value == null) return;

                        int jumpingTo = -1;
                        if (previousCell.Value is string) int.TryParse((string)previousCell.Value, out jumpingTo);
                        if (previousCell.Value is int) jumpingTo = (int)previousCell.Value;
                        if (jumpingTo < 0) return;

                        TreeNode topSelectingNode = treeView1.Nodes[treeView1.Nodes.Count - 1];
                        if (topSelectingNode.Nodes.Count > (jumpingTo - 1))
                        {
                            topSelectingNode = topSelectingNode.Nodes[jumpingTo - 1];
                            if (topSelectingNode.Nodes.Count > 0)
                            {
                                topSelectingNode = topSelectingNode.Nodes[0];
                            }
                        }
                        treeView1.SelectedNode = topSelectingNode;
                        break;
                    case "ChildJump":
                        int jumpingIndex = dataGridView1.CurrentCell.RowIndex;
                        if (jumpingIndex < 0 || jumpingIndex >= treeView1.SelectedNode.Nodes.Count)
                        {
                            MessageBox.Show("Please select View -> Recalculate Nodes before attempting to jump to this node.", "Notice");
                        }
                        else
                        {
                            treeView1.SelectedNode = treeView1.SelectedNode.Nodes[jumpingIndex];
                        }
                        break;
                }
            }
        }

        public void UpdateModeFromSelectedNode(TreeNode e)
        {
            if (e == null) return;

            string selectedNodeText = e.Text;
            string parentSelectedNodeText = e.Parent?.Text;
            if (tableEditor != null)
            {
                tableEditor.mode = TableHandlerMode.ExportData;
                switch (selectedNodeText)
                {
                    case "General Information":
                        tableEditor.mode = TableHandlerMode.GeneralInformation;
                        break;
                    case "Name Map":
                        tableEditor.mode = TableHandlerMode.NameMap;
                        break;
                    case "Soft Object Paths":
                        tableEditor.mode = TableHandlerMode.SoftObjectPathList;
                        break;
                    case "Import Data":
                        tableEditor.mode = TableHandlerMode.Imports;
                        break;
                    case "Export Information":
                        tableEditor.mode = TableHandlerMode.ExportInformation;
                        break;
                    case "Depends Map":
                        tableEditor.mode = TableHandlerMode.DependsMap;
                        break;
                    case "Soft Package References":
                        tableEditor.mode = TableHandlerMode.SoftPackageReferences;
                        break;
                    case "World Tile Info":
                        tableEditor.mode = TableHandlerMode.WorldTileInfo;
                        break;
                    case "Data Resources":
                        tableEditor.mode = TableHandlerMode.DataResources;
                        break;
                    case "Custom Version Container":
                        tableEditor.mode = TableHandlerMode.CustomVersionContainer;
                        break;
                }

                if (parentSelectedNodeText == "World Tile Info") tableEditor.mode = TableHandlerMode.WorldTileInfo;

                tableEditor.Load();
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (tableEditor != null && tableEditor.dirtySinceLastLoad)
            {
                // force refresh before tabbing out if we need to finalize changes before serialization (typically, when null entries exist to get rid of)
                // we don't just do this every time for performance reasons
                tableEditor.Save(true);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateModeFromSelectedNode(e.Node);
            UAGUtils.InvokeUI(treeView1.Select);
        }

        private void dataGridView1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count < 1) return;
            if (!UAGConfig.Data.ChangeValuesOnScroll) return;
            var selectedCell = dataGridView1.SelectedCells[0];
            if (selectedCell.ReadOnly) return;

            int deltaDir = e.Delta > 0 ? -1 : 1;

            bool didSomething = true;
            if (selectedCell.Value is int)
            {
                selectedCell.Value = (int)selectedCell.Value + deltaDir;
            }
            else if (selectedCell.Value is float)
            {
                selectedCell.Value = (float)selectedCell.Value + deltaDir;
            }
            else if (selectedCell.Value is bool)
            {
                selectedCell.Value = !(bool)selectedCell.Value;
            }
            else if (selectedCell.Value is string)
            {
                string rawVal = (string)selectedCell.Value;
                string rawValLower = rawVal.ToLowerInvariant();
                if (int.TryParse(rawVal, out int castedInt))
                {
                    selectedCell.Value = (castedInt + deltaDir).ToString();
                }
                else if (float.TryParse(rawVal, out float castedFloat))
                {
                    selectedCell.Value = (castedFloat + deltaDir).ToString();
                }
                else if (rawValLower == "true" || rawValLower == "false")
                {
                    selectedCell.Value = (rawValLower == "true" ? false : true).ToString();
                }
                else if (rawValLower == Encoding.ASCII.HeaderName || rawValLower == Encoding.Unicode.HeaderName)
                {
                    selectedCell.Value = rawValLower == Encoding.ASCII.HeaderName ? Encoding.Unicode.HeaderName : Encoding.ASCII.HeaderName;
                }
                else
                {
                    didSomething = false;
                }
            }
            else
            {
                didSomething = false;
            }

            if (didSomething)
            {
                dataGridView1.RefreshEdit();
                if (e is HandledMouseEventArgs ee)
                {
                    ee.Handled = true;
                }
            }
        }

        public void ForceResize()
        {
            if (byteView1 != null) byteView1.Refresh();
            if (jsonView != null) jsonView.Refresh();

            comboSpecifyVersion.Location = new Point(this.splitContainer1.Location.X + this.splitContainer1.Size.Width - this.comboSpecifyVersion.Width, this.menuStrip1.Size.Height - this.comboSpecifyVersion.Size.Height - 2);
            comboSpecifyMappings.Location = new Point(comboSpecifyVersion.Location.X - 5 - comboSpecifyMappings.Width, comboSpecifyVersion.Location.Y);

            // :skull_emoji:
            importBinaryData.Location = new Point(Math.Max(menuStrip1.Left + menuStrip1.GetPreferredSize(Size.Empty).Width, splitContainer1.Location.X + splitContainer1.SplitterDistance + splitContainer1.SplitterWidth), comboSpecifyVersion.Location.Y);
            exportBinaryData.Location = new Point(importBinaryData.Location.X + importBinaryData.Size.Width + 5, importBinaryData.Location.Y);
            setBinaryData.Location = new Point(exportBinaryData.Location.X + exportBinaryData.Size.Width + 5, importBinaryData.Location.Y);
            importBinaryData.Size = new Size(importBinaryData.Width, comboSpecifyVersion.Height); importBinaryData.Font = new Font(importBinaryData.Font.FontFamily, 6.75f, importBinaryData.Font.Style);
            exportBinaryData.Size = new Size(exportBinaryData.Width, comboSpecifyVersion.Height); exportBinaryData.Font = importBinaryData.Font;
            setBinaryData.Size = new Size(setBinaryData.Width, comboSpecifyVersion.Height); setBinaryData.Font = importBinaryData.Font;
        }

        private void frm_sizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void frm_closing(object sender, FormClosingEventArgs e)
        {
            if (UAGConfig.Data.RestoreSize)
            {
                UAGConfig.Data.StartupWidth = this.Size.Width;
                UAGConfig.Data.StartupHeight = this.Size.Height;
                UAGConfig.Save();
            }

            if (existsUnsavedChanges && !IsReadOnly())
            {
                DialogResult res = MessageBox.Show("Do you want to save your changes?", DisplayVersion, MessageBoxButtons.YesNoCancel);
                switch (res)
                {
                    case DialogResult.Yes:
                        if (!ForceSave(currentSavingPath)) e.Cancel = true;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

            if (!e.Cancel) DisposeDiscordRpc();

            // delete temp folder
            try
            {
                Directory.Delete(Path.Combine(Path.GetTempPath(), "UAG_read_only"), true);
            }
            catch { }
        }

        private void frm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void frm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) LoadFileAt(files[0]);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.Save(true);
            }
        }

        private void refreshFullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.Save(true);
                tableEditor.FillOutTree(!UAGConfig.Data.EnableDynamicTree);
            }
        }

        private void issuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.OpenURL("https://github.com/" + GitHubRepo + "/issues");
        }

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.OpenURL("https://github.com/" + GitHubRepo);
        }

        private void apiLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.OpenURL("https://github.com/atenfyr/UAssetAPI");
        }

        public void SetParsingVersion(EngineVersion ver)
        {
            if (ver == ParsingVersion) return;

            for (int i = 0; i < versionOptionsValues.Length; i++)
            {
                if (versionOptionsValues[i] == ver)
                {
                    comboSpecifyVersion.SelectedIndex = i;
                    UpdateComboSpecifyVersion();
                    return;
                }
            }

            string verStringRepresentation = "(" + Convert.ToString((int)ver) + ")";
            comboSpecifyVersion.Items.Add(verStringRepresentation);
            Array.Resize(ref versionOptionsKeys, versionOptionsKeys.Length + 1);
            versionOptionsKeys[versionOptionsKeys.Length - 1] = verStringRepresentation;
            Array.Resize(ref versionOptionsValues, versionOptionsValues.Length + 1);
            versionOptionsValues[versionOptionsValues.Length - 1] = ver;
            comboSpecifyVersion.SelectedIndex = versionOptionsKeys.Length - 1;
            UpdateComboSpecifyVersion();
        }

        private void UpdateComboSpecifyVersion()
        {
            ParsingVersion = versionOptionsValues[comboSpecifyVersion.SelectedIndex];
            UAGConfig.Data.PreferredVersion = versionOptionsKeys[comboSpecifyVersion.SelectedIndex];
            UAGConfig.Save();
        }

        private void UpdateComboSpecifyMappings(bool alsoCheckVersion = true)
        {
            if (!UAGConfig.TryGetMappings(allMappingsKeys[comboSpecifyMappings.SelectedIndex], out ParsingMappings)) comboSpecifyMappings.SelectedIndex = 0;
            if (tableEditor?.asset != null) tableEditor.asset.Mappings = ParsingMappings;
            UAGConfig.Data.PreferredMappings = allMappingsKeys[comboSpecifyMappings.SelectedIndex];
            UAGConfig.Save();

            if (alsoCheckVersion) UpdateVersionFromMappings();
        }

        private void UpdateVersionFromMappings()
        {
            comboSpecifyVersion.Enabled = !_UpdateVersionFromMappings();
        }

        private bool _UpdateVersionFromMappings()
        {
            if (comboSpecifyVersion.Items.Count == 0) return false;

            // update version information if we have it
            if (ParsingMappings != null)
            {
                var detVer = UAsset.GetEngineVersion(ParsingMappings.FileVersionUE4, ParsingMappings.FileVersionUE5, ParsingMappings.CustomVersionContainer);
                if (detVer != EngineVersion.UNKNOWN)
                {
                    SetParsingVersion(detVer);
                    return true;
                }
            }
            return false;
        }

        private void comboSpecifyVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboSpecifyVersion();
        }

        private void comboSpecifyMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                UpdateComboSpecifyMappings();
            });
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                TreeNode newNode = null;
                if (e.KeyCode.HasFlag(Keys.Up)) // SHIFT + UP = navigate to previous node @ same level
                {
                    newNode = treeView1.SelectedNode.PrevNode;
                    e.Handled = true;
                }
                else if (e.KeyCode.HasFlag(Keys.Down)) // SHIFT + DOWN = navigate to next node @ same level
                {
                    newNode = treeView1.SelectedNode.NextNode;
                    e.Handled = true;
                }

                if (newNode != null)
                {
                    treeView1.SelectedNode = newNode;
                    treeView1.SelectedNode.EnsureVisible();
                }
            }
        }

        private ContextMenuStrip _currentDataGridViewStrip;
        public ContextMenuStrip CurrentDataGridViewStrip
        {
            get
            {
                return _currentDataGridViewStrip;
            }
            set
            {
                _currentDataGridViewStrip = value;
                //UAGUtils.InvokeUI(UpdateDataGridViewWithExpectedStrip);
            }
        }

        public void ResetCurrentDataGridViewStrip()
        {
            _currentDataGridViewStrip = null;
            //UAGUtils.InvokeUI(UpdateDataGridViewWithExpectedStrip);
        }

        private void UpdateDataGridViewWithExpectedStrip()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows) UAGUtils.UpdateContextMenuStripOfRow(row, CurrentDataGridViewStrip);
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_currentDataGridViewStrip == null)
            {
                e.Control.ContextMenuStrip = null;
                return;
            }
            e.Control.ContextMenuStrip = UAGUtils.MergeContextMenus(e.Control.ContextMenuStrip, _currentDataGridViewStrip);
        }

        private void replaceAllReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewTextBoxEditingControl dadControl = dataGridView1.EditingControl as DataGridViewTextBoxEditingControl;
            if (dadControl == null) return;

            int changingRow = dadControl.EditingControlRowIndex;
            FString replacingName = tableEditor?.asset?.GetNameReference(changingRow);
            if (replacingName == null) return;

            TextPrompt replacementPrompt = new TextPrompt()
            {
                DisplayText = "Enter a string to replace references of this name with"
            };

            replacementPrompt.StartPosition = FormStartPosition.CenterParent;

            if (replacementPrompt.ShowDialog(this) == DialogResult.OK)
            {
                FString newTxt = FString.FromString(replacementPrompt.OutputText);
                int numReplaced = tableEditor.ReplaceAllReferencesInNameMap(replacingName, newTxt);
                dataGridView1.Rows[changingRow].Cells[0].Value = newTxt.Value;
                dataGridView1.Rows[changingRow].Cells[1].Value = newTxt.Encoding.HeaderName;
                dataGridView1.RefreshEdit();
                MessageBox.Show("Successfully replaced " + numReplaced + " reference" + (numReplaced == 1 ? "" : "s") + ".", this.Text);
            }
            replacementPrompt.Dispose();
        }

        private void mapStructTypeOverridesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mstoForm = new MapStructTypeOverrideForm();
            mstoForm.StartPosition = FormStartPosition.CenterParent;
            mstoForm.ShowDialog();
            mstoForm.Dispose();
        }

        private void comboSpecifyVersion_DrawItem(object sender, DrawItemEventArgs e)
        {
            var combo = sender as ComboBox;
            if (e.Index < 0 || e.Index >= combo.Items.Count) return;

            Color fontColor = UAGPalette.ForeColor;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(UAGPalette.HighlightBackColor), e.Bounds);
                fontColor = UAGPalette.BackColor;
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(UAGPalette.ButtonBackColor), e.Bounds);
            }

            e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, new SolidBrush(fontColor), new Point(e.Bounds.X, e.Bounds.Y));
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ajustesForm = new SettingsForm();
            ajustesForm.StartPosition = FormStartPosition.CenterParent;
            ajustesForm.ShowDialog(this);
            ajustesForm.Dispose();
        }

        private void importBinaryData_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Binary data (*.dat)|*.dat|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode && (pointerNode.Type == PointingTreeNodeType.ByteArray || pointerNode.Type == PointingTreeNodeType.KismetByteArray))
                    {
                        byte[] rawData = File.ReadAllBytes(openFileDialog.FileName);
                        if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            ((StructExport)pointerNode.Pointer).ScriptBytecodeRaw = rawData;
                        }
                        else if (pointerNode.Pointer is NormalExport usCategory)
                        {
                            usCategory.Extras = rawData;
                        }
                        else if (pointerNode.Pointer is RawExport usRawCategory)
                        {
                            usRawCategory.Data = rawData;
                        }

                        SetUnsavedChanges(true);
                        if (tableEditor != null)
                        {
                            tableEditor.Save(true);
                        }
                    }
                }
            }
        }

        private void exportBinaryData_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Binary data (*.dat)|*.dat|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode && (pointerNode.Type == PointingTreeNodeType.ByteArray || pointerNode.Type == PointingTreeNodeType.KismetByteArray))
                    {
                        byte[] rawData = new byte[0];
                        if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            rawData = ((StructExport)pointerNode.Pointer).ScriptBytecodeRaw;
                        }
                        else if (pointerNode.Pointer is NormalExport usCategory)
                        {
                            rawData = usCategory.Extras;
                        }
                        else if (pointerNode.Pointer is RawExport usRawCategory)
                        {
                            rawData = usRawCategory.Data;
                        }

                        File.WriteAllBytes(dialog.FileName, rawData);
                    }
                }
            }
        }

        private void setBinaryData_Click(object sender, EventArgs e)
        {
            TextPrompt replacementPrompt = new TextPrompt()
            {
                DisplayText = "How many null bytes?"
            };

            replacementPrompt.StartPosition = FormStartPosition.CenterParent;

            if (replacementPrompt.ShowDialog(this) == DialogResult.OK)
            {
                if (int.TryParse(replacementPrompt.OutputText, out int numBytes) && treeView1.SelectedNode is PointingTreeNode pointerNode && (pointerNode.Type == PointingTreeNodeType.ByteArray || pointerNode.Type == PointingTreeNodeType.KismetByteArray))
                {
                    if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                    {
                        ((StructExport)pointerNode.Pointer).ScriptBytecodeRaw = new byte[numBytes];
                    }
                    else if (pointerNode.Pointer is NormalExport usCategory)
                    {
                        usCategory.Extras = new byte[numBytes];
                    }
                    else if (pointerNode.Pointer is RawExport usRawCategory)
                    {
                        usRawCategory.Data = new byte[numBytes];
                    }

                    SetUnsavedChanges(true);
                    if (tableEditor != null)
                    {
                        tableEditor.Save(true);
                    }
                }
            }

            replacementPrompt.Dispose();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findForm = new FindForm();
            findForm.StartPosition = FormStartPosition.CenterParent;
            findForm.Owner = this;
            findForm.Show();
        }

        private int lastRowNum = -1;
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (UAGConfig.Data.DoubleClickToEdit) dataGridView1.BeginEdit(true);
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!UAGConfig.Data.DoubleClickToEdit || (dataGridView1?.SelectedCells?.Count > 0 && dataGridView1.SelectedCells[0].RowIndex == lastRowNum)) dataGridView1.BeginEdit(true);
            lastRowNum = -1;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            lastRowNum = e.RowIndex;
        }

        private readonly HashSet<string> invalidBaseFolders = new HashSet<string>() { "AIModule", "ALAudio", "AVEncoder", "AVIWriter", "Advertising", "Analytics", "Android", "AnimGraphRuntime", "AnimationCore", "AppFramework", "Apple", "ApplicationCore", "AssetRegistry", "AudioAnalyzer", "AudioCaptureCore", "AudioCaptureImplementations", "AudioExtensions", "AudioMixer", "AudioMixerCore", "AudioPlatformConfiguration", "AugmentedReality", "AutomationMessages", "AutomationWorker", "BlueprintRuntime", "BuildSettings", "CEF3Utils", "CUDA/Source", "Cbor", "CinematicCamera", "ClientPilot", "ClothingSystemRuntimeCommon", "ClothingSystemRuntimeInterface", "ClothingSystemRuntimeNv", "CookedIterativeFile", "Core", "CoreUObject", "CrashReportCore", "CrunchCompression", "D3D12RHI", "Datasmith", "DeveloperSettings", "EmptyRHI", "Engine", "EngineMessages", "EngineSettings", "Experimental", "ExternalRPCRegistry", "EyeTracker", "Foliage", "FriendsAndChat", "GameMenuBuilder", "GameplayMediaEncoder", "GameplayTags", "GameplayTasks", "HardwareSurvey", "HeadMountedDisplay", "IESFile", "IOS", "IPC", "ImageCore", "ImageWrapper", "ImageWriteQueue", "InputCore", "InputDevice", "InstallBundleManager", "Json", "JsonUtilities", "Landscape", "Launch", "LevelSequence", "Linux/AudioMixerSDL", "LiveLinkInterface", "LiveLinkMessageBusFramework", "Lumin/LuminRuntimeSettings", "MRMesh", "Mac", "MaterialShaderQualitySettings", "Media", "MediaAssets", "MediaInfo", "MediaUtils", "MeshDescription", "MeshUtilitiesCommon", "Messaging", "MessagingCommon", "MessagingRpc", "MoviePlayer", "MovieScene", "MovieSceneCapture", "MovieSceneTracks", "NVidia/GeForceNOW", "NavigationSystem", "Navmesh", "Net", "NetworkFile", "NetworkFileSystem", "NetworkReplayStreaming", "Networking", "NonRealtimeAudioRenderer", "NullDrv", "NullInstallBundleManager", "Online", "OpenGLDrv", "Overlay", "PacketHandlers", "PakFile", "PerfCounters", "PhysXCooking", "PhysicsCore", "PlatformThirdPartyHelpers/PosixShim", "Portal", "PreLoadScreen", "Projects", "PropertyAccess", "PropertyPath", "RHI", "RSA", "RawMesh", "RenderCore", "Renderer", "RigVM", "RuntimeAssetCache", "SandboxFile", "Serialization", "SessionMessages", "SessionServices", "SignalProcessing", "Slate", "SlateCore", "SlateNullRenderer", "SlateRHIRenderer", "Sockets", "SoundFieldRendering", "StaticMeshDescription", "StreamingFile", "StreamingPauseRendering", "SynthBenchmark", "TimeManagement", "Toolbox", "TraceLog", "UE4Game", "UELibrary", "UMG", "Unix/UnixCommonStartup", "UnrealAudio", "VectorVM", "VirtualProduction/StageDataCore", "VulkanRHI", "WebBrowser", "WebBrowserTexture", "WidgetCarousel", "Windows", "XmlParser" };
        private readonly HashSet<string> invalidExtraFolders = new HashSet<string>() { "AkAudio", "ClothingSystemRuntime", "SQEXSEAD" };
        private readonly string PROJECT_NAME_PREFIX = "/Script/";
        private readonly string CONTENT_NAME_PREFIX = "/Game/";
        private string GetProjectName()
        {
            if (tableEditor?.asset == null) return null;
            if (UAGConfig.Data.PreferredMappings != "No mappings" && UAGConfig.Data.PreferredMappings != "Mappings") return UAGConfig.Data.PreferredMappings;

            // use PackageName if available
            string pName = tableEditor.asset.FolderName?.Value;
            if (pName != null && pName.StartsWith(CONTENT_NAME_PREFIX))
            {
                string[] pName_inside = pName.Substring(CONTENT_NAME_PREFIX.Length, pName.Length - CONTENT_NAME_PREFIX.Length).Split('/');
                if (pName_inside.Length > 0) return pName_inside[0];
            }

            // check for C++ module name (not great)
            List<string> validPossibleProjectNames = new List<string>();
            var allPossibleFNames = tableEditor.asset.GetNameMapIndexList();
            foreach (FString n in allPossibleFNames)
            {
                string pkg = n.ToString();
                if (!pkg.StartsWith(PROJECT_NAME_PREFIX)) continue;
                string pkg_inside = pkg.Substring(PROJECT_NAME_PREFIX.Length, pkg.Length - PROJECT_NAME_PREFIX.Length);
                if (invalidBaseFolders.Contains(pkg_inside)) continue;
                if (invalidExtraFolders.Contains(pkg_inside)) continue;
                validPossibleProjectNames.Add(pkg_inside);
            }

            if (validPossibleProjectNames.Count != 1) return "Unknown";
            return validPossibleProjectNames[0];
        }

        private RichPresence rp;
        public void UpdateRPC()
        {
            if (DiscordRPC == null || !DiscordRPC.IsInitialized || DiscordRPC.IsDisposed) return;
            if (!UAGConfig.Data.EnableDiscordRPC) return;

            UAGUtils.InvokeUI(() =>
            {
                if (dataGridView1 == null) return;

                string currPath = currentSavingPath;
                DateTime lastOpenedTime = LastLoadTimestamp;

                bool isEditingAsset = saveToolStripMenuItem?.Enabled ?? false;
                if (string.IsNullOrEmpty(currPath)) isEditingAsset = false;

                if (rp == null)
                {
                    rp = new RichPresence
                    {
                        Timestamps = new Timestamps(),
                        Assets = new Assets()
                        {
                            LargeImageKey = "main_logo"
                        }
                    };
                }

                string projName = GetProjectName();
                rp.Details = projName == null ? string.Empty : ("Project: " + projName + " (" + UAGConfig.Data.PreferredVersion + ")");
                rp.State = isEditingAsset ? ("File: " + Path.GetFileName(currPath)) : "Idling";
                rp.Timestamps.Start = lastOpenedTime;

                DiscordRPC.SetPresence(rp);
            });
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (tableEditor?.asset == null) return;

            if (e.Node is PointingTreeNode ptn)
            {
                if (!ptn.ChildrenInitialized)
                {
                    tableEditor.FillOutSubnodes(ptn, false);
                }
            }
        }

        private void configDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.OpenDirectory(UAGConfig.ConfigFolder);
        }

        /*private void mappingsDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.OpenDirectory(UAGConfig.MappingsFolder);
        }*/

        private string DumpMappings(string searchName, bool recursive, Dictionary<string, string> customAnnotations = null)
        {
            string annotatedOutput = ParsingMappings.GetAllPropertiesAnnotated(searchName, tableEditor?.asset, customAnnotations, recursive);
            return UAGConfig.SaveCustomFile(searchName + ".txt", annotatedOutput, Path.Combine("ClassDumps", UAGConfig.Data.PreferredMappings));
        }

        private bool TryDumpParticular()
        {
            var selectedNode = treeView1.SelectedNode as PointingTreeNode;
            if (selectedNode == null) return false;

            FName searchName = null;
            Dictionary<string, string> customAnnotations = new Dictionary<string, string>();
            if (selectedNode?.Pointer != null && selectedNode.Pointer is Export exp)
            {
                searchName = exp.GetClassTypeForAncestry(null, out _);
                if (selectedNode.Pointer is NormalExport nExp)
                {
                    foreach (var entry in nExp.Data)
                    {
                        customAnnotations[entry.Name?.Value?.Value ?? FString.NullCase] = "!";
                    }
                }
                else if (selectedNode.Pointer is DataTableExport datExp && datExp.Table?.Data != null)
                {
                    foreach (var entry in datExp.Table.Data)
                    {
                        customAnnotations[entry.Name?.Value?.Value ?? FString.NullCase] = "!";
                    }
                }
            }
            else if (selectedNode?.Pointer != null && selectedNode.Pointer is StructPropertyData strucDat)
            {
                searchName = strucDat.StructType;
            }

            if (searchName?.Value?.Value == null) return false;

            UAGUtils.OpenURL(DumpMappings(searchName.Value.Value, true, customAnnotations));

            return true;
        }

        // if mappings are provided, give list of valid props
        private void listValidPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                if (ParsingMappings == null)
                {
                    MessageBox.Show("No mappings found. " + UsmapInstructionsNotice, Text);
                    return;
                }

                if (TryDumpParticular()) return;

                DialogResult res = MessageBox.Show("This operation will dump ALL classes in the mappings file to text. This may take a while. Proceed?", DisplayVersion, MessageBoxButtons.OKCancel);
                switch (res)
                {
                    case DialogResult.OK:
                        var timer = new Stopwatch();
                        string outputPath = null;
                        int numDumped = 0;
                        timer.Start();
                        foreach (var schema in ParsingMappings.Schemas)
                        {
                            outputPath = DumpMappings(schema.Key, false);
                            numDumped++;
                        }
                        timer.Stop();
                        if (outputPath != null) UAGUtils.OpenDirectory(Path.GetDirectoryName(outputPath));
                        MessageBox.Show(numDumped + " " + (numDumped == 1 ? "class" : "classes") + " successfully dumped in " + timer.Elapsed.TotalMilliseconds + " ms.", Name);
                        break;
                }
            });
        }

        /*private int ExtractIOStore(string inPath, string outPath)
        {
            var test = new IOStoreContainer(inPath);
            test.BeginRead();
            int numExtracted = test.Extract(outPath);
            test.EndRead();
            return numExtracted;
        }

        private void extractIOStoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string inPath = null;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "IO Store Container (*.utoc)|*.utoc|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    inPath = openFileDialog.FileName;
                }
            }

            if (inPath == null) return;

            string outPath = null;
            FolderBrowserEx.FolderBrowserDialog outputFolderDialog = new FolderBrowserEx.FolderBrowserDialog();
            outputFolderDialog.Title = "Select a folder to extract to";

            if (outputFolderDialog.ShowDialog() == DialogResult.OK)
            {
                outPath = outputFolderDialog.SelectedFolder;
            }
            outputFolderDialog.Dispose();

            if (outPath == null) return;

            Stopwatch timer = new Stopwatch();
            timer.Start();
            int numExtracted = ExtractIOStore(inPath, outPath);
            timer.Stop();

            MessageBox.Show("Extracted " + numExtracted + " files in " + timer.ElapsedMilliseconds + " ms.", this.Text);
        }*/

        private string SelectMappings()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Unreal mappings file (*.usmap)|*.usmap|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return null;
        }

        private void patchusmapWithsavVersionInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                string patchPath = SelectMappings();
                if (patchPath == null) return;

                string inPath = null;
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Unreal save game (*.sav, *.savegame)|*.sav;*.savegame|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        inPath = openFileDialog.FileName;
                    }
                }

                if (inPath == null) return;

                bool success = true;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                try
                {
                    var thing = new SaveGame(inPath);
                    thing.PatchUsmap(patchPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to patch mappings! " + ex.GetType() + ": " + ex.Message, "Uh oh!");
                    success = false;
                }
                finally
                {
                    timer.Stop();
                    UpdateMappings();
                    if (success) MessageBox.Show("Operation completed in " + timer.ElapsedMilliseconds + " ms.", this.Text);
                }
            });
        }

        private void ImportMappingsFromPathInteractive(string importPath)
        {
            UAGUtils.InvokeUI(() =>
            {
                string newFileName = Path.GetFileNameWithoutExtension(importPath);

                // special case if just "Mappings.usmap"
                if (newFileName == "Mappings")
                {
                    TextPrompt replacementPrompt = new TextPrompt()
                    {
                        DisplayText = "What is the name of the game these mappings are for?"
                    };

                    replacementPrompt.StartPosition = FormStartPosition.CenterParent;

                    if (replacementPrompt.ShowDialog(this) == DialogResult.OK)
                    {
                        newFileName = string.Join("_", replacementPrompt.OutputText.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                    }

                    replacementPrompt.Dispose();
                }

                try
                {
                    File.Copy(importPath, Path.ChangeExtension(Path.Combine(UAGConfig.MappingsFolder, newFileName), ".usmap"), true);
                    if (UAGConfig.AllMappings.ContainsKey(newFileName)) UAGConfig.AllMappings.Remove(newFileName);
                    UpdateMappings(newFileName);
                }
                catch
                {
                    MessageBox.Show("Failed to import mappings!", "Uh oh!");
                }
            });
        }

        private void importMappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                string importPath = SelectMappings();
                if (importPath == null) return;
                ImportMappingsFromPathInteractive(importPath);
            });
        }

        private void openContainersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileContainerForm();
        }

        private void stageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor?.asset == null) return;

            ForceSave(currentSavingPath); // even if read only, let's just save over it anyways here; only purpose of the read only stuff is so the user doesn't get confused
            // UAGConfig.StageFile(files[0], CurrentContainerPath);
            // this.RefreshTreeView(this.saveTreeView);

            if (UAGConfig.DifferentStagingPerPak)
            {
                // pick first open form we can find to do it with, otherwise we can't do it
                foreach (var form in Application.OpenForms)
                {
                    if (form is FileContainerForm fcForm)
                    {
                        UAGConfig.StageFile(currentSavingPath, fcForm.CurrentContainerPath);
                        fcForm.RefreshTreeView(fcForm.saveTreeView);
                        return;
                    }
                }

                MessageBox.Show("Please open a .pak file first to stage assets.", "Notice");
            }
            else
            {
                // check if any file container form exists, if not, open one
                bool needToOpenFileContainerForm = true;
                foreach (var form in Application.OpenForms)
                {
                    if (form is FileContainerForm fcForm)
                    {
                        needToOpenFileContainerForm = false;
                        break;
                    }
                }

                if (needToOpenFileContainerForm) OpenFileContainerForm();

                // stage it and refresh all open file container forms
                UAGConfig.StageFile(currentSavingPath, null);
                foreach (var form in Application.OpenForms)
                {
                    if (form is FileContainerForm fcForm)
                    {
                        fcForm.RefreshTreeView(fcForm.saveTreeView);
                        fcForm.Activate();
                    }
                }
            }
        }
    }
}
