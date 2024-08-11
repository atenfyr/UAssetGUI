using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            this.AdjustFormPosition(BaseForm);

            this.cutToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.X);
            this.copyToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.C);
            this.pasteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Control | Keys.V);
            this.deleteToolStripMenuItem.ShortcutKeyDisplayString = UAGUtils.ShortcutToText(Keys.Delete);

            this.loadTreeView.AllowDrop = true;
            this.loadTreeView.DragEnter += new DragEventHandler(event_DragEnter);
            this.loadTreeView.DragDrop += new DragEventHandler(loadTreeView_DragDrop);
            this.saveTreeView.AllowDrop = true;
            this.saveTreeView.DragEnter += new DragEventHandler(event_DragEnter);
            this.saveTreeView.DragDrop += new DragEventHandler(saveTreeView_DragDrop);

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

            LoadContainer(CurrentContainerPath);
            RefreshTreeView(saveTreeView);
        }

        private static volatile Dictionary<ToolStripItem, bool> isDropDownOpened = new Dictionary<ToolStripItem, bool>();
        public static bool IsDropDownOpened(ToolStripItem item)
        {
            if (!isDropDownOpened.ContainsKey(item)) return false;
            return isDropDownOpened[item];
        }

        public void AddDirectoryItemChildrenToTreeView(DirectoryTreeItem treeItem, PointingFileTreeNode dad, bool forceAddChildrenOfChildren = false)
        {
            dad.ChildrenInitialized = forceAddChildrenOfChildren || !UAGConfig.Data.EnableDynamicTree;
            foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in treeItem.Children)
            {
                var newDad = new PointingFileTreeNode(directoryItem.Value.Name, directoryItem.Value);
                newDad.ChildrenInitialized = false;
                dad.Nodes.Add(newDad);
                if (dad.ChildrenInitialized) AddDirectoryItemChildrenToTreeView(directoryItem.Value, newDad, false);
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
                    if (fullPath.StartsWith(pftNode.Pointer.FullPath)) // we know this is an ancestor node
                    {
                        if (!pftNode.ChildrenInitialized) InitializeChildren(pftNode);
                        var gotten = GetSpecificNode(node.Nodes, fullPath);
                        if (gotten != null) return gotten;
                        break; // obviously no sibling node to this node can be also an ancestor node
                    }
                }
            }
            return null;
        }

        public PointingFileTreeNode GetSpecificNode(TreeView treeView, string fullPath)
        {
            return GetSpecificNode(treeView.Nodes, fullPath);
        }

        public PointingFileTreeNode GetSpecificNode(string fullPath)
        {
            var res = GetSpecificNode(loadTreeView, fullPath);
            if (res != null) return res;
            res = GetSpecificNode(saveTreeView, fullPath);
            if (res != null) return res;

            return null;
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
            if (currentTree?.RootNodes != null)
            {
                foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in currentTree.RootNodes)
                {
                    var dad = new PointingFileTreeNode(directoryItem.Value.Name, directoryItem.Value);
                    treeView.Nodes.Add(dad);
                    AddDirectoryItemChildrenToTreeView(directoryItem.Value, dad);
                }
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

        public void UnloadContainer()
        {
            CurrentContainerPath = null;
            DirectoryTreeMap[loadTreeView] = null;
            Version = PakVersion.V4;
            MountPoint = "../../../";

            RefreshTreeView(loadTreeView);
            this.Text = BaseForm.DisplayVersion + " - " + CurrentContainerPath;
            SelectedTreeView = saveTreeView;
        }

        public void LoadContainer(string path)
        {
            if (path == null) return;
            try
            {
                CurrentContainerPath = path;

                string[] allFiles = Array.Empty<string>();
                using (FileStream stream = new FileStream(CurrentContainerPath, FileMode.Open))
                {
                    var pakReader = new PakBuilder().Reader(stream);
                    allFiles = pakReader.Files();
                    Version = pakReader.GetVersion();
                    MountPoint = pakReader.GetMountPoint();
                }

                // if the MountPoint starts with "../../../", then let's adjust all files so we can just change the MountPoint to that
                string mpPrefix = "../../../";
                string Prefix = "";
                if (MountPoint.StartsWith(mpPrefix))
                {
                    Prefix = MountPoint.Substring(mpPrefix.Length);
                    MountPoint = mpPrefix;

                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        allFiles[i] = Path.Combine(Prefix, allFiles[i]);
                    }
                }

                DirectoryTreeMap[loadTreeView] = new DirectoryTree(this, allFiles, null, Prefix);
                RefreshTreeView(loadTreeView);

                this.Text = BaseForm.DisplayVersion + " - " + CurrentContainerPath;
                SelectedTreeView = loadTreeView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file! " + ex.Message, "Uh oh!");

                UnloadContainer();
            }
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
            splitContainer1.Size = new Size(this.Size.Width - 28, this.Size.Height - menuStrip1.Size.Height - 50);
            splitContainer1.Panel1.Size = new Size(splitContainer1.SplitterDistance, splitContainer1.Size.Height);
            splitContainer1.Panel2.Size = new Size(splitContainer1.Size.Width - splitContainer1.SplitterDistance, splitContainer1.Size.Height);
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
            splitContainer1.SplitterDistance = (this.Size.Width - 28) / 2;
            ForceResize();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.X:
                    cutToolStripMenuItem.PerformClick();
                    return true;
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
            this.SelectedTreeView?.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectedTreeView?.CollapseAll();
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

        private DirectoryTreeItem copiedPftNode = null;
        private bool shouldDeleteCopiedPftNode = false;
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView?.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (pftNode == null) return;

                copiedPftNode = pftNode.Pointer;
                shouldDeleteCopiedPftNode = false;
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView?.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (copiedPftNode == null || pftNode == null) return;
                DirectoryTreeItem clipboardNode = copiedPftNode;

                if (string.IsNullOrEmpty(pftNode.Pointer.FixedPathOnDisk)) return; // only allow pasting into staging
                DirectoryTreeItem targetDirectory = pftNode.Pointer;
                if (targetDirectory.IsFile) targetDirectory = targetDirectory.Parent;

                string desiredStagingPath = Path.Combine(targetDirectory?.FullPath ?? string.Empty, clipboardNode.Name);
                clipboardNode.StageFile(desiredStagingPath);

                if (shouldDeleteCopiedPftNode)
                {
                    clipboardNode.DeleteFile();
                    shouldDeleteCopiedPftNode = false;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView?.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (pftNode == null) return;
                if (string.IsNullOrEmpty(pftNode.Pointer.FixedPathOnDisk)) return; // only allow deleting from staging

                pftNode.Pointer.DeleteFile();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedTreeView?.SelectedNode is PointingFileTreeNode pftNode)
            {
                if (pftNode == null) return;
                if (string.IsNullOrEmpty(pftNode.Pointer.FixedPathOnDisk)) return; // only allow cutting from staging

                copiedPftNode = pftNode.Pointer;
                shouldDeleteCopiedPftNode = true;
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

        private void stageFromDiskToPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    TextPrompt replacementPrompt = new TextPrompt()
                    {
                        DisplayText = "What path should this object be staged to?"
                    };

                    replacementPrompt.StartPosition = FormStartPosition.CenterParent;
                    replacementPrompt.PrefilledText = Path.GetFileName(openFileDialog.FileName) ?? string.Empty;

                    string newFileName = null;
                    if (replacementPrompt.ShowDialog(ParentForm) == DialogResult.OK)
                    {
                        newFileName = string.Join("_", replacementPrompt.OutputText.Replace('\\', '/').Split(Path.GetInvalidPathChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                    }

                    replacementPrompt.Dispose();

                    if (newFileName != null)
                    {
                        UAGConfig.StageFile(openFileDialog.FileName, CurrentContainerPath, newFileName);
                        this.RefreshTreeView(this.saveTreeView);
                    }
                }
            }
        }

        private void InitializeChildren(PointingFileTreeNode ptn)
        {
            if (!ptn.ChildrenInitialized)
            {
                ptn.Nodes.Clear();
                AddDirectoryItemChildrenToTreeView(ptn.Pointer, ptn, true);
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node is PointingFileTreeNode ptn) InitializeChildren(ptn);
        }

        private void ExtractVisit(DirectoryTreeItem processingNode, ProgressBarForm progressBarForm)
        {
            if (processingNode.IsFile)
            {
                UAGConfig.ExtractFile(processingNode);
                extractAllBackgroundWorker.ReportProgress(0); // the percentage we pass in is unused
                return;
            }

            foreach (var entry in processingNode.Children)
            {
                if (extractAllBackgroundWorker.CancellationPending) break;
                ExtractVisit(entry.Value, progressBarForm);
            }
        }

        private ProgressBarForm progressBarForm;
        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!DirectoryTreeMap.TryGetValue(loadTreeView, out DirectoryTree loadedTree) || loadedTree == null)
            {
                MessageBox.Show("Please load a container first to extract it.", "Notice");
                return;
            }

            if (extractAllBackgroundWorker.IsBusy) return;

            int numFiles = loadedTree.GetNumFiles();

            UAGUtils.InvokeUI(() =>
            {
                progressBarForm = new ProgressBarForm();
                progressBarForm.Value = 0;
                progressBarForm.Maximum = numFiles;
                progressBarForm.Text = this.Text;
                progressBarForm.BaseForm = this;
                progressBarForm.Show(this);
            });

            extractAllBackgroundWorker.RunWorkerAsync();
        }

        private void extractAllBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!DirectoryTreeMap.TryGetValue(loadTreeView, out DirectoryTree loadedTree) || loadedTree == null) throw new InvalidOperationException("No container loaded");
            foreach (var entry in loadedTree.RootNodes)
            {
                if (extractAllBackgroundWorker.CancellationPending) break;
                ExtractVisit(entry.Value, progressBarForm);
            }

            if (extractAllBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            UAGUtils.OpenDirectory(UAGConfig.ExtractedFolder);
        }

        private void extractAllBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UAGUtils.InvokeUI(() => progressBarForm.Progress(1));
        }

        private void extractAllBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                if (e.Cancelled)
                {
                    MessageBox.Show("Operation canceled.", "Notice");
                }
                else if (e.Error != null)
                {
                    MessageBox.Show("An error occured during extraction! " + e.Error.Message, "Uh oh!");
                }
                else
                {
                    MessageBox.Show("Extracted " + progressBarForm.Value + " files successfully.", "Notice");
                }
                progressBarForm.Close();
            });
        }
    }

    public class PointingFileTreeNode : TreeNode
    {
        public DirectoryTreeItem Pointer;
        public bool ChildrenInitialized = false;

        public PointingFileTreeNode(string text, DirectoryTreeItem item) : base(text)
        {
            Pointer = item;
            //NodeFont = new Font(new FontFamily("Microsoft Sans Serif"), 8.25f);

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
                    if (outPath == null) return;
                    if ((Path.GetExtension(outPath)?.Length ?? 0) > 0) outPath = Path.GetDirectoryName(outPath);
                    UAGUtils.OpenDirectory(outPath);
                };
                this.ContextMenuStrip.Items.Add(tsmItem);
                tsmItem = new ToolStripMenuItem("Stage");
                tsmItem.Click += (sender, args) => Pointer.StageFile();
                this.ContextMenuStrip.Items.Add(tsmItem);
                tsmItem = new ToolStripMenuItem("Stage to path...");
                tsmItem.Click += (sender, args) => Pointer.StageFileToPath();
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

        public DirectoryTree(FileContainerForm parentForm, string[] paths, string[] fixedAssetsOnDisk = null, string prefix = null)
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
            ParentForm = parentForm;
            if (fixedAssetsOnDisk != null && fixedAssetsOnDisk.Length == paths.Length)
            {
                for (int i = 0; i < paths.Length; i++) this.CreateNode(paths[i], fixedAssetsOnDisk[i], prefix);
            }
            else
            {
                for (int i = 0; i < paths.Length; i++) this.CreateNode(paths[i], null, prefix);
            }
        }

        private static int GetNumFilesVisit(DirectoryTreeItem item)
        {
            if (item.IsFile) return 1;

            int numFiles = 0; // to count directories too, set to 1
            foreach (var entry in item.Children) numFiles += GetNumFilesVisit(entry.Value);
            return numFiles;
        }

        public int GetNumFiles()
        {
            int numFiles = 0;
            foreach (var entry in RootNodes) numFiles += GetNumFilesVisit(entry.Value);
            return numFiles;
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

        public DirectoryTreeItem CreateNode(string path, string fixedAssetOnDisk = null, string prefix = null)
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

                RootNodes[pathComponents[0]] = new DirectoryTreeItem(ParentForm, pathComponents[0], pathComponents[0], ext.Length > 1, prefix);
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
                    currentItem.Children[pathComponents[i]] = new DirectoryTreeItem(ParentForm, pathComponents[i], currentItem.FullPath + "/" + pathComponents[i], ext.Length > 1, prefix);
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
        public string Prefix;
        public bool IsFile = false;
        public DirectoryTreeItem Parent;
        public IDictionary<string, DirectoryTreeItem> Children;

        public string SaveFileToTemp()
        {
            string outputPathDirectory = Path.Combine(Path.GetTempPath(), "UAG_read_only", Path.GetFileNameWithoutExtension(ParentForm.CurrentContainerPath));
            
            string outputPath1 = Path.Combine(outputPathDirectory, FullPath.Replace('/', Path.DirectorySeparatorChar));
            string outputPath2 = Path.Combine(outputPathDirectory, Path.ChangeExtension(FullPath, ".uexp").Replace('/', Path.DirectorySeparatorChar));
            string outputPath3 = Path.Combine(outputPathDirectory, Path.ChangeExtension(FullPath, ".ubulk").Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath1)); // same directory as outputPath2, no need to create that one too

            if (FixedPathOnDisk != null)
            {
                File.Copy(FixedPathOnDisk, outputPath1, true);
                try { File.Copy(Path.ChangeExtension(FixedPathOnDisk, ".uexp"), outputPath2, true); } catch { }
                try { File.Copy(Path.ChangeExtension(FixedPathOnDisk, ".ubulk"), outputPath3, true); } catch { }
                return outputPath1;
            }

            using (FileStream stream = new FileStream(ParentForm.CurrentContainerPath, FileMode.Open))
            {
                var reader = new PakBuilder().Reader(stream);

                byte[] res = reader.Get(stream, FullPath.Substring(Prefix?.Length ?? 0));
                if (res != null)
                {
                    File.WriteAllBytes(outputPath1, res);
                }
                else
                {
                    return null;
                }

                res = reader.Get(stream, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".uexp"));
                if (res != null) File.WriteAllBytes(outputPath2, res);
                res = reader.Get(stream, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".ubulk"));
                if (res != null) File.WriteAllBytes(outputPath2, res);
            }

            return outputPath1;
        }

        public void OpenFile()
        {
            string outputPath = FixedPathOnDisk != null ? FixedPathOnDisk : this.SaveFileToTemp();
            if (outputPath == null)
            {
                MessageBox.Show("Unable to open file!", "Uh oh!");
                return;
            }

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

        public void StageFileToPath()
        {
            UAGUtils.InvokeUI(() =>
            {
                TextPrompt replacementPrompt = new TextPrompt()
                {
                    DisplayText = "What path should this object be staged to?"
                };

                replacementPrompt.StartPosition = FormStartPosition.CenterParent;
                replacementPrompt.PrefilledText = this.FullPath ?? string.Empty;

                string newFileName = null;
                if (replacementPrompt.ShowDialog(ParentForm) == DialogResult.OK)
                {
                    newFileName = string.Join("_", replacementPrompt.OutputText.Replace('\\', '/').Split(Path.GetInvalidPathChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                }

                replacementPrompt.Dispose();

                if (newFileName != null)
                {
                    this.StageFile(newFileName);
                }
            });
        }

        public void DeleteFile()
        {
            if (FixedPathOnDisk == null) return;

            try
            {
                File.Delete(FixedPathOnDisk);
                try { File.Delete(Path.ChangeExtension(FixedPathOnDisk, ".uexp")); } catch { }
                try { File.Delete(Path.ChangeExtension(FixedPathOnDisk, ".ubulk")); } catch { }
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(FixedPathOnDisk, true);
            }

            FixedPathOnDisk = null;
            this.ParentForm.RefreshTreeView(this.ParentForm.saveTreeView);
        }

        public DirectoryTreeItem(FileContainerForm parentForm, string name, string fullPath, bool isFile, string prefix)
        {
            ParentForm = parentForm;
            FullPath = fullPath;
            Name = Path.GetFileName(name);
            IsFile = isFile;
            Prefix = prefix;
            Children = new Dictionary<string, DirectoryTreeItem>();
        }

        public DirectoryTreeItem(FileContainerForm parentForm)
        {
            ParentForm = parentForm;
            Children = new Dictionary<string, DirectoryTreeItem>();
        }
    }
}
