UAGUtils.InvokeUI(() =>
{
    // Your code goes here
    MessageBox.Show("Welcome to scripting with C# in UAssetGUI!\n\n" + Interface.GetBaseForm().GetProjectName() + "\n" + Interface.GetDisplayVersion() + "\n" + UAPUtils.DisplayVersion, "Hello, world!");
});
