using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UAssetGUI
{
    public enum InteropType
    {
        Pak,
        Retoc
    }

    public enum RetocEngineVersion
    {
        UE4_25 = EngineVersion.VER_UE4_25,
        UE4_26 = EngineVersion.VER_UE4_26,
        UE4_27 = EngineVersion.VER_UE4_27,
        UE5_0 = EngineVersion.VER_UE5_0,
        UE5_1 = EngineVersion.VER_UE5_1,
        UE5_2 = EngineVersion.VER_UE5_2,
        UE5_3 = EngineVersion.VER_UE5_3,
        UE5_4 = EngineVersion.VER_UE5_4,
        UE5_5 = EngineVersion.VER_UE5_5,
        UE5_6 = EngineVersion.VER_UE5_6,
        UE5_7 = EngineVersion.VER_UE5_7,
    }

    public struct RetocManifest
    {
        [JsonProperty("oplog")]
        public RetocOpLog OpLog;
    }

    public struct RetocOpLog
    {
        [JsonProperty("entries")]
        public List<RetocOp> Entries;
    }

    public struct RetocOp
    {
        [JsonProperty("packagestoreentry")]
        public RetocPackageStoreEntry PackageStoreEntry;
        [JsonProperty("packagedata")]
        public List<RetocChunkData> PackageData;
        [JsonProperty("bulkdata")]
        public List<RetocChunkData> BulkData;
    }

    public struct RetocPackageStoreEntry
    {
        [JsonProperty("packagename")]
        public string PackageName;
    }

    public struct RetocChunkData
    {
        [JsonProperty("id")]
        public string ID;
        [JsonProperty("filename")]
        public string FileName;
    }

    public partial class FileContainerForm : Form
    {
        public IDictionary<TreeView, DirectoryTree> DirectoryTreeMap = new Dictionary<TreeView, DirectoryTree>();
        public string CurrentContainerPath;
        public PakVersion Version = PakVersion.V4;
        public InteropType InteropType = InteropType.Pak;
        public string MountPoint = "../../../";
        public bool RetocAvailable = false;

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

            RetocAvailable = SendCommandToRetoc("--version", out string outputText, out _) && outputText.Contains("retoc_cli");

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

        public DirectoryTreeItem GetFromPackageName(TreeView treeView, string packageName)
        {
            DirectoryTree tree = DirectoryTreeMap[treeView];
            if (tree?.PackagePathToNode == null || !tree.PackagePathToNode.ContainsKey(packageName)) return null;
            return tree.PackagePathToNode[packageName];
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
            InteropType = InteropType.Pak;
            MountPoint = "../../../";

            RefreshTreeView(loadTreeView);
            this.Text = BaseForm.DisplayVersion + " - " + CurrentContainerPath;
            SelectedTreeView = saveTreeView;
        }

        public void LoadContainerPak(string path)
        {
            if (path == null) return;
            try
            {
                CurrentContainerPath = path;
                InteropType = InteropType.Pak;

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
                UAGUtils.InvokeUI(() => { MessageBox.Show("Failed to open file! " + ex.Message, "Uh oh!"); });

                UnloadContainer();
            }
        }

        public void LoadContainerUtoc(string path)
        {
            if (path == null) return;
            if (!RetocAvailable) return;
            try
            {
                CurrentContainerPath = path;
                InteropType = InteropType.Retoc;
                Version = PakVersion.V11;
                MountPoint = "../../../"; // retoc always uses this mount point
                string libsPath = Path.Combine(UAGConfig.ConfigFolder, "Libraries");

                // extract file manifest
                string expectedManifestPath = Path.Combine(libsPath, "pakstore.json");
                try { File.Delete(expectedManifestPath); } catch { }
                bool extractedManifest = SendCommandToRetoc($"manifest \"{Path.GetDirectoryName(path)}\"", out _, out _);
                RetocManifest manifestData = JsonConvert.DeserializeObject<RetocManifest>(File.ReadAllText(expectedManifestPath));

                if (manifestData.OpLog.Entries == null) throw new InvalidOperationException("Failed to extract manifest");

                // process manifest
                List<string> allFilesList = new List<string>();
                foreach (RetocOp op in manifestData.OpLog.Entries)
                {
                    if (op.PackageData == null) continue;
                    foreach (RetocChunkData chunk in op.PackageData)
                    {
                        if (!string.IsNullOrEmpty(chunk.FileName))
                        {
                            // we expect the filename to always start with MountPoint, but we check anyways
                            allFilesList.Add(chunk.FileName.StartsWith(MountPoint) ? chunk.FileName.Substring(MountPoint.Length) : chunk.FileName);
                        }
                    }
                }

                string[] allFiles = allFilesList.ToArray();
                DirectoryTreeMap[loadTreeView] = new DirectoryTree(this, allFiles, null, string.Empty);
                RefreshTreeView(loadTreeView);

                this.Text = BaseForm.DisplayVersion + " - " + CurrentContainerPath;
                SelectedTreeView = loadTreeView;
            }
            catch (Exception ex)
            {
                UAGUtils.InvokeUI(() => { MessageBox.Show("Failed to open file! " + ex.Message, "Uh oh!"); });

                UnloadContainer();
            }
        }

        public void LoadContainer(string path)
        {
            if (path == null) return;

            string ext = Path.GetExtension(path);
            switch(ext)
            {
                case ".pak":
                    LoadContainerPak(path);
                    break;
                case ".utoc":
                    LoadContainerUtoc(path);
                    break;
                case ".ucas":
                    LoadContainerUtoc(Path.ChangeExtension(path, ".utoc"));
                    break;
                default:
                    UAGUtils.InvokeUI(() => { MessageBox.Show($"Unknown file extension {ext}", "Uh oh!"); });
                    break;
            }
        }

        public bool SaveContainer(string path)
        {
            if (path == null) return false;
            string[] stagingFiles = UAGConfig.GetStagingFiles(this.CurrentContainerPath, out string[] fixedPathsOnDisk);
            if (stagingFiles == null || fixedPathsOnDisk == null || stagingFiles.Length != fixedPathsOnDisk.Length) return false;

            switch (Path.GetExtension(path))
            {
                case ".pak":
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        var pakWriter = new PakBuilder().Writer(stream, Version, MountPoint);
                        for (int i = 0; i < stagingFiles.Length; i++)
                        {
                            string ext = Path.GetExtension(fixedPathsOnDisk[i]);
                            if (ext == ".bak" || fixedPathsOnDisk[i].EndsWith(".bak.json")) continue;
                            pakWriter.WriteFile(stagingFiles[i], File.ReadAllBytes(fixedPathsOnDisk[i]));
                        }
                        pakWriter.WriteIndex();
                    }
                    break;
                case ".utoc":
                    string engVer = ((RetocEngineVersion)BaseForm.ParsingVersion).ToString();
                    FileContainerForm.SendCommandToRetoc($"to-zen --version {engVer} \"{UAGConfig.GetStagingDirectory(path)}\" \"{path}\"", out _, out _);
                    break;
            }

            return true;
        }

        public static bool SendCommandToRetoc(string args, out string outputText, out string errorText, bool displayConsole = false)
        {
            outputText = null;
            errorText = null;

            string libsPath = Path.Combine(UAGConfig.ConfigFolder, "Libraries");
            string retocPath = Path.Combine(libsPath, "retoc.exe");
            Directory.CreateDirectory(UAGConfig.ConfigFolder);
            Directory.CreateDirectory(libsPath);

            if (!HasAlreadyExtractedRetoc) ExtractRetoc();

            Process process = new Process();
            process.StartInfo.FileName = retocPath;
            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = libsPath;
            process.StartInfo.UseShellExecute = displayConsole ? true : false;
            process.StartInfo.RedirectStandardOutput = displayConsole ? false : true;
            process.StartInfo.RedirectStandardError = displayConsole ? false : true;
            process.StartInfo.CreateNoWindow = displayConsole ? false : true;

            process.Start();

            if (process.StartInfo.RedirectStandardOutput) outputText = process.StandardOutput.ReadToEnd().TrimEnd();
            if (process.StartInfo.RedirectStandardError) errorText = process.StandardError.ReadToEnd().TrimEnd();
            process.WaitForExit();
            int errorCode = process.ExitCode;
            process.Close();

            return errorCode == 0;
        }

        private static bool HasAlreadyExtractedRetoc = false;
        public static string ExtractRetoc()
        {
            HasAlreadyExtractedRetoc = true;
            string libsPath = Path.Combine(UAGConfig.ConfigFolder, "Libraries");
            Directory.CreateDirectory(UAGConfig.ConfigFolder);
            Directory.CreateDirectory(libsPath);

            return Program.ExtractCompressedResource($"UAssetGUI.retoc.exe.gz", Path.Combine(libsPath, "retoc.exe"));
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
                openFileDialog.Filter = "Unreal Engine Container Files (*.pak, *.utoc)|*.pak;*.utoc|All files (*.*)|*.*";
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
                dialog.Filter = ".pak Container Files (*.pak)|*.pak|.utoc Container Files (*.utoc)|*.utoc|All files (*.*)|*.*";
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

        private void ExtractVisit(DirectoryTreeItem processingNode, ProgressBarForm progressBarForm, FileStream stream2 = null, PakReader reader2 = null)
        {
            if (processingNode.IsFile)
            {
                UAGConfig.ExtractFile(processingNode, this.InteropType, stream2, reader2);
                extractAllBackgroundWorker.ReportProgress(0); // the percentage we pass in is unused
                return;
            }

            foreach (var entry in processingNode.Children)
            {
                if (extractAllBackgroundWorker.CancellationPending) break;
                ExtractVisit(entry.Value, progressBarForm, stream2, reader2);
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

            switch(InteropType)
            {
                case InteropType.Pak:
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
                    break;
                case InteropType.Retoc:
                    if (extractAllBackgroundWorker.IsBusy) return;
                    extractAllBackgroundWorker.RunWorkerAsync();
                    break;
                default:
                    MessageBox.Show($"This operation is not supported for the current interop type ({InteropType.ToString()}).", "Notice");
                    break;
            }
        }

        private void extractAllBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Directory.CreateDirectory(UAGConfig.ExtractedFolder);

            switch (InteropType)
            {
                case InteropType.Pak:
                    if (!DirectoryTreeMap.TryGetValue(loadTreeView, out DirectoryTree loadedTree) || loadedTree == null) throw new InvalidOperationException("No container loaded");

                    using (FileStream stream = new FileStream(this.CurrentContainerPath, FileMode.Open))
                    {
                        var reader = new PakBuilder().Reader(stream);
                        foreach (var entry in loadedTree.RootNodes)
                        {
                            if (extractAllBackgroundWorker.CancellationPending) break;
                            ExtractVisit(entry.Value, progressBarForm, stream, reader);
                        }
                    }

                    if (extractAllBackgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    UAGUtils.OpenDirectory(UAGConfig.ExtractedFolder);
                    break;
                case InteropType.Retoc:
                    FileContainerForm.SendCommandToRetoc($"to-legacy \"{Path.GetDirectoryName(CurrentContainerPath)}\" \"{UAGConfig.ExtractedFolder}\"", out _, out _, true);
                    UAGUtils.OpenDirectory(UAGConfig.ExtractedFolder);
                    break;
            }
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
                    MessageBox.Show(progressBarForm == null ? "Operation completed." : ("Extracted " + progressBarForm.Value + " files successfully."), "Notice");
                }
                progressBarForm?.Close();
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
                    string outPath = UAGConfig.ExtractFile(Pointer, item.ParentForm.InteropType);
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
        public IDictionary<string, DirectoryTreeItem> PackagePathToNode;

        public DirectoryTree(FileContainerForm parentForm)
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
            PackagePathToNode = new Dictionary<string, DirectoryTreeItem>();
            ParentForm = parentForm;
        }

        public DirectoryTree(FileContainerForm parentForm, string[] paths, string[] fixedAssetsOnDisk = null, string prefix = null)
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
            PackagePathToNode = new Dictionary<string, DirectoryTreeItem>();
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

        private static readonly Regex ProjectWithContentPrefixRegex = new Regex(@"^\/?[^\s\/]+\/Content", RegexOptions.Compiled);

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
                if (ext.Length > 1 && (ext == ".uexp" || ext == ".ubulk" || ext == ".bak" || pathComponents[i].EndsWith(".bak.json"))) return null;

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

            // todo, this algorithm needs to be fixed for plugins
            // Engine/Plugins/Animation/ControlRig/Content/Controls/ControlRigGizmoMaterial.uasset => /ControlRig/Controls/ControlRigGizmoMaterial
            string packageName = Path.ChangeExtension(currentItem.FullPath, null).Replace('\\', '/').Replace(Path.DirectorySeparatorChar, '/').Replace("Engine/Content/", "Engine/");
            packageName = ProjectWithContentPrefixRegex.Replace(packageName, "/Game", 1);
            if (!packageName.StartsWith('/')) packageName = '/' + packageName;
            PackagePathToNode[packageName] = currentItem;

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

        public string SaveFileToTemp(InteropType interopType, string outputPathDirectory = null, FileStream stream2 = null, PakReader reader2 = null)
        {
            outputPathDirectory = outputPathDirectory ?? Path.Combine(Path.GetTempPath(), "UAG_read_only", Path.GetFileNameWithoutExtension(ParentForm.CurrentContainerPath));
            
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

            switch(interopType)
            {
                case InteropType.Pak:
                    {
                        if (reader2 == null || stream2 == null)
                        {
                            using (FileStream stream = new FileStream(ParentForm.CurrentContainerPath, FileMode.Open))
                            {
                                var reader = new PakBuilder().Reader(stream);

                                byte[] res = reader.Get(stream, FullPath.Substring(Prefix?.Length ?? 0));
                                if (res != null)
                                {
                                    if (File.Exists(outputPath1)) { try { File.Delete(outputPath1); } catch { } }
                                    File.WriteAllBytes(outputPath1, res);
                                }
                                else
                                {
                                    return null;
                                }

                                res = reader.Get(stream, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".uexp"));
                                if (File.Exists(outputPath2)) { try { File.Delete(outputPath2); } catch { } }
                                if (res != null) File.WriteAllBytes(outputPath2, res);

                                res = reader.Get(stream, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".ubulk"));
                                if (File.Exists(outputPath3)) { try { File.Delete(outputPath3); } catch { } }
                                if (res != null) File.WriteAllBytes(outputPath3, res);
                            }
                        }
                        else
                        {
                            byte[] res = reader2.Get(stream2, FullPath.Substring(Prefix?.Length ?? 0));
                            if (res != null)
                            {
                                File.WriteAllBytes(outputPath1, res);
                            }
                            else
                            {
                                return null;
                            }

                            res = reader2.Get(stream2, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".uexp"));
                            if (res != null) File.WriteAllBytes(outputPath2, res);
                            res = reader2.Get(stream2, Path.ChangeExtension(FullPath.Substring(Prefix?.Length ?? 0), ".ubulk"));
                            if (res != null) File.WriteAllBytes(outputPath3, res);
                        }
                    }
                    break;
                case InteropType.Retoc:
                    string targetPath = FullPath.Substring(Prefix?.Length ?? 0);
                    string origPathPrefix = Path.Combine(Path.GetTempPath(), "UAG_retoc", "RetocFiles");
                    bool retocSuccess = FileContainerForm.SendCommandToRetoc($"to-legacy --filter \"{targetPath}\" \"{Path.GetDirectoryName(ParentForm.CurrentContainerPath)}\" \"{origPathPrefix}\"", out _, out _);
                    if (!retocSuccess) return null;

                    string origPath1 = Path.Combine(origPathPrefix, targetPath);
                    try { File.Move(Path.ChangeExtension(origPath1, "uasset"), outputPath1, true); } catch { }
                    try { File.Move(Path.ChangeExtension(origPath1, "umap"), outputPath1, true); } catch { }
                    try { File.Move(Path.ChangeExtension(origPath1, "uexp"), outputPath2, true); } catch { }
                    try { File.Move(Path.ChangeExtension(origPath1, "ubulk"), outputPath3, true); } catch { }
                    break;
            }

            return outputPath1;
        }

        public void OpenFile()
        {
            string outputPath = FixedPathOnDisk != null ? FixedPathOnDisk : this.SaveFileToTemp(this.ParentForm.InteropType);
            if (outputPath == null)
            {
                MessageBox.Show("Unable to open file!", "Uh oh!");
                return;
            }

            string ext = Path.GetExtension(outputPath);
            if (ext == ".uasset" || ext == ".umap")
            {
                this.ParentForm.BaseForm.LoadFileAt(outputPath, this.ParentForm);
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

            UAGConfig.StageFile(this, this.ParentForm.InteropType, newPath);
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
