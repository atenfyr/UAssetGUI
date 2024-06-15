using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
            Process.Start("https://github.com/" + Form1.GitHubRepo + "/blob/master/LICENSE");
        }

        private void noticeButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/" + Form1.GitHubRepo + "/blob/master/NOTICE.md");
        }
    }
}
