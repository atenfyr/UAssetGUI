using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UAssetGUI
{
    public partial class MarkdownViewer : Form
    {
        public string MarkdownToDisplay = string.Empty;

        public MarkdownViewer()
        {
            InitializeComponent();
        }

        private void MarkdownViewer_Load(object sender, EventArgs e)
        {
            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();

            string stylingHTML = "<!DOCTYPE html><html><head><style>html{font-family:Arial,Helvetica,sans-serif;background-color:" + ColorTranslator.ToHtml(this.BackColor) + ";color:" + ColorTranslator.ToHtml(this.ForeColor) + "}a{color:" + ColorTranslator.ToHtml(UAGPalette.LinkColor) + ";text-decoration:underline !important}</style></head>";

            this.browser1.DocumentText = stylingHTML + "<body>" + Markdown.ToHtml(MarkdownToDisplay) + "</body></html>";

            ForceResize();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        internal void ForceResize()
        {
            this.browser1.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - this.closeButton.Height - 20);
            this.closeButton.Location = new Point(this.ClientSize.Width - this.closeButton.Width - 10, this.ClientSize.Height - this.closeButton.Height - 10);
        }

        private void MarkdownViewer_Resize(object sender, EventArgs e)
        {
            ForceResize();
        }
    }
}
