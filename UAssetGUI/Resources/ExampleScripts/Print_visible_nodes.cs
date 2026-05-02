// This script outputs a textual representation of the currently visible nodes to node_structure.txt

using System.IO;

UAGUtils.InvokeUI(() =>
{
    List<string> nodeLabels = new List<string>();
    TreeNode node = Interface.GetTreeView().TopNode;
    while (node != null && node.IsVisible)
    {
        int numAncestors = 0;
        TreeNode parent = node.Parent;
        while (parent != null)
        {
            numAncestors++;
            parent = parent.Parent;
        }
        nodeLabels.Add(string.Concat(Enumerable.Repeat("    ", numAncestors)) + node.Text);
        node = node.NextVisibleNode;
    }
    
    string outPath = Path.Combine(Path.GetDirectoryName(Interface.GetLoadedAsset().FilePath), "node_structure.txt");
    File.WriteAllText(outPath, string.Join("\n", nodeLabels));
    MessageBox.Show($"Saved to {outPath}", Interface.GetDisplayVersion());
});
