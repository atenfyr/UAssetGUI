// this example script changes the value of all float entries in the currently selected node to 100
UAGUtils.InvokeUI(() =>
{
    // get currently selected node
    int numChanged = 0;
    var pointingTreeNode = Interface.GetTreeView().SelectedNode as PointingTreeNode;
    if (pointingTreeNode.Type == PointingTreeNodeType.Normal && pointingTreeNode.Pointer is NormalExport normalExport)
    {
        // update all FloatProperty and DoubleProperty entries in normalExport
        foreach (PropertyData propData in normalExport.Data)
        {
            if (propData is FloatPropertyData floatPropData)
            {
                floatPropData.Value = 100;
                numChanged++;
            }
            if (propData is DoublePropertyData doublePropData)
            {
                doublePropData.Value = 100;
                numChanged++;
            }
        }
    }

    // update display if we changed anything
    if (numChanged > 0)
    {
        Interface.GetTableHandler().Load();
        Interface.GetBaseForm().SetUnsavedChanges(true);
    }

    MessageBox.Show("Modified " + numChanged + " entries", Interface.GetDisplayVersion());
});
