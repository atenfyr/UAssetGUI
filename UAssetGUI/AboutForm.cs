using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace UAssetGUI
{
    public partial class AboutForm : Form
    {
        public string AboutText
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }

        public AboutForm()
        {
            InitializeComponent();
        }

        private Form1 BaseForm;
        private void AboutForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1) BaseForm = (Form1)this.Owner;

            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void licenseButton_Click(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                string rawMarkdownText = string.Empty;
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UAssetGUI.LICENSE"))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader != null) rawMarkdownText = reader.ReadToEnd().Trim();
                        }
                    }
                }

                if (string.IsNullOrEmpty(rawMarkdownText))
                {
                    UAGUtils.OpenURL("https://github.com/" + Form1.GitHubRepo + "/blob/master/LICENSE");
                    return;
                }

                var formPopup = new MarkdownViewer();
                formPopup.MarkdownToDisplay = "```\n" + rawMarkdownText + "\n```";
                formPopup.Text = "License";
                formPopup.StartPosition = FormStartPosition.CenterParent;
                formPopup.ShowDialog(this);
            });
        }

        private void noticeButton_Click(object sender, EventArgs e)
        {
            UAGUtils.InvokeUI(() =>
            {
                string rawMarkdownText = string.Empty;
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UAssetGUI.NOTICE.md"))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader != null) rawMarkdownText = reader.ReadToEnd().Trim();
                        }
                    }
                }

                if (string.IsNullOrEmpty(rawMarkdownText))
                {
                    UAGUtils.OpenURL("https://github.com/" + Form1.GitHubRepo + "/blob/master/NOTICE.md");
                    return;
                }

                var formPopup = new MarkdownViewer();
                formPopup.MarkdownToDisplay = rawMarkdownText;
                formPopup.Text = "List of 3rd-party software";
                formPopup.StartPosition = FormStartPosition.CenterParent;
                formPopup.ShowDialog(this);
            });
        }
    }
}
