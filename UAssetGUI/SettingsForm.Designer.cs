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
            infoLabel = new System.Windows.Forms.Label();
            closeButton = new System.Windows.Forms.Button();
            themeLabel = new System.Windows.Forms.Label();
            favoriteThingLabel = new System.Windows.Forms.Label();
            aboutButton = new System.Windows.Forms.Button();
            themeComboBox = new System.Windows.Forms.ComboBox();
            favoriteThingBox = new System.Windows.Forms.TextBox();
            valuesOnScroll = new System.Windows.Forms.CheckBox();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            zoomLabel = new System.Windows.Forms.Label();
            enableDiscordRpc = new System.Windows.Forms.CheckBox();
            enableDynamicTree = new System.Windows.Forms.CheckBox();
            doubleClickToEdit = new System.Windows.Forms.CheckBox();
            enableBak = new System.Windows.Forms.CheckBox();
            restoreSize = new System.Windows.Forms.CheckBox();
            enableUpdateNotice = new System.Windows.Forms.CheckBox();
            enablePrettyBytecode = new System.Windows.Forms.CheckBox();
            flagsLabel = new System.Windows.Forms.Label();
            customSerializationFlagsBox = new System.Windows.Forms.CheckedListBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            enableBakJson = new System.Windows.Forms.CheckBox();
            allowUntrustedScriptsBox = new System.Windows.Forms.CheckBox();
            gameOverrideBox = new System.Windows.Forms.ComboBox();
            gameOverrideLabel = new System.Windows.Forms.Label();
            languageComboBox = new System.Windows.Forms.ComboBox();
            languageLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // infoLabel
            // 
            infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            infoLabel.Location = new System.Drawing.Point(15, 10);
            infoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new System.Drawing.Size(441, 46);
            infoLabel.TabIndex = 1;
            infoLabel.Text = "Settings:";
            infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            closeButton.Location = new System.Drawing.Point(347, 409);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(109, 30);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // themeLabel
            // 
            themeLabel.AutoSize = true;
            themeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            themeLabel.Location = new System.Drawing.Point(97, 72);
            themeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            themeLabel.Name = "themeLabel";
            themeLabel.Size = new System.Drawing.Size(53, 16);
            themeLabel.TabIndex = 15;
            themeLabel.Text = "Theme:";
            // 
            // favoriteThingLabel
            // 
            favoriteThingLabel.AutoSize = true;
            favoriteThingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            favoriteThingLabel.Location = new System.Drawing.Point(49, 130);
            favoriteThingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            favoriteThingLabel.Name = "favoriteThingLabel";
            favoriteThingLabel.Size = new System.Drawing.Size(96, 16);
            favoriteThingLabel.TabIndex = 16;
            favoriteThingLabel.Text = "Favorite Thing:";
            // 
            // aboutButton
            // 
            aboutButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            aboutButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            aboutButton.ForeColor = System.Drawing.SystemColors.ControlText;
            aboutButton.Location = new System.Drawing.Point(14, 409);
            aboutButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            aboutButton.Name = "aboutButton";
            aboutButton.Size = new System.Drawing.Size(110, 30);
            aboutButton.TabIndex = 7;
            aboutButton.Text = "About...";
            aboutButton.UseVisualStyleBackColor = true;
            aboutButton.Click += aboutButton_Click;
            // 
            // themeComboBox
            // 
            themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            themeComboBox.FormattingEnabled = true;
            themeComboBox.Location = new System.Drawing.Point(169, 69);
            themeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            themeComboBox.Name = "themeComboBox";
            themeComboBox.Size = new System.Drawing.Size(252, 23);
            themeComboBox.TabIndex = 2;
            themeComboBox.SelectedIndexChanged += themeComboBox_SelectedIndexChanged;
            // 
            // favoriteThingBox
            // 
            favoriteThingBox.Location = new System.Drawing.Point(170, 126);
            favoriteThingBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            favoriteThingBox.Name = "favoriteThingBox";
            favoriteThingBox.Size = new System.Drawing.Size(251, 23);
            favoriteThingBox.TabIndex = 3;
            favoriteThingBox.TextChanged += favoriteThingBox_TextChanged;
            // 
            // valuesOnScroll
            // 
            valuesOnScroll.AutoSize = true;
            valuesOnScroll.Location = new System.Drawing.Point(253, 256);
            valuesOnScroll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            valuesOnScroll.Name = "valuesOnScroll";
            valuesOnScroll.Size = new System.Drawing.Size(151, 19);
            valuesOnScroll.TabIndex = 8;
            valuesOnScroll.Text = "Change values on scroll";
            valuesOnScroll.UseVisualStyleBackColor = true;
            valuesOnScroll.CheckedChanged += valuesOnScroll_CheckedChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(170, 154);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(251, 23);
            numericUpDown1.TabIndex = 4;
            numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // zoomLabel
            // 
            zoomLabel.AutoSize = true;
            zoomLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            zoomLabel.Location = new System.Drawing.Point(110, 157);
            zoomLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            zoomLabel.Name = "zoomLabel";
            zoomLabel.Size = new System.Drawing.Size(45, 16);
            zoomLabel.TabIndex = 17;
            zoomLabel.Text = "Zoom:";
            // 
            // enableDiscordRpc
            // 
            enableDiscordRpc.AutoSize = true;
            enableDiscordRpc.Location = new System.Drawing.Point(29, 256);
            enableDiscordRpc.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableDiscordRpc.Name = "enableDiscordRpc";
            enableDiscordRpc.Size = new System.Drawing.Size(129, 19);
            enableDiscordRpc.TabIndex = 7;
            enableDiscordRpc.Text = "Enable Discord RPC";
            enableDiscordRpc.UseVisualStyleBackColor = true;
            enableDiscordRpc.CheckedChanged += enableDiscordRpc_CheckedChanged;
            // 
            // enableDynamicTree
            // 
            enableDynamicTree.AutoSize = true;
            enableDynamicTree.Location = new System.Drawing.Point(29, 309);
            enableDynamicTree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableDynamicTree.Name = "enableDynamicTree";
            enableDynamicTree.Size = new System.Drawing.Size(133, 19);
            enableDynamicTree.TabIndex = 11;
            enableDynamicTree.Text = "Enable dynamic tree";
            enableDynamicTree.UseVisualStyleBackColor = true;
            enableDynamicTree.CheckedChanged += enableDynamicTree_CheckedChanged;
            // 
            // doubleClickToEdit
            // 
            doubleClickToEdit.AutoSize = true;
            doubleClickToEdit.Location = new System.Drawing.Point(29, 283);
            doubleClickToEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            doubleClickToEdit.Name = "doubleClickToEdit";
            doubleClickToEdit.Size = new System.Drawing.Size(128, 19);
            doubleClickToEdit.TabIndex = 9;
            doubleClickToEdit.Text = "Double click to edit";
            doubleClickToEdit.UseVisualStyleBackColor = true;
            doubleClickToEdit.CheckedChanged += doubleClickToEdit_CheckedChanged;
            // 
            // enableBak
            // 
            enableBak.AutoSize = true;
            enableBak.Location = new System.Drawing.Point(253, 336);
            enableBak.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableBak.Name = "enableBak";
            enableBak.Size = new System.Drawing.Size(157, 19);
            enableBak.TabIndex = 14;
            enableBak.Text = "Enable .bak files (.uasset)";
            enableBak.UseVisualStyleBackColor = true;
            enableBak.CheckedChanged += enableBak_CheckedChanged;
            // 
            // restoreSize
            // 
            restoreSize.AutoSize = true;
            restoreSize.Location = new System.Drawing.Point(253, 309);
            restoreSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            restoreSize.Name = "restoreSize";
            restoreSize.Size = new System.Drawing.Size(156, 19);
            restoreSize.TabIndex = 12;
            restoreSize.Text = "Restore GUI size on open";
            restoreSize.UseVisualStyleBackColor = true;
            restoreSize.CheckedChanged += restoreSize_CheckedChanged;
            // 
            // enableUpdateNotice
            // 
            enableUpdateNotice.AutoSize = true;
            enableUpdateNotice.Location = new System.Drawing.Point(29, 336);
            enableUpdateNotice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableUpdateNotice.Name = "enableUpdateNotice";
            enableUpdateNotice.Size = new System.Drawing.Size(137, 19);
            enableUpdateNotice.TabIndex = 13;
            enableUpdateNotice.Text = "Enable update notice";
            enableUpdateNotice.UseVisualStyleBackColor = true;
            enableUpdateNotice.CheckedChanged += enableUpdateNotice_CheckedChanged;
            // 
            // enablePrettyBytecode
            // 
            enablePrettyBytecode.AutoSize = true;
            enablePrettyBytecode.Location = new System.Drawing.Point(253, 283);
            enablePrettyBytecode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enablePrettyBytecode.Name = "enablePrettyBytecode";
            enablePrettyBytecode.Size = new System.Drawing.Size(147, 19);
            enablePrettyBytecode.TabIndex = 10;
            enablePrettyBytecode.Text = "Enable pretty bytecode";
            enablePrettyBytecode.UseVisualStyleBackColor = true;
            enablePrettyBytecode.CheckedChanged += enablePrettyBytecode_CheckedChanged;
            // 
            // flagsLabel
            // 
            flagsLabel.AutoSize = true;
            flagsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            flagsLabel.Location = new System.Drawing.Point(110, 188);
            flagsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            flagsLabel.Name = "flagsLabel";
            flagsLabel.Size = new System.Drawing.Size(44, 16);
            flagsLabel.TabIndex = 18;
            flagsLabel.Text = "Flags:";
            // 
            // customSerializationFlagsBox
            // 
            customSerializationFlagsBox.CheckOnClick = true;
            customSerializationFlagsBox.FormattingEnabled = true;
            customSerializationFlagsBox.Location = new System.Drawing.Point(170, 185);
            customSerializationFlagsBox.Name = "customSerializationFlagsBox";
            customSerializationFlagsBox.ScrollAlwaysVisible = true;
            customSerializationFlagsBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            customSerializationFlagsBox.Size = new System.Drawing.Size(252, 22);
            customSerializationFlagsBox.TabIndex = 5;
            customSerializationFlagsBox.Click += customSerializationFlagsBox_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new System.Drawing.Point(6, 10);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(61, 78);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 23;
            pictureBox1.TabStop = false;
            pictureBox1.Visible = false;
            // 
            // enableBakJson
            // 
            enableBakJson.AutoSize = true;
            enableBakJson.Location = new System.Drawing.Point(29, 363);
            enableBakJson.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableBakJson.Name = "enableBakJson";
            enableBakJson.Size = new System.Drawing.Size(146, 19);
            enableBakJson.TabIndex = 15;
            enableBakJson.Text = "Enable .bak files (.json)";
            enableBakJson.UseVisualStyleBackColor = true;
            enableBakJson.CheckedChanged += enableBakJson_CheckedChanged;
            // 
            // allowUntrustedScriptsBox
            // 
            allowUntrustedScriptsBox.AutoSize = true;
            allowUntrustedScriptsBox.Location = new System.Drawing.Point(253, 363);
            allowUntrustedScriptsBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            allowUntrustedScriptsBox.Name = "allowUntrustedScriptsBox";
            allowUntrustedScriptsBox.Size = new System.Drawing.Size(147, 19);
            allowUntrustedScriptsBox.TabIndex = 16;
            allowUntrustedScriptsBox.Text = "Allow untrusted scripts";
            allowUntrustedScriptsBox.UseVisualStyleBackColor = true;
            allowUntrustedScriptsBox.CheckedChanged += allowUntrustedScriptsBox_CheckedChanged;
            // 
            // gameOverrideBox
            // 
            gameOverrideBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            gameOverrideBox.FormattingEnabled = true;
            gameOverrideBox.Location = new System.Drawing.Point(170, 213);
            gameOverrideBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gameOverrideBox.Name = "gameOverrideBox";
            gameOverrideBox.Size = new System.Drawing.Size(252, 23);
            gameOverrideBox.TabIndex = 6;
            gameOverrideBox.SelectedIndexChanged += gameOverrideBox_SelectedIndexChanged;
            // 
            // gameOverrideLabel
            // 
            gameOverrideLabel.AutoSize = true;
            gameOverrideLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            gameOverrideLabel.Location = new System.Drawing.Point(43, 217);
            gameOverrideLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            gameOverrideLabel.Name = "gameOverrideLabel";
            gameOverrideLabel.Size = new System.Drawing.Size(102, 16);
            gameOverrideLabel.TabIndex = 26;
            gameOverrideLabel.Text = "Game Override:";
            // 
            // languageComboBox
            // 
            languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Location = new System.Drawing.Point(170, 97);
            languageComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new System.Drawing.Size(251, 23);
            languageComboBox.TabIndex = 27;
            languageComboBox.SelectedIndexChanged += languageComboBox_SelectedIndexChanged;
            // 
            // languageLabel
            // 
            languageLabel.AutoSize = true;
            languageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            languageLabel.Location = new System.Drawing.Point(81, 100);
            languageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            languageLabel.Name = "languageLabel";
            languageLabel.Size = new System.Drawing.Size(71, 16);
            languageLabel.TabIndex = 28;
            languageLabel.Text = "Language:";
            languageLabel.MouseDoubleClick += languageLabel_MouseDoubleClick;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(470, 447);
            Controls.Add(languageComboBox);
            Controls.Add(languageLabel);
            Controls.Add(gameOverrideBox);
            Controls.Add(gameOverrideLabel);
            Controls.Add(allowUntrustedScriptsBox);
            Controls.Add(enableBakJson);
            Controls.Add(pictureBox1);
            Controls.Add(customSerializationFlagsBox);
            Controls.Add(flagsLabel);
            Controls.Add(enablePrettyBytecode);
            Controls.Add(enableUpdateNotice);
            Controls.Add(restoreSize);
            Controls.Add(enableBak);
            Controls.Add(doubleClickToEdit);
            Controls.Add(enableDynamicTree);
            Controls.Add(enableDiscordRpc);
            Controls.Add(zoomLabel);
            Controls.Add(numericUpDown1);
            Controls.Add(valuesOnScroll);
            Controls.Add(favoriteThingBox);
            Controls.Add(themeComboBox);
            Controls.Add(aboutButton);
            Controls.Add(favoriteThingLabel);
            Controls.Add(themeLabel);
            Controls.Add(closeButton);
            Controls.Add(infoLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SettingsForm";
            Text = "Settings";
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label themeLabel;
        private System.Windows.Forms.Label favoriteThingLabel;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.ComboBox themeComboBox;
        private System.Windows.Forms.CheckBox valuesOnScroll;
        private System.Windows.Forms.CheckBox enableDiscordRpc;
        private System.Windows.Forms.CheckBox enableDynamicTree;
        private System.Windows.Forms.CheckBox doubleClickToEdit;
        private System.Windows.Forms.CheckBox enableBak;
        private System.Windows.Forms.CheckBox restoreSize;
        private System.Windows.Forms.CheckBox enableUpdateNotice;
        private System.Windows.Forms.CheckBox enablePrettyBytecode;
        internal System.Windows.Forms.NumericUpDown numericUpDown1;
        internal System.Windows.Forms.Label zoomLabel;
        internal System.Windows.Forms.TextBox favoriteThingBox;
        internal System.Windows.Forms.CheckedListBox customSerializationFlagsBox;
        internal System.Windows.Forms.Label flagsLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox enableBakJson;
        private System.Windows.Forms.CheckBox allowUntrustedScriptsBox;
        private System.Windows.Forms.ComboBox gameOverrideBox;
        private System.Windows.Forms.Label gameOverrideLabel;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Label languageLabel;
    }
}