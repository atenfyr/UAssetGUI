namespace UAssetGUI
{
    partial class SettingsForm
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
            this.infoLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.aboutButton = new System.Windows.Forms.Button();
            this.themeComboBox = new System.Windows.Forms.ComboBox();
            this.favoriteThingBox = new System.Windows.Forms.TextBox();
            this.valuesOnScroll = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.enableDiscordRpc = new System.Windows.Forms.CheckBox();
            this.enableDynamicTree = new System.Windows.Forms.CheckBox();
            this.doubleClickToEdit = new System.Windows.Forms.CheckBox();
            this.enableBak = new System.Windows.Forms.CheckBox();
            this.restoreSize = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.infoLabel.Location = new System.Drawing.Point(13, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(257, 40);
            this.infoLabel.TabIndex = 1;
            this.infoLabel.Text = "Settings:";
            this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.closeButton.Location = new System.Drawing.Point(195, 291);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 26);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.label1.Location = new System.Drawing.Point(56, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Theme:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.label2.Location = new System.Drawing.Point(13, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Favorite Thing:";
            // 
            // aboutButton
            // 
            this.aboutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.aboutButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.aboutButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.aboutButton.Location = new System.Drawing.Point(12, 291);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(75, 26);
            this.aboutButton.TabIndex = 7;
            this.aboutButton.Text = "About...";
            this.aboutButton.UseVisualStyleBackColor = true;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // themeComboBox
            // 
            this.themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeComboBox.FormattingEnabled = true;
            this.themeComboBox.Location = new System.Drawing.Point(116, 61);
            this.themeComboBox.Name = "themeComboBox";
            this.themeComboBox.Size = new System.Drawing.Size(121, 21);
            this.themeComboBox.TabIndex = 8;
            this.themeComboBox.SelectedIndexChanged += new System.EventHandler(this.themeComboBox_SelectedIndexChanged);
            // 
            // favoriteThingBox
            // 
            this.favoriteThingBox.Location = new System.Drawing.Point(116, 87);
            this.favoriteThingBox.Name = "favoriteThingBox";
            this.favoriteThingBox.Size = new System.Drawing.Size(121, 20);
            this.favoriteThingBox.TabIndex = 9;
            this.favoriteThingBox.TextChanged += new System.EventHandler(this.favoriteThingBox_TextChanged);
            // 
            // valuesOnScroll
            // 
            this.valuesOnScroll.AutoSize = true;
            this.valuesOnScroll.Location = new System.Drawing.Point(116, 139);
            this.valuesOnScroll.Name = "valuesOnScroll";
            this.valuesOnScroll.Size = new System.Drawing.Size(139, 17);
            this.valuesOnScroll.TabIndex = 10;
            this.valuesOnScroll.Text = "Change values on scroll";
            this.valuesOnScroll.UseVisualStyleBackColor = true;
            this.valuesOnScroll.CheckedChanged += new System.EventHandler(this.valuesOnScroll_CheckedChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(116, 113);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 11;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.label3.Location = new System.Drawing.Point(64, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Zoom:";
            // 
            // enableDiscordRpc
            // 
            this.enableDiscordRpc.AutoSize = true;
            this.enableDiscordRpc.Location = new System.Drawing.Point(116, 208);
            this.enableDiscordRpc.Name = "enableDiscordRpc";
            this.enableDiscordRpc.Size = new System.Drawing.Size(123, 17);
            this.enableDiscordRpc.TabIndex = 13;
            this.enableDiscordRpc.Text = "Enable Discord RPC";
            this.enableDiscordRpc.UseVisualStyleBackColor = true;
            this.enableDiscordRpc.CheckedChanged += new System.EventHandler(this.enableDiscordRpc_CheckedChanged);
            // 
            // enableDynamicTree
            // 
            this.enableDynamicTree.AutoSize = true;
            this.enableDynamicTree.Location = new System.Drawing.Point(116, 231);
            this.enableDynamicTree.Name = "enableDynamicTree";
            this.enableDynamicTree.Size = new System.Drawing.Size(122, 17);
            this.enableDynamicTree.TabIndex = 14;
            this.enableDynamicTree.Text = "Enable dynamic tree";
            this.enableDynamicTree.UseVisualStyleBackColor = true;
            this.enableDynamicTree.CheckedChanged += new System.EventHandler(this.enableDynamicTree_CheckedChanged);
            // 
            // doubleClickToEdit
            // 
            this.doubleClickToEdit.AutoSize = true;
            this.doubleClickToEdit.Location = new System.Drawing.Point(116, 162);
            this.doubleClickToEdit.Name = "doubleClickToEdit";
            this.doubleClickToEdit.Size = new System.Drawing.Size(117, 17);
            this.doubleClickToEdit.TabIndex = 15;
            this.doubleClickToEdit.Text = "Double click to edit";
            this.doubleClickToEdit.UseVisualStyleBackColor = true;
            this.doubleClickToEdit.CheckedChanged += new System.EventHandler(this.doubleClickToEdit_CheckedChanged);
            // 
            // enableBak
            // 
            this.enableBak.AutoSize = true;
            this.enableBak.Location = new System.Drawing.Point(116, 185);
            this.enableBak.Name = "enableBak";
            this.enableBak.Size = new System.Drawing.Size(104, 17);
            this.enableBak.TabIndex = 16;
            this.enableBak.Text = "Enable .bak files";
            this.enableBak.UseVisualStyleBackColor = true;
            this.enableBak.CheckedChanged += new System.EventHandler(this.enableBak_CheckedChanged);
            // 
            // restoreSize
            // 
            this.restoreSize.AutoSize = true;
            this.restoreSize.Location = new System.Drawing.Point(116, 254);
            this.restoreSize.Name = "restoreSize";
            this.restoreSize.Size = new System.Drawing.Size(148, 17);
            this.restoreSize.TabIndex = 17;
            this.restoreSize.Text = "Restore GUI size on open";
            this.restoreSize.UseVisualStyleBackColor = true;
            this.restoreSize.CheckedChanged += new System.EventHandler(this.restoreSize_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 323);
            this.Controls.Add(this.restoreSize);
            this.Controls.Add(this.enableBak);
            this.Controls.Add(this.doubleClickToEdit);
            this.Controls.Add(this.enableDynamicTree);
            this.Controls.Add(this.enableDiscordRpc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.valuesOnScroll);
            this.Controls.Add(this.favoriteThingBox);
            this.Controls.Add(this.themeComboBox);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.infoLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.ComboBox themeComboBox;
        private System.Windows.Forms.TextBox favoriteThingBox;
        private System.Windows.Forms.CheckBox valuesOnScroll;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox enableDiscordRpc;
        private System.Windows.Forms.CheckBox enableDynamicTree;
        private System.Windows.Forms.CheckBox doubleClickToEdit;
        private System.Windows.Forms.CheckBox enableBak;
        private System.Windows.Forms.CheckBox restoreSize;
    }
}