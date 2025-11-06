
namespace UAssetGUI
{
    partial class FindForm
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
            closeButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            searchForBox = new System.Windows.Forms.TextBox();
            nextButton = new System.Windows.Forms.Button();
            searchDirectionGroupBox = new System.Windows.Forms.GroupBox();
            searchDirBackwardButton = new System.Windows.Forms.RadioButton();
            searchDirForwardButton = new System.Windows.Forms.RadioButton();
            optionsGroupBox = new System.Windows.Forms.GroupBox();
            useRegexCheckBox = new System.Windows.Forms.CheckBox();
            caseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            buttonReplace = new System.Windows.Forms.Button();
            textBoxReplace = new System.Windows.Forms.TextBox();
            checkBoxReplace = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            buttonReplaceAll = new System.Windows.Forms.Button();
            comboBoxReplace = new System.Windows.Forms.ComboBox();
            labelStatus = new System.Windows.Forms.Label();
            searchDirectionGroupBox.SuspendLayout();
            optionsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            closeButton.Location = new System.Drawing.Point(354, 81);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 30);
            closeButton.TabIndex = 6;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 22);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 6;
            label1.Text = "Search for:";
            // 
            // searchForBox
            // 
            searchForBox.Location = new System.Drawing.Point(90, 18);
            searchForBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchForBox.Name = "searchForBox";
            searchForBox.Size = new System.Drawing.Size(258, 23);
            searchForBox.TabIndex = 0;
            // 
            // nextButton
            // 
            nextButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            nextButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            nextButton.ForeColor = System.Drawing.SystemColors.ControlText;
            nextButton.Location = new System.Drawing.Point(354, 7);
            nextButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nextButton.Name = "nextButton";
            nextButton.Size = new System.Drawing.Size(88, 30);
            nextButton.TabIndex = 5;
            nextButton.Text = "Next";
            nextButton.UseVisualStyleBackColor = true;
            nextButton.Click += nextButton_Click;
            // 
            // searchDirectionGroupBox
            // 
            searchDirectionGroupBox.Controls.Add(searchDirBackwardButton);
            searchDirectionGroupBox.Controls.Add(searchDirForwardButton);
            searchDirectionGroupBox.Location = new System.Drawing.Point(211, 81);
            searchDirectionGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchDirectionGroupBox.Name = "searchDirectionGroupBox";
            searchDirectionGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchDirectionGroupBox.Size = new System.Drawing.Size(115, 75);
            searchDirectionGroupBox.TabIndex = 11;
            searchDirectionGroupBox.TabStop = false;
            searchDirectionGroupBox.Text = "Search direction";
            // 
            // searchDirBackwardButton
            // 
            searchDirBackwardButton.AutoSize = true;
            searchDirBackwardButton.Location = new System.Drawing.Point(7, 48);
            searchDirBackwardButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchDirBackwardButton.Name = "searchDirBackwardButton";
            searchDirBackwardButton.Size = new System.Drawing.Size(76, 19);
            searchDirBackwardButton.TabIndex = 4;
            searchDirBackwardButton.TabStop = true;
            searchDirBackwardButton.Text = "Backward";
            searchDirBackwardButton.UseVisualStyleBackColor = true;
            // 
            // searchDirForwardButton
            // 
            searchDirForwardButton.AutoSize = true;
            searchDirForwardButton.Location = new System.Drawing.Point(8, 23);
            searchDirForwardButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchDirForwardButton.Name = "searchDirForwardButton";
            searchDirForwardButton.Size = new System.Drawing.Size(68, 19);
            searchDirForwardButton.TabIndex = 3;
            searchDirForwardButton.TabStop = true;
            searchDirForwardButton.Text = "Forward";
            searchDirForwardButton.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            optionsGroupBox.Controls.Add(useRegexCheckBox);
            optionsGroupBox.Controls.Add(caseSensitiveCheckBox);
            optionsGroupBox.Location = new System.Drawing.Point(12, 76);
            optionsGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            optionsGroupBox.Name = "optionsGroupBox";
            optionsGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            optionsGroupBox.Size = new System.Drawing.Size(191, 75);
            optionsGroupBox.TabIndex = 12;
            optionsGroupBox.TabStop = false;
            optionsGroupBox.Text = "Options";
            // 
            // useRegexCheckBox
            // 
            useRegexCheckBox.AutoSize = true;
            useRegexCheckBox.Location = new System.Drawing.Point(7, 48);
            useRegexCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            useRegexCheckBox.Name = "useRegexCheckBox";
            useRegexCheckBox.Size = new System.Drawing.Size(76, 19);
            useRegexCheckBox.TabIndex = 2;
            useRegexCheckBox.Text = "Use regex";
            useRegexCheckBox.UseVisualStyleBackColor = true;
            // 
            // caseSensitiveCheckBox
            // 
            caseSensitiveCheckBox.AutoSize = true;
            caseSensitiveCheckBox.Location = new System.Drawing.Point(7, 22);
            caseSensitiveCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            caseSensitiveCheckBox.Name = "caseSensitiveCheckBox";
            caseSensitiveCheckBox.Size = new System.Drawing.Size(99, 19);
            caseSensitiveCheckBox.TabIndex = 1;
            caseSensitiveCheckBox.Text = "Case sensitive";
            caseSensitiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(344, 129);
            progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(98, 27);
            progressBar1.TabIndex = 13;
            // 
            // buttonReplace
            // 
            buttonReplace.Location = new System.Drawing.Point(263, 46);
            buttonReplace.Name = "buttonReplace";
            buttonReplace.Size = new System.Drawing.Size(75, 23);
            buttonReplace.TabIndex = 14;
            buttonReplace.Text = "Replace";
            buttonReplace.UseVisualStyleBackColor = true;
            buttonReplace.Click += buttonReplace_Click;
            // 
            // textBoxReplace
            // 
            textBoxReplace.Location = new System.Drawing.Point(157, 47);
            textBoxReplace.Name = "textBoxReplace";
            textBoxReplace.Size = new System.Drawing.Size(100, 23);
            textBoxReplace.TabIndex = 15;
            textBoxReplace.TextChanged += textBoxReplace_TextChanged;
            // 
            // checkBoxReplace
            // 
            checkBoxReplace.AutoSize = true;
            checkBoxReplace.Location = new System.Drawing.Point(66, 49);
            checkBoxReplace.Name = "checkBoxReplace";
            checkBoxReplace.Size = new System.Drawing.Size(67, 19);
            checkBoxReplace.TabIndex = 16;
            checkBoxReplace.Text = "Relative";
            checkBoxReplace.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 51);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(48, 15);
            label2.TabIndex = 17;
            label2.Text = "Replace";
            // 
            // buttonReplaceAll
            // 
            buttonReplaceAll.Location = new System.Drawing.Point(344, 46);
            buttonReplaceAll.Name = "buttonReplaceAll";
            buttonReplaceAll.Size = new System.Drawing.Size(98, 23);
            buttonReplaceAll.TabIndex = 18;
            buttonReplaceAll.Text = "Replace All";
            buttonReplaceAll.UseVisualStyleBackColor = true;
            buttonReplaceAll.Click += buttonReplaceAll_Click;
            // 
            // comboBoxReplace
            // 
            comboBoxReplace.FormattingEnabled = true;
            comboBoxReplace.Items.AddRange(new object[] { "absolute", "relative", "percent" });
            comboBoxReplace.Location = new System.Drawing.Point(66, 46);
            comboBoxReplace.Name = "comboBoxReplace";
            comboBoxReplace.Size = new System.Drawing.Size(85, 23);
            comboBoxReplace.TabIndex = 19;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(12, 154);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(39, 15);
            labelStatus.TabIndex = 20;
            labelStatus.Text = "Status";
            // 
            // FindForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(446, 214);
            Controls.Add(labelStatus);
            Controls.Add(comboBoxReplace);
            Controls.Add(buttonReplaceAll);
            Controls.Add(label2);
            Controls.Add(checkBoxReplace);
            Controls.Add(textBoxReplace);
            Controls.Add(buttonReplace);
            Controls.Add(progressBar1);
            Controls.Add(optionsGroupBox);
            Controls.Add(searchDirectionGroupBox);
            Controls.Add(nextButton);
            Controls.Add(searchForBox);
            Controls.Add(label1);
            Controls.Add(closeButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FindForm";
            Text = "Find";
            Load += FindForm_Load;
            searchDirectionGroupBox.ResumeLayout(false);
            searchDirectionGroupBox.PerformLayout();
            optionsGroupBox.ResumeLayout(false);
            optionsGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox searchForBox;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.GroupBox searchDirectionGroupBox;
        private System.Windows.Forms.RadioButton searchDirBackwardButton;
        private System.Windows.Forms.RadioButton searchDirForwardButton;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox useRegexCheckBox;
        private System.Windows.Forms.CheckBox caseSensitiveCheckBox;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonReplace;
        private System.Windows.Forms.TextBox textBoxReplace;
        private System.Windows.Forms.CheckBox checkBoxReplace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonReplaceAll;
        private System.Windows.Forms.ComboBox comboBoxReplace;
        private System.Windows.Forms.Label labelStatus;
    }
}