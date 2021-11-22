using System;
using System.Drawing;
using System.Windows.Forms;

namespace UAssetGUI
{
    public static class UAGPalette
    {
        public static Color BackColor = Color.White;
        public static Color ButtonBackColor = Color.FromArgb(240, 240, 240);
        public static Color ForeColor = Color.Black;
        public static Color HighlightBackColor = SystemColors.Highlight;
        public static Color HighlightForeColor = SystemColors.HighlightText;
        public static Color InactiveColor = Color.FromArgb(211, 211, 211);
        public static Color DataGridViewActiveColor = Color.FromArgb(240, 240, 240);
        private static UAGTheme CurrentTheme = UAGTheme.Light;

        public static void InitializeTheme()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Theme)) Enum.TryParse(Properties.Settings.Default.Theme, out CurrentTheme);
        }

        public static UAGTheme GetCurrentTheme()
        {
            return CurrentTheme;
        }

        public static void SetCurrentTheme(UAGTheme newTheme)
        {
            CurrentTheme = newTheme;
            Properties.Settings.Default.Theme = CurrentTheme.ToString();
            Properties.Settings.Default.Save();
        }

        public static void RefreshTheme(Form frm)
        {
            UAGUtils.InvokeUI(() =>
            {
                RefreshThemeInternal(frm);
            });
        }

        private static void RefreshAllButtonsInControl(this Control ctrl)
        {
            foreach (Control ctrl2 in ctrl.Controls)
            {
                if (ctrl2 is Button butto)
                {
                    butto.FlatStyle = FlatStyle.Flat;
                    butto.ForeColor = UAGPalette.ForeColor;
                    butto.FlatAppearance.BorderColor = UAGPalette.ForeColor;
                    butto.FlatAppearance.BorderSize = 1;
                    butto.BackColor = UAGPalette.ButtonBackColor;
                    butto.MinimumSize = new Size(0, 26);
                }
                RefreshAllButtonsInControl(ctrl2);
            }
        }

        private static void RefreshThemeInternal(Form frm)
        {
            switch (CurrentTheme)
            {
                case UAGTheme.Light:
                    BackColor = Color.White;
                    ButtonBackColor = Color.FromArgb(240, 240, 240);
                    ForeColor = Color.Black;
                    HighlightBackColor = SystemColors.Highlight;
                    HighlightForeColor = SystemColors.HighlightText;
                    InactiveColor = Color.FromArgb(211, 211, 211);
                    DataGridViewActiveColor = Color.FromArgb(240, 240, 240);
                    break;
            }

            frm.Icon = Properties.Resources.icon;
            frm.BackColor = UAGPalette.BackColor;
            frm.ForeColor = UAGPalette.ForeColor;
            if (frm is Form1 frm1)
            {
                Color selectedListViewBackColor = frm1.listView1.Nodes.Count > 0 ? UAGPalette.BackColor : UAGPalette.InactiveColor;
                frm1.listView1.BackColor = selectedListViewBackColor;
                frm1.listView1.ForeColor = UAGPalette.ForeColor;

                frm1.menuStrip1.BackColor = UAGPalette.BackColor;
                frm1.menuStrip1.ForeColor = UAGPalette.ForeColor;
                foreach (ToolStripItem rootItem in frm1.menuStrip1.Items)
                {
                    rootItem.BackColor = UAGPalette.BackColor;
                    rootItem.ForeColor = UAGPalette.ForeColor;
                    if (rootItem is ToolStripMenuItem rootMenuItem)
                    {
                        foreach (ToolStripItem childItem in rootMenuItem.DropDownItems)
                        {
                            childItem.BackColor = UAGPalette.BackColor;
                            childItem.ForeColor = UAGPalette.ForeColor;
                        }
                    }
                }

                Color selectedDGVBackColor = frm1.dataGridView1.Columns.Count > 0 ? UAGPalette.DataGridViewActiveColor : UAGPalette.InactiveColor;
                frm1.dataGridView1.BackgroundColor = selectedDGVBackColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = UAGPalette.BackColor; // intentional
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = UAGPalette.ForeColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = UAGPalette.HighlightBackColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = UAGPalette.HighlightForeColor;
                frm1.dataGridView1.RowHeadersDefaultCellStyle = frm1.dataGridView1.ColumnHeadersDefaultCellStyle;
                frm1.dataGridView1.DefaultCellStyle = frm1.dataGridView1.ColumnHeadersDefaultCellStyle;
            }
            frm.RefreshAllButtonsInControl();
        }
    }
}
