
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
            this.closeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.searchForBox = new System.Windows.Forms.TextBox();
            this.nextButton = new System.Windows.Forms.Button();
            this.searchDirectionGroupBox = new System.Windows.Forms.GroupBox();
            this.searchDirBackwardButton = new System.Windows.Forms.RadioButton();
            this.searchDirForwardButton = new System.Windows.Forms.RadioButton();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.useRegexCheckBox = new System.Windows.Forms.CheckBox();
            this.caseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.searchDirectionGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.closeButton.Location = new System.Drawing.Point(224, 124);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 26);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Search for:";
            // 
            // searchForBox
            // 
            this.searchForBox.Location = new System.Drawing.Point(77, 16);
            this.searchForBox.Name = "searchForBox";
            this.searchForBox.Size = new System.Drawing.Size(222, 20);
            this.searchForBox.TabIndex = 0;
            // 
            // nextButton
            // 
            this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nextButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.nextButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.nextButton.Location = new System.Drawing.Point(143, 124);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 26);
            this.nextButton.TabIndex = 5;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // searchDirectionGroupBox
            // 
            this.searchDirectionGroupBox.Controls.Add(this.searchDirBackwardButton);
            this.searchDirectionGroupBox.Controls.Add(this.searchDirForwardButton);
            this.searchDirectionGroupBox.Location = new System.Drawing.Point(200, 42);
            this.searchDirectionGroupBox.Name = "searchDirectionGroupBox";
            this.searchDirectionGroupBox.Size = new System.Drawing.Size(99, 65);
            this.searchDirectionGroupBox.TabIndex = 11;
            this.searchDirectionGroupBox.TabStop = false;
            this.searchDirectionGroupBox.Text = "Search direction";
            // 
            // searchDirBackwardButton
            // 
            this.searchDirBackwardButton.AutoSize = true;
            this.searchDirBackwardButton.Location = new System.Drawing.Point(6, 42);
            this.searchDirBackwardButton.Name = "searchDirBackwardButton";
            this.searchDirBackwardButton.Size = new System.Drawing.Size(73, 17);
            this.searchDirBackwardButton.TabIndex = 4;
            this.searchDirBackwardButton.TabStop = true;
            this.searchDirBackwardButton.Text = "Backward";
            this.searchDirBackwardButton.UseVisualStyleBackColor = true;
            // 
            // searchDirForwardButton
            // 
            this.searchDirForwardButton.AutoSize = true;
            this.searchDirForwardButton.Location = new System.Drawing.Point(6, 19);
            this.searchDirForwardButton.Name = "searchDirForwardButton";
            this.searchDirForwardButton.Size = new System.Drawing.Size(63, 17);
            this.searchDirForwardButton.TabIndex = 3;
            this.searchDirForwardButton.TabStop = true;
            this.searchDirForwardButton.Text = "Forward";
            this.searchDirForwardButton.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.useRegexCheckBox);
            this.optionsGroupBox.Controls.Add(this.caseSensitiveCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(15, 42);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(164, 65);
            this.optionsGroupBox.TabIndex = 12;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // useRegexCheckBox
            // 
            this.useRegexCheckBox.AutoSize = true;
            this.useRegexCheckBox.Location = new System.Drawing.Point(6, 42);
            this.useRegexCheckBox.Name = "useRegexCheckBox";
            this.useRegexCheckBox.Size = new System.Drawing.Size(74, 17);
            this.useRegexCheckBox.TabIndex = 2;
            this.useRegexCheckBox.Text = "Use regex";
            this.useRegexCheckBox.UseVisualStyleBackColor = true;
            // 
            // caseSensitiveCheckBox
            // 
            this.caseSensitiveCheckBox.AutoSize = true;
            this.caseSensitiveCheckBox.Location = new System.Drawing.Point(6, 19);
            this.caseSensitiveCheckBox.Name = "caseSensitiveCheckBox";
            this.caseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
            this.caseSensitiveCheckBox.TabIndex = 1;
            this.caseSensitiveCheckBox.Text = "Case sensitive";
            this.caseSensitiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 126);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(114, 23);
            this.progressBar1.TabIndex = 13;
            // 
            // FindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 162);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.searchDirectionGroupBox);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.searchForBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FindForm";
            this.Text = "Find";
            this.Load += new System.EventHandler(this.FindForm_Load);
            this.searchDirectionGroupBox.ResumeLayout(false);
            this.searchDirectionGroupBox.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}