﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.PropertyTypes;

namespace UAssetGUI
{
    public partial class Form1 : Form
    {
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

            Utils.InitializeInvoke(this);

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

        private void Form1_Load(object sender, EventArgs e)
        {
            UAGPalette.RefreshTheme(this);
        }

        public void LoadFileAt(string filePath)
        {
            dataGridView1.Visible = true;
            byteView1.Visible = false;

            try
            {
                currentSavingPath = filePath;
                SetUnsavedChanges(false);

                tableEditor = new TableHandler(dataGridView1, new AssetWriter(filePath, true, true, null, null), listView1)
                {
                    mode = TableHandlerMode.HeaderList
                };

                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;

                tableEditor.FillOutTree();
                tableEditor.Load();

                UAGPalette.RefreshTheme(this);

                int failedCategoryCount = 0;
                List<string> unknownTypes = new List<string>();
                foreach (Category cat in tableEditor.asset.data.categories)
                {
                    if (cat is RawCategory) failedCategoryCount++;
                    if (cat is NormalCategory usNormal)
                    {
                        foreach (PropertyData dat in usNormal.Data)
                        {
                            if (dat is UnknownPropertyData && !string.IsNullOrEmpty(dat.Type) && !unknownTypes.Contains(dat.Type)) unknownTypes.Add(dat.Type);
                        }
                    }
                }
                if (failedCategoryCount > 0)
                {
                    MessageBox.Show("Failed to parse " + failedCategoryCount + " categories!", "Uh oh!");
                }
                if (unknownTypes.Count > 0)
                {
                    MessageBox.Show("Encountered " + unknownTypes.Count + " unknown property types:\n" + string.Join(", ", unknownTypes), "Uh oh!");
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
                    catch (HeaderOutOfRangeException ex)
                    {
                        tableEditor.asset.data.AddHeaderReference(ex.RequiredString);
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
            tableEditor?.RestoreTreeState(true);
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tableEditor?.RestoreTreeState(false);
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
                tableEditor.mode = TableHandlerMode.CategoryData;
                switch (selectedNodeText)
                {
                    case "Header List":
                        tableEditor.mode = TableHandlerMode.HeaderList;
                        break;
                    case "Linked Sectors":
                        tableEditor.mode = TableHandlerMode.LinkedSectors;
                        break;
                    case "Category Information":
                        tableEditor.mode = TableHandlerMode.CategoryInformation;
                        break;
                    case "Category Ints":
                        tableEditor.mode = TableHandlerMode.CategoryInts;
                        break;
                    case "Category Strings":
                        tableEditor.mode = TableHandlerMode.CategoryStrings;
                        break;
                    case "UExp Ints":
                        tableEditor.mode = TableHandlerMode.UExpInts;
                        break;
                }
                tableEditor.Load();
            }
        }

        public void ForceResize()
        {
            float widthAmount = 0.6f;
            dataGridView1.Size = new Size((int)(this.Size.Width * widthAmount), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            dataGridView1.Location = new Point(this.Size.Width - dataGridView1.Size.Width - this.menuStrip1.Size.Height, this.menuStrip1.Size.Height);
            if (byteView1 != null)
            {
                byteView1.Size = dataGridView1.Size;
                byteView1.Location = dataGridView1.Location;
                byteView1.Refresh();
            }

            listView1.Size = new Size((int)(this.Size.Width * (1 - widthAmount)) - (this.menuStrip1.Size.Height * 2), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            listView1.Location = new Point(this.menuStrip1.Size.Height / 2, this.menuStrip1.Size.Height);
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
    }
}
