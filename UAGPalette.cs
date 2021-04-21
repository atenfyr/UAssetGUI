using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UAssetGUI
{
    public static class UAGPalette
    {
        public static Color BackColor = Color.White;
        public static Color ForeColor = Color.Black;
        public static Color HighlightColor = Color.FromArgb(85, 85, 85);
        public static Color InactiveColor = Color.FromArgb(211, 211, 211);
        public static Color DataGridViewActiveColor = Color.FromArgb(240, 240, 240);
        public static UAGTheme CurrentTheme = UAGTheme.Light;

        public enum UAGTheme
        {
            Light
        }

        public static void RefreshTheme(Form frm)
        {
            Utils.InvokeUI(() =>
            {
                RefreshThemeInternal(frm);
            });
        }

        private static void RefreshThemeInternal(Form frm)
        {
            switch (CurrentTheme)
            {
                case UAGTheme.Light:
                    BackColor = Color.White;
                    ForeColor = Color.Black;
                    InactiveColor = Color.FromArgb(211, 211, 211);
                    DataGridViewActiveColor = Color.FromArgb(240, 240, 240);
                    HighlightColor = SystemColors.Highlight;
                    break;
            }

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

                Color selectedDGVBackColor = frm1.dataGridView1.Columns.Count > 0 ? UAGPalette.BackColor : UAGPalette.InactiveColor;
                frm1.dataGridView1.BackgroundColor = selectedDGVBackColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = UAGPalette.BackColor; // intentional
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = UAGPalette.ForeColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = UAGPalette.HighlightColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = UAGPalette.ForeColor;
                frm1.dataGridView1.RowHeadersDefaultCellStyle = frm1.dataGridView1.ColumnHeadersDefaultCellStyle;
                frm1.dataGridView1.DefaultCellStyle = frm1.dataGridView1.ColumnHeadersDefaultCellStyle;
            }
        }
    }
}
