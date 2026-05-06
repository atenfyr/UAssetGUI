namespace UAssetGUI
{
    partial class MapStructTypeOverrideForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            mstoDataGridView = new System.Windows.Forms.DataGridView();
            infoLabel = new System.Windows.Forms.Label();
            refreshButton = new System.Windows.Forms.Button();
            closeButton = new System.Windows.Forms.Button();
            exportButton = new System.Windows.Forms.Button();
            importButton = new System.Windows.Forms.Button();
            resetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)mstoDataGridView).BeginInit();
            SuspendLayout();
            // 
            // mstoDataGridView
            // 
            mstoDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mstoDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            mstoDataGridView.BackgroundColor = System.Drawing.Color.LightGray;
            mstoDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            mstoDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            mstoDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            mstoDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            mstoDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mstoDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            mstoDataGridView.EnableHeadersVisualStyles = false;
            mstoDataGridView.Location = new System.Drawing.Point(14, 60);
            mstoDataGridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mstoDataGridView.Name = "mstoDataGridView";
            mstoDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            mstoDataGridView.RowHeadersWidth = 60;
            mstoDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            mstoDataGridView.Size = new System.Drawing.Size(636, 333);
            mstoDataGridView.TabIndex = 1;
            mstoDataGridView.CellEndEdit += mstoDataGridView_CellEndEdit;
            // 
            // infoLabel
            // 
            infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            infoLabel.Location = new System.Drawing.Point(15, 10);
            infoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new System.Drawing.Size(635, 46);
            infoLabel.TabIndex = 0;
            infoLabel.Text = "blah blah blah";
            infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            refreshButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            refreshButton.Location = new System.Drawing.Point(14, 400);
            refreshButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new System.Drawing.Size(116, 30);
            refreshButton.TabIndex = 2;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            closeButton.Location = new System.Drawing.Point(550, 400);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(100, 30);
            closeButton.TabIndex = 3;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // exportButton
            // 
            exportButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            exportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            exportButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            exportButton.Location = new System.Drawing.Point(257, 400);
            exportButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            exportButton.Name = "exportButton";
            exportButton.Size = new System.Drawing.Size(109, 30);
            exportButton.TabIndex = 4;
            exportButton.Text = "Export";
            exportButton.UseVisualStyleBackColor = true;
            exportButton.Click += exportButton_Click;
            // 
            // importButton
            // 
            importButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            importButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            importButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            importButton.Location = new System.Drawing.Point(138, 400);
            importButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            importButton.Name = "importButton";
            importButton.Size = new System.Drawing.Size(111, 30);
            importButton.TabIndex = 5;
            importButton.Text = "Import";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += importButton_Click;
            // 
            // resetButton
            // 
            resetButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            resetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            resetButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            resetButton.Location = new System.Drawing.Point(438, 400);
            resetButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(104, 30);
            resetButton.TabIndex = 6;
            resetButton.Text = "Reset";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += resetButton_Click;
            // 
            // MapStructTypeOverrideForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(664, 437);
            Controls.Add(resetButton);
            Controls.Add(importButton);
            Controls.Add(exportButton);
            Controls.Add(closeButton);
            Controls.Add(refreshButton);
            Controls.Add(infoLabel);
            Controls.Add(mstoDataGridView);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "MapStructTypeOverrideForm";
            Text = "Map Struct Type Overrides";
            Load += MapStructTypeOverrideForm_Load;
            Resize += MapStructTypeOverrideForm_Resize;
            ((System.ComponentModel.ISupportInitialize)mstoDataGridView).EndInit();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label infoLabel;
        internal System.Windows.Forms.DataGridView mstoDataGridView;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button resetButton;
    }
}