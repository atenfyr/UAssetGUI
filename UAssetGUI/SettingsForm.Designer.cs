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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            aboutButton = new System.Windows.Forms.Button();
            themeComboBox = new System.Windows.Forms.ComboBox();
            favoriteThingBox = new System.Windows.Forms.TextBox();
            valuesOnScroll = new System.Windows.Forms.CheckBox();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            enableDiscordRpc = new System.Windows.Forms.CheckBox();
            enableDynamicTree = new System.Windows.Forms.CheckBox();
            doubleClickToEdit = new System.Windows.Forms.CheckBox();
            enableBak = new System.Windows.Forms.CheckBox();
            restoreSize = new System.Windows.Forms.CheckBox();
            enableUpdateNotice = new System.Windows.Forms.CheckBox();
            enablePrettyBytecode = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            customSerializationFlagsBox = new System.Windows.Forms.CheckedListBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            enableBakJson = new System.Windows.Forms.CheckBox();
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
            infoLabel.Size = new System.Drawing.Size(392, 46);
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
            closeButton.Location = new System.Drawing.Point(319, 357);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 30);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            label1.Location = new System.Drawing.Point(72, 72);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(53, 16);
            label1.TabIndex = 15;
            label1.Text = "Theme:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            label2.Location = new System.Drawing.Point(22, 102);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(96, 16);
            label2.TabIndex = 16;
            label2.Text = "Favorite Thing:";
            // 
            // aboutButton
            // 
            aboutButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            aboutButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            aboutButton.ForeColor = System.Drawing.SystemColors.ControlText;
            aboutButton.Location = new System.Drawing.Point(14, 357);
            aboutButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            aboutButton.Name = "aboutButton";
            aboutButton.Size = new System.Drawing.Size(88, 30);
            aboutButton.TabIndex = 7;
            aboutButton.Text = "About...";
            aboutButton.UseVisualStyleBackColor = true;
            aboutButton.Click += aboutButton_Click;
            // 
            // themeComboBox
            // 
            themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            themeComboBox.FormattingEnabled = true;
            themeComboBox.Location = new System.Drawing.Point(142, 70);
            themeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            themeComboBox.Name = "themeComboBox";
            themeComboBox.Size = new System.Drawing.Size(252, 23);
            themeComboBox.TabIndex = 2;
            themeComboBox.SelectedIndexChanged += themeComboBox_SelectedIndexChanged;
            // 
            // favoriteThingBox
            // 
            favoriteThingBox.Location = new System.Drawing.Point(142, 100);
            favoriteThingBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            favoriteThingBox.Name = "favoriteThingBox";
            favoriteThingBox.Size = new System.Drawing.Size(252, 23);
            favoriteThingBox.TabIndex = 3;
            favoriteThingBox.TextChanged += favoriteThingBox_TextChanged;
            // 
            // valuesOnScroll
            // 
            valuesOnScroll.AutoSize = true;
            valuesOnScroll.Location = new System.Drawing.Point(215, 198);
            valuesOnScroll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            valuesOnScroll.Name = "valuesOnScroll";
            valuesOnScroll.Size = new System.Drawing.Size(151, 19);
            valuesOnScroll.TabIndex = 7;
            valuesOnScroll.Text = "Change values on scroll";
            valuesOnScroll.UseVisualStyleBackColor = true;
            valuesOnScroll.CheckedChanged += valuesOnScroll_CheckedChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(142, 130);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(252, 23);
            numericUpDown1.TabIndex = 4;
            numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            label3.Location = new System.Drawing.Point(82, 130);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(45, 16);
            label3.TabIndex = 17;
            label3.Text = "Zoom:";
            // 
            // enableDiscordRpc
            // 
            enableDiscordRpc.AutoSize = true;
            enableDiscordRpc.Location = new System.Drawing.Point(29, 198);
            enableDiscordRpc.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableDiscordRpc.Name = "enableDiscordRpc";
            enableDiscordRpc.Size = new System.Drawing.Size(129, 19);
            enableDiscordRpc.TabIndex = 6;
            enableDiscordRpc.Text = "Enable Discord RPC";
            enableDiscordRpc.UseVisualStyleBackColor = true;
            enableDiscordRpc.CheckedChanged += enableDiscordRpc_CheckedChanged;
            // 
            // enableDynamicTree
            // 
            enableDynamicTree.AutoSize = true;
            enableDynamicTree.Location = new System.Drawing.Point(29, 251);
            enableDynamicTree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableDynamicTree.Name = "enableDynamicTree";
            enableDynamicTree.Size = new System.Drawing.Size(133, 19);
            enableDynamicTree.TabIndex = 10;
            enableDynamicTree.Text = "Enable dynamic tree";
            enableDynamicTree.UseVisualStyleBackColor = true;
            enableDynamicTree.CheckedChanged += enableDynamicTree_CheckedChanged;
            // 
            // doubleClickToEdit
            // 
            doubleClickToEdit.AutoSize = true;
            doubleClickToEdit.Location = new System.Drawing.Point(29, 225);
            doubleClickToEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            doubleClickToEdit.Name = "doubleClickToEdit";
            doubleClickToEdit.Size = new System.Drawing.Size(128, 19);
            doubleClickToEdit.TabIndex = 8;
            doubleClickToEdit.Text = "Double click to edit";
            doubleClickToEdit.UseVisualStyleBackColor = true;
            doubleClickToEdit.CheckedChanged += doubleClickToEdit_CheckedChanged;
            // 
            // enableBak
            // 
            enableBak.AutoSize = true;
            enableBak.Location = new System.Drawing.Point(215, 278);
            enableBak.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableBak.Name = "enableBak";
            enableBak.Size = new System.Drawing.Size(157, 19);
            enableBak.TabIndex = 13;
            enableBak.Text = "Enable .bak files (.uasset)";
            enableBak.UseVisualStyleBackColor = true;
            enableBak.CheckedChanged += enableBak_CheckedChanged;
            // 
            // restoreSize
            // 
            restoreSize.AutoSize = true;
            restoreSize.Location = new System.Drawing.Point(215, 251);
            restoreSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            restoreSize.Name = "restoreSize";
            restoreSize.Size = new System.Drawing.Size(156, 19);
            restoreSize.TabIndex = 11;
            restoreSize.Text = "Restore GUI size on open";
            restoreSize.UseVisualStyleBackColor = true;
            restoreSize.CheckedChanged += restoreSize_CheckedChanged;
            // 
            // enableUpdateNotice
            // 
            enableUpdateNotice.AutoSize = true;
            enableUpdateNotice.Location = new System.Drawing.Point(29, 278);
            enableUpdateNotice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableUpdateNotice.Name = "enableUpdateNotice";
            enableUpdateNotice.Size = new System.Drawing.Size(137, 19);
            enableUpdateNotice.TabIndex = 12;
            enableUpdateNotice.Text = "Enable update notice";
            enableUpdateNotice.UseVisualStyleBackColor = true;
            enableUpdateNotice.CheckedChanged += enableUpdateNotice_CheckedChanged;
            // 
            // enablePrettyBytecode
            // 
            enablePrettyBytecode.AutoSize = true;
            enablePrettyBytecode.Location = new System.Drawing.Point(215, 225);
            enablePrettyBytecode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enablePrettyBytecode.Name = "enablePrettyBytecode";
            enablePrettyBytecode.Size = new System.Drawing.Size(147, 19);
            enablePrettyBytecode.TabIndex = 9;
            enablePrettyBytecode.Text = "Enable pretty bytecode";
            enablePrettyBytecode.UseVisualStyleBackColor = true;
            enablePrettyBytecode.CheckedChanged += enablePrettyBytecode_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            label4.Location = new System.Drawing.Point(82, 161);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(44, 16);
            label4.TabIndex = 18;
            label4.Text = "Flags:";
            // 
            // customSerializationFlagsBox
            // 
            customSerializationFlagsBox.CheckOnClick = true;
            customSerializationFlagsBox.FormattingEnabled = true;
            customSerializationFlagsBox.Location = new System.Drawing.Point(142, 161);
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
            enableBakJson.Location = new System.Drawing.Point(29, 305);
            enableBakJson.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            enableBakJson.Name = "enableBakJson";
            enableBakJson.Size = new System.Drawing.Size(146, 19);
            enableBakJson.TabIndex = 14;
            enableBakJson.Text = "Enable .bak files (.json)";
            enableBakJson.UseVisualStyleBackColor = true;
            enableBakJson.CheckedChanged += enableBakJson_CheckedChanged;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(421, 395);
            Controls.Add(enableBakJson);
            Controls.Add(pictureBox1);
            Controls.Add(customSerializationFlagsBox);
            Controls.Add(label4);
            Controls.Add(enablePrettyBytecode);
            Controls.Add(enableUpdateNotice);
            Controls.Add(restoreSize);
            Controls.Add(enableBak);
            Controls.Add(doubleClickToEdit);
            Controls.Add(enableDynamicTree);
            Controls.Add(enableDiscordRpc);
            Controls.Add(label3);
            Controls.Add(numericUpDown1);
            Controls.Add(valuesOnScroll);
            Controls.Add(favoriteThingBox);
            Controls.Add(themeComboBox);
            Controls.Add(aboutButton);
            Controls.Add(label2);
            Controls.Add(label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
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
        internal System.Windows.Forms.Label label3;
        internal System.Windows.Forms.TextBox favoriteThingBox;
        internal System.Windows.Forms.CheckedListBox customSerializationFlagsBox;
        internal System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox enableBakJson;
    }
}