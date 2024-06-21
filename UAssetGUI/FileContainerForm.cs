using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    public partial class FileContainerForm : Form
    {
        public IDictionary<TreeView, DirectoryTree> DirectoryTreeMap = new Dictionary<TreeView, DirectoryTree>();
        public string CurrentContainerPath;
        public PakVersion Version = PakVersion.V4;
        public string MountPoint = "../../../";

        public FileContainerForm()
        {
            InitializeComponent();
        }

        public Form1 BaseForm;
        public TreeView SelectedTreeView;
        private void FileContainerForm_Load(object sender, EventArgs e)
        {
            this.Text = BaseForm.DisplayVersion;

            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();

            this.copyToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.C);
            this.pasteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.V);
            this.deleteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Delete);

            this.loadTreeView.AllowDrop = true;
            this.loadTreeView.DragEnter += new DragEventHandler(event_DragEnter);
            this.loadTreeView.DragDrop += new DragEventHandler(loadTreeView_DragDrop);
            this.saveTreeView.AllowDrop = true;
            this.saveTreeView.DragEnter += new DragEventHandler(event_DragEnter);
            this.saveTreeView.DragDrop += new DragEventHandler(saveTreeView_DragDrop);

            LoadContainer(CurrentContainerPath);
            RefreshTreeView(saveTreeView);
        }

        public void AddDirectoryTreeItemToTreeView(DirectoryTreeItem treeItem, PointingFileTreeNode dad)
        {
            foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in treeItem.Children)
            {
                var newDad = new PointingFileTreeNode(directoryItem.Value.Name, directoryItem.Value);
                dad.Nodes.Add(newDad);
                AddDirectoryTreeItemToTreeView(directoryItem.Value, newDad);
            }
        }

        public void ExpandedToHashSet(TreeNodeCollection nodes, HashSet<string> hashSet)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is PointingFileTreeNode pftNode)
                {
                    if (pftNode.IsExpanded) hashSet.Add(pftNode.Pointer.FullPath);
                    ExpandedToHashSet(pftNode.Nodes, hashSet);
                }
            }
        }

        // this method doesn't collapse already-expanded nodes
        public void HashSetToExpanded(TreeNodeCollection nodes, HashSet<string> hashSet)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is PointingFileTreeNode pftNode)
                {
                    if (hashSet.Contains(pftNode.Pointer.FullPath)) pftNode.Expand();
                    HashSetToExpanded(pftNode.Nodes, hashSet);
                }
            }
        }

        public PointingFileTreeNode GetSpecificNode(TreeNodeCollection nodes, string fullPath)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is PointingFileTreeNode pftNode)
                {
                    if (fullPath == pftNode.Pointer.FullPath) return pftNode;

                    var gotten = GetSpecificNode(node.Nodes, fullPath);
                    if (gotten != null) return gotten;
                }
            }
            return null;
        }

        public PointingFileTreeNode GetSpecificNode(TreeView treeView, string fullPath)
        {
            return GetSpecificNode(treeView.Nodes, fullPath);
        }

        public void RefreshTreeView(TreeView treeView)
        {
            // get existing expanded nodes
            HashSet<string> FullPathsOfNodesToExpand = new HashSet<string>();
            ExpandedToHashSet(treeView.Nodes, FullPathsOfNodesToExpand);

            if (treeView == saveTreeView)
            {
                // update directory tree
                string[] stagingFiles = UAGConfig.GetStagingFiles(this.CurrentContainerPath, out string[] fixedPathsOnDisk);
                DirectoryTreeMap[treeView] = new DirectoryTree(this, stagingFiles, fixedPathsOnDisk);
            }

            treeView.Nodes.Clear();
            DirectoryTree currentTree = DirectoryTreeMap[treeView];
            foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in currentTree.RootNodes)
            {
                var dad = new PointingFileTreeNode(directoryItem.Value.Name, directoryItem.Value);
                treeView.Nodes.Add(dad);
                AddDirectoryTreeItemToTreeView(directoryItem.Value, dad);
            }

            treeView.Sort();

            HashSetToExpanded(treeView.Nodes, FullPathsOfNodesToExpand);

            if (treeView.Nodes.Count > 0)
            {
                treeView.BackColor = UAGPalette.BackColor;
            }
            else
            {
                treeView.BackColor = UAGPalette.InactiveColor;
            }
        }

        public void LoadContainer(string path)
        {
            if (path == null) return;
            CurrentContainerPath = path;

            string[] allFiles = Array.Empty<string>();
            using (FileStream stream = new FileStream(CurrentContainerPath, FileMode.Open))
            {
                allFiles = new PakBuilder().Reader(stream).Files();
            }

            DirectoryTreeMap[loadTreeView] = new DirectoryTree(this, allFiles);
            RefreshTreeView(loadTreeView);

            this.Text = BaseForm.DisplayVersion + " - " + CurrentContainerPath;
        }

        public bool SaveContainer(string path)
        {
            if (path == null) return false;
            string[] stagingFiles = UAGConfig.GetStagingFiles(this.CurrentContainerPath, out string[] fixedPathsOnDisk);
            if (stagingFiles == null || fixedPathsOnDisk == null || stagingFiles.Length != fixedPathsOnDisk.Length) return false;

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                var pakWriter = new PakBuilder().Writer(stream, Version, MountPoint);
                for (int i = 0; i < stagingFiles.Length; i++)
                {
                    string ext = Path.GetExtension(fixedPathsOnDisk[i]);
                    if (ext == ".bak") continue;
                    pakWriter.WriteFile(stagingFiles[i], File.ReadAllBytes(fixedPathsOnDisk[i]));
                }
                pakWriter.WriteIndex();
            }

            return true;
        }

        public void ForceResize()
        {
            splitContainer1.Size = new Size(splitContainer1.Size.Width, this.Size.Height - menuStrip1.Size.Height - 50);
            loadButton.Location = new Point((splitContainer1.Panel1.Size.Width - loadButton.Size.Width) / 2, loadButton.Location.Y);
            saveButton.Location = new Point((splitContainer1.Panel2.Size.Width - saveButton.Size.Width) / 2, saveButton.Location.Y);
            loadTreeView.Location = new Point(0, loadButton.Location.Y + loadButton.Size.Height + 6);
            loadTreeView.Size = new Size(splitContainer1.Panel1.Size.Width, splitContainer1.Panel1.Size.Height - loadTreeView.Location.Y);
            saveTreeView.Location = new Point(0, saveButton.Location.Y + saveButton.Size.Height + 6);
            saveTreeView.Size = new Size(splitContainer1.Panel2.Size.Width, splitContainer1.Panel2.Size.Height - saveTreeView.Location.Y);
        }

        private void FileContainerForm_SizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ForceResize();
        }

        private void FileContainerForm_Shown(object sender, EventArgs e)
        {
            ForceResize();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    copyToolStripMenuItem.PerformClick();
                    return true;
                case Keys.Control | Keys.V:
                    pasteToolStripMenuItem.PerformClick();
                    return true;
                case Keys.Delete:
                    deleteToolStripMenuItem.PerformClick();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectedTreeView.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectedTreeView.CollapseAll();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshTreeView(loadTreeView);
            RefreshTreeView(saveTreeView);
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            loadToolStripMenuItem.PerformClick();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem.PerformClick();
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e?.Node != null && e.Node is PointingFileTreeNode pftNode)
            {
                if (pftNode.Pointer.IsFile) pftNode.Pointer.OpenFile();
            }
        }

        private PointingFileTreeNode copiedPftNode = null;
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView.SelectedNode is PointingFileTreeNode pftNode)
            {
                copiedPftNode = pftNode;
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (copiedPftNode == null || pftNode == null) return;
                DirectoryTreeItem clipboardNode = copiedPftNode.Pointer;

                if (string.IsNullOrEmpty(pftNode.Pointer.FixedPathOnDisk)) return; // only allow pasting into staging
                DirectoryTreeItem targetDirectory = pftNode.Pointer;
                if (targetDirectory.IsFile) targetDirectory = targetDirectory.Parent;

                string desiredStagingPath = Path.Combine(targetDirectory?.FullPath ?? string.Empty, clipboardNode.Name);
                clipboardNode.StageFile(desiredStagingPath);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (pftNode == null) return;
                if (string.IsNullOrEmpty(pftNode.Pointer.FixedPathOnDisk)) return; // only allow deleting from staging

                pftNode.Pointer.DeleteFile();
            }
        }

        private void event_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void loadTreeView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) LoadContainer(files[0]);
        }

        private void saveTreeView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                UAGConfig.StageFile(files[0], CurrentContainerPath);
                this.RefreshTreeView(this.saveTreeView);
            }
        }

        private void loadTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            saveTreeView.SelectedNode = null;
            this.SelectedTreeView = loadTreeView;
        }

        private void saveTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            loadTreeView.SelectedNode = null;
            this.SelectedTreeView = saveTreeView;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = ".pak Container Files (*.pak)|*.pak|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadContainer(openFileDialog.FileName);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = ".pak Container Files (*.pak)|*.pak|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    bool success = SaveContainer(dialog.FileName);
                    if (!success) MessageBox.Show("Failed to save!", "Uh oh!");
                }
                else if (res != DialogResult.Cancel)
                {
                    MessageBox.Show("Failed to save!", "Uh oh!");
                }
            }
        }

        private void stageFromDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    UAGConfig.StageFile(openFileDialog.FileName, CurrentContainerPath);
                    this.RefreshTreeView(this.saveTreeView);
                }
            }
        }
    }

    public class PointingFileTreeNode : TreeNode
    {
        public DirectoryTreeItem Pointer;

        public PointingFileTreeNode(string text, DirectoryTreeItem item) : base(text)
        {
            Pointer = item;


            this.ContextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem tsmItem = null;

            if (Pointer.IsFile)
            {
                tsmItem = new ToolStripMenuItem("Open");
                tsmItem.Click += (sender, args) => Pointer.OpenFile();
                this.ContextMenuStrip.Items.Add(tsmItem);
            }

            if (Pointer.FixedPathOnDisk == null)
            {
                tsmItem = new ToolStripMenuItem("Extract");
                tsmItem.Click += (sender, args) =>
                {
                    string outPath = UAGConfig.ExtractFile(Pointer);
                    if ((Path.GetExtension(outPath)?.Length ?? 0) > 0) outPath = Path.GetDirectoryName(outPath);
                    UAGUtils.OpenDirectory(outPath);
                };
                this.ContextMenuStrip.Items.Add(tsmItem);
                tsmItem = new ToolStripMenuItem("Stage");
                tsmItem.Click += (sender, args) => Pointer.StageFile();
                this.ContextMenuStrip.Items.Add(tsmItem);
            }

            if (Pointer.FixedPathOnDisk != null)
            {
                tsmItem = new ToolStripMenuItem("Delete");
                tsmItem.Click += (sender, args) => Pointer.DeleteFile();
                this.ContextMenuStrip.Items.Add(tsmItem);
            }
        }
    }

    public class DirectoryTree
    {
        public FileContainerForm ParentForm;
        public IDictionary<string, DirectoryTreeItem> RootNodes;

        public DirectoryTree(FileContainerForm parentForm)
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
            ParentForm = parentForm;
        }

        public DirectoryTree(FileContainerForm parentForm, string[] paths, string[] fixedAssetsOnDisk = null)
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
            ParentForm = parentForm;
            if (fixedAssetsOnDisk != null && fixedAssetsOnDisk.Length == paths.Length)
            {
                for (int i = 0; i < paths.Length; i++) this.CreateNode(paths[i], fixedAssetsOnDisk[i]);
            }
            else
            {
                for (int i = 0; i < paths.Length; i++) this.CreateNode(paths[i]);
            }
        }

        public DirectoryTreeItem GetRootNode(string component)
        {
            if (RootNodes.TryGetValue(component, out var node)) return node;
            return null;
        }

        public DirectoryTreeItem GetNode(string path)
        {
            string[] pathComponents = path.Split('/');
            if (pathComponents.Length == 0) return null;
            if (!RootNodes.ContainsKey(pathComponents[0])) return null;

            DirectoryTreeItem currentItem = RootNodes[pathComponents[0]];
            for (int i = 1; i < pathComponents.Length; i++)
            {
                currentItem = currentItem.Children[pathComponents[i]];
            }
            return currentItem;
        }

        public DirectoryTreeItem CreateNode(string path, string fixedAssetOnDisk = null)
        {
            string[] pathComponents = path.Split('/');
            if (pathComponents.Length == 0) return null;

            string[] fixedAssetOnDiskComponents = fixedAssetOnDisk?.Split(Path.DirectorySeparatorChar) ?? Array.Empty<string>();
            string startingFixedAssetOnDisk = string.Empty;
            if (fixedAssetOnDiskComponents.Length > pathComponents.Length)
            {
                string[] fixedAssetOnDiskComponentsNuevo = new string[fixedAssetOnDiskComponents.Length - pathComponents.Length];
                Array.Copy(fixedAssetOnDiskComponents, fixedAssetOnDiskComponentsNuevo, fixedAssetOnDiskComponentsNuevo.Length);
                startingFixedAssetOnDisk = string.Join(Path.DirectorySeparatorChar, fixedAssetOnDiskComponentsNuevo);
            }

            if (!RootNodes.ContainsKey(pathComponents[0]))
            {
                string ext = Path.GetExtension(pathComponents[0]);
                if (ext.Length > 1 && (ext == ".uexp" || ext == ".ubulk" || ext == ".bak")) return null;

                RootNodes[pathComponents[0]] = new DirectoryTreeItem(ParentForm, pathComponents[0], pathComponents[0], ext.Length > 1);
            }

            DirectoryTreeItem currentItem = RootNodes[pathComponents[0]];
            if (startingFixedAssetOnDisk.Length > 0)
            {
                startingFixedAssetOnDisk = Path.Combine(startingFixedAssetOnDisk, pathComponents[0]);
                currentItem.FixedPathOnDisk = startingFixedAssetOnDisk;
            }

            for (int i = 1; i < pathComponents.Length; i++)
            {
                string ext = Path.GetExtension(pathComponents[i]);
                if (ext.Length > 1 && (ext == ".uexp" || ext == ".ubulk" || ext == ".bak")) return null;

                if (!currentItem.Children.ContainsKey(pathComponents[i]))
                {
                    currentItem.Children[pathComponents[i]] = new DirectoryTreeItem(ParentForm, pathComponents[i], currentItem.FullPath + "/" + pathComponents[i], ext.Length > 1);
                    currentItem.Children[pathComponents[i]].Parent = currentItem;
                }
                currentItem = currentItem.Children[pathComponents[i]];

                if (startingFixedAssetOnDisk.Length > 0)
                {
                    startingFixedAssetOnDisk = Path.Combine(startingFixedAssetOnDisk, pathComponents[i]);
                    currentItem.FixedPathOnDisk = startingFixedAssetOnDisk;
                }
            }

            return currentItem;
        }
    }

    public class DirectoryTreeItem
    {
        public FileContainerForm ParentForm;
        public string Name;
        public string FullPath;
        public string FixedPathOnDisk;
        public bool IsFile = false;
        public DirectoryTreeItem Parent;
        public IDictionary<string, DirectoryTreeItem> Children;

        public string SaveFileToTemp()
        {
            string outputPathDirectory = Path.Combine(Path.GetTempPath(), "UAG_read_only", Path.GetFileNameWithoutExtension(ParentForm.CurrentContainerPath));
            
            string outputPath1 = Path.Combine(outputPathDirectory, FullPath.Replace('/', Path.DirectorySeparatorChar));
            string outputPath2 = Path.Combine(outputPathDirectory, Path.ChangeExtension(FullPath, ".uexp").Replace('/', Path.DirectorySeparatorChar));

            if (FixedPathOnDisk != null)
            {
                File.Copy(FixedPathOnDisk, outputPath1, true);
                try { File.Copy(Path.ChangeExtension(FixedPathOnDisk, ".uexp"), outputPath2, true); } catch { }
                return outputPath1;
            }

            using (FileStream stream = new FileStream(ParentForm.CurrentContainerPath, FileMode.Open))
            {
                var reader = new PakBuilder().Reader(stream);

                byte[] res = reader.Get(stream, FullPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath1));
                if (res.Length > 0) File.WriteAllBytes(outputPath1, res);

                res = reader.Get(stream, Path.ChangeExtension(FullPath, ".uexp"));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath2));
                if (res.Length > 0) File.WriteAllBytes(outputPath2, res);
            }

            return outputPath1;
        }

        public void OpenFile()
        {
            string outputPath = FixedPathOnDisk != null ? FixedPathOnDisk : this.SaveFileToTemp();

            string ext = Path.GetExtension(outputPath);
            if (ext == ".uasset" || ext == ".umap")
            {
                this.ParentForm.BaseForm.LoadFileAt(outputPath);
                this.ParentForm.BaseForm.Focus();
            }
            else
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo(outputPath) { UseShellExecute = true }
                }.Start(); // open externally
            }
        }

        public void StageFile(string newPath = null)
        {
            if (newPath == null) newPath = this.FullPath;

            UAGConfig.StageFile(this, newPath);
            this.ParentForm.RefreshTreeView(this.ParentForm.saveTreeView);

            var generatedNode = this.ParentForm.GetSpecificNode(this.ParentForm.saveTreeView, newPath);
            if (generatedNode == null) return;
            generatedNode.EnsureVisible();
            this.ParentForm.saveTreeView.SelectedNode = generatedNode;
        }

        public void DeleteFile()
        {
            if (FixedPathOnDisk == null) return;

            try
            {
                File.Delete(FixedPathOnDisk);
                try { File.Delete(Path.ChangeExtension(FixedPathOnDisk, ".uexp")); } catch { }
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(FixedPathOnDisk, true);
            }

            FixedPathOnDisk = null;
            this.ParentForm.RefreshTreeView(this.ParentForm.saveTreeView);
        }

        public DirectoryTreeItem(FileContainerForm parentForm, string name, string fullPath, bool isFile)
        {
            ParentForm = parentForm;
            FullPath = fullPath;
            Name = Path.GetFileName(name);
            IsFile = isFile;
            Children = new Dictionary<string, DirectoryTreeItem>();
        }

        public DirectoryTreeItem(FileContainerForm parentForm)
        {
            ParentForm = parentForm;
            Children = new Dictionary<string, DirectoryTreeItem>();
        }
    }
}
