
namespace UAssetGUI
{
    partial class AboutForm
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
            licenseButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            noticeButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            closeButton.Location = new System.Drawing.Point(437, 316);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 30);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // licenseButton
            // 
            licenseButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            licenseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            licenseButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            licenseButton.ForeColor = System.Drawing.SystemColors.ControlText;
            licenseButton.Location = new System.Drawing.Point(14, 316);
            licenseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            licenseButton.Name = "licenseButton";
            licenseButton.Size = new System.Drawing.Size(120, 30);
            licenseButton.TabIndex = 2;
            licenseButton.Text = "View license...";
            licenseButton.UseVisualStyleBackColor = true;
            licenseButton.Click += licenseButton_Click;
            // 
            // label1
            // 
            label1.Dock = System.Windows.Forms.DockStyle.Top;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(539, 313);
            label1.TabIndex = 1;
            label1.Text = "AboutText should be overridden before displaying this form";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // noticeButton
            // 
            noticeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            noticeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            noticeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            noticeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            noticeButton.Location = new System.Drawing.Point(141, 316);
            noticeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            noticeButton.Name = "noticeButton";
            noticeButton.Size = new System.Drawing.Size(217, 30);
            noticeButton.TabIndex = 3;
            noticeButton.Text = "View 3rd-party software...";
            noticeButton.UseVisualStyleBackColor = true;
            noticeButton.Click += noticeButton_Click;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(539, 360);
            Controls.Add(noticeButton);
            Controls.Add(label1);
            Controls.Add(licenseButton);
            Controls.Add(closeButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AboutForm";
            Text = "About";
            Load += AboutForm_Load;
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button licenseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button noticeButton;
    }
}