using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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

        private static void CheckIfMapStructTypeOverrideIsNull()
        {
            if (MapStructTypeOverride == null) MapStructTypeOverride = new UAsset().MapStructTypeOverride;
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
            CheckIfMapStructTypeOverrideIsNull();

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
            CheckIfMapStructTypeOverrideIsNull();

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
            CheckIfMapStructTypeOverrideIsNull();

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

        internal static void SaveToConfig()
        {
            CheckIfMapStructTypeOverrideIsNull();

            Properties.Settings.Default.MapStructTypeOverride = ExportData();
            Properties.Settings.Default.Save();
        }

        internal static void LoadFromConfig()
        {
            CheckIfMapStructTypeOverrideIsNull();

            ImportData(Properties.Settings.Default.MapStructTypeOverride);
        }

        private static void ImportData(string data)
        {
            if (data == null) return;
            CheckIfMapStructTypeOverrideIsNull();

            Dictionary<string, string[]> temp = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(data);
            if (temp == null || temp.Count == 0) return;

            MapStructTypeOverride.Clear();
            foreach (KeyValuePair<string, string[]> entry in temp)
            {
                MapStructTypeOverride.Add(entry.Key, new Tuple<FName, FName>(FName.FromString(entry.Value[0]), FName.FromString(entry.Value[1])));
            }
        }

        private static string ExportData()
        {
            CheckIfMapStructTypeOverrideIsNull();

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                foreach (KeyValuePair<string, Tuple<FName, FName>> entry in MapStructTypeOverride)
                {
                    writer.WritePropertyName(entry.Key.ToString());
                    writer.WriteStartArray();
                    writer.WriteValue(entry.Value.Item1?.ToString());
                    writer.WriteValue(entry.Value.Item2?.ToString());
                    writer.WriteEnd();
                }
                writer.WriteEndObject();
            }

            return sb.ToString();
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

        private void importButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Map Struct Type Override JSON (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImportData(File.ReadAllText(openFileDialog.FileName));
                    LoadOntoDGV();
                    SaveToConfig();
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Map Struct Type Override JSON (*.json)|*.json|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    SaveFromDGV();
                    File.WriteAllText(dialog.FileName, ExportData());
                }
            }
        }
    }
}
