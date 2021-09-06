using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.PropertyTypes;

namespace UAssetGUI
{
    public partial class Form1 : Form
    {
        internal UE4Version ParsingVersion = UE4Version.UNKNOWN;
        internal DataGridView dataGridView1;
        internal TreeView listView1;
        internal MenuStrip menuStrip1;

        public static string GUIVersion;
        public TableHandler tableEditor;
        public ByteViewer byteView1;

        public Form1()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            GUIVersion = fvi.FileVersion;

            UAGUtils.InitializeInvoke(this);

            this.Text = "UAssetGUI v" + GUIVersion;
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

            // Command line parameter support
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LoadFileAt(args[1]);
            }
        }

        private string[] versionOptions = new string[]
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
            "4.26"
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            UAGPalette.RefreshTheme(this);
            comboSpecifyVersion.Items.AddRange(versionOptions);
            comboSpecifyVersion.SelectedIndex = 0;
        }

        public void LoadFileAt(string filePath)
        {
            dataGridView1.Visible = true;
            byteView1.Visible = false;

            try
            {
                currentSavingPath = filePath;
                SetUnsavedChanges(false);

                tableEditor = new TableHandler(dataGridView1, new UAsset(filePath, ParsingVersion, true, true, null, null), listView1)
                {
                    mode = TableHandlerMode.NameMap
                };

                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;

                tableEditor.FillOutTree();
                tableEditor.Load();

                UAGPalette.RefreshTheme(this);

                int failedCategoryCount = 0;
                List<string> unknownTypes = new List<string>();
                foreach (Export cat in tableEditor.asset.Exports)
                {
                    if (cat is RawExport) failedCategoryCount++;
                    if (cat is NormalExport usNormal)
                    {
                        foreach (PropertyData dat in usNormal.Data)
                        {
                            if (dat is UnknownPropertyData && !string.IsNullOrEmpty(dat.Type.Value.Value) && !unknownTypes.Contains(dat.Type.Value.Value)) unknownTypes.Add(dat.Type.Value.Value);
                        }
                    }
                }
                if (failedCategoryCount > 0)
                {
                    MessageBox.Show("Failed to parse " + failedCategoryCount + " categories!", "Notice");
                }
                if (unknownTypes.Count > 0)
                {
                    MessageBox.Show("Encountered " + unknownTypes.Count + " unknown property types:\n" + string.Join(", ", unknownTypes), "Notice");
                }

                if (!tableEditor.asset.VerifyParsing())
                {
                    MessageBox.Show("Failed to verify parsing! You may not be able to load this file in-game if modified.", "Uh oh!");
                }
            }
            catch (Exception ex)
            {
                currentSavingPath = "";
                SetUnsavedChanges(false);
                tableEditor = null;
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;

                listView1.Nodes.Clear();
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
                UAGPalette.RefreshTheme(this);
                switch(ex)
                {
                    case IOException _:
                        MessageBox.Show("Failed to open this file!", "Uh oh!");
                        break;
                    case FormatException formatEx:
                        MessageBox.Show("Failed to parse this file!\n" + formatEx.GetType() + ": " + formatEx.Message, "Uh oh!");
                        break;
                    case InvalidOperationException invalidOperationException:
                        if (invalidOperationException.Message == "Cannot begin serialization before an engine version is specified")
                        {
                            MessageBox.Show("Please specify an engine version before opening an unversioned asset.", "Uh oh!");
                        }
                        else
                        {
                            MessageBox.Show("An invalid operation was performed while trying to open this file!\n" + ex.GetType() + ": " + ex.Message, "Uh oh!");
                        }
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
                dataGridView1.Parent.Text = "UAssetGUI v" + GUIVersion;
            }
            else
            {
                if (existsUnsavedChanges)
                {
                    dataGridView1.Parent.Text = "UAssetGUI v" + GUIVersion + " - *" + currentSavingPath;
                }
                else
                {
                    dataGridView1.Parent.Text = "UAssetGUI v" + GUIVersion + " - " + currentSavingPath;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Unreal Assets (*.uasset, *.umap)|*.uasset;*.umap|All files (*.*)|*.*";
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
                        tableEditor.asset.AddNameReference(ex.RequiredName);
                        isLooping = true;
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
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Unreal Assets (*.uasset, *.umap)|*.uasset;*.umap|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    currentSavingPath = dialog.FileName;
                    ForceSave(currentSavingPath);
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
                switch(dataGridView1.CurrentCell.Tag)
                {
                    case "CategoryJump":
                        if (dataGridView1.CurrentCell.ColumnIndex == 3)
                        {
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
                        }
                        break;
                    case "ChildJump":
                        if (dataGridView1.CurrentCell.ColumnIndex == 3)
                        {
                            listView1.SelectedNode = listView1.SelectedNode.Nodes[dataGridView1.CurrentCell.RowIndex];
                        }
                        break;
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            string selectedNodeText = e.Node.Text;
            if (tableEditor != null)
            {
                tableEditor.mode = TableHandlerMode.ExportData;
                switch (selectedNodeText)
                {
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
                    case "Preload Dependency Map":
                        tableEditor.mode = TableHandlerMode.PreloadDependencyMap;
                        break;
                    case "Custom Version Container":
                        tableEditor.mode = TableHandlerMode.CustomVersionContainer;
                        break;
                }
                tableEditor.Load();
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

            listView1.Size = new Size((int)(this.Size.Width * (1 - widthAmount)) - (this.menuStrip1.Size.Height * 2), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            listView1.Location = new Point(this.menuStrip1.Size.Height / 2, this.menuStrip1.Size.Height);
            comboSpecifyVersion.Location = new Point(this.dataGridView1.Location.X + this.dataGridView1.Size.Width - this.comboSpecifyVersion.Width, this.menuStrip1.Size.Height - this.comboSpecifyVersion.Size.Height - 3);
        }

        private void frm_sizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void frm_closing(object sender, FormClosingEventArgs e)
        {
            if (!existsUnsavedChanges) return;

            DialogResult res = MessageBox.Show("Do you want to save your changes?", "UAssetGUI v" + GUIVersion, MessageBoxButtons.YesNoCancel);
            switch(res)
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

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetgui");
        }

        private void apiLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetapi");
        }

        private static string AboutText;

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutText = "UAssetGUI v" + GUIVersion + "\n" +
            "By Atenfyr\n" +
            "\nThanks to the Astro-Techies club for the help\n" +
            "\nThanks to David Hill (Kaiheilos) for in-depth information on Sections 1-5\n" +
            "\n(Here's where a soppy monologue goes)\n";

            var formPopup = new Form
            {
                Text = "About",
                Size = new Size(350, 300)
            };
            formPopup.StartPosition = FormStartPosition.CenterParent;

            formPopup.Controls.Add(new Label()
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Text = AboutText,
                Font = new Font(this.Font.FontFamily, 10)
            });

            formPopup.ShowDialog(this);
        }

        private void comboSpecifyVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(versionOptions[comboSpecifyVersion.SelectedIndex])
            {
                case "Unknown version":
                    ParsingVersion = UE4Version.UNKNOWN;
                    break;
                case "4.0":
                    ParsingVersion = UE4Version.VER_UE4_0;
                    break;
                case "4.1":
                    ParsingVersion = UE4Version.VER_UE4_1;
                    break;
                case "4.2":
                    ParsingVersion = UE4Version.VER_UE4_2;
                    break;
                case "4.3":
                    ParsingVersion = UE4Version.VER_UE4_3;
                    break;
                case "4.4":
                    ParsingVersion = UE4Version.VER_UE4_4;
                    break;
                case "4.5":
                    ParsingVersion = UE4Version.VER_UE4_5;
                    break;
                case "4.6":
                    ParsingVersion = UE4Version.VER_UE4_6;
                    break;
                case "4.7":
                    ParsingVersion = UE4Version.VER_UE4_7;
                    break;
                case "4.8":
                    ParsingVersion = UE4Version.VER_UE4_8;
                    break;
                case "4.9":
                    ParsingVersion = UE4Version.VER_UE4_9;
                    break;
                case "4.10":
                    ParsingVersion = UE4Version.VER_UE4_10;
                    break;
                case "4.11":
                    ParsingVersion = UE4Version.VER_UE4_11;
                    break;
                case "4.12":
                    ParsingVersion = UE4Version.VER_UE4_12;
                    break;
                case "4.13":
                    ParsingVersion = UE4Version.VER_UE4_13;
                    break;
                case "4.14":
                    ParsingVersion = UE4Version.VER_UE4_14;
                    break;
                case "4.15":
                    ParsingVersion = UE4Version.VER_UE4_15;
                    break;
                case "4.16":
                    ParsingVersion = UE4Version.VER_UE4_16;
                    break;
                case "4.17":
                    ParsingVersion = UE4Version.VER_UE4_17;
                    break;
                case "4.18":
                    ParsingVersion = UE4Version.VER_UE4_18;
                    break;
                case "4.19":
                    ParsingVersion = UE4Version.VER_UE4_19;
                    break;
                case "4.20":
                    ParsingVersion = UE4Version.VER_UE4_20;
                    break;
                case "4.21":
                    ParsingVersion = UE4Version.VER_UE4_21;
                    break;
                case "4.22":
                    ParsingVersion = UE4Version.VER_UE4_22;
                    break;
                case "4.23":
                    ParsingVersion = UE4Version.VER_UE4_23;
                    break;
                case "4.24":
                    ParsingVersion = UE4Version.VER_UE4_24;
                    break;
                case "4.25":
                    ParsingVersion = UE4Version.VER_UE4_25;
                    break;
                case "4.26":
                    ParsingVersion = UE4Version.VER_UE4_26;
                    break;
                default:
                    throw new NotImplementedException("Unimplemented version chosen in combo box");
            }
        }
    }
}
