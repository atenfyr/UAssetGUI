namespace UAssetGUI
{
    partial class MarkdownViewer
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
            browser1 = new System.Windows.Forms.WebBrowser();
            closeButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // browser1
            // 
            browser1.Dock = System.Windows.Forms.DockStyle.Top;
            browser1.Location = new System.Drawing.Point(0, 0);
            browser1.Name = "browser1";
            browser1.Size = new System.Drawing.Size(619, 408);
            browser1.TabIndex = 0;
            browser1.Navigating += browser1_Navigating;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            closeButton.ForeColor = System.Drawing.SystemColors.ControlText;
            closeButton.Location = new System.Drawing.Point(518, 414);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 30);
            closeButton.TabIndex = 5;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // MarkdownViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(619, 454);
            Controls.Add(closeButton);
            Controls.Add(browser1);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            Name = "MarkdownViewer";
            Text = "MarkdownViewer";
            Load += MarkdownViewer_Load;
            Resize += MarkdownViewer_Resize;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.WebBrowser browser1;
        private System.Windows.Forms.Button closeButton;
    }
}