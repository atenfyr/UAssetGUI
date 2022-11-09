using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UAssetGUI
{
    public partial class FindForm : Form
    {
        public string SearchTerm = string.Empty;
        public bool UseRegex = false;
        public Regex RegularExpression;
        public bool CaseSensitive = false;
        public SearchDirection CurrentSearchDirection = SearchDirection.Forward;

        public FindForm()
        {
            InitializeComponent();
            progressBar1.Visible = false;
            searchDirForwardButton.Tag = SearchDirection.Forward;
            searchDirBackwardButton.Tag = SearchDirection.Backward;
            searchDirForwardButton.Checked = true;
        }

        private Form1 BaseForm;
        private void FindForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1) BaseForm = (Form1)this.Owner;

            UAGPalette.RefreshTheme(this);
            this.AdjustFormPosition();
        }

        private void SyncSettings()
        {
            SearchTerm = searchForBox.Text.Trim();
            CaseSensitive = caseSensitiveCheckBox.Checked;
            UseRegex = useRegexCheckBox.Checked;
            CurrentSearchDirection = (SearchDirection)(searchDirectionGroupBox.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked).Tag);

            if (UseRegex)
            {
                RegexOptions ourOptions = RegexOptions.Compiled | RegexOptions.Multiline;
                if (!CaseSensitive) ourOptions |= RegexOptions.IgnoreCase;
                RegularExpression = new Regex(SearchTerm, ourOptions);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (ts != null) ts.Cancel();
            this.Dispose();
        }

        private bool DoesTextQualify(string txt)
        {
            if (UseRegex) return RegularExpression.IsMatch(txt);
            return CaseSensitive ? txt.Contains(SearchTerm) : (txt.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void PerformSearch(CancellationToken cancelToken)
        {
            bool foundSomething = false; bool wasCanceled = false; TreeNode originalNode = null; TreeNode examiningNode = null; int minRow = -1;

            UAGUtils.InvokeUI(() =>
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = BaseForm.listView1.GetNodeCount(true);
                BaseForm.dataGridView1.SuspendLayout();
                BaseForm.listView1.SuspendLayout();
                BaseForm.listView1.BeginUpdate();

                originalNode = BaseForm.listView1.SelectedNode;
                examiningNode = BaseForm.listView1.SelectedNode;
                minRow = BaseForm.dataGridView1.SelectedRows.Count > 0 ? BaseForm.dataGridView1.SelectedRows[0].Index : (BaseForm.dataGridView1.SelectedCells.Count > 0 ? BaseForm.dataGridView1.SelectedCells[0].RowIndex : -1);
            });

            while (examiningNode != null)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    foundSomething = false; wasCanceled = true;
                    MessageBox.Show("Operation canceled.");
                    break;
                }

                UAGUtils.InvokeUI(() =>
                {
                    BaseForm.listView1.SelectedNode = examiningNode;
                    BaseForm.UpdateModeFromSelectedNode(examiningNode);

                    // check node name
                    if (DoesTextQualify(examiningNode.Text) && examiningNode != originalNode)
                    {
                        foundSomething = true;
                        return;
                    }

                    // check dgv
                    if (BaseForm.dataGridView1 != null && BaseForm.dataGridView1.Enabled && BaseForm.dataGridView1.Visible && BaseForm.dataGridView1.Rows.Count > 0)
                    {
                        int rowNum = CurrentSearchDirection == SearchDirection.Forward ? 0 : BaseForm.dataGridView1.Rows.Count - 1;
                        bool isSatisfied = false;
                        while (!isSatisfied)
                        {
                            int oldRowNum = rowNum;

                            rowNum += CurrentSearchDirection == SearchDirection.Forward ? 1 : -1;
                            isSatisfied = CurrentSearchDirection == SearchDirection.Forward ? rowNum >= BaseForm.dataGridView1.Rows.Count : rowNum < 0;

                            DataGridViewRow row = BaseForm.dataGridView1.Rows[oldRowNum];
                            if (minRow >= 0 && CurrentSearchDirection == SearchDirection.Forward && oldRowNum <= minRow) continue;
                            if (minRow >= 0 && CurrentSearchDirection == SearchDirection.Backward && oldRowNum >= minRow) continue;

                            if (row == null || row.Cells == null) continue;
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                if (cell == null || cell.Value == null) continue;

                                if (DoesTextQualify(cell.Value.ToString()))
                                {
                                    if (!cell.Displayed) BaseForm.dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                                    BaseForm.listView1.SelectedNode.EnsureVisible();
                                    row.Selected = true;
                                    cell.Selected = true;
                                    foundSomething = true;
                                }

                                if (foundSomething) return;
                            }

                            if (foundSomething) return;
                        }
                    }

                    if (foundSomething) return;
                    minRow = -1;

                    if (progressBar1.Value < progressBar1.Maximum) progressBar1.Value++;
                    examiningNode = UAGFindUtils.GetNextNode(examiningNode, CurrentSearchDirection);
                });

                if (foundSomething) break;
            }

            UAGUtils.InvokeUI(() =>
            {
                progressBar1.Value = progressBar1.Maximum;
                BaseForm.dataGridView1.ResumeLayout();
                BaseForm.listView1.ResumeLayout();
                BaseForm.listView1.EndUpdate();

                if (!foundSomething)
                {
                    BaseForm.listView1.SelectedNode = originalNode;
                    BaseForm.UpdateModeFromSelectedNode(originalNode);
                    if (!wasCanceled) MessageBox.Show("0 results found.");
                }
            });
        }

        private readonly CancellationTokenSource ts = new CancellationTokenSource();
        private void nextButton_Click(object sender, EventArgs e)
        {
            SyncSettings();
            if (BaseForm.tableEditor == null || BaseForm.tableEditor.asset == null) return;

            Task.Run(() =>
            {
                PerformSearch(ts.Token);
            });
        }
    }

    public enum SearchDirection
    {
        Forward,
        Backward
    }

    public static class UAGFindUtils
    {
        public static TreeNode GetLastNode(TreeNode node)
        {
            if (node.Nodes.Count == 0) return node;
            return GetLastNode(node.Nodes[node.Nodes.Count - 1]);
        }

        public static TreeNode GetNextNode(TreeNode node, SearchDirection dir, bool canGoDown = true)
        {
            if (node == null) return null;

            if (dir == SearchDirection.Forward)
            {
                if (node.Nodes.Count != 0 && canGoDown) return node.Nodes[0]; // go down one
                if (node.NextNode != null) return node.NextNode; // go forward one
                return GetNextNode(node.Parent, dir, false); // go up one
            }
            else if (dir == SearchDirection.Backward)
            {
                if (node.PrevNode != null && node.PrevNode.Nodes.Count != 0) return GetLastNode(node.PrevNode); // go backwards one
                if (node.PrevNode != null) return node.PrevNode; // go backwards one
                return node.Parent; // go up one
            }

            return null;
        }
    }
}
