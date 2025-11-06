using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            comboBoxReplace.SelectedIndex = 0;
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

        private void TraverseToPopulatePreviouslyExpandedNodes(ISet<TreeNode> previouslyExpandedNodes, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded) previouslyExpandedNodes.Add(node);
                TraverseToPopulatePreviouslyExpandedNodes(previouslyExpandedNodes, node.Nodes);
            }
        }

        private void PerformSearch(CancellationToken cancelToken)
        {
            bool foundSomething = false; bool wasCanceled = false; TreeNode originalNode = null; TreeNode examiningNode = null; int minRow = -1;

            ISet<TreeNode> previouslyExpandedNodes = new HashSet<TreeNode>();

            UAGUtils.InvokeUI(() =>
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = BaseForm.treeView1.GetNodeCount(true);
                BaseForm.dataGridView1.SuspendLayout();
                BaseForm.treeView1.SuspendLayout();
                BaseForm.treeView1.BeginUpdate();

                originalNode = BaseForm.treeView1.SelectedNode;
                examiningNode = BaseForm.treeView1.SelectedNode;
                minRow = BaseForm.dataGridView1.SelectedRows.Count > 0 ? BaseForm.dataGridView1.SelectedRows[0].Index : (BaseForm.dataGridView1.SelectedCells.Count > 0 ? BaseForm.dataGridView1.SelectedCells[0].RowIndex : -1);

                // store previously expanded nodes; could take a second
                TraverseToPopulatePreviouslyExpandedNodes(previouslyExpandedNodes, BaseForm.treeView1.Nodes);
            });

            try
            {
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
                        // if dynamic tree, expand
                        if (examiningNode is PointingTreeNode ptn)
                        {
                            if (!ptn.ChildrenInitialized)
                            {
                                BaseForm.tableEditor.FillOutSubnodes(ptn, false);
                            }
                        }

                        BaseForm.treeView1.SelectedNode = examiningNode;
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
                                        BaseForm.treeView1.SelectedNode.EnsureVisible();
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
                        examiningNode = UAGFindUtils.GetNextNode(examiningNode, CurrentSearchDirection, previouslyExpandedNodes, BaseForm.tableEditor);
                    });

                    if (foundSomething) break;
                }
            }
            finally
            {
                UAGUtils.InvokeUI(() =>
                {
                    progressBar1.Value = progressBar1.Maximum;
                    BaseForm.dataGridView1.ResumeLayout();
                    BaseForm.treeView1.ResumeLayout();
                    BaseForm.treeView1.EndUpdate();

                    if (!foundSomething)
                    {
                        BaseForm.treeView1.SelectedNode = originalNode;
                        BaseForm.UpdateModeFromSelectedNode(originalNode);
                        if (!wasCanceled) MessageBox.Show("0 results found.");
                    }
                });
            }
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

        private void buttonReplace_Click(object sender, EventArgs e)
        {
            if (BaseForm.dataGridView1.SelectedRows.Count < 1)
            {
                labelStatus.Text = "No row selected please insert a name and click above the desired value.";
                return;
            }
            if (BaseForm.dataGridView1.SelectedRows[0].Cells.Count < 4)
            {
                labelStatus.Text = "Not enough cells (cant edit on this position.)";
                return;
            }
            string storedValue;
            storedValue = (string)BaseForm.dataGridView1.SelectedRows[0].Cells[3].Value;

            if (storedValue == null)
            {
                labelStatus.Text = "Cell is null.";
                return;
            }
            
            string input = storedValue;
            string input2 = textBoxReplace.Text;
            if (String.IsNullOrEmpty(input2))
            {
                labelStatus.Text = "Replace field is empty.";
                return;
            }

            // Konvertieren zu float
            if (float.TryParse(input, out float number))
            {
                if (float.TryParse(input2.Replace(".", ","), out float number2))
                {
                    if ((string)comboBoxReplace.SelectedItem == "relative")
                    {
                        number += number2;
                    }
                    else if ((string)comboBoxReplace.SelectedItem == "absolute")
                    {
                        number = number2;
                    }
                    else if ((string)comboBoxReplace.SelectedItem == "percent")
                    {
                        float bbase = number / 100;
                        if (number2 < 0)
                        {
                            number -= bbase * Math.Abs(number2);
                        }
                        else
                        {
                            number += bbase * Math.Abs(number2);
                        }
                    }
                    else
                    {
                        labelStatus.Text = "Selected method is not implemented.";
                        return;
                    }
                    
                }
                else
                {
                    labelStatus.Text = "Replace field only accept numbers.";
                    return;
                }


                    // Zurück in String konvertieren
                    string result = number.ToString();

                BaseForm.dataGridView1.SelectedRows[0].Cells[3].Value = result;
                //Console.WriteLine("Ergebnis: " + result);
            }
            else
            {
                labelStatus.Text = "Cell content is not a number only number are allowed.";
                return;
            }
        }
        private void BlockingSearch()
        {
            SyncSettings();
            if (
            BaseForm.tableEditor == null || BaseForm.tableEditor.asset == null
            ) return;

            PerformSearch(ts.Token);
        }

        private void buttonReplaceAll_Click(object sender, EventArgs e)
        {
            BlockingSearch();
            while (BaseForm.dataGridView1.SelectedRows.Count>0)
            {
                buttonReplace_Click(null, null);
                BlockingSearch();
            }
        }

        private void textBoxReplace_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public enum SearchDirection
    {
        Forward,
        Backward
    }

    public static class UAGFindUtils
    {
        public static TreeNode GetLastNode(TreeNode node, TableHandler handler)
        {
            // if dynamic tree, expand
            if (node is PointingTreeNode ptn)
            {
                if (!ptn.ChildrenInitialized)
                {
                    handler.FillOutSubnodes(ptn, false);
                }
            }

            if (node.Nodes.Count == 0) return node;
            return GetLastNode(node.Nodes[node.Nodes.Count - 1], handler);
        }

        public static TreeNode GetNextNode(TreeNode node, SearchDirection dir, ISet<TreeNode> previouslyExpandedNodes, TableHandler handler, bool canGoDown = true)
        {
            if (node == null) return null;

            if (dir == SearchDirection.Forward)
            {
                if (node.Nodes.Count != 0 && canGoDown) return node.Nodes[0]; // go down one

                // we don't need this node anymore
                if (previouslyExpandedNodes.Contains(node))
                {
                    node.Expand();
                }
                else
                {
                    node.Collapse();
                }

                if (node.NextNode != null) return node.NextNode; // go forward one
                return GetNextNode(node.Parent, dir, previouslyExpandedNodes, handler, false); // go up one
            }
            else if (dir == SearchDirection.Backward)
            {
                // we don't need this node anymore
                if (previouslyExpandedNodes.Contains(node))
                {
                    node.Expand();
                }
                else
                {
                    node.Collapse();
                }

                if (node.PrevNode != null && node.PrevNode.Nodes.Count != 0) return GetLastNode(node.PrevNode, handler); // go backwards one (to previous sibling's last descendant)
                if (node.PrevNode != null) return node.PrevNode; // go backwards one (to previous sibling directly)
                return node.Parent; // go up one (to parent)
            }

            return null;
        }
    }
}
