using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace UAssetGUI
{
    public class ScriptAssemblyLoadContext : AssemblyLoadContext
    {
        public ScriptAssemblyLoadContext() : base("ScriptAssemblyLoadContext", isCollectible: true)
        {

        }
    }
}
