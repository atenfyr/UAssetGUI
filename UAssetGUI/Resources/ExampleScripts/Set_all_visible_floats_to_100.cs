// this example script changes the value of all float entries in the currently selected node to 100
int numChanged = 0;

void DoSwaps(List<PropertyData> props)
{
    foreach (PropertyData propData in props)
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

// UAGUtils.InvokeUI ensures that a block of code runs on the UI thread
// you should always use this whenever you are accessing UI components
// e.g. GetTreeView(), GetBaseForm()
UAGUtils.InvokeUI(() =>
{
    // get currently selected node
    var pointingTreeNode = Interface.GetTreeView().SelectedNode as PointingTreeNode;
    if (pointingTreeNode == null) return;

    if (pointingTreeNode.Pointer is NormalExport normalExport) DoSwaps(normalExport.Data);
    if (pointingTreeNode.Pointer is ArrayPropertyData arrProp) DoSwaps(arrProp.Value.ToList());
    if (pointingTreeNode.Pointer is StructPropertyData structProp) DoSwaps(structProp.Value);
    if (pointingTreeNode.Pointer is PropertyData[] arr) DoSwaps(arr.ToList());
    // more lines can be added here for other types of pointingTreeNode.Pointer

    // update display if we changed anything
    if (numChanged > 0)
    {
        Interface.GetTableHandler().Load();
        Interface.GetBaseForm().SetUnsavedChanges(true);
    }

    MessageBox.Show("Modified " + numChanged + " entries", Interface.GetDisplayVersion());
});
