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
    public partial class ProgressBarForm : Form
    {
        public Form BaseForm;
        public int Value;
        public int Maximum;

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        public void Progress(int numToAdd)
        {
            Value += numToAdd;
            UpdateGUI();
        }

        public void UpdateGUI()
        {
            ProgressBar.Value = Value;
            label1.Text = ProgressBar.Value.ToString() + "/" + ProgressBar.Maximum.ToString();
        }

        public ProgressBarForm()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (BaseForm is FileContainerForm fcForm) fcForm.extractAllBackgroundWorker.CancelAsync();
        }

        private void ProgressBarForm_Load(object sender, EventArgs e)
        {
            ProgressBar.Value = Value;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = Maximum;
            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition(BaseForm);
        }
    }
}
