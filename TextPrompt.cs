using System;
using System.Drawing;
using System.Windows.Forms;

namespace UAssetGUI
{

    public partial class TextPrompt : Form
    {
        public string DisplayText = null;
        public string OutputText = null;
        public string PrefilledText = null;

        public TextPrompt()
        {
            InitializeComponent();
        }

        private void InitialPathPrompt_Load(object sender, EventArgs e)
        {
            mainLabel.Text = DisplayText;
            if (this.Owner is Form1 parentForm)
            {
                this.Text = parentForm.Text;
            }
            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();
            gamePathBox.Size = new Size(this.ClientSize.Width - 24, gamePathBox.ClientSize.Height);

            if (!string.IsNullOrEmpty(PrefilledText))
            {
                gamePathBox.Text = PrefilledText;
            }
        }

        private void RunOKButton()
        {
            if (gamePathBox.Text != null && gamePathBox.Text.Length > 0)
            {
                OutputText = gamePathBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            RunOKButton();
        }

        private void RunCancelButton()
        {
            OutputText = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            RunCancelButton();
        }

        private void TextPrompt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                RunOKButton();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                RunCancelButton();
            }
        }
    }
}
