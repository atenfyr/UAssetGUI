using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.CustomVersions;
using UAssetAPI.ExportTypes;
using UAssetAPI.Kismet.Bytecode;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;
using static UAssetAPI.Kismet.KismetSerializer;

namespace UAssetGUI
{
    public enum TableHandlerMode
    {
        None = -1,
        GeneralInformation,
        NameMap,
        SoftObjectPathList,
        Imports,
        ExportInformation,
        SoftPackageReferences,
        DependsMap,
        WorldTileInfo,
        DataResources,
        CustomVersionContainer,
        ExportData
    }

    public enum ExportDetailsParseType
    {
        None = -1,
        Int,
        FPackageIndex,
        FName,
        EObjectFlags,
        Long,
        Bool,
        Guid,
        UInt,
        FPackageIndexList
    }

    public enum PointingTreeNodeType
    {
        Normal,
        StructData,
        ClassData,
        EnumData,
        UPropertyData,
        ByteArray,
        Dummy,
        UserDefinedStructData,
        Kismet,
        KismetByteArray
    }

    public class PointingTreeNode : TreeNode
    {
        public object Pointer;
        public PointingTreeNodeType Type;
        public int ExportNum;
        public bool WillCopyWholeExport;

        private bool _childrenInitialized = false;
        public bool ChildrenInitialized
        {
            get
            {
                return _childrenInitialized;
            }
            set
            {
                if (UAGConfig.Data.EnableDynamicTree && Type != PointingTreeNodeType.Dummy && !_childrenInitialized && value && this.Nodes.Count > 0 && this.Nodes[0] is PointingTreeNode m && m.Type == PointingTreeNodeType.Dummy) this.Nodes.RemoveAt(0);
                _childrenInitialized = value;
            }
        }

        public PointingTreeNode(string label, object pointer, PointingTreeNodeType type = 0, int exportNum = -1, bool willCopyWholeExport = false)
        {
            Pointer = pointer;
            Type = type;
            this.Text = label;
            ExportNum = exportNum;
            WillCopyWholeExport = willCopyWholeExport;

            if (UAGConfig.Data.EnableDynamicTree && Type != PointingTreeNodeType.Dummy && pointer != null)
            {
                this.Nodes.Clear();
                this.Nodes.Add(new PointingTreeNode("", null, PointingTreeNodeType.Dummy, -1, false));
            }
        }
    }

    public class ExportPointingTreeNode : PointingTreeNode
    {
        public string ObjectName;

        public ExportPointingTreeNode(string objectName, object pointer, PointingTreeNodeType type = 0, int exportNum = -1, bool willCopyWholeExport = false)
            : base("Export " + (exportNum + 1) + " (" + objectName + ")", pointer, type, exportNum, willCopyWholeExport)
        {
            ObjectName = objectName;

            ToolStripMenuItem tsmItem = new ToolStripMenuItem("Copy object name");
            tsmItem.Click += (sender, args) => Clipboard.SetText(ObjectName);
            this.ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Items.Add(tsmItem);
        }
    }

    public class PointingDictionaryEntry
    {
        public KeyValuePair<PropertyData, PropertyData> Entry;
        public object Pointer;

        public PointingDictionaryEntry(KeyValuePair<PropertyData, PropertyData> entry, object pointer)
        {
            Entry = entry;
            Pointer = pointer;
        }
    }

    public class TableHandler
    {
        public TableHandlerMode mode;
        public UAsset asset;
        public TreeView treeView1;
        public DataGridView dataGridView1;
        public TextBox jsonView;

        public bool readyToSave = true;
        public bool dirtySinceLastLoad = false; 

        public static Color ARGBtoRGB(Color ARGB)
        {
            double alpha = ARGB.A / 255.0;
            return Color.FromArgb(
                255,
                (int)((ARGB.R * alpha) + (255 * (1 - alpha))),
                (int)((ARGB.G * alpha) + (255 * (1 - alpha))),
                (int)((ARGB.B * alpha) + (255 * (1 - alpha)))
            );
        }

        public void FillOutSubnodes(PointingTreeNode topNode, bool fillAllSubNodes)
        {
            if (topNode.ChildrenInitialized) return;

            topNode.ChildrenInitialized = true;
            if (topNode.Pointer is NormalExport me1)
            {
                for (int j = 0; j < me1.Data.Count; j++) InterpretThing(me1.Data[j], topNode, topNode.ExportNum, fillAllSubNodes);
            }
            else if (topNode.Pointer is PropertyData me)
            {
                switch (me.PropertyType.Value)
                {
                    case "StructProperty":
                    case "ClothLODData":
                        var struc = (StructPropertyData)me;
                        for (int j = 0; j < struc.Value.Count; j++)
                        {
                            InterpretThing(struc.Value[j], topNode, topNode.ExportNum, fillAllSubNodes);
                        }
                        break;
                    case "SetProperty":
                    case "ArrayProperty":
                        var arr = (ArrayPropertyData)me;

                        for (int j = 0; j < arr.Value.Length; j++)
                        {
                            InterpretThing(arr.Value[j], topNode, topNode.ExportNum, fillAllSubNodes);
                        }
                        break;
                    case "MapProperty":
                        var mapp = (MapPropertyData)me;

                        foreach (var entry in mapp.Value)
                        {
                            entry.Key.Name = FName.DefineDummy(asset, "Key");
                            entry.Value.Name = FName.DefineDummy(asset, "Value");

                            var softEntryNode = new PointingTreeNode(mapp.Name.Value.Value + " (2)", new PointingDictionaryEntry(entry, mapp), 0, topNode.ExportNum);
                            topNode.Nodes.Add(softEntryNode);
                            InterpretThing(entry.Key, softEntryNode, topNode.ExportNum, fillAllSubNodes);
                            InterpretThing(entry.Value, softEntryNode, topNode.ExportNum, fillAllSubNodes);
                        }
                        break;
                    case "NiagaraVariableBase":
                    case "NiagaraVariable":
                    case "NiagaraVariableWithOffset":
                        InterpretThing(((NiagaraVariableBasePropertyData)me).TypeDef, topNode, topNode.ExportNum, fillAllSubNodes);
                        break;
                }
            }
        }

        public void FillOutTree(bool fillAllSubNodes)
        {
            int numDependsInts = 0;
            if (asset.DependsMap != null)
            {
                foreach (var entry in asset.DependsMap) numDependsInts += entry.Length;
            }
            // if numDependsInts == 0, then it's really just unused

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.BackColor = UAGPalette.BackColor;
            treeView1.Nodes.Add(new PointingTreeNode("General Information", null));
            if (asset.SoftObjectPathList != null && (asset.SoftObjectPathList.Count > 0 || !asset.IsFilterEditorOnly)) treeView1.Nodes.Add(new PointingTreeNode("Soft Object Paths", null));
            treeView1.Nodes.Add(new PointingTreeNode("Name Map", null));
            treeView1.Nodes.Add(new PointingTreeNode("Import Data", null));
            treeView1.Nodes.Add(new PointingTreeNode("Export Information", null));
            if (numDependsInts != 0) treeView1.Nodes.Add(new PointingTreeNode("Depends Map", null));
            if (asset.SoftPackageReferenceList != null) treeView1.Nodes.Add(new PointingTreeNode("Soft Package References", null));
            if (asset.WorldTileInfo != null)
            {
                treeView1.Nodes.Add(new PointingTreeNode("World Tile Info", null));
                PointingTreeNode worldInfoNode = (PointingTreeNode)treeView1.Nodes[treeView1.Nodes.Count - 1];
                worldInfoNode.Nodes.Add(new PointingTreeNode("Layer (5)", asset.WorldTileInfo.Layer));
                worldInfoNode.Nodes.Add(new PointingTreeNode("LODList (" + asset.WorldTileInfo.LODList.Length + ")", asset.WorldTileInfo.LODList));
                PointingTreeNode lodListNode = (PointingTreeNode)treeView1.Nodes[treeView1.Nodes.Count - 1];
                for (int i = 0; i < asset.WorldTileInfo.LODList.Length; i++)
                {
                    lodListNode.Nodes.Add(new PointingTreeNode("LOD entry #" + (i + 1), asset.WorldTileInfo.LODList[i]));
                }
            }
            if (asset.ObjectVersionUE5 >= ObjectVersionUE5.DATA_RESOURCES) treeView1.Nodes.Add(new PointingTreeNode("Data Resources", null));
            treeView1.Nodes.Add(new PointingTreeNode("Custom Version Container", null));
            treeView1.Nodes.Add(new PointingTreeNode("Export Data", null));

            PointingTreeNode superTopNode = (PointingTreeNode)treeView1.Nodes[treeView1.Nodes.Count - 1];
            for (int i = 0; i < asset.Exports.Count; i++)
            {
                Export baseUs = asset.Exports[i];
                var categoryNode = new ExportPointingTreeNode(baseUs.ObjectName.Value.Value, null, 0, i, true);
                superTopNode.Nodes.Add(categoryNode);
                switch (baseUs)
                {
                    case RawExport us3:
                        {
                            var parentNode = new PointingTreeNode("Raw Data (" + us3.Data.Length + " B)", us3, PointingTreeNodeType.ByteArray, i);
                            parentNode.ChildrenInitialized = true;
                            categoryNode.Nodes.Add(parentNode);
                            break;
                        }
                    case NormalExport us:
                        {
                            var parentNode = new PointingTreeNode((baseUs.ClassIndex.IsImport() ? baseUs.ClassIndex.ToImport(asset).ObjectName.Value.Value : baseUs.ClassIndex.Index.ToString()) + " (" + us.Data.Count + ")", us, 0, i);
                            categoryNode.Nodes.Add(parentNode);

                            if (fillAllSubNodes)
                            {
                                for (int j = 0; j < us.Data.Count; j++) InterpretThing(us.Data[j], parentNode, i, fillAllSubNodes);
                            }

                            bool hasChildren = false;
                            for (int j = 0; j < us.Data.Count; j++)
                            {
                                if (hasChildrenProperties.Contains(us.Data[j].PropertyType.Value))
                                {
                                    hasChildren = true;
                                    break;
                                }
                            }
                            if (!hasChildren) parentNode.ChildrenInitialized = true;

                            if (us is StringTableExport us2)
                            {
                                var parentNode2 = new PointingTreeNode((us2.Table?.TableNamespace?.ToString() ?? FString.NullCase) + " (" + us2.Table.Count + ")", us2.Table, 0, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                            }

                            if (us is StructExport structUs)
                            {
                                var parentNode2 = new PointingTreeNode("UStruct Data", structUs, PointingTreeNodeType.StructData, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                                if (structUs.ScriptBytecode == null)
                                {
                                    var bytecodeNode = new PointingTreeNode("ScriptBytecode (" + structUs.ScriptBytecodeRaw.Length + " B)", structUs, PointingTreeNodeType.KismetByteArray, i);
                                    bytecodeNode.ChildrenInitialized = true;
                                    parentNode2.Nodes.Add(bytecodeNode);
                                }
                                else
                                {
                                    var bytecodeNode = new PointingTreeNode("ScriptBytecode (" + structUs.ScriptBytecode.Length + " instructions)", structUs, PointingTreeNodeType.Kismet, i);
                                    bytecodeNode.ChildrenInitialized = true;
                                    parentNode2.Nodes.Add(bytecodeNode);
                                }
                            }

                            if (us is UserDefinedStructExport us6)
                            {
                                var parentNode2 = new PointingTreeNode("UserDefinedStruct Data (" + us6.StructData.Count + ")", us, PointingTreeNodeType.UserDefinedStructData, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);

                                for (int j = 0; j < us6.StructData.Count; j++) InterpretThing(us6.StructData[j], parentNode2, i, fillAllSubNodes);
                            }

                            if (us is ClassExport)
                            {
                                var parentNode2 = new PointingTreeNode("UClass Data", (ClassExport)us, PointingTreeNodeType.ClassData, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                            }

                            if (us is PropertyExport)
                            {
                                var parentNode2 = new PointingTreeNode("UProperty Data", (PropertyExport)us, PointingTreeNodeType.UPropertyData, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                            }

                            if (us is DataTableExport us4)
                            {
                                var parentNode2 = new PointingTreeNode("Table Info (" + us4.Table.Data.Count + ")", us4.Table, 0, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                                foreach (StructPropertyData entry in us4.Table.Data)
                                {
                                    string decidedName = entry.Name.Value.Value;

                                    var structNode = new PointingTreeNode(decidedName + " (" + entry.Value.Count + ")", entry, 0, i);
                                    parentNode2.Nodes.Add(structNode);
                                    if (fillAllSubNodes)
                                    {
                                        for (int j = 0; j < entry.Value.Count; j++)
                                        {
                                            InterpretThing(entry.Value[j], structNode, i, fillAllSubNodes);
                                        }
                                    }
                                }
                            }

                            if (us is EnumExport us5)
                            {
                                var parentNode2 = new PointingTreeNode("Enum Data", us5, PointingTreeNodeType.EnumData, i);
                                parentNode2.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode2);
                            }

                            {
                                var parentNode3 = new PointingTreeNode("Extra Data (" + us.Extras.Length + " B)", us, PointingTreeNodeType.ByteArray, i);
                                parentNode3.ChildrenInitialized = true;
                                categoryNode.Nodes.Add(parentNode3);
                            }

                            break;
                        }
                }
            }

            treeView1.SelectedNode = treeView1.Nodes[0];
            treeView1.EndUpdate();
        }

        private HashSet<string> hasChildrenProperties = new HashSet<string>()
        {
            "StructProperty",
            "ClothLODData",
            "SetProperty",
            "ArrayProperty",
            "GameplayTagContainer",
            "MapProperty",
            "MulticastDelegateProperty",
            "Box",
            "Box2D",
            "Box2f",
            "NiagaraVariableBase",
            "NiagaraVariable",
            "NiagaraVariableWithOffset"
        };

        private void InterpretThing(PropertyData me, PointingTreeNode ourNode, int exportNum, bool fillAllSubNodes)
        {
            ourNode.ChildrenInitialized = true;
            if (me == null) return;
            switch (me.PropertyType.Value)
            {
                case "StructProperty":
                case "ClothLODData":
                    var struc = (StructPropertyData)me;

                    string decidedName = struc.Name.Value.Value;
                    if (ourNode.Pointer is PropertyData && ((PropertyData)ourNode.Pointer).Name.Equals(decidedName)) decidedName = struc.StructType.Value.Value;

                    var structNode = new PointingTreeNode(decidedName + " (" + struc.Value.Count + ")", struc, 0, exportNum);
                    ourNode.Nodes.Add(structNode);

                    bool hasChildren = false;
                    if (fillAllSubNodes)
                    {
                        for (int j = 0; j < struc.Value.Count; j++)
                        {
                            InterpretThing(struc.Value[j], structNode, exportNum, fillAllSubNodes);
                        }
                    }
                    else
                    {
                        // check if there is children
                        for (int j = 0; j < struc.Value.Count; j++)
                        {
                            if (hasChildrenProperties.Contains(struc.Value[j].PropertyType.Value))
                            {
                                hasChildren = true;
                                break;
                            }
                        }
                    }

                    if (!hasChildren) structNode.ChildrenInitialized = true;
                    break;
                case "SetProperty":
                case "ArrayProperty":
                    var arr = (ArrayPropertyData)me;

                    var arrNode = new PointingTreeNode(arr.Name.Value.Value + " (" + arr.Value.Length + ")", arr, 0, exportNum);
                    ourNode.Nodes.Add(arrNode);

                    bool hasChildren2 = false;
                    if (fillAllSubNodes)
                    {
                        for (int j = 0; j < arr.Value.Length; j++)
                        {
                            InterpretThing(arr.Value[j], arrNode, exportNum, fillAllSubNodes);
                        }
                    }
                    else
                    {
                        // check if there is children
                        for (int j = 0; j < arr.Value.Length; j++)
                        {
                            if (hasChildrenProperties.Contains(arr.Value[j].PropertyType.Value))
                            {
                                hasChildren2 = true;
                                break;
                            }
                        }
                    }

                    if (!hasChildren2) arrNode.ChildrenInitialized = true;
                    break;
                case "GameplayTagContainer":
                    var arr2 = (GameplayTagContainerPropertyData)me;

                    var arrNode2 = new PointingTreeNode(arr2.Name.Value.Value + " (" + arr2.Value.Length + ")", arr2, 0, exportNum);
                    ourNode.Nodes.Add(arrNode2);
                    break;
                case "MapProperty":
                    var mapp = (MapPropertyData)me;

                    var mapNode = new PointingTreeNode(mapp.Name.Value.Value + " (" + mapp.Value.Keys.Count + ")", mapp, 0, exportNum);
                    ourNode.Nodes.Add(mapNode);

                    foreach (var entry in mapp.Value)
                    {
                        entry.Key.Name = FName.DefineDummy(asset, "Key");
                        entry.Value.Name = FName.DefineDummy(asset, "Value");

                        var softEntryNode = new PointingTreeNode(mapp.Name.Value.Value + " (2)", new PointingDictionaryEntry(entry, mapp), 0, exportNum);
                        mapNode.Nodes.Add(softEntryNode);
                        InterpretThing(entry.Key, softEntryNode, exportNum, fillAllSubNodes);
                        InterpretThing(entry.Value, softEntryNode, exportNum, fillAllSubNodes);
                    }

                    mapNode.ChildrenInitialized = true;
                    break;
                case "MulticastDelegateProperty":
                    var mdp = (MulticastDelegatePropertyData)me;

                    ourNode.Nodes.Add(new PointingTreeNode(mdp.Name.Value.Value + " (" + mdp.Value.Length + ")", mdp.Value, 0, exportNum));
                    break;
                case "Box":
                    {
                        var box = (BoxPropertyData)me;

                        ourNode.Nodes.Add(new PointingTreeNode(box.Name.Value.Value + " (2)", box, 0, exportNum));
                    }
                    break;
                case "Box2D":
                    {
                        var box = (Box2DPropertyData)me;

                        ourNode.Nodes.Add(new PointingTreeNode(box.Name.Value.Value + " (2)", box, 0, exportNum));
                    }
                    break;
                case "Box2f":
                    {
                        var box = (Box2fPropertyData)me;

                        ourNode.Nodes.Add(new PointingTreeNode(box.Name.Value.Value + " (2)", box, 0, exportNum));
                    }
                    break;
                case "NiagaraVariableBase":
                case "NiagaraVariable":
                case "NiagaraVariableWithOffset":
                    InterpretThing(((NiagaraVariableBasePropertyData)me).TypeDef, ourNode, exportNum, fillAllSubNodes);
                    break;
            }
        }

        private void AddRowsForArray(PropertyData[] arr)
        {
            if (arr.Length == 0) return;

            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            for (int i = 0; i < arr.Length; i++)
            {
                PropertyData thisPD = arr[i];
                if (thisPD == null) continue;

                //try
                {
                    int columnIndexer = -1;
                    int absoluteColumnIndexer = columnIndexer;
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1);
                    row.Cells[++columnIndexer].Value = thisPD.Name.ToString();
                    row.Cells[++columnIndexer].Value = thisPD.PropertyType.ToString();
                    if (thisPD is UnknownPropertyData)
                    {
                        row.Cells[columnIndexer].Value = ((UnknownPropertyData)thisPD).SerializingPropertyType.ToString();
                        row.Cells[++columnIndexer].Value = "Unknown ser.";
                        row.Cells[++columnIndexer].Value = ((UnknownPropertyData)thisPD).Value.ConvertByteArrayToString();
                    }
                    else
                    {
                        switch (thisPD.PropertyType.Value)
                        {
                            case "BoolProperty":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((BoolPropertyData)thisPD).Value.ToString();
                                break;
                            case "FloatProperty":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((FloatPropertyData)thisPD).Value.ToString();
                                break;
                            case "DoubleProperty":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((DoublePropertyData)thisPD).Value.ToString();
                                break;
                            case "FrameNumber":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((FrameNumberPropertyData)thisPD).Value.Value.ToString();
                                break;
                            case "ObjectProperty":
                                var objData = (ObjectPropertyData)thisPD;
                                int decidedIndex = objData.Value?.Index ?? 0;
                                row.Cells[++columnIndexer].Value = decidedIndex;
                                if (decidedIndex != 0) UAGUtils.UpdateObjectPropertyValues(asset, row, dataGridView1, objData.Value);
                                break;
                            case "SoftObjectProperty":
                                var objData2 = (SoftObjectPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = objData2.Value.AssetPath.PackageName == null ? FString.NullCase : objData2.Value.AssetPath.PackageName.ToString();
                                row.Cells[columnIndexer].ToolTipText = "AssetPath.PackageName";
                                row.Cells[++columnIndexer].Value = objData2.Value.AssetPath.AssetName == null ? FString.NullCase : objData2.Value.AssetPath.AssetName.ToString();
                                row.Cells[columnIndexer].ToolTipText = "AssetPath.AssetName";
                                row.Cells[++columnIndexer].Value = objData2.Value.SubPathString == null ? FString.NullCase : objData2.Value.SubPathString.ToString();
                                row.Cells[columnIndexer].ToolTipText = "SubPathString";
                                break;
                            case "RichCurveKey":
                                var curveData = (RichCurveKeyPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = curveData.Value.InterpMode;
                                row.Cells[++columnIndexer].Value = curveData.Value.TangentMode;
                                row.Cells[++columnIndexer].Value = curveData.Value.Time;
                                row.Cells[++columnIndexer].Value = curveData.Value.Value;
                                row.Cells[++columnIndexer].Value = curveData.Value.ArriveTangent;
                                row.Cells[++columnIndexer].Value = curveData.Value.LeaveTangent;
                                break;
                            case "TextProperty":
                                var txtData = (TextPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = txtData.HistoryType.ToString();
                                row.Cells[columnIndexer].ToolTipText = "HistoryType";
                                switch (txtData.HistoryType)
                                {
                                    case TextHistoryType.None:
                                        row.Cells[++columnIndexer].Value = txtData?.CultureInvariantString == null ? FString.NullCase : txtData.CultureInvariantString.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "CultureInvariantString";
                                        break;
                                    case TextHistoryType.Base:
                                        row.Cells[++columnIndexer].Value = txtData?.Namespace == null ? FString.NullCase : txtData.Namespace.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "Namespace";
                                        row.Cells[++columnIndexer].Value = txtData?.Value == null ? FString.NullCase : txtData.Value.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "Key";
                                        row.Cells[++columnIndexer].Value = txtData?.CultureInvariantString == null ? FString.NullCase : txtData.CultureInvariantString.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "CultureInvariantString";
                                        break;
                                    case TextHistoryType.StringTableEntry:
                                        row.Cells[++columnIndexer].Value = txtData?.TableId == null ? FString.NullCase : txtData.TableId.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "TableId";
                                        row.Cells[++columnIndexer].Value = txtData?.Value == null ? FString.NullCase : txtData.Value.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "Key";
                                        break;
                                    case TextHistoryType.RawText:
                                        row.Cells[++columnIndexer].Value = txtData?.Value == null ? FString.NullCase : txtData.Value.ToString();
                                        row.Cells[columnIndexer].ToolTipText = "Value";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "NameProperty":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((NamePropertyData)thisPD).ToString();
                                break;
                            case "ViewTargetBlendParams":
                                var viewTargetBlendParamsData = (ViewTargetBlendParamsPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = viewTargetBlendParamsData.BlendTime;
                                row.Cells[++columnIndexer].Value = viewTargetBlendParamsData.BlendFunction;
                                row.Cells[++columnIndexer].Value = viewTargetBlendParamsData.BlendExp;
                                row.Cells[++columnIndexer].Value = viewTargetBlendParamsData.bLockOutgoing;
                                break;
                            case "EnumProperty":
                                var enumData = (EnumPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = enumData.EnumType?.Value?.Value == null ? FString.NullCase : enumData.EnumType.ToString();
                                row.Cells[++columnIndexer].Value = enumData.Value?.Value?.Value == null ? FString.NullCase : enumData.Value.ToString();
                                //row.Cells[5].Value = enumData.Extra;
                                break;
                            case "ByteProperty":
                                var byteData = (BytePropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = byteData.GetEnumBase()?.Value.Value == null ? FString.NullCase : byteData.GetEnumBase()?.Value.Value;
                                if (byteData.ByteType == BytePropertyType.Byte)
                                {
                                    row.Cells[++columnIndexer].Value = byteData.Value;
                                }
                                else
                                {
                                    row.Cells[++columnIndexer].Value = byteData.GetEnumFull()?.Value.Value == null ? FString.NullCase : byteData.GetEnumFull()?.Value.Value;
                                }
                                break;
                            case "StructProperty":
                            case "ClothLODData":
                                row.Cells[++columnIndexer].Value = ((StructPropertyData)thisPD).StructType?.ToString() ?? FString.NullCase;
                                break;
                            case "ArrayProperty":
                            case "SetProperty":
                                row.Cells[++columnIndexer].Value = ((ArrayPropertyData)thisPD).ArrayType?.ToString() ?? FString.NullCase;
                                break;
                            case "GameplayTagContainer":
                            case "MapProperty":
                            case "SkeletalMeshSamplingLODBuiltData":
                                break;
                            case "Box":
                            case "Box2D":
                            case "Box2f":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                break;
                            case "MulticastDelegateProperty":
                                var mdpData = (MulticastDelegatePropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                break;
                            case "LinearColor":
                                var colorData = (LinearColorPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[columnIndexer].ReadOnly = true;
                                if (colorData.RawValue != null)
                                {
                                    row.Cells[columnIndexer].Style.BackColor = ARGBtoRGB(LinearHelpers.Convert(colorData.Value));
                                    row.Cells[columnIndexer].ToolTipText = "Preview";
                                }
                                row.Cells[++columnIndexer].Value = colorData.Value.R;
                                row.Cells[columnIndexer].ToolTipText = "Red";
                                row.Cells[++columnIndexer].Value = colorData.Value.G;
                                row.Cells[columnIndexer].ToolTipText = "Green";
                                row.Cells[++columnIndexer].Value = colorData.Value.B;
                                row.Cells[columnIndexer].ToolTipText = "Blue";
                                row.Cells[++columnIndexer].Value = colorData.Value.A;
                                row.Cells[columnIndexer].ToolTipText = "Alpha";
                                break;
                            case "Color":
                                var colorData2 = (ColorPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[columnIndexer].ReadOnly = true;
                                if (colorData2.RawValue != null)
                                {
                                    row.Cells[columnIndexer].Style.BackColor = colorData2.Value;
                                    row.Cells[columnIndexer].ToolTipText = "Preview";
                                }
                                row.Cells[++columnIndexer].Value = colorData2.Value.R;
                                row.Cells[columnIndexer].ToolTipText = "Red";
                                row.Cells[++columnIndexer].Value = colorData2.Value.G;
                                row.Cells[columnIndexer].ToolTipText = "Green";
                                row.Cells[++columnIndexer].Value = colorData2.Value.B;
                                row.Cells[columnIndexer].ToolTipText = "Blue";
                                row.Cells[++columnIndexer].Value = colorData2.Value.A;
                                row.Cells[columnIndexer].ToolTipText = "Alpha";
                                break;
                            case "Vector":
                                var vectorData = (VectorPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = vectorData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = vectorData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                row.Cells[++columnIndexer].Value = vectorData.Value.Z;
                                row.Cells[columnIndexer].ToolTipText = "Z";
                                break;
                            case "Vector2D":
                                var vector2DData = (Vector2DPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = vector2DData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = vector2DData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                break;
                            case "Vector4":
                                var vector4DData = (Vector4PropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = vector4DData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = vector4DData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                row.Cells[++columnIndexer].Value = vector4DData.Value.Z;
                                row.Cells[columnIndexer].ToolTipText = "Z";
                                row.Cells[++columnIndexer].Value = vector4DData.Value.W;
                                row.Cells[columnIndexer].ToolTipText = "W";
                                break;
                            case "Plane":
                                var planeData = (PlanePropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = planeData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X (Xx)";
                                row.Cells[++columnIndexer].Value = planeData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y (+Yy)";
                                row.Cells[++columnIndexer].Value = planeData.Value.Z;
                                row.Cells[columnIndexer].ToolTipText = "Z (+Zz)";
                                row.Cells[++columnIndexer].Value = planeData.Value.W;
                                row.Cells[columnIndexer].ToolTipText = "W (= W)";
                                break;
                            case "IntPoint":
                                var intPointData = (IntPointPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = intPointData.Value[0];
                                row.Cells[++columnIndexer].Value = intPointData.Value[1];
                                break;
                            case "IntVector2":
                                var intVector2Data = (IntVector2PropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = intVector2Data.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = intVector2Data.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                break;
                            case "IntVector":
                                var intVectorData = (IntVectorPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = intVectorData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = intVectorData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                row.Cells[++columnIndexer].Value = intVectorData.Value.Z;
                                row.Cells[columnIndexer].ToolTipText = "Z";
                                break;
                            case "FloatRange":
                                var floatRangeData = (FloatRangePropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = floatRangeData.LowerBound;
                                row.Cells[columnIndexer].ToolTipText = "LowerBound";
                                row.Cells[++columnIndexer].Value = floatRangeData.UpperBound;
                                row.Cells[columnIndexer].ToolTipText = "UpperBound";
                                break;
                            case "Guid":
                                var guidData = (GuidPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = guidData.Value.ConvertToString();
                                break;
                            case "Rotator":
                                var rotatorData = (RotatorPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = rotatorData.Value.Roll;
                                row.Cells[columnIndexer].ToolTipText = "Roll (X)";
                                row.Cells[++columnIndexer].Value = rotatorData.Value.Pitch;
                                row.Cells[columnIndexer].ToolTipText = "Pitch (Y)";
                                row.Cells[++columnIndexer].Value = rotatorData.Value.Yaw;
                                row.Cells[columnIndexer].ToolTipText = "Yaw (Z)";
                                break;
                            case "Quat":
                                var quatData = (QuatPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = quatData.Value.X;
                                row.Cells[columnIndexer].ToolTipText = "X";
                                row.Cells[++columnIndexer].Value = quatData.Value.Y;
                                row.Cells[columnIndexer].ToolTipText = "Y";
                                row.Cells[++columnIndexer].Value = quatData.Value.Z;
                                row.Cells[columnIndexer].ToolTipText = "Z";
                                row.Cells[++columnIndexer].Value = quatData.Value.W;
                                row.Cells[columnIndexer].ToolTipText = "W";
                                break;
                            case "PerPlatformBool":
                                {
                                    var PerPlatformData = (PerPlatformBoolPropertyData)thisPD;
                                    row.Cells[++columnIndexer].Value = string.Empty;
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 0 ? PerPlatformData.Value[0].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[0]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 1 ? PerPlatformData.Value[1].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[1]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 2 ? PerPlatformData.Value[2].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[2]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 3 ? PerPlatformData.Value[3].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[3]";
                                }
                                break;
                            case "PerPlatformInt":
                                {
                                    var PerPlatformData = (PerPlatformIntPropertyData)thisPD;
                                    row.Cells[++columnIndexer].Value = string.Empty;
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 0 ? PerPlatformData.Value[0].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[0]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 1 ? PerPlatformData.Value[1].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[1]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 2 ? PerPlatformData.Value[2].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[2]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 3 ? PerPlatformData.Value[3].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[3]";
                                }
                                break;
                            case "PerPlatformFloat":
                                {
                                    var PerPlatformData = (PerPlatformFloatPropertyData)thisPD;
                                    row.Cells[++columnIndexer].Value = string.Empty;
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 0 ? PerPlatformData.Value[0].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[0]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 1 ? PerPlatformData.Value[1].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[1]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 2 ? PerPlatformData.Value[2].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[2]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 3 ? PerPlatformData.Value[3].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[3]";
                                }
                                break;
                            case "PerPlatformFrameRate":
                                {
                                    var PerPlatformData = (PerPlatformFrameRatePropertyData)thisPD;
                                    row.Cells[++columnIndexer].Value = string.Empty;
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 0 ? PerPlatformData.Value[0].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[0]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 1 ? PerPlatformData.Value[1].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[1]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 2 ? PerPlatformData.Value[2].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[2]";
                                    row.Cells[++columnIndexer].Value = PerPlatformData.Value.Length > 3 ? PerPlatformData.Value[3].ToString() : string.Empty;
                                    row.Cells[columnIndexer].ToolTipText = "[3]";
                                }
                                break;
                            case "NiagaraVariableBase":
                            case "NiagaraVariable":
                            case "NiagaraVariableWithOffset":
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = ((NiagaraVariableBasePropertyData)thisPD).VariableName;
                                row.Cells[columnIndexer].ToolTipText = "VariableName";
                                row.Cells[++columnIndexer].Value = "NiagaraTypeDefinition"; // just for display really
                                row.Cells[columnIndexer].ToolTipText = "TypeDef";
                                if (thisPD.PropertyType.Value == "NiagaraVariable")
                                {
                                    row.Cells[++columnIndexer].Value = ((NiagaraVariablePropertyData)thisPD).VarData.ConvertByteArrayToString();
                                    row.Cells[columnIndexer].ToolTipText = "VarData";
                                }
                                else if (thisPD.PropertyType.Value == "NiagaraVariableWithOffset")
                                {
                                    row.Cells[++columnIndexer].Value = ((NiagaraVariableWithOffsetPropertyData)thisPD).VariableOffset;
                                    row.Cells[columnIndexer].ToolTipText = "VariableOffset";
                                }
                                break;
                            case "SmartName":
                                var smartNameData = (SmartNamePropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = smartNameData.DisplayName == null ? FString.NullCase : smartNameData.DisplayName.ToString();
                                row.Cells[columnIndexer].ToolTipText = "DisplayName";
                                if (asset.GetCustomVersion<FAnimPhysObjectVersion>() < FAnimPhysObjectVersion.RemoveUIDFromSmartNameSerialize)
                                {
                                    row.Cells[++columnIndexer].Value = smartNameData.SmartNameID;
                                    row.Cells[columnIndexer].ToolTipText = "SmartNameID";
                                }
                                if (asset.GetCustomVersion<FAnimPhysObjectVersion>() < FAnimPhysObjectVersion.SmartNameRefactorForDeterministicCooking)
                                {
                                    row.Cells[++columnIndexer].Value = smartNameData.TempGUID.ConvertToString();
                                    row.Cells[columnIndexer].ToolTipText = "TempGUID";
                                }
                                break;
                            case "StrProperty":
                                var strPropData = (StrPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = (strPropData.Value?.Encoding ?? Encoding.ASCII).HeaderName;
                                row.Cells[++columnIndexer].Value = strPropData.Value?.Value == null ? FString.NullCase : Convert.ToString(strPropData.Value.Value);
                                break;
                            case "SoftObjectPath":
                            case "SoftAssetPath":
                            case "SoftClassPath":
                            case "StringAssetReference":
                                var sopPropData = (SoftObjectPathPropertyData)thisPD;
                                row.Cells[++columnIndexer].Value = string.Empty;
                                if (asset.ObjectVersion < ObjectVersion.VER_UE4_ADDED_SOFT_OBJECT_PATH)
                                {
                                    row.Cells[++columnIndexer].Value = sopPropData.Path == null ? FString.NullCase : sopPropData.Path.ToString();
                                    row.Cells[columnIndexer].ToolTipText = "Path";
                                }
                                else
                                {
                                    row.Cells[++columnIndexer].Value = sopPropData.Value.AssetPath.PackageName == null ? FString.NullCase : sopPropData.Value.AssetPath.PackageName.ToString();
                                    row.Cells[columnIndexer].ToolTipText = "AssetPath.PackageName";
                                    row.Cells[++columnIndexer].Value = sopPropData.Value.AssetPath.AssetName == null ? FString.NullCase : sopPropData.Value.AssetPath.AssetName.ToString();
                                    row.Cells[columnIndexer].ToolTipText = "AssetPath.AssetName";
                                    row.Cells[++columnIndexer].Value = sopPropData.Value.SubPathString == null ? FString.NullCase : sopPropData.Value.SubPathString.ToString();
                                    row.Cells[columnIndexer].ToolTipText = "SubPathString";
                                }
                                break;
                            default:
                                row.Cells[++columnIndexer].Value = string.Empty;
                                row.Cells[++columnIndexer].Value = Convert.ToString(thisPD.RawValue);
                                break;
                        }
                    }

                    long determinedOffset = asset.UseSeparateBulkDataFiles ? (thisPD.Offset - asset.Exports[0].SerialOffset) : thisPD.Offset;

                    row.Cells[absoluteColumnIndexer + 9].Value = thisPD.ArrayIndex;
                    row.Cells[absoluteColumnIndexer + 10].Value = determinedOffset < 0 ? "N/A" : determinedOffset.ToString();
                    row.Cells[absoluteColumnIndexer + 10].ReadOnly = true;
                    row.Cells[absoluteColumnIndexer + 11].Value = thisPD.IsZero.ToString();
                    row.HeaderCell.Value = Convert.ToString(i);
                    rows.Add(row);
                }
                //catch (Exception)
                //{

                //}
            }
            dataGridView1.Rows.AddRange(rows.ToArray());
        }

        /// <summary>
        /// Interpret a specific row in the current data grid view as a PropertyData instance and return it.
        /// </summary>
        /// <param name="rowNum">The row number.</param>
        /// <param name="original">The original PropertyData instance that this row was intended to represent. Used to clone values not represented in the display.</param>
        /// <param name="namesAreDummies">Whether or not the Name column is not serialized to disk (so shouldn't be appended to the name map).</param>
        /// <param name="useUnversionedProperties">Whether or not unversioned properties are being used. If true, namesAreDummies is overriden to be true.</param>
        /// <param name="expectedContext">Expected PropertySerializationContext, otherwise Normal.</param>
        /// <returns>The interpreted PropertyData instance.</returns>
        private PropertyData RowToPD(int rowNum, PropertyData original, bool namesAreDummies = false, bool useUnversionedProperties = false, PropertySerializationContext expectedContext = PropertySerializationContext.Normal)
        {
            if (useUnversionedProperties) namesAreDummies = true;

            try
            {
                DataGridViewRow row = dataGridView1.Rows[rowNum];
                int columnIndexer = -1;
                object nameB = row.Cells[++columnIndexer].Value;
                object typeB = row.Cells[++columnIndexer].Value;
                object transformB = row.Cells[++columnIndexer].Value;
                object value1B = row.Cells[++columnIndexer].Value;
                object value2B = row.Cells[++columnIndexer].Value;
                object value3B = row.Cells[++columnIndexer].Value;
                object value4B = row.Cells[++columnIndexer].Value;
                object value5B = row.Cells[++columnIndexer].Value;

                if (nameB == null || typeB == null) return null;
                if (!(nameB is string) || !(typeB is string)) return null;

                string name = ((string)nameB)?.Trim();
                string type = ((string)typeB)?.Trim();
                if (name.Equals(string.Empty) || type.Equals(string.Empty)) return null;

                FName nameName = namesAreDummies ? FName.DefineDummy(asset, name) : FName.FromString(asset, name);
                PropertyData finalProp = null;

                if (value1B != null && value1B is string && transformB != null && transformB is string && (string)transformB == "Unknown ser.")
                {
                    finalProp = new UnknownPropertyData(nameName)
                    {
                        Value = ((string)value1B).ConvertStringToByteArray()
                    };
                    ((UnknownPropertyData)finalProp).SetSerializingPropertyType(new FString(type));
                }
                else
                {
                    switch ((useUnversionedProperties ? FName.DefineDummy(asset, type) : FName.FromString(asset, type)).Value.Value)
                    {
                        case "TextProperty":
                            TextPropertyData decidedTextData = null;
                            if (original != null && original is TextPropertyData)
                            {
                                decidedTextData = (TextPropertyData)original;
                                decidedTextData.Name = nameName;
                            }
                            else
                            {
                                decidedTextData = new TextPropertyData(nameName);
                            }

                            TextHistoryType histType = TextHistoryType.Base;
                            if (transformB == null) return null;
                            if (transformB is string) Enum.TryParse((string)transformB, out histType);

                            decidedTextData.HistoryType = histType;
                            switch (histType)
                            {
                                case TextHistoryType.None:
                                    decidedTextData.Value = null;
                                    if (value1B != null && value1B is string) decidedTextData.CultureInvariantString = (string)value1B == FString.NullCase ? null : FString.FromString((string)value1B);
                                    break;
                                case TextHistoryType.Base:
                                    if (value1B == null || value2B == null || value3B == null || !(value1B is string) || !(value2B is string) || !(value3B is string)) return null;
                                    decidedTextData.Namespace = (string)value1B == FString.NullCase ? null : FString.FromString((string)value1B);
                                    decidedTextData.Value = (string)value2B == FString.NullCase ? null : FString.FromString((string)value2B);
                                    decidedTextData.CultureInvariantString = (string)value3B == FString.NullCase ? null : FString.FromString((string)value3B);
                                    break;
                                case TextHistoryType.StringTableEntry:
                                    if (value1B == null || !(value1B is string) || !(value2B is string)) return null;

                                    decidedTextData.TableId = FName.FromString(asset, (string)value1B);
                                    decidedTextData.Value = (string)value2B == FString.NullCase ? null : FString.FromString((string)value2B);
                                    break;
                                case TextHistoryType.RawText:
                                    if (value1B == null || !(value1B is string)) return null;
                                    decidedTextData.Value = (string)value1B == FString.NullCase ? null : FString.FromString((string)value1B);
                                    break;
                                default:
                                    break;
                            }

                            if (value4B != null && value4B is string) Enum.TryParse((string)value4B, out decidedTextData.Flags);

                            finalProp = decidedTextData;
                            break;
                        case "ObjectProperty":
                            ObjectPropertyData decidedObjData = null;
                            if (original != null && original is ObjectPropertyData)
                            {
                                decidedObjData = (ObjectPropertyData)original;
                                decidedObjData.Name = nameName;
                            }
                            else
                            {
                                decidedObjData = new ObjectPropertyData(nameName);
                            }

                            int objValue = int.MinValue;
                            if (transformB == null) return null;
                            if (transformB is string) int.TryParse((string)transformB, out objValue);
                            if (transformB is int) objValue = (int)transformB;
                            if (objValue == int.MinValue) return null;

                            decidedObjData.Value = new FPackageIndex(objValue);
                            UAGUtils.UpdateObjectPropertyValues(asset, row, dataGridView1, decidedObjData.Value);
                            finalProp = decidedObjData;
                            break;
                        case "RichCurveKey":
                            RichCurveKeyPropertyData decidedRCKProperty = null;
                            if (original != null && original is RichCurveKeyPropertyData)
                            {
                                decidedRCKProperty = (RichCurveKeyPropertyData)original;
                                decidedRCKProperty.Name = nameName;
                            }
                            else
                            {
                                decidedRCKProperty = new RichCurveKeyPropertyData(nameName);
                            }

                            FRichCurveKey nuevo = decidedRCKProperty.Value;

                            if (transformB is string) Enum.TryParse((string)transformB, out nuevo.InterpMode);
                            if (value1B is string) Enum.TryParse((string)value1B, out nuevo.TangentMode);

                            if (value2B is string) float.TryParse((string)value2B, out nuevo.Time);
                            if (value2B is int) nuevo.Time = (float)(int)value2B;
                            if (value2B is float) nuevo.Time = (float)value2B;
                            if (value3B is string) float.TryParse((string)value3B, out nuevo.Value);
                            if (value3B is int) nuevo.Value = (float)(int)value3B;
                            if (value3B is float) nuevo.Value = (float)value3B;
                            if (value4B is string) float.TryParse((string)value4B, out nuevo.ArriveTangent);
                            if (value4B is int) nuevo.ArriveTangent = (float)(int)value4B;
                            if (value4B is float) nuevo.ArriveTangent = (float)value4B;
                            if (value5B is string) float.TryParse((string)value5B, out nuevo.LeaveTangent);
                            if (value5B is int) nuevo.LeaveTangent = (float)(int)value5B;
                            if (value5B is float) nuevo.LeaveTangent = (float)value5B;

                            decidedRCKProperty.Value = nuevo;

                            finalProp = decidedRCKProperty;
                            break;
                        default:
                            PropertyData newThing = MainSerializer.TypeToClass(useUnversionedProperties ? FName.DefineDummy(asset, type) : FName.FromString(asset, type), nameName, null, null, null, asset);
                            if (original != null && original.GetType() == newThing.GetType())
                            {
                                newThing = original;
                                newThing.Name = nameName;
                            }

                            string[] existingStrings = new string[5];
                            if (value1B != null) existingStrings[0] = Convert.ToString(value1B);
                            if (value2B != null) existingStrings[1] = Convert.ToString(value2B);
                            if (value3B != null) existingStrings[2] = Convert.ToString(value3B);
                            if (value4B != null) existingStrings[3] = Convert.ToString(value4B);
                            if (transformB != null) existingStrings[4] = Convert.ToString(transformB);

                            newThing.FromString(existingStrings, asset);

                            // override for enums if needed to ensure Value is not dummy
                            if (namesAreDummies && newThing is EnumPropertyData newThingEnum && expectedContext != PropertySerializationContext.Normal)
                            {
                                // convert Value from dummy to non-dummy
                                newThingEnum.Value = FName.FromString(asset, newThingEnum.Value.ToString());
                            }

                            finalProp = newThing;
                            break;
                    }
                }

                string ArrayIndex = row.Cells[row.Cells.Count - 4]?.Value?.ToString() ?? "0";
                string isZero = row.Cells[row.Cells.Count - 2]?.Value?.ToString() ?? "false";

                int.TryParse(ArrayIndex, out finalProp.ArrayIndex);
                finalProp.IsZero = (isZero.ToLowerInvariant() == "true" || isZero == "1");
                return finalProp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void ChangeAllExpansionStatus(bool expanding)
        {
            treeView1.BeginUpdate();
            foreach (TreeNode node in Collect(treeView1.Nodes))
            {
                if (expanding)
                {
                    node.Expand();
                }
                else
                {
                    node.Collapse();
                }
            }
            treeView1.EndUpdate();

            (treeView1.SelectedNode ?? treeView1.Nodes[0]).EnsureVisible();
        }

        private static IEnumerable<TreeNode> Collect(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;
                foreach (TreeNode child in Collect(node.Nodes))
                {
                    yield return child;
                }
            }
        }

        internal int ReplaceAllReferencesInNameMap(FString antiguo, FString nuevo)
        {
            int replacedCount = 0;
            List<FName> allNamesThatExist = UAPUtils.FindAllInstances<FName>(asset);
            for (int i = 0; i < allNamesThatExist.Count; i++)
            {
                FName thisOne = allNamesThatExist[i];
                if (thisOne.Value == antiguo)
                {
                    thisOne.Value = nuevo;
                    replacedCount++;
                }
            }
            return replacedCount;
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
                dataGridView1.Columns.Add(dgc);
            }
        }

        private void ClearScreen()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.BackgroundColor = UAGPalette.InactiveColor;
        }

        private Form1 GetParentForm()
        {
            return (Form1) dataGridView1.Parent.Parent.Parent;
        }

        public void Load() // Updates the table with selected asset data
        {
            if (mode == TableHandlerMode.None)
            {
                ClearScreen();
                return;
            }

            var origForm = GetParentForm();
            var byteView1 = origForm.byteView1;
            byteView1.Visible = false;
            origForm.importBinaryData.Visible = false;
            origForm.exportBinaryData.Visible = false;
            origForm.setBinaryData.Visible = false;
            var jsonView = origForm.jsonView;
            jsonView.Visible = false;
            dataGridView1.Visible = true;
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.ReadOnly = false;

            dataGridView1.BackgroundColor = UAGPalette.DataGridViewActiveColor;
            readyToSave = false;
            dirtySinceLastLoad = false;

            origForm.ResetCurrentDataGridViewStrip();

            switch (mode)
            {
                case TableHandlerMode.GeneralInformation:
                    AddColumns(new string[] { "Property Name", "Value", "" });

                    dataGridView1.Rows.Add(new object[] { "LegacyFileVersion", asset.LegacyFileVersion.ToString() });
                    dataGridView1.Rows.Add(new object[] { "IsUnversioned", asset.IsUnversioned.ToString() });
                    dataGridView1.Rows.Add(new object[] { "FileVersionLicenseeUE", asset.FileVersionLicenseeUE.ToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageGuid", asset.PackageGuid.ConvertToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageFlags", asset.PackageFlags.ToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageSource", asset.PackageSource.ToString() });
                    dataGridView1.Rows.Add(new object[] { asset.ObjectVersionUE5 >= ObjectVersionUE5.ADD_SOFTOBJECTPATH_LIST ? "PackageName" : "FolderName", asset.FolderName.Value });

                    dataGridView1.Rows[0].Cells[0].ToolTipText = "The package file version number when this package was saved. Unrelated to imports.";
                    dataGridView1.Rows[1].Cells[0].ToolTipText = "Should this asset not serialize its engine and custom versions?";
                    dataGridView1.Rows[2].Cells[0].ToolTipText = "The licensee file version. Used by some games to add their own Engine-level versioning.";
                    dataGridView1.Rows[3].Cells[0].ToolTipText = "Current ID for this package. Effectively unused.";
                    dataGridView1.Rows[4].Cells[0].ToolTipText = "The flags for this package.";
                    dataGridView1.Rows[5].Cells[0].ToolTipText = "Value that is used to determine if the package was saved by Epic, a licensee, modder, etc.";
                    dataGridView1.Rows[6].Cells[0].ToolTipText = asset.ObjectVersionUE5 >= ObjectVersionUE5.ADD_SOFTOBJECTPATH_LIST ? "The package name the file was last saved with." : "The Generic Browser folder name that this package lives in. Usually \"None\" in cooked assets.";

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    }

                    dataGridView1.AllowUserToAddRows = false;
                    break;
                case TableHandlerMode.NameMap:
                    if (asset.GetCustomVersion<FReleaseObjectVersion>() < FReleaseObjectVersion.PropertiesSerializeRepCondition)
                    {
                        AddColumns(new string[] { "Name", "Case Preserving?", "Encoding", "" });

                        IReadOnlyList<FString> headerIndexList = asset.GetNameMapIndexList();
                        for (int num = 0; num < headerIndexList.Count; num++)
                        {
                            dataGridView1.Rows.Add(headerIndexList[num].Value, headerIndexList[num].IsCasePreserving, headerIndexList[num].Encoding.HeaderName);
                            dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                        }
                    }
                    else
                    {
                        AddColumns(new string[] { "Name", "Encoding", "" });

                        IReadOnlyList<FString> headerIndexList = asset.GetNameMapIndexList();
                        for (int num = 0; num < headerIndexList.Count; num++)
                        {
                            dataGridView1.Rows.Add(headerIndexList[num].Value, headerIndexList[num].Encoding.HeaderName);
                            dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                        }
                    }
                    //((Form1)dataGridView1.Parent).CurrentDataGridViewStrip = ((Form1)dataGridView1.Parent).nameMapContext;
                    break;
                case TableHandlerMode.SoftObjectPathList:
                    AddColumns(new string[] { "PackageName", "AssetName", "SubPathString", "" });

                    for (int num = 0; num < asset.SoftObjectPathList.Count; num++)
                    {
                        string a = asset.SoftObjectPathList[num].AssetPath.PackageName == null ? FString.NullCase : asset.SoftObjectPathList[num].AssetPath.PackageName.ToString();
                        string b = asset.SoftObjectPathList[num].AssetPath.AssetName == null ? FString.NullCase : asset.SoftObjectPathList[num].AssetPath.AssetName.ToString();
                        string c = asset.SoftObjectPathList[num].SubPathString == null ? FString.NullCase : asset.SoftObjectPathList[num].SubPathString.ToString();
                        dataGridView1.Rows.Add(a, b, c);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                    }
                    break;
                case TableHandlerMode.Imports:
                    AddColumns(new string[] { "ClassPackage", "ClassName", "OuterIndex", "ObjectName", "bImportOptional", "" });

                    for (int num = 0; num < asset.Imports.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.Imports[num].ClassPackage.ToString(), asset.Imports[num].ClassName.ToString(), asset.Imports[num].OuterIndex.Index, asset.Imports[num].ObjectName.ToString(), asset.Imports[num].bImportOptional);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(FPackageIndex.FromImport(num));
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    string[] allExportDetailsFields = Export.GetAllFieldNames(asset);
                    string[] allExportDetailsFields2 = new string[allExportDetailsFields.Length + 1];
                    allExportDetailsFields.CopyTo(allExportDetailsFields2, 0);
                    allExportDetailsFields2[allExportDetailsFields2.Length - 1] = "";
                    AddColumns(allExportDetailsFields2);

                    for (int num = 0; num < asset.Exports.Count; num++)
                    {
                        Export refer = asset.Exports[num];
                        string[] newCellsTooltips = new string[allExportDetailsFields.Length];
                        object[] newCells = new object[allExportDetailsFields.Length];
                        for (int num2 = 0; num2 < allExportDetailsFields.Length; num2++)
                        {
                            string cellTooltip = null;

                            object printingVal = refer.GetType().GetMember(allExportDetailsFields[num2])[0].GetValue(refer);
                            if (printingVal is FName parsingName)
                            {
                                string actualName = parsingName?.ToString();
                                if (actualName == null) actualName = FString.NullCase;
                                newCells[num2] = actualName;
                            }
                            else if (printingVal is FPackageIndex parsingIndex)
                            {
                                newCells[num2] = parsingIndex.Index;
                            }
                            else if (printingVal is List<FPackageIndex> parsingIndices)
                            {
                                newCells[num2] = parsingIndices.Count == 0 ? string.Empty : string.Join(",", parsingIndices.Select(x => x.Index).ToArray());
                            }
                            else
                            {
                                newCells[num2] = printingVal;
                            }

                            if (printingVal is int testInt)
                            {
                                if (testInt < 0) cellTooltip = new FPackageIndex(testInt).ToImport(asset).ObjectName.Value.Value;
                            }

                            newCellsTooltips[num2] = cellTooltip;
                        }

                        dataGridView1.Rows.Add(newCells);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num + 1);

                        for (int num3 = 0; num3 < newCellsTooltips.Length; num3++)
                        {
                            if (!string.IsNullOrEmpty(newCellsTooltips[num3]))
                            {
                                dataGridView1.Rows[num].Cells[num3].ToolTipText = newCellsTooltips[num3];
                            }
                        }
                    }

                    break;
                case TableHandlerMode.DependsMap:
                    AddColumns(new string[] { "Export Index", "Value", "" });

                    if (asset.DependsMap == null) break;
                    for (int num = 0; num < asset.DependsMap.Count; num++)
                    {
                        for (int num2 = 0; num2 < asset.DependsMap[num].Length; num2++)
                        {
                            dataGridView1.Rows.Add((num + 1), asset.DependsMap[num][num2]);
                        }
                    }
                    break;
                case TableHandlerMode.SoftPackageReferences:
                    AddColumns(new string[] { "Value", "" });

                    if (asset.SoftPackageReferenceList == null) break;
                    for (int num = 0; num < asset.SoftPackageReferenceList.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.SoftPackageReferenceList[num]?.ToString() ?? FString.NullCase);
                    }
                    break;
                case TableHandlerMode.WorldTileInfo:
                    AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "" });

                    if (treeView1.SelectedNode is PointingTreeNode wtlPointerNode)
                    {
                        if (wtlPointerNode.Pointer == null)
                        {
                            dataGridView1.Rows.Add(new object[] { "Position", asset.WorldTileInfo.Position[0], asset.WorldTileInfo.Position[1], asset.WorldTileInfo.Position[2] });
                            dataGridView1.Rows.Add(new object[] { "AbsolutePosition", asset.WorldTileInfo.AbsolutePosition[0], asset.WorldTileInfo.AbsolutePosition[1], asset.WorldTileInfo.AbsolutePosition[2] });
                            dataGridView1.Rows.Add(new object[] { "Bounds", asset.WorldTileInfo.Bounds.ToString() });
                            dataGridView1.Rows.Add(new object[] { "Layer", "" });
                            dataGridView1.Rows.Add(new object[] { "bHideInTileView", asset.WorldTileInfo.bHideInTileView });
                            dataGridView1.Rows.Add(new object[] { "ParentTilePackageName", asset.WorldTileInfo.ParentTilePackageName.Value });
                            dataGridView1.Rows.Add(new object[] { "LODList", "" });
                            dataGridView1.Rows.Add(new object[] { "ZOrder", asset.WorldTileInfo.ZOrder });
                        }
                        else if (wtlPointerNode.Pointer is FWorldTileLayer fWTL)
                        {
                            dataGridView1.Rows.Add(new object[] { "Name", fWTL.Name });
                            dataGridView1.Rows.Add(new object[] { "Reserved0", fWTL.Reserved0 });
                            dataGridView1.Rows.Add(new object[] { "Reserved1", fWTL.Reserved1.ToString() });
                            dataGridView1.Rows.Add(new object[] { "StreamingDistance", fWTL.StreamingDistance });
                            dataGridView1.Rows.Add(new object[] { "DistanceStreamingEnabled", fWTL.DistanceStreamingEnabled });
                        }
                        else if (wtlPointerNode.Pointer is FWorldTileLODInfo fWTLI)
                        {
                            dataGridView1.Rows.Add(new object[] { "Name", fWTLI.RelativeStreamingDistance });
                            dataGridView1.Rows.Add(new object[] { "Reserved0", fWTLI.Reserved0 });
                            dataGridView1.Rows.Add(new object[] { "Reserved1", fWTLI.Reserved1 });
                            dataGridView1.Rows.Add(new object[] { "Reserved2", fWTLI.Reserved2 });
                            dataGridView1.Rows.Add(new object[] { "Reserved3", fWTLI.Reserved3 });
                        }
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    }

                    // Modification is disabled
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.ReadOnly = true;
                    break;
                case TableHandlerMode.DataResources:
                    AddColumns(new string[] { "Index", "Flags", "SerialOffset", "DuplicateSerialOffset", "SerialSize", "RawSize", "OuterIndex", "LegacyBulkDataFlags", "" });

                    if (asset.DataResources == null) break;
                    for (int num = 0; num < asset.DataResources.Count; num++)
                    {
                        var dataResource = asset.DataResources[num];
                        dataGridView1.Rows.Add(new object[] { num, dataResource.Flags.ToString(), dataResource.SerialOffset.ToString(), dataResource.DuplicateSerialOffset.ToString(), dataResource.SerialSize.ToString(), dataResource.RawSize.ToString(), dataResource.OuterIndex.ToString(), dataResource.LegacyBulkDataFlags.ToString() });
                    }
                    break;
                case TableHandlerMode.CustomVersionContainer:
                    AddColumns(new string[] { "Name", "Version", "" });

                    if (asset.CustomVersionContainer == null) break;
                    for (int num = 0; num < asset.CustomVersionContainer.Count; num++)
                    {
                        dataGridView1.Rows.Add(new object[] { asset.CustomVersionContainer[num].FriendlyName == null ? Convert.ToString(asset.CustomVersionContainer[num].Key) : asset.CustomVersionContainer[num].FriendlyName, asset.CustomVersionContainer[num].Version });
                    }
                    break;
                case TableHandlerMode.ExportData:
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        AddColumns(new string[] { "Name", "Type", "Variant", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "ArrayIndex", "Serial Offset", "Is Zero", "" });
                        bool standardRendering = true;
                        PropertyData[] renderingArr = null;

                        if (pointerNode.Type == PointingTreeNodeType.ByteArray || pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                        {
                            Control currentlyFocusedControl = origForm.ActiveControl;
                            dataGridView1.Visible = false;
                            byteView1.SetBytes(new byte[] { });
                            if (pointerNode.Type == PointingTreeNodeType.KismetByteArray)
                            {
                                byteView1.SetBytes(((StructExport)pointerNode.Pointer).ScriptBytecodeRaw);
                            }
                            else if (pointerNode.Pointer is RawExport)
                            {
                                byteView1.SetBytes(((RawExport)pointerNode.Pointer).Data);
                            }
                            else if (pointerNode.Pointer is NormalExport)
                            {
                                byteView1.SetBytes(((NormalExport)pointerNode.Pointer).Extras);
                            }
                            byteView1.Visible = true;
                            origForm.importBinaryData.Visible = true;
                            origForm.exportBinaryData.Visible = true;
                            origForm.setBinaryData.Visible = true;
                            currentlyFocusedControl.Focus();
                            origForm.ForceResize();
                            standardRendering = false;
                        }
                        else if (pointerNode.Type == PointingTreeNodeType.Kismet)
                        {
                            var bytecode = ((StructExport)pointerNode.Pointer).ScriptBytecode;
                            Control currentlyFocusedControl1 = origForm.ActiveControl;
                            if (UAGConfig.Data.EnablePrettyBytecode)
                            {
                                UAssetAPI.Kismet.KismetSerializer.asset = asset;
                                dataGridView1.Visible = false;
                                jsonView.Text = new JObject(new JProperty("Script", SerializeScript(bytecode))).ToString();
                                jsonView.Visible = true;
                                jsonView.ReadOnly = true;
                            }
                            else
                            {
                                dataGridView1.Visible = false;
                                jsonView.Text = asset.SerializeJsonObject(bytecode, true);
                                jsonView.Visible = true;
                                jsonView.ReadOnly = false;
                            }
                            currentlyFocusedControl1.Focus();
                            origForm.ForceResize();
                            standardRendering = false;
                        }
                        else
                        {
                            switch (pointerNode.Pointer)
                            {
                                case NormalExport usCategory:
                                    switch (pointerNode.Type)
                                    {
                                        case PointingTreeNodeType.Normal:
                                            for (int num = 0; num < usCategory.Data.Count; num++)
                                            {
                                                if (usCategory.Data[num] == null)
                                                {
                                                    usCategory.Data.RemoveAt(num);
                                                    num--;
                                                }
                                            }
                                            renderingArr = usCategory.Data.ToArray();
                                            break;
                                        case PointingTreeNodeType.UserDefinedStructData:
                                            var usCategoryUDS = (UserDefinedStructExport)usCategory;
                                            for (int num = 0; num < usCategoryUDS.StructData.Count; num++)
                                            {
                                                if (usCategoryUDS.StructData[num] == null)
                                                {
                                                    usCategoryUDS.StructData.RemoveAt(num);
                                                    num--;
                                                }
                                            }
                                            renderingArr = usCategoryUDS.StructData.ToArray();
                                            break;
                                        case PointingTreeNodeType.StructData:
                                            dataGridView1.Columns.Clear();
                                            AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "" });

                                            StructExport strucCat = (StructExport)usCategory;
                                            List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                            {
                                                ObjectPropertyData testProperty = new ObjectPropertyData(FName.DefineDummy(asset, "Super Struct"));
                                                testProperty.Value = strucCat.SuperStruct;

                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "Next";
                                                row.Cells[0].ToolTipText = "Next Field in the linked list";
                                                row.Cells[1].Value = strucCat.Field.Next;
                                                rows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "Super Struct";
                                                row.Cells[1].Value = testProperty.Value;
                                                UAGUtils.UpdateObjectPropertyValues(asset, row, dataGridView1, testProperty.Value, 2);
                                                rows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ScriptBytecodeSize";
                                                row.Cells[0].ToolTipText = "Number of bytecode instructions in this UStruct";
                                                row.Cells[1].Value = strucCat.ScriptBytecodeSize;
                                                rows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ScriptBytecode";
                                                row.Cells[1].Value = string.Empty;
                                                rows.Add(row);
                                            }

                                            // Header 1
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "---";
                                                row.Cells[1].Value = "CHILDREN";
                                                row.Cells[2].Value = "---";
                                                rows.Add(row);
                                            }

                                            for (int i = 0; i < strucCat.Children.Length; i++)
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = strucCat.Children[i];
                                                rows.Add(row);
                                            }

                                            // Header 2
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "---";
                                                row.Cells[1].Value = "LOADED PROPERTIES";
                                                row.Cells[2].Value = "---";
                                                rows.Add(row);
                                            }

                                            for (int i = 0; i < strucCat.LoadedProperties.Length; i++)
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = strucCat.LoadedProperties[i].Name.ToString();
                                                row.Cells[1].Value = strucCat.LoadedProperties[i].SerializedType.ToString();
                                                row.Cells[2].Value = strucCat.LoadedProperties[i].Flags.ToString();
                                                rows.Add(row);
                                            }

                                            // Header 3
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "---";
                                                row.Cells[1].Value = "END";
                                                row.Cells[2].Value = "---";
                                                rows.Add(row);
                                            }

                                            dataGridView1.Rows.AddRange(rows.ToArray());
                                            dataGridView1.ReadOnly = true;
                                            dataGridView1.AllowUserToAddRows = false;
                                            standardRendering = false;
                                            break;
                                        case PointingTreeNodeType.ClassData:
                                            dataGridView1.Columns.Clear();
                                            AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "" });

                                            ClassExport bgcCat = (ClassExport)usCategory;
                                            List<DataGridViewRow> classRows = new List<DataGridViewRow>();

                                            {
                                                ObjectPropertyData testProperty = new ObjectPropertyData(FName.DefineDummy(asset, "Super Struct"));
                                                testProperty.Value = bgcCat.SuperStruct;

                                                DataGridViewRow row = new DataGridViewRow();
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ClassFlags";
                                                row.Cells[1].Value = bgcCat.ClassFlags.ToString();
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ClassWithin";
                                                row.Cells[1].Value = bgcCat.ClassWithin;
                                                row.Cells[2].Value = bgcCat.ClassWithin.IsImport() ? bgcCat.ClassWithin.ToImport(asset).ObjectName.ToString() : "";
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ClassConfigName";
                                                row.Cells[1].Value = bgcCat.ClassConfigName.ToString();
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ClassGeneratedBy";
                                                row.Cells[1].Value = bgcCat.ClassGeneratedBy;
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "bDeprecatedForceScriptOrder";
                                                row.Cells[1].Value = bgcCat.bDeprecatedForceScriptOrder;
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "bCooked";
                                                row.Cells[1].Value = bgcCat.bCooked;
                                                classRows.Add(row);
                                                row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "ClassDefaultObject";
                                                row.Cells[2].Value = bgcCat.ClassDefaultObject;
                                                row.Cells[3].Value = "Jump";
                                                row.Cells[3].Tag = "CategoryJump";
                                                DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                                Font styFont = new Font(dataGridView1.Font.Name, UAGPalette.RecommendedFontSize, FontStyle.Underline);
                                                sty.Font = styFont;
                                                sty.ForeColor = UAGPalette.LinkColor;
                                                row.Cells[3].Style = sty;
                                                classRows.Add(row);
                                            }

                                            // Header 1
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "---";
                                                row.Cells[1].Value = "FUNCTION MAP";
                                                row.Cells[2].Value = "---";
                                                classRows.Add(row);
                                            }

                                            for (int i = 0; i < bgcCat.FuncMap.Count; i++)
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = bgcCat.FuncMap.Keys.ElementAt(i).ToString();
                                                row.Cells[2].Value = bgcCat.FuncMap[i].Index;
                                                if (bgcCat.FuncMap[i].Index != 0)
                                                {
                                                    row.Cells[3].Value = "Jump";
                                                    row.Cells[3].Tag = "CategoryJump";
                                                    DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                                    Font styFont = new Font(dataGridView1.Font.Name, UAGPalette.RecommendedFontSize, FontStyle.Underline);
                                                    sty.Font = styFont;
                                                    sty.ForeColor = UAGPalette.LinkColor;
                                                    row.Cells[3].Style = sty;
                                                }
                                                classRows.Add(row);
                                            }

                                            // Header 2
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "---";
                                                row.Cells[1].Value = "END";
                                                row.Cells[2].Value = "---";
                                                classRows.Add(row);
                                            }

                                            dataGridView1.Rows.AddRange(classRows.ToArray());
                                            dataGridView1.ReadOnly = true;
                                            dataGridView1.AllowUserToAddRows = false;
                                            standardRendering = false;
                                            break;
                                        case PointingTreeNodeType.EnumData:
                                            dataGridView1.Columns.Clear();
                                            AddColumns(new string[] { "Name", "Value", "" });

                                            EnumExport enumCat = (EnumExport)usCategory;
                                            List<DataGridViewRow> enumRows = new List<DataGridViewRow>();

                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = "CppForm";
                                                row.Cells[0].ToolTipText = "How the enum was originally defined.";
                                                row.Cells[1].Value = enumCat.Enum.CppForm.ToString();
                                                enumRows.Add(row);
                                            }

                                            for (int i = 0; i < enumCat.Enum.Names.Count; i++)
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = enumCat.Enum.Names[i].Item1.ToString();
                                                row.Cells[1].Value = enumCat.Enum.Names[i].Item2.ToString();
                                                enumRows.Add(row);
                                            }

                                            dataGridView1.Rows.AddRange(enumRows.ToArray());
                                            standardRendering = false;
                                            break;
                                        case PointingTreeNodeType.UPropertyData:
                                            dataGridView1.Columns.Clear();
                                            AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "" });

                                            PropertyExport uPropData = (PropertyExport)usCategory;
                                            List<DataGridViewRow> uPropRows = new List<DataGridViewRow>();

                                            {
                                                var allUPropFields = UAPUtils.GetOrderedFields(uPropData.Property.GetType());
                                                for (int i = 0; i < allUPropFields.Length; i++)
                                                {
                                                    FieldInfo currFieldInfo = allUPropFields[i];
                                                    string currFieldName = currFieldInfo.Name;
                                                    object currFieldValue = currFieldInfo.GetValue(uPropData.Property);
                                                    if (currFieldInfo.Name == "Next" && currFieldValue == null) continue;

                                                    DataGridViewRow row = new DataGridViewRow();
                                                    row.CreateCells(dataGridView1);
                                                    row.Cells[0].Value = currFieldName;
                                                    row.Cells[1].Value = currFieldValue.ToString();
                                                    if (currFieldValue is FPackageIndex fpi) UAGUtils.UpdateObjectPropertyValues(asset, row, dataGridView1, fpi, 2);
                                                    uPropRows.Add(row);
                                                }
                                            }

                                            dataGridView1.Rows.AddRange(uPropRows.ToArray());
                                            dataGridView1.ReadOnly = true;
                                            dataGridView1.AllowUserToAddRows = false;
                                            standardRendering = false;
                                            break;
                                    }

                                    break;
                                case FStringTable strUs:
                                    {
                                        dataGridView1.Columns.Clear();
                                        AddColumns(new string[] { "Key", "Encoding", "Source String", "Encoding", "" });
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();
                                        for (int i = 0; i < strUs.Count; i++)
                                        {
                                            FString key = strUs.Keys.ElementAt(i);
                                            FString value = strUs[i];

                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = key.FEncode();
                                            row.Cells[1].Value = key?.Encoding?.HeaderName ?? Encoding.ASCII.HeaderName;
                                            row.Cells[2].Value = value.FEncode();
                                            row.Cells[3].Value = value?.Encoding?.HeaderName ?? Encoding.ASCII.HeaderName;
                                            row.HeaderCell.Value = Convert.ToString(i);
                                            rows.Add(row);
                                        }
                                        dataGridView1.Rows.AddRange(rows.ToArray());
                                        standardRendering = false;
                                        break;
                                    }
                                case UDataTable dtUs:
                                    dtUs.Data.StripNullsFromList();
                                    renderingArr = dtUs.Data.ToArray();
                                    break;
                                case MapPropertyData usMap:
                                    {
                                        if (usMap.Value.Count > 0)
                                        {
                                            FName mapKeyType = usMap.KeyType;
                                            FName mapValueType = usMap.ValueType;
                                            if (usMap.Value.Count != 0)
                                            {
                                                mapKeyType = asset.HasUnversionedProperties ? FName.DefineDummy(asset, usMap.Value.Keys.ElementAt(0).PropertyType) : new FName(asset, usMap.Value.Keys.ElementAt(0).PropertyType);
                                                mapValueType = asset.HasUnversionedProperties ? FName.DefineDummy(asset, usMap.Value[0].PropertyType) : new FName(asset, usMap.Value[0].PropertyType);
                                            }

                                            List<DataGridViewRow> rows = new List<DataGridViewRow>();
                                            for (int i = 0; i < usMap.Value.Count; i++)
                                            {
                                                DataGridViewRow row = new DataGridViewRow();
                                                row.CreateCells(dataGridView1);
                                                row.Cells[0].Value = usMap.Name.ToString();
                                                row.Cells[1].Value = "MapEntry";
                                                row.Cells[2].Value = string.Empty;

                                                row.Cells[3].Value = "Jump";
                                                row.Cells[3].Tag = "ChildJump";

                                                DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                                Font styFont = new Font(dataGridView1.Font.Name, UAGPalette.RecommendedFontSize, FontStyle.Underline);
                                                sty.Font = styFont;
                                                sty.ForeColor = UAGPalette.LinkColor;
                                                row.Cells[3].Style = sty;

                                                row.Cells[4].Value = mapKeyType.ToString();
                                                row.Cells[5].Value = mapValueType.ToString();
                                                row.HeaderCell.Value = Convert.ToString(i);
                                                row.Tag = new KeyValuePair<PropertyData, PropertyData>(usMap.Value.Keys.ElementAt(i), usMap.Value[i]);
                                                rows.Add(row);
                                            }
                                            dataGridView1.Rows.AddRange(rows.ToArray());
                                        }
                                        standardRendering = false;
                                        break;
                                    }
                                case StructPropertyData usStruct:
                                    usStruct.Value.StripNullsFromList();
                                    renderingArr = usStruct.Value.ToArray();
                                    break;
                                case ArrayPropertyData usArr:
                                    usArr.Value = usArr.Value.StripNullsFromArray();
                                    renderingArr = usArr.Value;
                                    break;
                                case GameplayTagContainerPropertyData usArr2:
                                    dataGridView1.Columns.Clear();
                                    AddColumns(new string[] { "Tag Name", "" });
                                    usArr2.Value = usArr2.Value.StripNullsFromArray();
                                    for (int i = 0; i < usArr2.Value.Length; i++)
                                    {
                                        dataGridView1.Rows.Add(usArr2.Value[i].ToString());
                                    }
                                    standardRendering = false;
                                    break;
                                case BoxPropertyData box1:
                                    {
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                        DataGridViewRow row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Min";
                                        row.Cells[1].Value = box1.Value.Min.X;
                                        row.Cells[2].Value = box1.Value.Min.Y;
                                        row.Cells[3].Value = box1.Value.Min.Z;
                                        row.HeaderCell.Value = Convert.ToString(0);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Max";
                                        row.Cells[1].Value = box1.Value.Max.X;
                                        row.Cells[2].Value = box1.Value.Max.Y;
                                        row.Cells[3].Value = box1.Value.Max.Z;
                                        row.HeaderCell.Value = Convert.ToString(1);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "IsValid";
                                        row.Cells[1].Value = box1.Value.IsValid > 0;
                                        row.HeaderCell.Value = Convert.ToString(2);
                                        rows.Add(row);

                                        dataGridView1.Rows.AddRange(rows.ToArray());
                                    }

                                    dataGridView1.AllowUserToAddRows = false;
                                    standardRendering = false;
                                    break;
                                case Box2DPropertyData box1:
                                    {
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                        DataGridViewRow row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Min";
                                        row.Cells[1].Value = box1.Value.Min.X;
                                        row.Cells[2].Value = box1.Value.Min.Y;
                                        row.HeaderCell.Value = Convert.ToString(0);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Max";
                                        row.Cells[1].Value = box1.Value.Max.X;
                                        row.Cells[2].Value = box1.Value.Max.Y;
                                        row.HeaderCell.Value = Convert.ToString(1);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "IsValid";
                                        row.Cells[1].Value = box1.Value.IsValid > 0;
                                        row.HeaderCell.Value = Convert.ToString(2);
                                        rows.Add(row);

                                        dataGridView1.Rows.AddRange(rows.ToArray());
                                    }

                                    dataGridView1.AllowUserToAddRows = false;
                                    standardRendering = false;
                                    break;
                                case Box2fPropertyData box1:
                                    {
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                        DataGridViewRow row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Min";
                                        row.Cells[1].Value = box1.Value.Min.X;
                                        row.Cells[2].Value = box1.Value.Min.Y;
                                        row.HeaderCell.Value = Convert.ToString(0);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "Max";
                                        row.Cells[1].Value = box1.Value.Max.X;
                                        row.Cells[2].Value = box1.Value.Max.Y;
                                        row.HeaderCell.Value = Convert.ToString(1);
                                        rows.Add(row);

                                        row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = "IsValid";
                                        row.Cells[1].Value = box1.Value.IsValid > 0;
                                        row.HeaderCell.Value = Convert.ToString(2);
                                        rows.Add(row);

                                        dataGridView1.Rows.AddRange(rows.ToArray());
                                    }

                                    dataGridView1.AllowUserToAddRows = false;
                                    standardRendering = false;
                                    break;
                                case PointingDictionaryEntry usDictEntry:
                                    dataGridView1.AllowUserToAddRows = false;
                                    var ourKey = usDictEntry.Entry.Key;
                                    var ourValue = usDictEntry.Entry.Value;
                                    if (ourKey != null) ourKey.Name = FName.DefineDummy(asset, "Key");
                                    if (ourValue != null) ourValue.Name = FName.DefineDummy(asset, "Value");
                                    renderingArr = [ourKey, ourValue];
                                    break;
                                case FDelegate[] usRealMDArr:
                                    for (int i = 0; i < usRealMDArr.Length; i++)
                                    {
                                        dataGridView1.Rows.Add(usRealMDArr[i].Delegate, "FDelegate", usRealMDArr[i].Object.Index);
                                    }
                                    standardRendering = false;
                                    break;
                                case PropertyData[] usRealArr:
                                    renderingArr = usRealArr;
                                    dataGridView1.AllowUserToAddRows = false;
                                    break;
                            }
                        }

                        if (standardRendering)
                        {
                            if (renderingArr != null)
                            {
                                if (renderingArr.Length > 0) AddRowsForArray(renderingArr);
                            }
                            else
                            {
                                ClearScreen();
                            }
                        }
                    }
                    else
                    {
                        ClearScreen();
                    }
                    break;
            }

            // go through each row and make sure it's a good height and width
            if (dataGridView1.Rows?.Count > 0)
            {
                dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Height < dataGridView1.RowTemplate.MinimumHeight) this.dataGridView1.AutoResizeRow(i);
                }
            }

            origForm.UpdateRPC();

            readyToSave = true;
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        public void Save(bool forceNewLoad) // Reads from the table and updates the asset data as needed
        {
            if (!readyToSave) return;
            if (dataGridView1?.Rows == null) return;

            switch (mode)
            {
                case TableHandlerMode.GeneralInformation:
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string propertyName = (string)row.Cells[0].Value;
                        string propertyValue = (string)row.Cells[1].Value;
                        switch (propertyName)
                        {
                            case "LegacyFileVersion":
                                asset.LegacyFileVersion = Convert.ToInt32(propertyValue);
                                break;
                            case "IsUnversioned":
                                bool nextUvVal = propertyValue.ToLowerInvariant() == "true" || propertyValue == "1";
                                if (asset.IsUnversioned && !nextUvVal)
                                {
                                    // currently unversioned, switching to versioned
                                    // make all custom versions serialized
                                    foreach (CustomVersion cVer in asset.CustomVersionContainer)
                                    {
                                        cVer.IsSerialized = true;
                                    }
                                }
                                asset.IsUnversioned = nextUvVal;
                                break;
                            case "FileVersionLicenseeUE":
                                asset.FileVersionLicenseeUE = Convert.ToInt32(propertyValue);
                                break;
                            case "PackageGuid":
                                Guid newPackageGuid = propertyValue.ConvertToGUID();
                                if (newPackageGuid != Guid.Empty) asset.PackageGuid = newPackageGuid;
                                break;
                            case "PackageFlags":
                                if (Enum.TryParse(propertyValue, out EPackageFlags newPackageFlags)) asset.PackageFlags = newPackageFlags;
                                break;
                            case "PackageSource":
                                if (uint.TryParse(propertyValue, out uint newPackageSource)) asset.PackageSource = newPackageSource;
                                break;
                            case "FolderName":
                            case "PackageName":
                                asset.FolderName = FString.FromString(propertyValue, Encoding.UTF8.GetByteCount(propertyValue) == propertyValue.Length ? Encoding.ASCII : Encoding.Unicode);
                                break;
                        }

                    }
                    break;
                case TableHandlerMode.NameMap:
                    bool hasCasePreservingColumn = asset.GetCustomVersion<FReleaseObjectVersion>() < FReleaseObjectVersion.PropertiesSerializeRepCondition;
                    asset.ClearNameIndexList();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        int r = 0; bool isCasePreserving = true;

                        string ourValue = (string)row.Cells[r++].Value;
                        if (hasCasePreservingColumn)
                        {
                            object isCasePreservingTemp = row.Cells[r++].Value;
                            if (isCasePreservingTemp is string)
                            {
                                isCasePreserving = ((string)isCasePreservingTemp).Equals("1") || ((string)isCasePreservingTemp).ToLowerInvariant().Equals("true");
                            }
                            else if (isCasePreservingTemp is bool)
                            {
                                isCasePreserving = (bool)isCasePreservingTemp;
                            }
                        }
                        string encoding = (string)row.Cells[r++].Value;

                        if (string.IsNullOrWhiteSpace(encoding)) encoding = "ascii";
                        if (!string.IsNullOrWhiteSpace(ourValue))
                        {
                            var finalStr = FString.FromString(ourValue, encoding.Equals(Encoding.Unicode.HeaderName) ? Encoding.Unicode : Encoding.ASCII);
                            finalStr.IsCasePreserving = isCasePreserving;
                            asset.AddNameReference(finalStr, true);
                        }
                    }
                    break;
                case TableHandlerMode.SoftObjectPathList:
                    if (asset.SoftObjectPathList == null) asset.SoftObjectPathList = new List<FSoftObjectPath>();

                    asset.SoftObjectPathList.Clear();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string a = row.Cells[0].Value as string;
                        string b = row.Cells[1].Value as string;
                        string c = row.Cells[2].Value as string;

                        if (a == FString.NullCase) a = null;
                        if (b == FString.NullCase) b = null;
                        if (c == FString.NullCase) c = null;

                        // if all empty, then remove (invalid, probably just the last row)
                        if (a == null && b == null && c == null) continue;

                        FSoftObjectPath nuevo = new FSoftObjectPath(FName.FromString(asset, a), FName.FromString(asset, b), FString.FromString(c));
                        asset.SoftObjectPathList.Add(nuevo);
                    }
                    break;
                case TableHandlerMode.Imports:
                    asset.Imports = new List<Import>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        object val1 = row.Cells[0].Value;
                        object val2 = row.Cells[1].Value;
                        object val3 = row.Cells[2].Value;
                        object val4 = row.Cells[3].Value;
                        object val5 = row.Cells[4].Value;

                        bool realVal5 = false;
                        if (val5 is bool) realVal5 = (bool)val5;
                        if (val5 is string) realVal5 = ((string)val5).Equals("1") || ((string)val5).ToLowerInvariant().Equals("true");

                        if (val1 == null || val2 == null || val4 == null) continue;
                        if (!(val1 is string) || !(val2 is string) || !(val4 is string)) continue;

                        int realVal3 = 0;
                        if (val3 is string)
                        {
                            int.TryParse((string)val3, out realVal3);
                        }
                        else
                        {
                            realVal3 = Convert.ToInt32(val3);
                        }

                        string realVal1 = (string)val1;
                        string realVal2 = (string)val2;
                        string realVal4 = (string)val4;
                        if (string.IsNullOrWhiteSpace(realVal1) || string.IsNullOrWhiteSpace(realVal2) || string.IsNullOrWhiteSpace(realVal4)) continue;

                        FName parsedVal1 = FName.FromString(asset, realVal1);
                        FName parsedVal2 = FName.FromString(asset, realVal2);
                        FName parsedVal4 = FName.FromString(asset, realVal4);
                        asset.AddNameReference(parsedVal1.Value);
                        asset.AddNameReference(parsedVal2.Value);
                        asset.AddNameReference(parsedVal4.Value);
                        Import newLink = new Import(parsedVal1, parsedVal2, new FPackageIndex(realVal3), parsedVal4, realVal5);
                        asset.Imports.Add(newLink);
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    MemberInfo[] allExportDetailsFields = Export.GetAllObjectExportFields(asset);
                    ExportDetailsParseType[] parsingTypes = new ExportDetailsParseType[]
                    {
                        ExportDetailsParseType.FName,
                        ExportDetailsParseType.FPackageIndex,

                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.EObjectFlags,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Guid,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.UInt,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,

                        ExportDetailsParseType.FPackageIndexList,
                        ExportDetailsParseType.FPackageIndexList,
                        ExportDetailsParseType.FPackageIndexList,
                        ExportDetailsParseType.FPackageIndexList,
                        ExportDetailsParseType.FPackageIndexList
                    };

                    int rowNum = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        bool isNewExport = false;
                        if (asset.Exports.Count <= rowNum)
                        {
                            // If we add a new category, we'll make a new NormalExport (None-terminated UProperty list). If you want to make some other kind of export, you'll need to do it manually with UAssetAPI
                            var newCat = new NormalExport(asset, Array.Empty<Byte>());
                            newCat.Data = new List<PropertyData>();
                            asset.Exports.Add(newCat);
                            isNewExport = true;
                        }

                        bool isInvalidRow = false;

                        for (int i = 0; i < allExportDetailsFields.Length; i++)
                        {
                            object currentVal = row.Cells[i].Value;
                            object settingVal = null;
                            switch (parsingTypes[i])
                            {
                                case ExportDetailsParseType.Int:
                                    settingVal = 0;
                                    if (currentVal is string)
                                    {
                                        int x = 0;
                                        int.TryParse((string)currentVal, out x);
                                        settingVal = x;
                                    }
                                    else
                                    {
                                        settingVal = Convert.ToInt32(currentVal);
                                    }
                                    break;
                                case ExportDetailsParseType.FPackageIndex:
                                    settingVal = 0;
                                    if (currentVal is string)
                                    {
                                        int x = 0;
                                        int.TryParse((string)currentVal, out x);
                                        settingVal = new FPackageIndex(x);
                                    }
                                    else
                                    {
                                        settingVal = new FPackageIndex(Convert.ToInt32(currentVal));
                                    }
                                    break;
                                case ExportDetailsParseType.FName:
                                    settingVal = null;
                                    if (currentVal is string rawFName)
                                    {
                                        settingVal = FName.FromString(asset, rawFName);
                                    }
                                    else
                                    {
                                        isInvalidRow = true;
                                    }
                                    break;
                                case ExportDetailsParseType.EObjectFlags:
                                    settingVal = EObjectFlags.RF_NoFlags;
                                    if (currentVal is string)
                                    {
                                        EObjectFlags x;
                                        Enum.TryParse((string)currentVal, out x);
                                        settingVal = x;
                                    }
                                    else if (currentVal is EObjectFlags)
                                    {
                                        settingVal = (EObjectFlags)currentVal;
                                    }
                                    break;
                                case ExportDetailsParseType.Long:
                                    settingVal = 0;
                                    if (currentVal is string)
                                    {
                                        long x = 0;
                                        long.TryParse((string)currentVal, out x);
                                        settingVal = x;
                                    }
                                    else
                                    {
                                        settingVal = Convert.ToInt64(currentVal);
                                    }
                                    break;
                                case ExportDetailsParseType.Bool:
                                    settingVal = false;
                                    if (currentVal is string)
                                    {
                                        settingVal = ((string)currentVal).Equals("1") || ((string)currentVal).ToLowerInvariant().Equals("true");
                                    }
                                    else if (currentVal is bool)
                                    {
                                        settingVal = (bool)currentVal;
                                    }
                                    else
                                    {
                                        settingVal = false;
                                    }
                                    break;
                                case ExportDetailsParseType.Guid:
                                    settingVal = new Guid();
                                    if (currentVal is string)
                                    {
                                        Guid x = new Guid();
                                        x = ((string)currentVal).ConvertToGUID();
                                        settingVal = x;
                                    }
                                    else if (currentVal is Guid)
                                    {
                                        settingVal = (Guid)currentVal;
                                    }
                                    break;
                                case ExportDetailsParseType.UInt:
                                    settingVal = 0;
                                    if (currentVal is string)
                                    {
                                        uint x = 0;
                                        uint.TryParse((string)currentVal, out x);
                                        settingVal = x;
                                    }
                                    else
                                    {
                                        settingVal = Convert.ToUInt32(currentVal);
                                    }
                                    break;
                                case ExportDetailsParseType.FPackageIndexList:
                                    var finalList = new List<FPackageIndex>();
                                    if (currentVal is string)
                                    {
                                        string[] separateInts = ((string)currentVal).Split(',');
                                        foreach (string separateInt in separateInts)
                                        {
                                            if (int.TryParse(separateInt.Trim(), out int x)) finalList.Add(new FPackageIndex(x));
                                        }
                                    }
                                    else if (currentVal is List<FPackageIndex>)
                                    {
                                        finalList = (List<FPackageIndex>)currentVal;
                                    }
                                    settingVal = finalList;
                                    break;
                            }

                            allExportDetailsFields[i].SetValue(asset.Exports[rowNum], settingVal);
                        }

                        if (isInvalidRow && isNewExport)
                        {
                            asset.Exports.RemoveAt(asset.Exports.Count - 1);
                        }
                        rowNum++;
                    }

                    // remove garbage exports
                    for (int num = 0; num < asset.Exports.Count; num++)
                    {
                        if (asset.Exports[num].ObjectName == null)
                        {
                            asset.Exports.RemoveAt(num);
                            num--;
                        }
                    }

                    break;
                case TableHandlerMode.DependsMap:
                    var newDM = new List<int[]>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        int[] vals = new int[2];
                        for (int i = 0; i < 2; i++)
                        {
                            if (row.Cells[i].Value is string)
                            {
                                bool result = int.TryParse((string)row.Cells[i].Value, out int x);
                                if (!result) return;
                                vals[i] = x;
                            }
                            else
                            {
                                vals[i] = Convert.ToInt32(row.Cells[i].Value);
                            }
                        }

                        if (vals[0] == 0) continue;

                        if (newDM.Count > vals[0])
                        {
                            var arr = newDM[vals[0]];
                            Array.Resize(ref arr, arr.Length + 1);
                            arr[arr.Length - 1] = vals[1];
                            newDM[vals[0]] = arr;
                        }
                        else
                        {
                            newDM.Insert(vals[0], new int[] { vals[1] });
                        }
                    }

                    int numDependsInts = 0;
                    foreach (var entry in newDM) numDependsInts += entry.Length;

                    if (asset.DependsMap == null && numDependsInts == 0) break;
                    asset.DependsMap = newDM;
                    break;
                case TableHandlerMode.SoftPackageReferences:
                    var newSPR = new List<FString>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string strVal = (string)row.Cells[0].Value;
                        if (!string.IsNullOrEmpty(strVal)) newSPR.Add(FString.FromString(strVal));
                    }

                    if (asset.SoftPackageReferenceList == null && newSPR.Count == 0) break;
                    asset.SoftPackageReferenceList = newSPR;
                    break;
                case TableHandlerMode.WorldTileInfo:
                    // Modification is disabled

                    break;
                case TableHandlerMode.DataResources:
                    var newDR = new List<FObjectDataResource>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells.Count < 8 || string.IsNullOrWhiteSpace(row.Cells[0]?.Value?.ToString())) continue;
                        EObjectDataResourceFlags.TryParse(row.Cells[1]?.Value?.ToString(), out EObjectDataResourceFlags flags);
                        long.TryParse(row.Cells[2]?.Value?.ToString(), out long SerialOffset);
                        long.TryParse(row.Cells[3]?.Value?.ToString(), out long DuplicateSerialOffset);
                        long.TryParse(row.Cells[4]?.Value?.ToString(), out long SerialSize);
                        long.TryParse(row.Cells[5]?.Value?.ToString(), out long RawSize);
                        int.TryParse(row.Cells[6]?.Value?.ToString(), out int OuterIndex);
                        uint.TryParse(row.Cells[7]?.Value?.ToString(), out uint LegacyBulkDataFlags);

                        FObjectDataResource nuevo = new FObjectDataResource(flags, SerialOffset, DuplicateSerialOffset, SerialSize, RawSize, new FPackageIndex(OuterIndex), LegacyBulkDataFlags);
                        newDR.Add(nuevo);
                    }

                    if (asset.DataResources == null && newDR.Count == 0) break;
                    asset.DataResources = newDR;
                    break;
                case TableHandlerMode.CustomVersionContainer:
                    asset.CustomVersionContainer = new List<CustomVersion>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string rawCustomVersion = (string)row.Cells[0].Value;
                        Guid customVersionKey = CustomVersion.UnusedCustomVersionKey;
                        Guid parsedVersionKey = rawCustomVersion.ConvertToGUID();
                        if (parsedVersionKey == Guid.Empty)
                        {
                            customVersionKey = CustomVersion.GetCustomVersionGuidFromFriendlyName(rawCustomVersion);
                        }
                        else
                        {
                            customVersionKey = parsedVersionKey;
                        }
                        if (customVersionKey == CustomVersion.UnusedCustomVersionKey) continue;

                        int customVersionNumber;
                        if (row.Cells[1].Value is string)
                        {
                            if (!int.TryParse((string)row.Cells[1].Value, out customVersionNumber)) continue;
                        }
                        else
                        {
                            customVersionNumber = Convert.ToInt32(row.Cells[1].Value);
                        }

                        asset.CustomVersionContainer.Add(new CustomVersion(customVersionKey, customVersionNumber));
                    }
                    break;
                case TableHandlerMode.ExportData:
                    if (treeView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Pointer is FStringTable usStrTable)
                        {
                            usStrTable.Clear();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                DataGridViewRow row = dataGridView1.Rows[i];
                                object val0 = row.Cells[0].Value as string;
                                object val1 = row.Cells[1].Value as string;
                                object val2 = row.Cells[2].Value as string;
                                object val3 = row.Cells[3].Value as string;
                                if (val0 == null || val1 == null || val2 == null || val3 == null) continue;

                                FString key = ((string)val0).FDecode((string)val1);
                                FString value = ((string)val2).FDecode((string)val3);
                                usStrTable.Add(key, value);
                            }

                            pointerNode.Text = (usStrTable.TableNamespace?.ToString() ?? FString.NullCase) + " (" + usStrTable.Count + ")";
                        }
                        else if (pointerNode.Pointer is MapPropertyData usMap)
                        {
                            FName mapKeyType = usMap.KeyType;
                            FName mapValueType = usMap.ValueType;
                            if (usMap.Value.Count != 0)
                            {
                                mapKeyType = asset.HasUnversionedProperties ? FName.DefineDummy(asset, usMap.Value.Keys.ElementAt(0).PropertyType) : new FName(asset, usMap.Value.Keys.ElementAt(0).PropertyType);
                                mapValueType = asset.HasUnversionedProperties ? FName.DefineDummy(asset, usMap.Value[0].PropertyType) : new FName(asset, usMap.Value[0].PropertyType);
                            }

                            if (dataGridView1.Rows.Count > 0)
                            {
                                DataGridViewRow row = dataGridView1.Rows[0];
                                if (row.Cells.Count >= 6 && row.Cells[1].Value != null && row.Cells[4].Value != null && row.Cells[5].Value != null && row.Cells[1].Value.Equals("MapEntry"))
                                {
                                    FName newKey = FName.FromString(asset, row.Cells[4].Value as string);
                                    FName newVal = FName.FromString(asset, row.Cells[5].Value as string);
                                    if (newKey != null) mapKeyType = newKey;
                                    if (newVal != null) mapValueType = newVal;
                                }
                            }

                            // Failsafe
                            if (mapKeyType == null) mapKeyType = FName.FromString(asset, "IntProperty");
                            if (mapValueType == null) mapValueType = FName.FromString(asset, "IntProperty");

                            var newData = new TMap<PropertyData, PropertyData>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                DataGridViewRow row = dataGridView1.Rows[i];
                                if (row.Cells.Count <= 1 || row.Cells[1].Value == null || !row.Cells[1].Value.Equals("MapEntry")) continue;

                                if (row.Tag is KeyValuePair<PropertyData, PropertyData> dictBit && dictBit.Key.PropertyType == mapKeyType.Value && dictBit.Value.PropertyType == mapValueType.Value)
                                {
                                    newData.Add(dictBit.Key, dictBit.Value);
                                }
                                else
                                {
                                    var newKeyProp = MainSerializer.TypeToClass(mapKeyType, usMap.Name, null, null, null, asset);
                                    var newValProp = MainSerializer.TypeToClass(mapValueType, usMap.Name, null, null, null, asset);
                                    if (newKeyProp is StructPropertyData) ((StructPropertyData)newKeyProp).StructType = FName.DefineDummy(asset, "Generic");
                                    if (newValProp is StructPropertyData) ((StructPropertyData)newValProp).StructType = FName.DefineDummy(asset, "Generic");
                                    newData.Add(newKeyProp, newValProp);
                                }
                            }
                            usMap.Value = newData;

                            pointerNode.Text = usMap.Name.Value.Value + " (" + usMap.Value.Count + ")";
                        }
                        else if (pointerNode.Pointer is StructPropertyData usStruct)
                        {
                            int newCount = 0;
                            List<PropertyData> newData = new List<PropertyData>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, usStruct.Value.ElementAtOrDefault(i), false, asset.HasUnversionedProperties);
                                if (val == null)
                                {
                                    dirtySinceLastLoad = true;
                                    newData.Add(null);
                                    continue;
                                    // deliberately do not increment newCount
                                }
                                newData.Add(val);
                                newCount++;
                            }
                            if (newData[newData.Count - 1] == null) newData.RemoveAt(newData.Count - 1);
                            usStruct.Value = newData;

                            string decidedName = usStruct.Name.Value.Value;
                            if (((PointingTreeNode)pointerNode.Parent).Pointer is PropertyData && ((PropertyData)((PointingTreeNode)pointerNode.Parent).Pointer).Name.Equals(decidedName)) decidedName = usStruct.StructType.Value.Value;
                            pointerNode.Text = decidedName + " (" + newCount + ")";
                        }
                        else if (pointerNode.Pointer is BoxPropertyData box1)
                        {
                            FVector min = new();
                            FVector max = new();
                            byte isValid = 0;
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                if (dataGridView1.Rows[i].Cells.Count < 1) continue;
                                string name = dataGridView1.Rows[i].Cells[0].Value.ToString().Trim();
                                switch (name)
                                {
                                    case "Min":
                                        if (dataGridView1.Rows[i].Cells.Count < 4) continue;
                                        {
                                            double.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out double val1);
                                            double.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out double val2);
                                            double.TryParse(dataGridView1.Rows[i].Cells[3].Value.ToString(), out double val3);
                                            min = new(val1, val2, val3);
                                        }
                                        break;
                                    case "Max":
                                        if (dataGridView1.Rows[i].Cells.Count < 4) continue;
                                        {
                                            double.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out double val1);
                                            double.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out double val2);
                                            double.TryParse(dataGridView1.Rows[i].Cells[3].Value.ToString(), out double val3);
                                            max = new(val1, val2, val3);
                                        }
                                        break;
                                    case "IsValid":
                                        if (dataGridView1.Rows[i].Cells.Count < 2) continue;
                                        var val = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                        isValid = (val.Equals("1") || val.ToLowerInvariant().Equals("true")) ? (byte)1 : (byte)0;
                                        break;
                                }
                            }
                            box1.Value = new TBox<FVector>(min, max, isValid);
                        }
                        else if (pointerNode.Pointer is Box2DPropertyData box2)
                        {
                            FVector2D min = new();
                            FVector2D max = new();
                            byte isValid = 0;
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                if (dataGridView1.Rows[i].Cells.Count < 1) continue;
                                string name = dataGridView1.Rows[i].Cells[0].Value.ToString().Trim();
                                switch (name)
                                {
                                    case "Min":
                                        if (dataGridView1.Rows[i].Cells.Count < 3) continue;
                                        {
                                            double.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out double val1);
                                            double.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out double val2);
                                            min = new(val1, val2);
                                        }
                                        break;
                                    case "Max":
                                        if (dataGridView1.Rows[i].Cells.Count < 3) continue;
                                        {
                                            double.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out double val1);
                                            double.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out double val2);
                                            max = new(val1, val2);
                                        }
                                        break;
                                    case "IsValid":
                                        if (dataGridView1.Rows[i].Cells.Count < 2) continue;
                                        var val = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                        isValid = (val.Equals("1") || val.ToLowerInvariant().Equals("true")) ? (byte)1 : (byte)0;
                                        break;
                                }
                            }
                            box2.Value = new TBox<FVector2D>(min, max, isValid);
                        }
                        else if (pointerNode.Pointer is Box2fPropertyData box3)
                        {
                            FVector2f min = new();
                            FVector2f max = new();
                            byte isValid = 0;
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                if (dataGridView1.Rows[i].Cells.Count < 1) continue;
                                string name = dataGridView1.Rows[i].Cells[0].Value.ToString().Trim();
                                switch (name)
                                {
                                    case "Min":
                                        if (dataGridView1.Rows[i].Cells.Count < 3) continue;
                                        {
                                            float.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out float val1);
                                            float.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out float val2);
                                            min = new(val1, val2);
                                        }
                                        break;
                                    case "Max":
                                        if (dataGridView1.Rows[i].Cells.Count < 3) continue;
                                        {
                                            float.TryParse(dataGridView1.Rows[i].Cells[1].Value.ToString(), out float val1);
                                            float.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out float val2);
                                            max = new(val1, val2);
                                        }
                                        break;
                                    case "IsValid":
                                        if (dataGridView1.Rows[i].Cells.Count < 2) continue;
                                        var val = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                        isValid = (val.Equals("1") || val.ToLowerInvariant().Equals("true")) ? (byte)1 : (byte)0;
                                        break;
                                }
                            }
                            box3.Value = new TBox<FVector2f>(min, max, isValid);
                        }
                        else if (pointerNode.Pointer is NormalExport usCat)
                        {
                            switch (pointerNode.Type)
                            {
                                case PointingTreeNodeType.Normal:
                                    List<PropertyData> newData = new List<PropertyData>();
                                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                    {
                                        PropertyData val = RowToPD(i, usCat.Data.ElementAtOrDefault(i), false, asset.HasUnversionedProperties);
                                        if (val == null)
                                        {
                                            dirtySinceLastLoad = true;
                                            newData.Add(null);
                                            continue;
                                        }
                                        newData.Add(val);
                                    }
                                    if (newData[newData.Count - 1] == null) newData.RemoveAt(newData.Count - 1);
                                    usCat.Data = newData;
                                    pointerNode.Text = (usCat.ClassIndex.IsImport() ? usCat.ClassIndex.ToImport(asset).ObjectName.Value.Value : usCat.ClassIndex.Index.ToString()) + " (" + usCat.Data.Count + ")";
                                    break;
                                case PointingTreeNodeType.UserDefinedStructData:
                                    UserDefinedStructExport usCatUDS = (UserDefinedStructExport)usCat;
                                    List<PropertyData> newData2 = new List<PropertyData>();
                                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                    {
                                        PropertyData val = RowToPD(i, usCatUDS.StructData.ElementAtOrDefault(i), false, asset.HasUnversionedProperties);
                                        if (val == null)
                                        {
                                            dirtySinceLastLoad = true;
                                            newData2.Add(null);
                                            continue;
                                        }
                                        newData2.Add(val);
                                    }
                                    if (newData2[newData2.Count - 1] == null) newData2.RemoveAt(newData2.Count - 1);
                                    usCatUDS.StructData = newData2;
                                    pointerNode.Text = "UserDefinedStruct Data (" + newData2.Count + ")";
                                    break;
                                case PointingTreeNodeType.EnumData:
                                    if (usCat is EnumExport enumCat)
                                    {
                                        enumCat.Enum = new UEnum();
                                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                        {
                                            var currRow = dataGridView1.Rows[i];
                                            if (currRow == null || currRow.Cells.Count < 2) continue;

                                            string enumFrontValue = (string)currRow.Cells[0].Value;
                                            string enumValueValue = (string)currRow.Cells[1].Value;
                                            if (enumFrontValue == "CppForm")
                                            {
                                                Enum.TryParse(enumValueValue, out enumCat.Enum.CppForm);
                                            }
                                            else
                                            {
                                                long.TryParse(enumValueValue, out long enumValue);
                                                FName enumEntryName = FName.FromString(asset, enumFrontValue);
                                                if (enumEntryName != null) enumCat.Enum.Names.Add(new Tuple<FName, long>(enumEntryName, enumValue));
                                            }
                                        }
                                    }
                                    break;
                                case PointingTreeNodeType.Kismet:
                                    if (!UAGConfig.Data.EnablePrettyBytecode)
                                    {
                                        try
                                        {
                                            ((StructExport)pointerNode.Pointer).ScriptBytecode = asset.DeserializeJsonObject<KismetExpression[]>(jsonView.Text);
                                        }
                                        catch { }
                                    }
                                    break;
                            }
                        }
                        else if (pointerNode.Pointer is UDataTable dtUs)
                        {
                            int count = 0;
                            List<StructPropertyData> newData = new List<StructPropertyData>();
                            ///var numTimesNameUses = new Dictionary<string, int>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, dtUs.Data.ElementAtOrDefault(i), false, false);
                                if (val == null || !(val is StructPropertyData))
                                {
                                    dirtySinceLastLoad = true;
                                    newData.Add(null);
                                    continue;
                                }

                                // Cannot be guaranteed
                                /*if (numTimesNameUses.ContainsKey(val.Name.Value.Value))
                                {
                                    numTimesNameUses[val.Name.Value.Value]++;
                                }
                                else
                                {
                                    numTimesNameUses.Add(val.Name.Value.Value, 0);
                                }
                                val.Name.Number = numTimesNameUses[val.Name.Value.Value];*/
                                newData.Add((StructPropertyData)val);
                                count++;
                            }
                            dtUs.Data = newData;
                            pointerNode.Text = "Table Info (" + count + ")";
                            break;
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData usArr)
                        {
                            int count = 0;
                            List<PropertyData> newData = new List<PropertyData>();
                            List<PropertyData> origArr = usArr.Value.ToList();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i), !(origArr.ElementAtOrDefault(i) is StructPropertyData), asset.HasUnversionedProperties, PropertySerializationContext.Array);
                                if (val == null)
                                {
                                    dirtySinceLastLoad = true;
                                    newData.Add(null);
                                    continue;
                                    // deliberately do not increment count
                                }
                                count++;
                                newData.Add(val);
                            }
                            usArr.Value = newData.ToArray();
                            pointerNode.Text = usArr.Name.Value.Value + " (" + count + ")";
                        }
                        else if (pointerNode.Pointer is GameplayTagContainerPropertyData usArr2)
                        {
                            List<FName> newData = new List<FName>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                object val = dataGridView1.Rows[i].Cells[0].Value;
                                if (val == null || !(val is string)) continue;
                                newData.Add(FName.FromString(asset, (string)val));
                            }
                            usArr2.Value = newData.ToArray();
                            pointerNode.Text = usArr2.Name.Value.Value + " (" + usArr2.Value.Length + ")";
                        }
                        else if (pointerNode.Pointer is PointingDictionaryEntry usDictEntry)
                        {
                            MapPropertyData parentMap = ((PointingDictionaryEntry)pointerNode.Pointer).Pointer as MapPropertyData;

                            var allKeys = parentMap.Value.Keys.ToArray();
                            int currentEntry = -1;
                            for (int i = 0; i < parentMap.Value.Count; i++)
                            {
                                if (allKeys[i] == usDictEntry.Entry.Key)
                                {
                                    currentEntry = i;
                                    break;
                                }
                            }

                            PropertyData desiredKey = RowToPD(0, usDictEntry.Entry.Key, true, asset.HasUnversionedProperties, PropertySerializationContext.Map);
                            PropertyData desiredValue = RowToPD(1, usDictEntry.Entry.Value, true, asset.HasUnversionedProperties, PropertySerializationContext.Map);

                            if (currentEntry >= 0)
                            {
                                parentMap.Value.RemoveAt(currentEntry);
                                parentMap.Value.Insert(currentEntry, desiredKey, desiredValue);
                            }
                            else
                            {
                                parentMap.Value.Add(desiredKey, desiredValue);
                            }
                        }
                        else if (pointerNode.Pointer is PropertyData[] usRealArrEntry)
                        {
                            List<PropertyData> newData = new List<PropertyData>();
                            List<PropertyData> origArr = usRealArrEntry.ToList();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i), false, asset.HasUnversionedProperties);
                                if (val == null)
                                {
                                    dirtySinceLastLoad = true;
                                    continue;
                                }
                                newData.Add(val);
                            }
                            pointerNode.Pointer = newData.ToArray();
                        }
                    }
                    break;
            }

            if (forceNewLoad)
            {
                Load();
            }
            else
            {
                GetParentForm().UpdateRPC();
                GetParentForm().SetUnsavedChanges(true);
            }
        }

        public TableHandler(DataGridView dataGridView1, UAsset asset, TreeView treeView1, TextBox jsonView)
        {
            this.asset = asset;
            this.dataGridView1 = dataGridView1;
            this.treeView1 = treeView1;
            this.jsonView = jsonView;
            this.mode = 0;
        }
    }
}
