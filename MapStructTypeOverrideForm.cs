using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    public partial class MapStructTypeOverrideForm : Form
    {
        public static Dictionary<string, Tuple<FName, FName>> MapStructTypeOverride = null;

        public MapStructTypeOverrideForm()
        {
            InitializeComponent();

            infoLabel.Text = "This table maps MapProperty names to the type of the structs within them. You can use header dumps to determine these values if necessary.";

            if (!SystemInformation.TerminalServerSession)
            {
                Type ourGridType = mstoDataGridView.GetType();
                PropertyInfo pi = ourGridType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(mstoDataGridView, true, null);
            }
        }

        private void MapStructTypeOverrideForm_Resize(object sender, EventArgs e)
        {
            ForceResize();
        }

        public void ForceResize()
        {
            mstoDataGridView.Height = closeButton.Location.Y - infoLabel.Location.Y - infoLabel.Height - 9;
        }

        private void AddColumns(string[] ourColumns)
        {
            for (int i = 0; i < ourColumns.Length; i++)
            {
                DataGridViewColumn dgc = new DataGridViewTextBoxColumn
                {
                    HeaderText = ourColumns[i]
                };

                dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (i >= (ourColumns.Length - 1))
                {
                    dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                dgc.SortMode = DataGridViewColumnSortMode.NotSortable;
                mstoDataGridView.Columns.Add(dgc);
            }
        }

        private void MapStructTypeOverrideForm_Load(object sender, EventArgs e)
        {
            if (MapStructTypeOverride == null) MapStructTypeOverride = new UAsset().MapStructTypeOverride;
            LoadFromConfig();

            if (this.Owner is Form1 parentForm)
            {
                this.Text = parentForm.Text;
            }
            UAGPalette.RefreshTheme(this);
            ForceResize();
            this.AdjustFormPosition();

            LoadOntoDGV();
        }

        private void SaveFromDGV()
        {
            if (MapStructTypeOverride == null) return;

            MapStructTypeOverride.Clear();
            foreach (DataGridViewRow row in mstoDataGridView.Rows)
            {
                string mapName = row.Cells[0].Value as string;
                string keyName = row.Cells[1].Value as string;
                string valueName = row.Cells[2].Value as string;
                if (mapName == null || keyName == null || valueName == null) continue;
                MapStructTypeOverride.Add(mapName, new Tuple<FName, FName>(FName.FromString(keyName), FName.FromString(valueName)));
            }
            SaveToConfig();
        }

        private void LoadOntoDGV()
        {
            if (MapStructTypeOverride == null) return;

            mstoDataGridView.Visible = true;
            mstoDataGridView.Columns.Clear();
            mstoDataGridView.Rows.Clear();
            mstoDataGridView.AllowUserToAddRows = true;
            mstoDataGridView.ReadOnly = false;
            mstoDataGridView.BackgroundColor = UAGPalette.DataGridViewActiveColor;
            AddColumns(new string[] { "Map Name", "Key Struct Type Name", "Value Struct Type Name" });

            foreach (KeyValuePair<string, Tuple<FName, FName>> entry in MapStructTypeOverride)
            {
                mstoDataGridView.Rows.Add(new object[] { entry.Key.ToString(), entry.Value.Item1?.ToString() ?? FString.NullCase, entry.Value.Item2?.ToString() ?? FString.NullCase });
            }
        }

        private void SaveToConfig()
        {
            if (MapStructTypeOverride == null) return;

            Properties.Settings.Default.MapStructTypeOverride = new StringCollection();
            foreach (KeyValuePair<string, Tuple<FName, FName>> entry in MapStructTypeOverride)
            {
                Properties.Settings.Default.MapStructTypeOverride.Add(entry.Key.ToString());
                Properties.Settings.Default.MapStructTypeOverride.Add(entry.Value.Item1?.ToString() ?? FString.NullCase);
                Properties.Settings.Default.MapStructTypeOverride.Add(entry.Value.Item2?.ToString() ?? FString.NullCase);
            }

            Properties.Settings.Default.Save();
        }

        private void LoadFromConfig()
        {
            if (MapStructTypeOverride == null) return;

            StringCollection serializedMapStructTypeOverride = Properties.Settings.Default.MapStructTypeOverride;
            if (serializedMapStructTypeOverride == null || serializedMapStructTypeOverride.Count == 0) return;

            MapStructTypeOverride.Clear();
            for (int i = 0; i < serializedMapStructTypeOverride.Count; i += 3)
            {
                MapStructTypeOverride.Add(serializedMapStructTypeOverride[i], new Tuple<FName, FName>(FName.FromString(serializedMapStructTypeOverride[i + 1]), FName.FromString(serializedMapStructTypeOverride[i + 2])));
            }
        }

        private void mstoDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SaveFromDGV();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            LoadOntoDGV();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
