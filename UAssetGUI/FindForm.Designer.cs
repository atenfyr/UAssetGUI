
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
            searchForLabel = new System.Windows.Forms.Label();
            searchForBox = new System.Windows.Forms.TextBox();
            nextButton = new System.Windows.Forms.Button();
            searchDirectionGroupBox = new System.Windows.Forms.GroupBox();
            searchDirBackwardButton = new System.Windows.Forms.RadioButton();
            searchDirForwardButton = new System.Windows.Forms.RadioButton();
            optionsGroupBox = new System.Windows.Forms.GroupBox();
            useRegexCheckBox = new System.Windows.Forms.CheckBox();
            caseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            progressBar1 = new System.Windows.Forms.ProgressBar();
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
            closeButton.Location = new System.Drawing.Point(261, 143);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 30);
            closeButton.TabIndex = 6;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // searchForLabel
            // 
            searchForLabel.AutoSize = true;
            searchForLabel.Location = new System.Drawing.Point(14, 22);
            searchForLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            searchForLabel.Name = "searchForLabel";
            searchForLabel.Size = new System.Drawing.Size(63, 15);
            searchForLabel.TabIndex = 6;
            searchForLabel.Text = "Search for:";
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
            nextButton.Location = new System.Drawing.Point(167, 143);
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
            searchDirectionGroupBox.Location = new System.Drawing.Point(233, 48);
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
            searchDirForwardButton.Location = new System.Drawing.Point(7, 22);
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
            optionsGroupBox.Location = new System.Drawing.Point(18, 48);
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
            progressBar1.Location = new System.Drawing.Point(18, 145);
            progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(133, 27);
            progressBar1.TabIndex = 13;
            // 
            // FindForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(363, 187);
            Controls.Add(progressBar1);
            Controls.Add(optionsGroupBox);
            Controls.Add(searchDirectionGroupBox);
            Controls.Add(nextButton);
            Controls.Add(searchForBox);
            Controls.Add(searchForLabel);
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
        private System.Windows.Forms.Label searchForLabel;
        private System.Windows.Forms.TextBox searchForBox;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.GroupBox searchDirectionGroupBox;
        private System.Windows.Forms.RadioButton searchDirBackwardButton;
        private System.Windows.Forms.RadioButton searchDirForwardButton;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox useRegexCheckBox;
        private System.Windows.Forms.CheckBox caseSensitiveCheckBox;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}