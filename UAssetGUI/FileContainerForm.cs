using System;
using System.Collections.Generic;
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
        private void FileContainerForm_Load(object sender, EventArgs e)
        {
            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();

            LoadContainer(CurrentContainerPath);
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
                    if (pftNode.IsExpanded) hashSet.Add(pftNode.FullPath);
                    ExpandedToHashSet(pftNode.Nodes, hashSet);
                }
            }
        }

        public void HashSetToExpanded(TreeNodeCollection nodes, HashSet<string> hashSet)
        {
            foreach (TreeNode node in nodes)
            {
                if (node is PointingFileTreeNode pftNode)
                {
                    if (hashSet.Contains(pftNode.FullPath)) pftNode.Expand();
                    HashSetToExpanded(pftNode.Nodes, hashSet);
                }
            }
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
            RefreshTreeView(saveTreeView);
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
                    pakWriter.WriteFile(stagingFiles[i], File.ReadAllBytes(fixedPathsOnDisk[i]));
                }
                pakWriter.WriteIndex();
            }

            return true;
        }

        public void ForceResize()
        {
            splitContainer1.Size = new Size(splitContainer1.Size.Width, this.Size.Height - menuStrip1.Size.Height - 50);
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

        private void loadButton_Click(object sender, EventArgs e)
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

        private void saveButton_Click(object sender, EventArgs e)
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
    }

    public class PointingFileTreeNode : TreeNode
    {
        public DirectoryTreeItem Pointer;

        public PointingFileTreeNode(string text, DirectoryTreeItem item) : base(text)
        {
            Pointer = item;

            this.ContextMenuStrip = new ContextMenuStrip();

            if (Pointer.IsFile)
            {
                ToolStripMenuItem tsmItem = new ToolStripMenuItem("Open");
                tsmItem.Click += (sender, args) => Pointer.OpenFile();
                this.ContextMenuStrip.Items.Add(tsmItem);
                tsmItem = new ToolStripMenuItem("Extract");
                tsmItem.Click += (sender, args) => UAGConfig.ExtractFile(Pointer);
                this.ContextMenuStrip.Items.Add(tsmItem);

                if (Pointer.FixedPathOnDisk == null)
                {
                    tsmItem = new ToolStripMenuItem("Stage");
                    tsmItem.Click += (sender, args) => Pointer.StageFile();
                    this.ContextMenuStrip.Items.Add(tsmItem);
                }
                else
                {
                    tsmItem = new ToolStripMenuItem("Delete");
                    tsmItem.Click += (sender, args) => Pointer.DeleteFile();
                    this.ContextMenuStrip.Items.Add(tsmItem);
                }
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

            if (!RootNodes.ContainsKey(pathComponents[0]))
            {
                string ext = Path.GetExtension(pathComponents[0]);
                if (ext.Length > 1 && ext != ".uasset" && ext != ".umap") return null;

                RootNodes[pathComponents[0]] = new DirectoryTreeItem(ParentForm, pathComponents[0], pathComponents[0], ext.Length > 1);
            }

            DirectoryTreeItem currentItem = RootNodes[pathComponents[0]];
            for (int i = 1; i < pathComponents.Length; i++)
            {
                string ext = Path.GetExtension(pathComponents[i]);
                if (ext.Length > 1 && ext != ".uasset" && ext != ".umap") return null;

                if (!currentItem.Children.ContainsKey(pathComponents[i]))
                {
                    currentItem.Children[pathComponents[i]] = new DirectoryTreeItem(ParentForm, pathComponents[i], currentItem.FullPath + "/" + pathComponents[i], ext.Length > 1);
                }
                currentItem = currentItem.Children[pathComponents[i]];
            }

            currentItem.FixedPathOnDisk = fixedAssetOnDisk;
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
        public IDictionary<string, DirectoryTreeItem> Children;

        public string SaveFileToTemp()
        {
            string outputPathDirectory = Path.Combine(Path.GetTempPath(), "UAG_read_only", Path.GetFileNameWithoutExtension(ParentForm.CurrentContainerPath));
            
            string outputPath1 = Path.Combine(outputPathDirectory, FullPath.Replace('/', Path.DirectorySeparatorChar));
            string outputPath2 = Path.Combine(outputPathDirectory, Path.ChangeExtension(FullPath, ".uexp").Replace('/', Path.DirectorySeparatorChar));
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
            this.ParentForm.BaseForm.LoadFileAt(outputPath);
            this.ParentForm.BaseForm.Focus();
            if (FixedPathOnDisk != null)
            {
                try { File.Delete(outputPath); } catch { } // doesnt matter at all if we cant actually delete it
            }
        }

        public void StageFile()
        {
            UAGConfig.StageFile(this);
            this.ParentForm.RefreshTreeView(this.ParentForm.saveTreeView);
        }

        public void DeleteFile()
        {
            if (FixedPathOnDisk == null) return;

            File.Delete(FixedPathOnDisk);
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
