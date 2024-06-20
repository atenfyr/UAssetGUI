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
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            loadButton = new System.Windows.Forms.Button();
            loadTreeView = new ColorfulTreeView();
            saveButton = new System.Windows.Forms.Button();
            saveTreeView = new ColorfulTreeView();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            splitContainer1.Location = new System.Drawing.Point(9, 45);
            splitContainer1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
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
            splitContainer1.Size = new System.Drawing.Size(1182, 747);
            splitContainer1.SplitterDistance = 582;
            splitContainer1.SplitterWidth = 6;
            splitContainer1.TabIndex = 1;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // loadButton
            // 
            loadButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            loadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            loadButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            loadButton.ForeColor = System.Drawing.SystemColors.ControlText;
            loadButton.Location = new System.Drawing.Point(207, 5);
            loadButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            loadButton.Name = "loadButton";
            loadButton.Size = new System.Drawing.Size(171, 70);
            loadButton.TabIndex = 3;
            loadButton.Text = "Load...";
            loadButton.UseVisualStyleBackColor = true;
            loadButton.Click += loadButton_Click;
            // 
            // loadTreeView
            // 
            loadTreeView.BackColor = System.Drawing.Color.LightGray;
            loadTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            loadTreeView.Location = new System.Drawing.Point(0, 85);
            loadTreeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            loadTreeView.Name = "loadTreeView";
            loadTreeView.Size = new System.Drawing.Size(581, 704);
            loadTreeView.TabIndex = 0;
            // 
            // saveButton
            // 
            saveButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            saveButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            saveButton.ForeColor = System.Drawing.SystemColors.ControlText;
            saveButton.Location = new System.Drawing.Point(209, 5);
            saveButton.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(171, 70);
            saveButton.TabIndex = 4;
            saveButton.Text = "Save...";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // saveTreeView
            // 
            saveTreeView.BackColor = System.Drawing.Color.LightGray;
            saveTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            saveTreeView.Location = new System.Drawing.Point(0, 85);
            saveTreeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            saveTreeView.Name = "saveTreeView";
            saveTreeView.Size = new System.Drawing.Size(593, 704);
            saveTreeView.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editToolStripMenuItem, viewToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(9, 10);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            menuStrip1.Size = new System.Drawing.Size(1182, 35);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(58, 29);
            editToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            viewToolStripMenuItem.Text = "View";
            // 
            // FileContainerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 812);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "FileContainerForm";
            Padding = new System.Windows.Forms.Padding(9, 10, 9, 10);
            Text = "FileContainerForm";
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
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        internal System.Windows.Forms.MenuStrip menuStrip1;
    }
}