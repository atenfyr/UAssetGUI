using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            numericUpDown1.MouseWheel += NumericUpDown1_MouseWheel;
        }

        private Form1 BaseForm;
        private bool _readyToUpdateTheme = false;
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1) BaseForm = (Form1)this.Owner;

            customSerializationFlagsBox.BeginUpdate();
            customSerializationFlagsBox.Items.Clear();
            List<CustomSerializationFlags> allFlags = Enum.GetValues(typeof(CustomSerializationFlags)).Cast<CustomSerializationFlags>().ToList();
            int entryIdx = 0;
            for (int i = 0; i < allFlags.Count; i++)
            {
                CustomSerializationFlags option = allFlags[i];
                if (option == 0) continue;

                customSerializationFlagsBox.Items.Add(option.ToString());
                customSerializationFlagsBox.SetItemChecked(entryIdx++, (UAGConfig.Data.CustomSerializationFlags & (int)option) > 0);
            }
            customSerializationFlagsBox.EndUpdate();

            themeComboBox.DataSource = Enum.GetValues(typeof(UAGTheme));
            themeComboBox.SelectedIndex = (int)UAGPalette.GetCurrentTheme();
            valuesOnScroll.Checked = UAGConfig.Data.ChangeValuesOnScroll;
            doubleClickToEdit.Checked = UAGConfig.Data.DoubleClickToEdit;
            enableDiscordRpc.Checked = UAGConfig.Data.EnableDiscordRPC;
            enableDynamicTree.Checked = UAGConfig.Data.EnableDynamicTree;
            favoriteThingBox.Text = UAGConfig.Data.FavoriteThing;
            numericUpDown1.Value = UAGConfig.Data.DataZoom;
            enableBak.Checked = UAGConfig.Data.EnableBak;
            enablePrettyBytecode.Checked = UAGConfig.Data.EnablePrettyBytecode;
            restoreSize.Checked = UAGConfig.Data.RestoreSize;
            enableUpdateNotice.Checked = UAGConfig.Data.EnableUpdateNotice;
            enableBakJson.Checked = UAGConfig.Data.EnableBakJson;

            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();
            _readyToUpdateTheme = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            var softwareAgeInYears = (int.Parse(DateTime.Now.ToString("yyyyMMdd")) - 20200723) / 10000;

            UAGUtils.InvokeUI(() =>
            {
                var formPopup = new AboutForm();

                formPopup.AboutText = (this.Owner as Form1).DisplayVersion + "\n" +
                "By atenfyr\n" +
                "\nThank you to trumank, LongerWarrior, Kaiheilos, and others for all your generous contributions to this software\n" +
                "\nThank you to the love of my life for listening to me and supporting me despite not caring at all about any of this\n" +
                "\nThank you for using this thing even after " + softwareAgeInYears + " years\n";

                formPopup.StartPosition = FormStartPosition.CenterParent;
                formPopup.ShowDialog(this);
            });
        }

        private void themeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            Enum.TryParse(themeComboBox.SelectedValue.ToString(), out UAGTheme nextTheme);
            UAGPalette.SetCurrentTheme(nextTheme);
            UAGPalette.RefreshTheme(BaseForm);
            UAGPalette.RefreshTheme(this);
        }

        private void valuesOnScroll_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.ChangeValuesOnScroll = valuesOnScroll.Checked;
        }

        private void enableDynamicTree_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnableDynamicTree = enableDynamicTree.Checked;
        }

        private void doubleClickToEdit_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.DoubleClickToEdit = doubleClickToEdit.Checked;
            //BaseForm.dataGridView1.EditMode = UAGConfig.Data.DoubleClickToEdit ? DataGridViewEditMode.EditProgrammatically : DataGridViewEditMode.EditOnEnter;
        }

        private void enableBak_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnableBak = enableBak.Checked;
        }

        private void restoreSize_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.RestoreSize = restoreSize.Checked;
        }

        private void enableUpdateNotice_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnableUpdateNotice = enableUpdateNotice.Checked;
        }

        private void enablePrettyBytecode_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnablePrettyBytecode = enablePrettyBytecode.Checked;
        }

        private void enableBakJson_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnableBakJson = enableBakJson.Checked;
        }

        private void enableDiscordRpc_CheckedChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.EnableDiscordRPC = enableDiscordRpc.Checked;
            if (UAGConfig.Data.EnableDiscordRPC)
            {
                BaseForm.UpdateRPC();
            }
            else
            {
                BaseForm.DiscordRPC.ClearPresence();
            }
        }

        private bool isCurrentlyComicSans = false;
        private void favoriteThingBox_TextChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.FavoriteThing = favoriteThingBox.Text;
            string favoriteThingLowered = UAGConfig.Data.FavoriteThing.ToLowerInvariant().Trim();

            if (UAGPalette.IsComicSans())
            {
                isCurrentlyComicSans = true;
                UAGPalette.RefreshTheme(BaseForm);
                UAGPalette.RefreshTheme(this);
            }
            else if (isCurrentlyComicSans)
            {
                isCurrentlyComicSans = false;
                UAGPalette.RefreshTheme(BaseForm);
                UAGPalette.RefreshTheme(this);
            }

            if (favoriteThingLowered == "atenfyr" || favoriteThingLowered == "adolescent")
            {
                // need MemoryStream to remain open until we're done using the image
                // no need to dispose MemoryStream so let's just let GC handle it once image stops being used
                var strm = new MemoryStream(Properties.Resources.dancing_cat);
                this.pictureBox1.Image = Image.FromStream(strm);
                this.pictureBox1.Visible = true;
            }
            else
            {
                this.pictureBox1.Visible = false;
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UAGConfig.Save();
            UAGPalette.RefreshTheme(BaseForm);
            UAGPalette.RefreshTheme(this);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            UAGConfig.Data.DataZoom = (int)numericUpDown1.Value;
            UAGPalette.RefreshTheme(BaseForm);
            UAGPalette.RefreshTheme(this);

            // Refresh dgv row heights
            UAGUtils.InvokeUI(() =>
            {
                if (BaseForm.tableEditor != null)
                {
                    BaseForm.tableEditor.Save(true);
                }
            });
        }

        private void customSerializationFlagsBox_Click(object sender, EventArgs e)
        {
            // this logic is here to prevent default list box selection logic

            // The following block of code is modified and adapted from source code on StackOverflow created and licensed by user Hath, copyright 2008: https://stackoverflow.com/a/334672
            // The original code is adapted for usage in this software under the terms of the CC BY-SA 2.5 license: https://creativecommons.org/licenses/by-sa/2.5/
            /*
                UNLESS OTHERWISE AGREED TO BY THE PARTIES IN WRITING, LICENSOR OFFERS THE
                WORK AS-IS AND MAKES NO REPRESENTATIONS OR WARRANTIES OF ANY KIND
                CONCERNING THE MATERIALS, EXPRESS, IMPLIED, STATUTORY OR OTHERWISE,
                INCLUDING, WITHOUT LIMITATION, WARRANTIES OF TITLE, MERCHANTIBILITY, FITNESS
                FOR A PARTICULAR PURPOSE, NONINFRINGEMENT, OR THE ABSENCE OF LATENT
                OR OTHER DEFECTS, ACCURACY, OR THE PRESENCE OF ABSENCE OF ERRORS, WHETHER OR
                NOT DISCOVERABLE. SOME JURISDICTIONS DO NOT ALLOW THE EXCLUSION OF IMPLIED
                WARRANTIES, SO SUCH EXCLUSION MAY NOT APPLY TO YOU.
            */
            for (int i = 0; i < customSerializationFlagsBox.Items.Count; i++)
            {
                if (customSerializationFlagsBox.GetItemRectangle(i).Contains(customSerializationFlagsBox.PointToClient(MousePosition)))
                {
                    customSerializationFlagsBox.SetItemChecked(i, !customSerializationFlagsBox.GetItemChecked(i));
                }
            }

            // update config
            CustomSerializationFlags res = 0;
            for (int i = 0; i < customSerializationFlagsBox.Items.Count; i++)
            {
                string item = customSerializationFlagsBox.Items[i] as string;
                if (item == null) continue;
                if (customSerializationFlagsBox.GetItemChecked(i))
                {
                    res |= Enum.Parse<CustomSerializationFlags>(item);
                }
            }

            UAGConfig.Data.CustomSerializationFlags = (int)res;
        }

        private void NumericUpDown1_MouseWheel(object sender, MouseEventArgs e)
        {
            // override default scroll logic to prevent weird +3 problem
            if (e.Delta == 0) return;
            ((HandledMouseEventArgs)e).Handled = true;

            UAGUtils.InvokeUI(() =>
            {
                decimal newValue = numericUpDown1.Value + (e.Delta > 0 ? 1 : -1);
                if (newValue < numericUpDown1.Minimum) newValue = numericUpDown1.Minimum;
                if (newValue > numericUpDown1.Maximum) newValue = numericUpDown1.Maximum;
                numericUpDown1.Value = newValue;
            });
        }
    }
}
