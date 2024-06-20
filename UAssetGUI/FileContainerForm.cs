using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    public partial class FileContainerForm : Form
    {
        public IDictionary<TreeView, DirectoryTree> DirectoryTreeMap = new Dictionary<TreeView, DirectoryTree>();
        public PakBuilder pakBuilder;
        public string CurrentContainerPath;

        public FileContainerForm()
        {
            InitializeComponent();
        }

        private Form1 BaseForm;
        private void FileContainerForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1) BaseForm = (Form1)this.Owner;

            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();

            pakBuilder = new PakBuilder();

            LoadFile(CurrentContainerPath);
        }

        public void AddDirectoryTreeItemToTreeView(DirectoryTreeItem treeItem, PointingFileTreeNode dad)
        {
            foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in treeItem.Children)
            {
                var newDad = new PointingFileTreeNode(directoryItem.Value.Name);
                dad.Nodes.Add(newDad);
                AddDirectoryTreeItemToTreeView(directoryItem.Value, newDad);
            }
        }

        public void RefreshTreeView(TreeView treeView)
        {
            treeView.Nodes.Clear();
            DirectoryTree currentTree = DirectoryTreeMap[treeView];
            foreach (KeyValuePair<string, DirectoryTreeItem> directoryItem in currentTree.RootNodes)
            {
                var dad = new PointingFileTreeNode(directoryItem.Value.Name);
                treeView.Nodes.Add(dad);
                AddDirectoryTreeItemToTreeView(directoryItem.Value, dad);
            }

            treeView.Sort();
            if (treeView.Nodes.Count > 0)
            {
                treeView.BackColor = UAGPalette.BackColor;
            }
            else
            {
                treeView.BackColor = UAGPalette.InactiveColor;
            }
        }

        public void LoadFile(string path)
        {
            if (path == null) return;
            CurrentContainerPath = path;

            string[] allFiles = Array.Empty<string>();
            using (FileStream stream = new FileStream(CurrentContainerPath, FileMode.Open))
            {
                allFiles = pakBuilder.Reader(stream).Files();
            }

            var res = new DirectoryTree();
            foreach (string assetPath in allFiles) res.CreateNode(assetPath);
            DirectoryTreeMap[loadTreeView] = res;
            RefreshTreeView(loadTreeView);
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
    }

    public class PointingFileTreeNode : TreeNode
    {
        public PointingFileTreeNode(string text) : base(text)
        {

        }
    }

    public class DirectoryTree
    {
        public IDictionary<string, DirectoryTreeItem> RootNodes;

        public DirectoryTree()
        {
            RootNodes = new Dictionary<string, DirectoryTreeItem>();
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

        public DirectoryTreeItem CreateNode(string path)
        {
            string[] pathComponents = path.Split('/');
            if (pathComponents.Length == 0) return null;

            if (!RootNodes.ContainsKey(pathComponents[0]))
            {
                string ext = Path.GetExtension(pathComponents[0]);
                if (ext.Length > 1 && ext != ".uasset" && ext != ".umap") return null;

                RootNodes[pathComponents[0]] = new DirectoryTreeItem(pathComponents[0], pathComponents[0], ext.Length > 1);
            }

            DirectoryTreeItem currentItem = RootNodes[pathComponents[0]];
            for (int i = 1; i < pathComponents.Length; i++)
            {
                string ext = Path.GetExtension(pathComponents[i]);
                if (ext.Length > 1 && ext != ".uasset" && ext != ".umap") return null;

                if (!currentItem.Children.ContainsKey(pathComponents[i]))
                {
                    currentItem.Children[pathComponents[i]] = new DirectoryTreeItem(pathComponents[i], currentItem.FullPath + "/" + pathComponents[i], ext.Length > 1);
                }
                currentItem = currentItem.Children[pathComponents[i]];
            }
            return currentItem;
        }
    }

    public class DirectoryTreeItem
    {
        public string Name;
        public string FullPath;
        public bool IsFile = false;
        public IDictionary<string, DirectoryTreeItem> Children;

        public DirectoryTreeItem(string name, string fullPath, bool isFile)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(name);
            IsFile = isFile;
            Children = new Dictionary<string, DirectoryTreeItem>();
        }

        public DirectoryTreeItem()
        {
            Children = new Dictionary<string, DirectoryTreeItem>();
        }
    }
}
