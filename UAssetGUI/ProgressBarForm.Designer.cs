namespace UAssetGUI
{
    partial class ProgressBarForm
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
            ProgressBar = new System.Windows.Forms.ProgressBar();
            cancelButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new System.Drawing.Point(18, 17);
            ProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(201, 51);
            ProgressBar.TabIndex = 14;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cancelButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            cancelButton.ForeColor = System.Drawing.SystemColors.ControlText;
            cancelButton.Location = new System.Drawing.Point(134, 79);
            cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(90, 31);
            cancelButton.TabIndex = 15;
            cancelButton.Text = "Cancel...";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(16, 79);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(95, 30);
            label1.TabIndex = 16;
            label1.Text = "label1";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressBarForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(237, 122);
            Controls.Add(label1);
            Controls.Add(cancelButton);
            Controls.Add(ProgressBar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProgressBarForm";
            Text = "ProgressBarForm";
            Load += ProgressBarForm_Load;
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
    }
}