using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace UAssetGUI
{
    public partial class Form1 : Form
    {
        internal UE4Version ParsingVersion = UE4Version.UNKNOWN;
        internal DataGridView dataGridView1;
        internal ColorfulTreeView listView1;
        internal MenuStrip menuStrip1;

        public TableHandler tableEditor;
        public ByteViewer byteView1;
        public TextBox jsonView;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            UAGUtils._displayVersion = fvi.FileVersion;

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

            if (!string.IsNullOrEmpty(gitVersionGUI))
            {
                UAGUtils._displayVersion += " (" + gitVersionGUI;
                if (!string.IsNullOrEmpty(gitVersionAPI))
                {
                    UAGUtils._displayVersion += " - " + gitVersionAPI;
                }
                UAGUtils._displayVersion += ")";
            }

            UAGUtils.InitializeInvoke(this);

            this.Text = DisplayVersion;
            this.AllowDrop = true;
            dataGridView1.Visible = true;

            // Extra data viewer
            byteView1 = new ByteViewer
            {
                AutoScroll = true,
                AutoSize = true,
                Visible = false
            };
            Controls.Add(byteView1);

            jsonView = new TextBox
            {
                Visible = false,
                AutoSize = true,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both
            };
            Controls.Add(jsonView);

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

            // Drag-and-drop support
            DragEnter += new DragEventHandler(frm_DragEnter);
            DragDrop += new DragEventHandler(frm_DragDrop);

            dataGridView1.MouseWheel += dataGridView1_MouseWheel;

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
        }

        private static Dictionary<ToolStripItem, bool> isDropDownOpened = new Dictionary<ToolStripItem, bool>();
        public static bool IsDropDownOpened(ToolStripItem item)
        {
            if (!isDropDownOpened.ContainsKey(item)) return false;
            return isDropDownOpened[item];
        }

        private string[] versionOptionsKeys = new string[]
        {
            "Unknown version",
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
            "4.27"
        };

        private UE4Version[] versionOptionsValues = new UE4Version[]
        {
            UE4Version.UNKNOWN,
            UE4Version.VER_UE4_0,
            UE4Version.VER_UE4_1,
            UE4Version.VER_UE4_2,
            UE4Version.VER_UE4_3,
            UE4Version.VER_UE4_4,
            UE4Version.VER_UE4_5,
            UE4Version.VER_UE4_6,
            UE4Version.VER_UE4_7,
            UE4Version.VER_UE4_8,
            UE4Version.VER_UE4_9,
            UE4Version.VER_UE4_10,
            UE4Version.VER_UE4_11,
            UE4Version.VER_UE4_12,
            UE4Version.VER_UE4_13,
            UE4Version.VER_UE4_14,
            UE4Version.VER_UE4_15,
            UE4Version.VER_UE4_16,
            UE4Version.VER_UE4_17,
            UE4Version.VER_UE4_18,
            UE4Version.VER_UE4_19,
            UE4Version.VER_UE4_20,
            UE4Version.VER_UE4_21,
            UE4Version.VER_UE4_22,
            UE4Version.VER_UE4_23,
            UE4Version.VER_UE4_24,
            UE4Version.VER_UE4_25,
            UE4Version.VER_UE4_26,
            UE4Version.VER_UE4_27,
        };

        public static readonly string GitHubRepo = "atenfyr/UAssetGUI";
        private Version latestOnlineVersion = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            UAGPalette.InitializeTheme();
            UAGPalette.RefreshTheme(this);

            string initialSelection = versionOptionsKeys[0];
            try
            {
                initialSelection = Properties.Settings.Default.PreferredVersion;
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

            // Fetch the latest version from github
            Task.Run(() =>
            {
                latestOnlineVersion = GitHubAPI.GetLatestVersionFromGitHub(GitHubRepo);
            }).ContinueWith(res =>
            {
                if (latestOnlineVersion != null && latestOnlineVersion.IsUAGVersionLower())
                {
                    MessageBox.Show("A new version of UAssetGUI (v" + latestOnlineVersion + ") is available to download!");
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            // Command line parameter support
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LoadFileAt(args[1]);
            }
        }

        private List<string> unknownTypes = new List<string>();
        private bool RecordUnknownProperty(PropertyData dat)
        {
            if (dat == null) return false;

            if (dat is UnknownPropertyData unknownDat)
            {
                string serializingType = unknownDat?.SerializingPropertyType?.Value;
                if (!string.IsNullOrEmpty(serializingType) && !unknownTypes.Contains(serializingType))
                {
                    unknownTypes.Add(serializingType);
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

        public void LoadFileAt(string filePath)
        {
            dataGridView1.Visible = true;
            byteView1.Visible = false;
            jsonView.Visible = false;

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
                        desiredSetUnsavedChanges = true;
                        break;
                    default:
                        MapStructTypeOverrideForm.LoadFromConfig();

                        targetAsset = new UAsset(ParsingVersion);
                        targetAsset.FilePath = filePath;
                        if (MapStructTypeOverrideForm.MapStructTypeOverride != null) targetAsset.MapStructTypeOverride = MapStructTypeOverrideForm.MapStructTypeOverride;
                        targetAsset.Read(targetAsset.PathToReader(targetAsset.FilePath));
                        break;
                }

                currentSavingPath = savingPath;
                SetUnsavedChanges(false);

                tableEditor = new TableHandler(dataGridView1, targetAsset, listView1);

                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;

                tableEditor.FillOutTree();
                tableEditor.Load();

                UAGPalette.RefreshTheme(this);

                bool hasDuplicates = false;
                Dictionary<string, bool> nameMapRefs = new Dictionary<string, bool>();
                foreach (FString x in tableEditor.asset.GetNameMapIndexList())
                {
                    if (nameMapRefs.ContainsKey(x.Value))
                    {
                        hasDuplicates = true;
                        break;
                    }
                    nameMapRefs.Add(x.Value, true);
                }
                nameMapRefs = null;

                int failedCategoryCount = 0;
                unknownTypes = new List<string>();
                foreach (Export cat in tableEditor.asset.Exports)
                {
                    if (cat is RawExport) failedCategoryCount++;
                    if (cat is NormalExport usNormal)
                    {
                        foreach (PropertyData dat in usNormal.Data) GetUnknownProperties(dat);
                    }
                }

                bool failedToMaintainBinaryEquality = !string.IsNullOrEmpty(tableEditor.asset.FilePath) && !tableEditor.asset.VerifyBinaryEquality();

                if (failedCategoryCount > 0)
                {
                    MessageBox.Show("Failed to parse " + failedCategoryCount + " exports!", "Notice");
                }

                if (hasDuplicates)
                {
                    MessageBox.Show("Encountered duplicate name map entries! Serialized FNames will coalesce to one of the entries in the map and binary equality may not be maintained.", "Notice");
                }

                if (unknownTypes.Count > 0)
                {
                    MessageBox.Show("Encountered " + unknownTypes.Count + " unknown property types:\n" + string.Join(", ", unknownTypes) + (failedToMaintainBinaryEquality ? "" : "\n\nThis is not a cause for alarm. The asset will still parse normally."), "Notice");
                }

                if (failedToMaintainBinaryEquality)
                {
                    MessageBox.Show("Failed to maintain binary equality! UAssetAPI may not be able to parse this particular asset correctly, and you may not be able to load this file in-game if modified.", "Uh oh!");
                }

                SetParsingVersion(tableEditor.asset.EngineVersion);
                if (desiredSetUnsavedChanges) SetUnsavedChanges(desiredSetUnsavedChanges);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.StackTrace);
                currentSavingPath = "";
                SetUnsavedChanges(false);
                tableEditor = null;
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;

                listView1.Nodes.Clear();
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
            }
        }

        public bool existsUnsavedChanges = false;
        public void SetUnsavedChanges(bool flag)
        {
            existsUnsavedChanges = flag;
            if (string.IsNullOrEmpty(currentSavingPath))
            {
                dataGridView1.Parent.Text = DisplayVersion;
            }
            else
            {
                if (existsUnsavedChanges)
                {
                    dataGridView1.Parent.Text = DisplayVersion + " - *" + currentSavingPath;
                }
                else
                {
                    dataGridView1.Parent.Text = DisplayVersion + " - " + currentSavingPath;
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

        private string currentSavingPath = "";

        private bool ForceSave(string path)
        {
            if (tableEditor != null && !string.IsNullOrEmpty(currentSavingPath))
            {
                if (File.Exists(path)) File.Copy(path, path + ".bak", true);
                if (File.Exists(Path.ChangeExtension(path, "uexp"))) File.Copy(Path.ChangeExtension(path, "uexp"), Path.ChangeExtension(path, "uexp") + ".bak", true);

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

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^C");
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^V");
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

                        TreeNode topSelectingNode = listView1.Nodes[listView1.Nodes.Count - 1];
                        if (topSelectingNode.Nodes.Count > (jumpingTo - 1))
                        {
                            topSelectingNode = topSelectingNode.Nodes[jumpingTo - 1];
                            if (topSelectingNode.Nodes.Count > 0)
                            {
                                topSelectingNode = topSelectingNode.Nodes[0];
                            }
                        }
                        listView1.SelectedNode = topSelectingNode;
                        break;
                    case "ChildJump":
                        int jumpingIndex = dataGridView1.CurrentCell.RowIndex;
                        if (jumpingIndex < 0 || jumpingIndex >= listView1.SelectedNode.Nodes.Count)
                        {
                            MessageBox.Show("Please select View -> Recalculate Nodes before attempting to jump to this node.", "Notice");
                        }
                        else
                        {
                            listView1.SelectedNode = listView1.SelectedNode.Nodes[jumpingIndex];
                        }
                        break;
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            string selectedNodeText = e.Node.Text;
            string parentSelectedNodeText = e.Node.Parent?.Text;
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
                    case "Custom Version Container":
                        tableEditor.mode = TableHandlerMode.CustomVersionContainer;
                        break;
                }

                if (parentSelectedNodeText == "World Tile Info") tableEditor.mode = TableHandlerMode.WorldTileInfo;

                tableEditor.Load();
            }
        }

        private void dataGridView1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count < 1) return;
            if (!Properties.Settings.Default.ChangeValuesOnScroll) return;
            var selectedCell = dataGridView1.SelectedCells[0];

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
            float widthAmount = 0.6f;
            dataGridView1.Size = new Size((int)(this.Size.Width * widthAmount), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            dataGridView1.Location = new Point(this.Size.Width - this.dataGridView1.Size.Width - this.menuStrip1.Size.Height, this.menuStrip1.Size.Height);
            if (byteView1 != null)
            {
                byteView1.Size = dataGridView1.Size;
                byteView1.Location = dataGridView1.Location;
                byteView1.Refresh();
            }

            if (jsonView != null)
            {
                jsonView.Size = dataGridView1.Size;
                jsonView.Location = dataGridView1.Location;
                jsonView.Refresh();
            }

            listView1.Size = new Size((int)(this.Size.Width * (1 - widthAmount)) - (this.menuStrip1.Size.Height * 2), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            listView1.Location = new Point(this.menuStrip1.Size.Height / 2, this.menuStrip1.Size.Height);
            comboSpecifyVersion.Location = new Point(this.dataGridView1.Location.X + this.dataGridView1.Size.Width - this.comboSpecifyVersion.Width, this.menuStrip1.Size.Height - this.comboSpecifyVersion.Size.Height - 2);
            importBinaryData.Location = new Point(dataGridView1.Location.X, comboSpecifyVersion.Location.Y);
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
            if (!existsUnsavedChanges) return;

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
                tableEditor.FillOutTree();
            }
        }

        private void issuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetgui/issues");
        }

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetgui");
        }

        private void apiLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetapi");
        }

        public void SetParsingVersion(UE4Version ver)
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
            Properties.Settings.Default.PreferredVersion = versionOptionsKeys[comboSpecifyVersion.SelectedIndex];
            Properties.Settings.Default.Save();
        }

        private void comboSpecifyVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboSpecifyVersion();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                TreeNode newNode = null;
                if (e.KeyCode.HasFlag(Keys.Up)) // SHIFT + UP = navigate to previous node @ same level
                {
                    newNode = listView1.SelectedNode.PrevNode;
                    e.Handled = true;
                }
                else if (e.KeyCode.HasFlag(Keys.Down)) // SHIFT + DOWN = navigate to next node @ same level
                {
                    newNode = listView1.SelectedNode.NextNode;
                    e.Handled = true;
                }

                if (newNode != null)
                {
                    listView1.SelectedNode = newNode;
                    listView1.SelectedNode.EnsureVisible();
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
                    if (listView1.SelectedNode is PointingTreeNode pointerNode && pointerNode.Pointer is NormalExport usCategory && pointerNode.Type == PointingTreeNodeType.ByteArray)
                    {
                        usCategory.Extras = File.ReadAllBytes(openFileDialog.FileName);
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
                    if (listView1.SelectedNode is PointingTreeNode pointerNode && pointerNode.Pointer is NormalExport usCategory && pointerNode.Type == PointingTreeNodeType.ByteArray)
                    {
                        File.WriteAllBytes(dialog.FileName, usCategory.Extras);
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
                if (int.TryParse(replacementPrompt.OutputText, out int numBytes) && listView1.SelectedNode is PointingTreeNode pointerNode && pointerNode.Pointer is NormalExport usCategory && pointerNode.Type == PointingTreeNodeType.ByteArray)
                {
                    usCategory.Extras = new byte[numBytes];
                    if (tableEditor != null)
                    {
                        tableEditor.Save(true);
                    }
                }
            }
            replacementPrompt.Dispose();

        }
    }
}
