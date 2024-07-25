using System.Windows.Forms;

namespace UAssetGUI
{
    partial class FileContainerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            loadButton = new Button();
            loadTreeView = new ColorfulTreeView();
            saveButton = new Button();
            saveTreeView = new ColorfulTreeView();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            stageFromDiskToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            expandAllToolStripMenuItem = new ToolStripMenuItem();
            collapseAllToolStripMenuItem = new ToolStripMenuItem();
            refreshToolStripMenuItem = new ToolStripMenuItem();
            utilsToolStripMenuItem = new ToolStripMenuItem();
            extractAllToolStripMenuItem = new ToolStripMenuItem();
            extractAllBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Top;
            splitContainer1.Location = new System.Drawing.Point(6, 30);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(loadButton);
            splitContainer1.Panel1.Controls.Add(loadTreeView);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(saveButton);
            splitContainer1.Panel2.Controls.Add(saveTreeView);
            splitContainer1.Size = new System.Drawing.Size(828, 448);
            splitContainer1.SplitterDistance = 406;
            splitContainer1.TabIndex = 1;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // loadButton
            // 
            loadButton.Anchor = AnchorStyles.Top;
            loadButton.FlatStyle = FlatStyle.Flat;
            loadButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            loadButton.ForeColor = System.Drawing.SystemColors.ControlText;
            loadButton.Location = new System.Drawing.Point(145, 3);
            loadButton.Margin = new Padding(4, 3, 4, 3);
            loadButton.Name = "loadButton";
            loadButton.Size = new System.Drawing.Size(120, 42);
            loadButton.TabIndex = 3;
            loadButton.Text = "Load...";
            loadButton.UseVisualStyleBackColor = true;
            loadButton.Click += loadButton_Click;
            // 
            // loadTreeView
            // 
            loadTreeView.AllowDrop = true;
            loadTreeView.BackColor = System.Drawing.Color.LightGray;
            loadTreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            loadTreeView.Location = new System.Drawing.Point(0, 51);
            loadTreeView.Name = "loadTreeView";
            loadTreeView.Size = new System.Drawing.Size(408, 394);
            loadTreeView.TabIndex = 0;
            loadTreeView.BeforeExpand += treeView_BeforeExpand;
            loadTreeView.NodeMouseClick += loadTreeView_NodeMouseClick;
            loadTreeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            // 
            // saveButton
            // 
            saveButton.Anchor = AnchorStyles.Top;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            saveButton.ForeColor = System.Drawing.SystemColors.ControlText;
            saveButton.Location = new System.Drawing.Point(147, 3);
            saveButton.Margin = new Padding(4, 3, 4, 3);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(120, 42);
            saveButton.TabIndex = 4;
            saveButton.Text = "Save...";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // saveTreeView
            // 
            saveTreeView.AllowDrop = true;
            saveTreeView.BackColor = System.Drawing.Color.LightGray;
            saveTreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            saveTreeView.Location = new System.Drawing.Point(0, 51);
            saveTreeView.Name = "saveTreeView";
            saveTreeView.Size = new System.Drawing.Size(416, 397);
            saveTreeView.TabIndex = 0;
            saveTreeView.BeforeExpand += treeView_BeforeExpand;
            saveTreeView.NodeMouseClick += saveTreeView_NodeMouseClick;
            saveTreeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, utilsToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(6, 6);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new System.Drawing.Size(828, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, stageFromDiskToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            loadToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            loadToolStripMenuItem.Text = "Open";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // stageFromDiskToolStripMenuItem
            // 
            stageFromDiskToolStripMenuItem.Name = "stageFromDiskToolStripMenuItem";
            stageFromDiskToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            stageFromDiskToolStripMenuItem.Text = "Stage from disk...";
            stageFromDiskToolStripMenuItem.Click += stageFromDiskToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, deleteToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            cutToolStripMenuItem.Text = "Cut";
            cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { expandAllToolStripMenuItem, collapseAllToolStripMenuItem, refreshToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // expandAllToolStripMenuItem
            // 
            expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            expandAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.M;
            expandAllToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            expandAllToolStripMenuItem.Text = "Expand All";
            expandAllToolStripMenuItem.Click += expandAllToolStripMenuItem_Click;
            // 
            // collapseAllToolStripMenuItem
            // 
            collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            collapseAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.L;
            collapseAllToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            collapseAllToolStripMenuItem.Text = "Collapse All";
            collapseAllToolStripMenuItem.Click += collapseAllToolStripMenuItem_Click;
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.ShortcutKeys = Keys.F5;
            refreshToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            refreshToolStripMenuItem.Text = "Refresh";
            refreshToolStripMenuItem.Click += refreshToolStripMenuItem_Click;
            // 
            // utilsToolStripMenuItem
            // 
            utilsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { extractAllToolStripMenuItem });
            utilsToolStripMenuItem.Name = "utilsToolStripMenuItem";
            utilsToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            utilsToolStripMenuItem.Text = "Utils";
            // 
            // extractAllToolStripMenuItem
            // 
            extractAllToolStripMenuItem.Name = "extractAllToolStripMenuItem";
            extractAllToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            extractAllToolStripMenuItem.Text = "Extract all...";
            extractAllToolStripMenuItem.Click += extractAllToolStripMenuItem_Click;
            // 
            // extractAllBackgroundWorker
            // 
            extractAllBackgroundWorker.WorkerReportsProgress = true;
            extractAllBackgroundWorker.WorkerSupportsCancellation = true;
            extractAllBackgroundWorker.DoWork += extractAllBackgroundWorker_DoWork;
            extractAllBackgroundWorker.ProgressChanged += extractAllBackgroundWorker_ProgressChanged;
            extractAllBackgroundWorker.RunWorkerCompleted += extractAllBackgroundWorker_RunWorkerCompleted;
            // 
            // FileContainerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(840, 487);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Name = "FileContainerForm";
            Padding = new Padding(6);
            Text = "UAssetGUI";
            Load += FileContainerForm_Load;
            Shown += FileContainerForm_Shown;
            SizeChanged += FileContainerForm_SizeChanged;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        internal ColorfulTreeView saveTreeView;
        internal ColorfulTreeView loadTreeView;
        internal System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        internal System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stageFromDiskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private ToolStripMenuItem refreshToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem utilsToolStripMenuItem;
        private ToolStripMenuItem extractAllToolStripMenuItem;
        internal System.ComponentModel.BackgroundWorker extractAllBackgroundWorker;
    }
}