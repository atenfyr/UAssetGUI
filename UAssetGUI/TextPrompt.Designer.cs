using System.Windows.Forms;

namespace UAssetGUI
{
    partial class TextPrompt
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
            mainLabel = new Label();
            gamePathBox = new TextBox();
            cancelButton = new Button();
            okButton = new Button();
            SuspendLayout();
            // 
            // mainLabel
            // 
            mainLabel.Anchor = AnchorStyles.Top;
            mainLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            mainLabel.Location = new System.Drawing.Point(12, 13);
            mainLabel.Name = "mainLabel";
            mainLabel.Size = new System.Drawing.Size(361, 44);
            mainLabel.TabIndex = 4;
            mainLabel.Text = "Select your game installation directory:";
            mainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gamePathBox
            // 
            gamePathBox.Anchor = AnchorStyles.Left;
            gamePathBox.Location = new System.Drawing.Point(12, 66);
            gamePathBox.Name = "gamePathBox";
            gamePathBox.Size = new System.Drawing.Size(361, 23);
            gamePathBox.TabIndex = 0;
            gamePathBox.KeyDown += TextPrompt_KeyDown;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cancelButton.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            cancelButton.ForeColor = System.Drawing.Color.FromArgb(225, 225, 225);
            cancelButton.Location = new System.Drawing.Point(70, 95);
            cancelButton.MinimumSize = new System.Drawing.Size(0, 26);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(67, 26);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okButton.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            okButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            okButton.ForeColor = System.Drawing.Color.FromArgb(225, 225, 225);
            okButton.Location = new System.Drawing.Point(15, 95);
            okButton.MinimumSize = new System.Drawing.Size(0, 26);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(49, 26);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // TextPrompt
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(40, 42, 45);
            ClientSize = new System.Drawing.Size(385, 130);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(gamePathBox);
            Controls.Add(mainLabel);
            ForeColor = System.Drawing.Color.FromArgb(225, 225, 225);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "TextPrompt";
            Text = "InitialPathPrompt";
            Load += InitialPathPrompt_Load;
            KeyDown += TextPrompt_KeyDown;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mainLabel;
        private System.Windows.Forms.TextBox gamePathBox;
        private Button okButton;
        private Button cancelButton;
    }
}