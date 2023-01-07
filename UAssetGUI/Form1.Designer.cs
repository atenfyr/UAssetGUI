using System.Windows.Forms;

namespace UAssetGUI
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapStructTypeOverridesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recalculateNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mappingsDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.issuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.githubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apiLinkToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboSpecifyVersion = new System.Windows.Forms.ComboBox();
            this.nameMapContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.replaceAllReferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importBinaryData = new System.Windows.Forms.Button();
            this.exportBinaryData = new System.Windows.Forms.Button();
            this.setBinaryData = new System.Windows.Forms.Button();
            this.comboSpecifyMappings = new System.Windows.Forms.ComboBox();
            this.listView1 = new UAssetGUI.ColorfulTreeView();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.nameMapContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.findToolStripMenuItem,
            this.mapStructTypeOverridesToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Enabled = false;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.findToolStripMenuItem.Text = "Find...";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // mapStructTypeOverridesToolStripMenuItem
            // 
            this.mapStructTypeOverridesToolStripMenuItem.Name = "mapStructTypeOverridesToolStripMenuItem";
            this.mapStructTypeOverridesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.mapStructTypeOverridesToolStripMenuItem.Text = "Edit map struct type overrides...";
            this.mapStructTypeOverridesToolStripMenuItem.Click += new System.EventHandler(this.mapStructTypeOverridesToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.settingsToolStripMenuItem.Text = "Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.recalculateNodesToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // recalculateNodesToolStripMenuItem
            // 
            this.recalculateNodesToolStripMenuItem.Name = "recalculateNodesToolStripMenuItem";
            this.recalculateNodesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.recalculateNodesToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.recalculateNodesToolStripMenuItem.Text = "Recalculate Nodes";
            this.recalculateNodesToolStripMenuItem.Click += new System.EventHandler(this.refreshFullToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configDirToolStripMenuItem,
            this.mappingsDirToolStripMenuItem,
            this.issuesToolStripMenuItem,
            this.githubToolStripMenuItem,
            this.apiLinkToolStripMenuItem1});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // configDirToolStripMenuItem
            // 
            this.configDirToolStripMenuItem.Name = "configDirToolStripMenuItem";
            this.configDirToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.configDirToolStripMenuItem.Text = "Open config directory...";
            this.configDirToolStripMenuItem.Click += new System.EventHandler(this.configDirToolStripMenuItem_Click);
            // 
            // mappingsDirToolStripMenuItem
            // 
            this.mappingsDirToolStripMenuItem.Name = "mappingsDirToolStripMenuItem";
            this.mappingsDirToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.mappingsDirToolStripMenuItem.Text = "Open mappings directory...";
            this.mappingsDirToolStripMenuItem.Click += new System.EventHandler(this.mappingsDirToolStripMenuItem_Click);
            // 
            // issuesToolStripMenuItem
            // 
            this.issuesToolStripMenuItem.Name = "issuesToolStripMenuItem";
            this.issuesToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.issuesToolStripMenuItem.Text = "Give feedback";
            this.issuesToolStripMenuItem.Click += new System.EventHandler(this.issuesToolStripMenuItem_Click);
            // 
            // githubToolStripMenuItem
            // 
            this.githubToolStripMenuItem.Name = "githubToolStripMenuItem";
            this.githubToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.githubToolStripMenuItem.Text = "UAssetGUI on GitHub";
            this.githubToolStripMenuItem.Click += new System.EventHandler(this.githubToolStripMenuItem_Click);
            // 
            // apiLinkToolStripMenuItem1
            // 
            this.apiLinkToolStripMenuItem1.Name = "apiLinkToolStripMenuItem1";
            this.apiLinkToolStripMenuItem1.Size = new System.Drawing.Size(218, 22);
            this.apiLinkToolStripMenuItem1.Text = "UAssetAPI on GitHub";
            this.apiLinkToolStripMenuItem1.Click += new System.EventHandler(this.apiLinkToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(381, 27);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.RowHeadersWidth = 60;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.Size = new System.Drawing.Size(419, 411);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridClickCell);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridEditCell);
            this.dataGridView1.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView1_EditingControlShowing);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 200;
            // 
            // comboSpecifyVersion
            // 
            this.comboSpecifyVersion.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboSpecifyVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSpecifyVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboSpecifyVersion.FormattingEnabled = true;
            this.comboSpecifyVersion.Location = new System.Drawing.Point(723, 3);
            this.comboSpecifyVersion.Name = "comboSpecifyVersion";
            this.comboSpecifyVersion.Size = new System.Drawing.Size(77, 21);
            this.comboSpecifyVersion.TabIndex = 3;
            this.comboSpecifyVersion.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboSpecifyVersion_DrawItem);
            this.comboSpecifyVersion.SelectedIndexChanged += new System.EventHandler(this.comboSpecifyVersion_SelectedIndexChanged);
            // 
            // nameMapContext
            // 
            this.nameMapContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceAllReferencesToolStripMenuItem});
            this.nameMapContext.Name = "treeNodeContext";
            this.nameMapContext.Size = new System.Drawing.Size(197, 26);
            // 
            // replaceAllReferencesToolStripMenuItem
            // 
            this.replaceAllReferencesToolStripMenuItem.Name = "replaceAllReferencesToolStripMenuItem";
            this.replaceAllReferencesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.replaceAllReferencesToolStripMenuItem.Text = "Replace all references...";
            this.replaceAllReferencesToolStripMenuItem.Click += new System.EventHandler(this.replaceAllReferencesToolStripMenuItem_Click);
            // 
            // importBinaryData
            // 
            this.importBinaryData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.importBinaryData.Location = new System.Drawing.Point(381, 1);
            this.importBinaryData.Name = "importBinaryData";
            this.importBinaryData.Size = new System.Drawing.Size(75, 23);
            this.importBinaryData.TabIndex = 4;
            this.importBinaryData.Text = "Import";
            this.importBinaryData.UseVisualStyleBackColor = true;
            this.importBinaryData.Click += new System.EventHandler(this.importBinaryData_Click);
            // 
            // exportBinaryData
            // 
            this.exportBinaryData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exportBinaryData.Location = new System.Drawing.Point(462, 1);
            this.exportBinaryData.Name = "exportBinaryData";
            this.exportBinaryData.Size = new System.Drawing.Size(75, 23);
            this.exportBinaryData.TabIndex = 5;
            this.exportBinaryData.Text = "Export";
            this.exportBinaryData.UseVisualStyleBackColor = true;
            this.exportBinaryData.Click += new System.EventHandler(this.exportBinaryData_Click);
            // 
            // setBinaryData
            // 
            this.setBinaryData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setBinaryData.Location = new System.Drawing.Point(543, 1);
            this.setBinaryData.Name = "setBinaryData";
            this.setBinaryData.Size = new System.Drawing.Size(75, 23);
            this.setBinaryData.TabIndex = 6;
            this.setBinaryData.Text = "Set to null...";
            this.setBinaryData.UseVisualStyleBackColor = true;
            this.setBinaryData.Click += new System.EventHandler(this.setBinaryData_Click);
            // 
            // comboSpecifyMappings
            // 
            this.comboSpecifyMappings.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboSpecifyMappings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSpecifyMappings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboSpecifyMappings.FormattingEnabled = true;
            this.comboSpecifyMappings.Location = new System.Drawing.Point(624, 3);
            this.comboSpecifyMappings.Name = "comboSpecifyMappings";
            this.comboSpecifyMappings.Size = new System.Drawing.Size(93, 21);
            this.comboSpecifyMappings.TabIndex = 3;
            this.comboSpecifyMappings.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboSpecifyVersion_DrawItem);
            this.comboSpecifyMappings.SelectedIndexChanged += new System.EventHandler(this.comboSpecifyMappings_SelectedIndexChanged);
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            this.listView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 27);
            this.listView1.Name = "listView1";
            this.listView1.ShowLines = false;
            this.listView1.ShowNodeToolTips = true;
            this.listView1.Size = new System.Drawing.Size(344, 411);
            this.listView1.TabIndex = 1;
            this.listView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.listView1_BeforeExpand);
            this.listView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.comboSpecifyMappings);
            this.Controls.Add(this.setBinaryData);
            this.Controls.Add(this.exportBinaryData);
            this.Controls.Add(this.importBinaryData);
            this.Controls.Add(this.comboSpecifyVersion);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "UAssetGUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.nameMapContext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private ColumnHeader columnHeader1;
        private ToolStripMenuItem refreshToolStripMenuItem;
        private ToolStripMenuItem recalculateNodesToolStripMenuItem;
        private ToolStripMenuItem apiLinkToolStripMenuItem1;
        private ToolStripMenuItem githubToolStripMenuItem;
        private ToolStripMenuItem replaceAllReferencesToolStripMenuItem;
        public ContextMenuStrip nameMapContext;
        private ToolStripMenuItem issuesToolStripMenuItem;
        private ToolStripMenuItem configDirToolStripMenuItem;
        private ToolStripMenuItem mappingsDirToolStripMenuItem;
        private ToolStripMenuItem mapStructTypeOverridesToolStripMenuItem;
        public ComboBox comboSpecifyVersion;
        private ToolStripMenuItem settingsToolStripMenuItem;
        public Button importBinaryData;
        public Button exportBinaryData;
        public Button setBinaryData;
        internal ToolStripMenuItem saveToolStripMenuItem;
        public ComboBox comboSpecifyMappings;
    }
}

