using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    /// <summary>
    /// Interface used for interacting with UAssetGUI scripts. An instance is provided as a global variable called "Interface" in all scripts.
    /// </summary>
    public interface IScriptInterface
    {
        public string GetDisplayVersion();
        public TableHandler GetTableHandler();
        public Form1 GetBaseForm();
        public ColorfulTreeView GetTreeView();
        public FileContainerForm GetFileContainerForm();
        public UAsset GetLoadedAsset();
    }

    public class ScriptInterfaceWrapper : IScriptInterface
    {
        private Form1 baseForm;
        public string GetDisplayVersion()
        {
            return baseForm?.DisplayVersion;
        }
        public TableHandler GetTableHandler()
        {
            return baseForm?.tableEditor;
        }
        public Form1 GetBaseForm()
        {
            return baseForm;
        }
        public ColorfulTreeView GetTreeView()
        {
            return baseForm?.treeView1;
        }
        public FileContainerForm GetFileContainerForm()
        {
            foreach (var form in Application.OpenForms)
            {
                if (form is FileContainerForm fcForm)
                {
                    return fcForm;
                }
            }
            return null;
        }
        public UAsset GetLoadedAsset()
        {
            return baseForm?.tableEditor?.asset;
        }

        public ScriptInterfaceWrapper(Form1 baseForm)
        {
            this.baseForm = baseForm;
        }
    }
}
