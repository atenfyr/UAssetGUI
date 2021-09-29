using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.PropertyTypes;
using UAssetAPI.StructTypes;

namespace UAssetGUI
{
    public enum TableHandlerMode
    {
        None = -1,
        GeneralInformation,
        NameMap,
        Imports,
        ExportInformation,
        SoftPackageReferences,
        DependsMap,
        WorldTileInfo,
        PreloadDependencies,
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
        UInt
    }

    public enum PointingTreeNodeType
    {
        Normal,
        StructData,
        ClassData,
        EnumData,
    }

    public class PointingTreeNode : TreeNode
    {
        public object Pointer;
        public PointingTreeNodeType Type;

        public PointingTreeNode(string label, object pointer, PointingTreeNodeType type = 0)
        {
            Pointer = pointer;
            Type = type;
            this.Text = label;
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
        public TreeView listView1;
        public DataGridView dataGridView1;

        public bool readyToSave = true;

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

        public void FillOutTree()
        {
            listView1.BeginUpdate();
            listView1.Nodes.Clear();
            listView1.BackColor = UAGPalette.BackColor;
            listView1.Nodes.Add(new PointingTreeNode("General Information", null));
            listView1.Nodes.Add(new PointingTreeNode("Name Map", null));
            listView1.Nodes.Add(new PointingTreeNode("Import Data", null));
            listView1.Nodes.Add(new PointingTreeNode("Export Information", null));
            listView1.Nodes.Add(new PointingTreeNode("Depends Map", null));
            listView1.Nodes.Add(new PointingTreeNode("Soft Package References", null));
            if (asset.WorldTileInfo != null)
            {
                listView1.Nodes.Add(new PointingTreeNode("World Tile Info", null));
                PointingTreeNode worldInfoNode = (PointingTreeNode)listView1.Nodes[listView1.Nodes.Count - 1];
                worldInfoNode.Nodes.Add(new PointingTreeNode("Layer (5)", asset.WorldTileInfo.Layer));
                worldInfoNode.Nodes.Add(new PointingTreeNode("LODList (" + asset.WorldTileInfo.LODList.Length + ")", asset.WorldTileInfo.LODList));
                PointingTreeNode lodListNode = (PointingTreeNode)listView1.Nodes[listView1.Nodes.Count - 1];
                for (int i = 0; i < asset.WorldTileInfo.LODList.Length; i++)
                {
                    lodListNode.Nodes.Add(new PointingTreeNode("LOD entry #" + (i + 1), asset.WorldTileInfo.LODList[i]));
                }
            }
            if (asset.UseSeparateBulkDataFiles) listView1.Nodes.Add(new PointingTreeNode("Preload Dependencies", null));
            listView1.Nodes.Add(new PointingTreeNode("Custom Version Container", null));
            listView1.Nodes.Add(new PointingTreeNode("Export Data", null));

            PointingTreeNode superTopNode = (PointingTreeNode)listView1.Nodes[listView1.Nodes.Count - 1];
            for (int i = 0; i < asset.Exports.Count; i++)
            {
                Export baseUs = asset.Exports[i];
                var categoryNode = new PointingTreeNode("Export " + (i + 1) + " (" + baseUs.ObjectName.Value.Value + ")", null);
                superTopNode.Nodes.Add(categoryNode);
                switch (baseUs)
                {
                    case RawExport us3:
                    {
                        var parentNode = new PointingTreeNode("Raw Data (" + us3.Data.Length + " B)", us3.Data);
                        categoryNode.Nodes.Add(parentNode);
                        break;
                    }
                    case NormalExport us:
                    {
                        var parentNode = new PointingTreeNode((baseUs.ClassIndex.IsImport() ? baseUs.ClassIndex.ToImport(asset).ObjectName.Value.Value : baseUs.ClassIndex.Index.ToString()) + " (" + us.Data.Count + ")", us);
                        categoryNode.Nodes.Add(parentNode);

                        for (int j = 0; j < us.Data.Count; j++) InterpretThing(us.Data[j], parentNode);

                        if (us is StringTableExport us2)
                        {
                            var parentNode2 = new PointingTreeNode(us2.Data2.Name + " (" + us2.Data2.Count + ")", us2.Data2);
                            categoryNode.Nodes.Add(parentNode2);
                        }

                        if (us is StructExport structUs)
                        {
                            var parentNode2 = new PointingTreeNode("UStruct Data", structUs, PointingTreeNodeType.StructData);
                            categoryNode.Nodes.Add(parentNode2);
                            var bytecodeNode = new PointingTreeNode("ScriptBytecode (" + structUs.ScriptBytecode.Length + " B)", structUs.ScriptBytecode, PointingTreeNodeType.Normal);
                            parentNode2.Nodes.Add(bytecodeNode);
                        }

                        if (us is ClassExport)
                        {
                            var parentNode2 = new PointingTreeNode("UClass Data", (ClassExport)us, PointingTreeNodeType.ClassData);
                            categoryNode.Nodes.Add(parentNode2);
                        }

                        if (us is DataTableExport us4)
                        {
                            var parentNode2 = new PointingTreeNode("Table Info (" + us4.Table.Data.Count + ")", us4.Table);
                            categoryNode.Nodes.Add(parentNode2);
                            foreach (StructPropertyData entry in us4.Table.Data)
                            {
                                string decidedName = entry.Name.ToString();

                                var structNode = new PointingTreeNode(decidedName + " (" + entry.Value.Count + ")", entry);
                                parentNode2.Nodes.Add(structNode);
                                for (int j = 0; j < entry.Value.Count; j++)
                                {
                                    InterpretThing(entry.Value[j], structNode);
                                }
                            }
                        }

                        if (us is EnumExport us5)
                        {
                            var parentNode2 = new PointingTreeNode("Enum Data", us5, PointingTreeNodeType.EnumData);
                            categoryNode.Nodes.Add(parentNode2);
                        }

                        {
                            var parentNode3 = new PointingTreeNode("Extra Data (" + us.Extras.Length + " B)", us.Extras);
                            categoryNode.Nodes.Add(parentNode3);
                        }

                        break;
                    }
                }
            }

            listView1.SelectedNode = listView1.Nodes[0];
            listView1.EndUpdate();
        }

        private void InterpretThing(PropertyData me, PointingTreeNode ourNode)
        {
            if (me == null) return;
            switch (me.PropertyType.Value.Value)
            {
                case "StructProperty":
                    var struc = (StructPropertyData)me;

                    string decidedName = struc.Name.Value.Value;
                    if (ourNode.Pointer is PropertyData && ((PropertyData)ourNode.Pointer).Name.Equals(decidedName)) decidedName = struc.StructType.Value.Value;

                    var structNode = new PointingTreeNode(decidedName + " (" + struc.Value.Count + ")", struc);
                    ourNode.Nodes.Add(structNode);
                    for (int j = 0; j < struc.Value.Count; j++)
                    {
                        InterpretThing(struc.Value[j], structNode);
                    }
                    break;
                case "SetProperty":
                case "ArrayProperty":
                    var arr = (ArrayPropertyData)me;

                    var arrNode = new PointingTreeNode(arr.Name.Value.Value + " (" + arr.Value.Length + ")", arr);
                    ourNode.Nodes.Add(arrNode);
                    for (int j = 0; j < arr.Value.Length; j++)
                    {
                        InterpretThing(arr.Value[j], arrNode);
                    }
                    break;
                case "GameplayTagContainer":
                    var arr2 = (GameplayTagContainerPropertyData)me;

                    var arrNode2 = new PointingTreeNode(arr2.Name.Value.Value + " (" + arr2.Value.Length + ")", arr2);
                    ourNode.Nodes.Add(arrNode2);
                    for (int j = 0; j < arr2.Value.Length; j++)
                    {
                        InterpretThing(arr2.Value[j], arrNode2);
                    }
                    break;
                case "MapProperty":
                    var mapp = (MapPropertyData)me;

                    var mapNode = new PointingTreeNode(mapp.Name.Value.Value + " (" + mapp.Value.Keys.Count + ")", mapp);
                    ourNode.Nodes.Add(mapNode);

                    foreach (var entry in mapp.Value)
                    {
                        entry.Key.Name = new FName("Key");
                        entry.Value.Name = new FName("Value");

                        var softEntryNode = new PointingTreeNode(mapp.Name.Value.Value + " (2)", new PointingDictionaryEntry(entry, mapp));
                        mapNode.Nodes.Add(softEntryNode);
                        InterpretThing(entry.Key, softEntryNode);
                        InterpretThing(entry.Value, softEntryNode);
                    }
                    break;
                case "MulticastDelegateProperty":
                    var mdp = (MulticastDelegatePropertyData)me;

                    ourNode.Nodes.Add(new PointingTreeNode(mdp.Name.Value.Value + " (" + mdp.Value.Length + ")", mdp.Value));
                    break;
                case "Box":
                    var box = (BoxPropertyData)me;

                    ourNode.Nodes.Add(new PointingTreeNode(box.Name.Value.Value + " (2)", box.Value));
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
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1);
                    row.Cells[0].Value = thisPD.Name.ToString();
                    row.Cells[1].Value = thisPD.PropertyType.ToString();
                    if (thisPD is UnknownPropertyData)
                    {
                        row.Cells[1].Value = ((UnknownPropertyData)thisPD).SerializingPropertyType.ToString();
                        row.Cells[2].Value = "Unknown ser.";
                        row.Cells[3].Value = ((UnknownPropertyData)thisPD).Value.ConvertByteArrayToString();
                    }
                    else
                    {
                        switch (thisPD.PropertyType.Value.Value)
                        {
                            case "BoolProperty":
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = ((BoolPropertyData)thisPD).Value.ToString();
                                break;
                            case "ObjectProperty":
                                var objData = (ObjectPropertyData)thisPD;
                                int decidedIndex = objData.Value?.Index ?? 0;
                                row.Cells[2].Value = decidedIndex;
                                if (decidedIndex != 0) UAGUtils.UpdateObjectPropertyValues(row, dataGridView1, objData);
                                break;
                            case "SoftObjectProperty":
                                var objData2 = (SoftObjectPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = objData2.Value;
                                break;
                            case "RichCurveKey":
                                var curveData = (RichCurveKeyPropertyData)thisPD;
                                row.Cells[2].Value = curveData.InterpMode;
                                row.Cells[3].Value = curveData.TangentMode;
                                row.Cells[4].Value = curveData.Time;
                                row.Cells[5].Value = curveData.Value;
                                row.Cells[6].Value = curveData.ArriveTangent;
                                row.Cells[7].Value = curveData.LeaveTangent;
                                break;
                            case "TextProperty":
                                var txtData = (TextPropertyData)thisPD;
                                row.Cells[2].Value = txtData.HistoryType.ToString();
                                row.Cells[2].ToolTipText = "HistoryType";
                                switch (txtData.HistoryType)
                                {
                                    case TextHistoryType.None:
                                        row.Cells[3].Value = txtData?.CultureInvariantString == null ? "null" : txtData.CultureInvariantString.ToString();
                                        row.Cells[3].ToolTipText = "CultureInvariantString";
                                        break;
                                    case TextHistoryType.Base:
                                        row.Cells[3].Value = txtData?.Namespace == null ? "null" : txtData.Namespace.ToString();
                                        row.Cells[3].ToolTipText = "Namespace";
                                        row.Cells[4].Value = txtData?.Value == null ? "null" : txtData.Value.ToString();
                                        row.Cells[4].ToolTipText = "Key";
                                        row.Cells[5].Value = txtData?.CultureInvariantString == null ? "null" : txtData.CultureInvariantString.ToString();
                                        row.Cells[5].ToolTipText = "CultureInvariantString";
                                        break;
                                    case TextHistoryType.StringTableEntry:
                                        row.Cells[3].Value = txtData?.TableId == null ? "null" : txtData.TableId.ToString();
                                        row.Cells[3].ToolTipText = "TableId";
                                        row.Cells[4].Value = txtData?.Value == null ? "null" : txtData.Value.ToString();
                                        row.Cells[4].ToolTipText = "Key";
                                        break;
                                    default:
                                        throw new NotImplementedException("Unimplemented display for " + txtData.HistoryType.ToString());
                                }
                                break;
                            case "NameProperty":
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = ((NamePropertyData)thisPD).ToString();
                                break;
                            case "ViewTargetBlendParams":
                                var viewTargetBlendParamsData = (ViewTargetBlendParamsPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = viewTargetBlendParamsData.BlendTime;
                                row.Cells[4].Value = viewTargetBlendParamsData.BlendFunction;
                                row.Cells[5].Value = viewTargetBlendParamsData.BlendExp;
                                row.Cells[6].Value = viewTargetBlendParamsData.bLockOutgoing;
                                break;
                            case "EnumProperty":
                                var enumData = (EnumPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = enumData.EnumType?.Value?.Value == null ? "null" : enumData.EnumType.ToString();
                                row.Cells[4].Value = enumData.Value?.Value?.Value == null ? "null" : enumData.Value.ToString();
                                //row.Cells[5].Value = enumData.Extra;
                                break;
                            case "ByteProperty":
                                var byteData = (BytePropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = byteData.GetEnumBase().Value;
                                if (byteData.ByteType == BytePropertyType.Byte)
                                {
                                    row.Cells[4].Value = byteData.Value;
                                }
                                else
                                {
                                    row.Cells[4].Value = byteData.GetEnumFull().Value;
                                }
                                break;
                            case "StructProperty":
                                row.Cells[2].Value = ((StructPropertyData)thisPD).StructType.ToString();
                                break;
                            case "ArrayProperty":
                            case "SetProperty":
                                row.Cells[2].Value = ((ArrayPropertyData)thisPD).ArrayType.ToString();
                                break;
                            case "GameplayTagContainer":
                            case "MapProperty":
                            case "SkeletalMeshSamplingLODBuiltData":
                                break;
                            case "Box":
                                var boxData = (BoxPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = boxData.IsValid;
                                break;
                            case "MulticastDelegateProperty":
                                var mdpData = (MulticastDelegatePropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                break;
                            case "LinearColor":
                                var colorData = (LinearColorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[2].ReadOnly = true;
                                if (colorData.RawValue != null)
                                {
                                    row.Cells[2].Style.BackColor = ARGBtoRGB(LinearHelpers.Convert(colorData.Value));
                                    row.Cells[2].ToolTipText = "Preview";
                                }
                                row.Cells[3].Value = colorData.Value.R;
                                row.Cells[3].ToolTipText = "Red";
                                row.Cells[4].Value = colorData.Value.G;
                                row.Cells[4].ToolTipText = "Green";
                                row.Cells[5].Value = colorData.Value.B;
                                row.Cells[5].ToolTipText = "Blue";
                                row.Cells[6].Value = colorData.Value.A;
                                row.Cells[6].ToolTipText = "Alpha";
                                break;
                            case "Color":
                                var colorData2 = (ColorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[2].ReadOnly = true;
                                if (colorData2.RawValue != null)
                                {
                                    row.Cells[2].Style.BackColor = colorData2.Value;
                                    row.Cells[2].ToolTipText = "Preview";
                                }
                                row.Cells[3].Value = colorData2.Value.R;
                                row.Cells[3].ToolTipText = "Red";
                                row.Cells[4].Value = colorData2.Value.G;
                                row.Cells[4].ToolTipText = "Green";
                                row.Cells[5].Value = colorData2.Value.B;
                                row.Cells[5].ToolTipText = "Blue";
                                row.Cells[6].Value = colorData2.Value.A;
                                row.Cells[6].ToolTipText = "Alpha";
                                break;
                            case "Vector":
                                var vectorData = (VectorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vectorData.X;
                                row.Cells[3].ToolTipText = "X";
                                row.Cells[4].Value = vectorData.Y;
                                row.Cells[4].ToolTipText = "Y";
                                row.Cells[5].Value = vectorData.Z;
                                row.Cells[5].ToolTipText = "Z";
                                break;
                            case "Vector2D":
                                var vector2DData = (Vector2DPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vector2DData.X;
                                row.Cells[3].ToolTipText = "X";
                                row.Cells[4].Value = vector2DData.Y;
                                row.Cells[4].ToolTipText = "Y";
                                break;
                            case "Vector4":
                                var vector4DData = (Vector4PropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vector4DData.X;
                                row.Cells[3].ToolTipText = "X";
                                row.Cells[4].Value = vector4DData.Y;
                                row.Cells[4].ToolTipText = "Y";
                                row.Cells[5].Value = vector4DData.Z;
                                row.Cells[5].ToolTipText = "Y";
                                row.Cells[6].Value = vector4DData.W;
                                row.Cells[6].ToolTipText = "W";
                                break;
                            case "IntPoint":
                                var intPointData = (IntPointPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = intPointData.Value[0];
                                row.Cells[4].Value = intPointData.Value[1];
                                break;
                            case "Rotator":
                                var rotatorData = (RotatorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = rotatorData.Pitch;
                                row.Cells[3].ToolTipText = "Pitch";
                                row.Cells[4].Value = rotatorData.Yaw;
                                row.Cells[4].ToolTipText = "Yaw";
                                row.Cells[5].Value = rotatorData.Roll;
                                row.Cells[5].ToolTipText = "Roll";
                                break;
                            case "Quat":
                                var quatData = (QuatPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = quatData.X;
                                row.Cells[3].ToolTipText = "X";
                                row.Cells[4].Value = quatData.Y;
                                row.Cells[4].ToolTipText = "Y";
                                row.Cells[5].Value = quatData.Z;
                                row.Cells[5].ToolTipText = "Z";
                                row.Cells[6].Value = quatData.W;
                                row.Cells[6].ToolTipText = "W";
                                break;
                            case "StrProperty":
                                var strPropData = (StrPropertyData)thisPD;
                                row.Cells[2].Value = (strPropData.Value?.Encoding ?? Encoding.ASCII).HeaderName;
                                row.Cells[3].Value = strPropData.Value?.Value == null ? "null" : Convert.ToString(strPropData.Value.Value);
                                break;
                            default:
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = Convert.ToString(thisPD.RawValue);
                                break;
                        }
                    }

                    row.Cells[8].Value = thisPD.DuplicationIndex;
                    row.Cells[9].Value = thisPD.Offset < 0 ? "N/A" : (asset.UseSeparateBulkDataFiles ? (thisPD.Offset - asset.Exports[0].SerialOffset) : thisPD.Offset).ToString();
                    row.HeaderCell.Value = Convert.ToString(i);
                    rows.Add(row);
                }
                //catch (Exception)
                //{

                //}
            }
            dataGridView1.Rows.AddRange(rows.ToArray());
        }

        private PropertyData RowToPD(int rowNum, PropertyData original)
        {
            try
            {
                DataGridViewRow row = dataGridView1.Rows[rowNum];
                object nameB = row.Cells[0].Value;
                object typeB = row.Cells[1].Value;
                object transformB = row.Cells[2].Value;
                object value1B = row.Cells[3].Value;
                object value2B = row.Cells[4].Value;
                object value3B = row.Cells[5].Value;
                object value4B = row.Cells[6].Value;
                object value5B = row.Cells[7].Value;

                if (nameB == null || typeB == null) return null;
                if (!(nameB is string) || !(typeB is string)) return null;

                string name = (string)nameB;
                string type = (string)typeB;
                if (name.Equals(string.Empty) || type.Equals(string.Empty)) return null;

                if (value1B != null && value1B is string && transformB != null && transformB is string && (string)transformB == "Unknown ser.")
                {
                    var res = new UnknownPropertyData(FName.FromString(name), asset)
                    {
                        Value = ((string)value1B).ConvertStringToByteArray()
                    };
                    res.SetSerializingPropertyType(FName.FromString(type));
                    return res;
                }

                switch (FName.FromString(type).Value.Value)
                {
                    case "TextProperty":
                        TextPropertyData decidedTextData = null;
                        if (original != null && original is TextPropertyData)
                        {
                            decidedTextData = (TextPropertyData)original;
                        }
                        else
                        {
                            decidedTextData = new TextPropertyData(FName.FromString(name), asset);
                        }

                        TextHistoryType histType = TextHistoryType.Base;
                        if (transformB == null || value1B == null || !(value1B is string)) return null;
                        if (transformB is string) Enum.TryParse((string)transformB, out histType);

                        decidedTextData.HistoryType = histType;
                        switch (histType)
                        {
                            case TextHistoryType.None:
                                decidedTextData.Value = null;
                                if (value1B != null && value1B is string) decidedTextData.CultureInvariantString = (string)value1B == "null" ? null : new FString((string)value1B);
                                break;
                            case TextHistoryType.Base:
                                if (value1B == null || value2B == null || value3B == null || !(value1B is string) || !(value2B is string) || !(value3B is string)) return null;
                                decidedTextData.Namespace = (string)value1B == "null" ? null : new FString((string)value1B);
                                decidedTextData.Value = (string)value2B == "null" ? null :  new FString((string)value2B);
                                decidedTextData.CultureInvariantString = (string)value3B == "null" ? null : new FString((string)value3B);
                                break;
                            case TextHistoryType.StringTableEntry:
                                if (value1B == null || !(value1B is string) || !(value2B is string)) return null;

                                decidedTextData.TableId = FName.FromString((string)value1B);
                                decidedTextData.Value = (string)value2B == "null" ? null : new FString((string)value2B);
                                break;
                            default:
                                throw new FormatException("Unimplemented text history type " + histType);
                        }

                        if (value4B != null && value4B is string) Enum.TryParse((string)value4B, out decidedTextData.Flags);

                        return decidedTextData;
                    case "ObjectProperty":
                        ObjectPropertyData decidedObjData = null;
                        if (original != null && original is ObjectPropertyData)
                        {
                            decidedObjData = (ObjectPropertyData)original;
                        }
                        else
                        {
                            decidedObjData = new ObjectPropertyData(FName.FromString(name), asset);
                        }

                        int objValue = int.MinValue;
                        if (transformB == null) return null;
                        if (transformB is string) int.TryParse((string)transformB, out objValue);
                        if (transformB is int) objValue = (int)transformB;
                        if (objValue == int.MinValue) return null;

                        decidedObjData.Value = new FPackageIndex(objValue);
                        UAGUtils.UpdateObjectPropertyValues(row, dataGridView1, decidedObjData);
                        return decidedObjData;
                    case "RichCurveKey":
                        RichCurveKeyPropertyData decidedRCKProperty = null;
                        if (original != null && original is RichCurveKeyPropertyData)
                        {
                            decidedRCKProperty = (RichCurveKeyPropertyData)original;
                        }
                        else
                        {
                            decidedRCKProperty = new RichCurveKeyPropertyData(FName.FromString(name), asset);
                        }

                        if (transformB is string) Enum.TryParse((string)transformB, out decidedRCKProperty.InterpMode);
                        if (value1B is string) Enum.TryParse((string)value1B, out decidedRCKProperty.TangentMode);

                        if (value2B is string) float.TryParse((string)value2B, out decidedRCKProperty.Time);
                        if (value2B is int) decidedRCKProperty.Time = (float)(int)value2B;
                        if (value2B is float) decidedRCKProperty.Time = (float)value2B;
                        if (value3B is string) float.TryParse((string)value3B, out decidedRCKProperty.Value);
                        if (value3B is int) decidedRCKProperty.Value = (float)(int)value3B;
                        if (value3B is float) decidedRCKProperty.Value = (float)value3B;
                        if (value4B is string) float.TryParse((string)value4B, out decidedRCKProperty.ArriveTangent);
                        if (value4B is int) decidedRCKProperty.ArriveTangent = (float)(int)value4B;
                        if (value4B is float) decidedRCKProperty.ArriveTangent = (float)value4B;
                        if (value5B is string) float.TryParse((string)value5B, out decidedRCKProperty.LeaveTangent);
                        if (value5B is int) decidedRCKProperty.LeaveTangent = (float)(int)value5B;
                        if (value5B is float) decidedRCKProperty.LeaveTangent = (float)value5B;

                        return decidedRCKProperty;
                    default:
                        PropertyData newThing = MainSerializer.TypeToClass(FName.FromString(type), FName.FromString(name), asset);
                        if (original != null && original.GetType() == newThing.GetType())
                        {
                            newThing = original;
                            newThing.Name = FName.FromString(name);
                        }

                        string[] existingStrings = new string[5];
                        if (value1B != null) existingStrings[0] = Convert.ToString(value1B);
                        if (value2B != null) existingStrings[1] = Convert.ToString(value2B);
                        if (value3B != null) existingStrings[2] = Convert.ToString(value3B);
                        if (value4B != null) existingStrings[3] = Convert.ToString(value4B);
                        if (transformB != null) existingStrings[4] = Convert.ToString(transformB);

                        newThing.FromString(existingStrings);
                        return newThing;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void ChangeAllExpansionStatus(bool expanding)
        {
            listView1.BeginUpdate();
            foreach (TreeNode node in Collect(listView1.Nodes))
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
            listView1.EndUpdate();

            (listView1.SelectedNode ?? listView1.Nodes[0]).EnsureVisible();
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

        public void Load() // Updates the table with selected asset data
        {
            if (mode == TableHandlerMode.None)
            {
                ClearScreen();
                return;
            }

            Form1 origForm = (Form1)dataGridView1.Parent;
            var byteView1 = origForm.byteView1;
            byteView1.Visible = false;
            dataGridView1.Visible = true;
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.ReadOnly = false;

            dataGridView1.BackgroundColor = UAGPalette.DataGridViewActiveColor;
            readyToSave = false;

            switch (mode)
            {
                case TableHandlerMode.GeneralInformation:
                    AddColumns(new string[] { "Property Name", "Value", "" });

                    dataGridView1.Rows.Add(new object[] { "LegacyFileVersion", asset.LegacyFileVersion.ToString() });
                    dataGridView1.Rows.Add(new object[] { "IsUnversioned", asset.IsUnversioned.ToString() });
                    dataGridView1.Rows.Add(new object[] { "FileVersionLicenseeUE4", asset.FileVersionLicenseeUE4.ToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageGuid", asset.PackageGuid.ToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageFlags", asset.PackageFlags.ToString() });
                    dataGridView1.Rows.Add(new object[] { "PackageSource", asset.PackageSource.ToString() });
                    dataGridView1.Rows.Add(new object[] { "FolderName", asset.FolderName.Value });

                    dataGridView1.Rows[0].Cells[0].ToolTipText = "The package file version number when this package was saved. Unrelated to imports.";
                    dataGridView1.Rows[1].Cells[0].ToolTipText = "Should this asset not serialize its engine and custom versions?";
                    dataGridView1.Rows[2].Cells[0].ToolTipText = "The licensee file version. Used by some games to add their own Engine-level versioning.";
                    dataGridView1.Rows[3].Cells[0].ToolTipText = "Current ID for this package. Effectively unused.";
                    dataGridView1.Rows[4].Cells[0].ToolTipText = "The flags for this package.";
                    dataGridView1.Rows[5].Cells[0].ToolTipText = "Value that is used to determine if the package was saved by Epic, a licensee, modder, etc.";
                    dataGridView1.Rows[6].Cells[0].ToolTipText = "The Generic Browser folder name that this package lives in. Usually \"None\" in cooked assets.";

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    }

                    dataGridView1.AllowUserToAddRows = false;
                    break;
                case TableHandlerMode.NameMap:
                    AddColumns(new string[] { "Name", "Encoding", "" });

                    IReadOnlyList<FString> headerIndexList = asset.GetNameMapIndexList();
                    for (int num = 0; num < headerIndexList.Count; num++)
                    {
                        dataGridView1.Rows.Add(headerIndexList[num].Value, headerIndexList[num].Encoding.HeaderName);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                    }
                    break;
                case TableHandlerMode.Imports:
                    AddColumns(new string[] { "ClassPackage", "ClassName", "OuterIndex", "ObjectName", "" });

                    for (int num = 0; num < asset.Imports.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.Imports[num].ClassPackage.ToString(), asset.Imports[num].ClassName.ToString(), asset.Imports[num].OuterIndex, asset.Imports[num].ObjectName.ToString());
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(FPackageIndex.FromImport(num));
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    string[] allExportDetailsFields = Export.GetAllFieldNames();
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

                            object printingVal = refer.GetType().GetField(allExportDetailsFields[num2]).GetValue(refer);
                            if (printingVal is FName parsingName)
                            {
                                string actualName = parsingName?.ToString();
                                if (actualName == null) actualName = "null";
                                newCells[num2] = actualName;
                            }
                            else if (printingVal is FPackageIndex parsingIndex)
                            {
                                newCells[num2] = parsingIndex.Index;
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

                    for (int num = 0; num < asset.SoftPackageReferenceList.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.SoftPackageReferenceList[num]);
                    }
                    break;
                case TableHandlerMode.WorldTileInfo:
                    AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "" });

                    if (listView1.SelectedNode is PointingTreeNode wtlPointerNode)
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
                case TableHandlerMode.PreloadDependencies:
                    AddColumns(new string[] { "Value", "" });

                    for (int num = 0; num < asset.PreloadDependencies.Count; num++)
                    {
                        dataGridView1.Rows.Add(new object[] { asset.PreloadDependencies[num].Index });
                    }
                    break;
                case TableHandlerMode.CustomVersionContainer:
                    AddColumns(new string[] { "Name", "Version", "" });

                    for (int num = 0; num < asset.CustomVersionContainer.Count; num++)
                    {
                        dataGridView1.Rows.Add(new object[] { asset.CustomVersionContainer[num].FriendlyName == null ? Convert.ToString(asset.CustomVersionContainer[num].Key) : asset.CustomVersionContainer[num].FriendlyName, asset.CustomVersionContainer[num].Version });
                    }
                    break;
                case TableHandlerMode.ExportData:
                    if (listView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        AddColumns(new string[] { "Name", "Type", "Variant", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "DupIndex", "Serial Offset", "" });
                        bool standardRendering = true;
                        PropertyData[] renderingArr = null;

                        switch (pointerNode.Pointer)
                        {
                            case NormalExport usCategory:
                                switch(pointerNode.Type)
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
                                    case PointingTreeNodeType.StructData:
                                        dataGridView1.Columns.Clear();
                                        AddColumns(new string[] { "Property Name", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "" });

                                        StructExport strucCat = (StructExport)usCategory;
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                        {
                                            ObjectPropertyData testProperty = new ObjectPropertyData(new FName("Super Struct"), asset);
                                            testProperty.Value = strucCat.SuperStruct;

                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = "Super Struct";
                                            row.Cells[1].Value = testProperty.Value;
                                            row.Cells[2].Value = testProperty.Value.IsImport() ? testProperty.Value.ToImport(asset).ObjectName.ToString() : "";
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
                                            ObjectPropertyData testProperty = new ObjectPropertyData(new FName("Super Struct"), asset);
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
                                            Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                            sty.Font = styFont;
                                            sty.ForeColor = Color.Blue;
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
                                                Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                                sty.Font = styFont;
                                                sty.ForeColor = Color.Blue;
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
                                }
   
                                break;
                            case StringTable strUs:
                                {
                                    List<DataGridViewRow> rows = new List<DataGridViewRow>();
                                    for (int i = 0; i < strUs.Count; i++)
                                    {
                                        DataGridViewRow row = new DataGridViewRow();
                                        row.CreateCells(dataGridView1);
                                        row.Cells[0].Value = strUs.Name;
                                        row.Cells[1].Value = "StringTableEntry";
                                        row.Cells[2].Value = strUs[i].Encoding.HeaderName;
                                        row.Cells[3].Value = strUs[i].Value.Replace("\n", "\\n").Replace("\r", "\\r");
                                        row.HeaderCell.Value = Convert.ToString(i);
                                        rows.Add(row);
                                    }
                                    dataGridView1.Rows.AddRange(rows.ToArray());
                                    standardRendering = false;
                                    break;
                                }
                            case DataTable dtUs:
                                {
                                    renderingArr = dtUs.Data.ToArray();
                                    break;
                                }
                            case MapPropertyData usMap:
                                {
                                    if (usMap.Value.Count > 0)
                                    {
                                        FName mapKeyType = usMap.Value.Keys.ElementAt(0).PropertyType;
                                        FName mapValueType = usMap.Value[0].PropertyType;

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
                                            Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                            sty.Font = styFont;
                                            sty.ForeColor = Color.Blue;
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
                                for (int num = 0; num < usStruct.Value.Count; num++)
                                {
                                    if (usStruct.Value[num] == null)
                                    {
                                        usStruct.Value.RemoveAt(num);
                                        num--;
                                    }
                                }
                                renderingArr = usStruct.Value.ToArray();
                                break;
                            case ArrayPropertyData usArr:
                                renderingArr = usArr.Value;
                                break;
                            case GameplayTagContainerPropertyData usArr2:
                                renderingArr = usArr2.Value;
                                break;
                            case PointingDictionaryEntry usDictEntry:
                                dataGridView1.AllowUserToAddRows = false;
                                var ourKey = usDictEntry.Entry.Key;
                                var ourValue = usDictEntry.Entry.Value;
                                if (ourKey != null) ourKey.Name = new FName("Key");
                                if (ourValue != null) ourValue.Name = new FName("Value");
                                renderingArr = new PropertyData[2] { ourKey, ourValue };
                                break;
                            case FMulticastDelegate[] usRealMDArr:
                                for (int i = 0; i < usRealMDArr.Length; i++)
                                {
                                    dataGridView1.Rows.Add(usRealMDArr[i].Delegate, "FMulticastDelegate", usRealMDArr[i].Number);
                                }
                                standardRendering = false;
                                break;
                            case PropertyData[] usRealArr:
                                renderingArr = usRealArr;
                                dataGridView1.AllowUserToAddRows = false;
                                break;
                            case byte[] bytes:
                                Control currentlyFocusedControl = ((Form1)dataGridView1.Parent).ActiveControl;
                                dataGridView1.Visible = false;
                                byteView1.SetBytes(new byte[] { });
                                byteView1.SetBytes(bytes);
                                byteView1.Visible = true;
                                currentlyFocusedControl.Focus();
                                origForm.ForceResize();
                                standardRendering = false;
                                break;
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

            readyToSave = true;
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        public void Save(bool forceNewLoad) // Reads from the table and updates the asset data as needed
        {
            if (!readyToSave) return;

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
                                asset.IsUnversioned = propertyValue.ToLowerInvariant() == "true" || propertyValue == "1";
                                break;
                            case "FileVersionLicenseeUE4":
                                asset.FileVersionLicenseeUE4 = Convert.ToInt32(propertyValue);
                                break;
                            case "PackageGuid":
                                if (Guid.TryParse(propertyValue, out Guid newPackageGuid)) asset.PackageGuid = newPackageGuid;
                                break;
                            case "PackageFlags":
                                if (Enum.TryParse(propertyValue, out EPackageFlags newPackageFlags)) asset.PackageFlags = newPackageFlags;
                                break;
                            case "PackageSource":
                                if (uint.TryParse(propertyValue, out uint newPackageSource)) asset.PackageSource = newPackageSource;
                                break;
                            case "FolderName":
                                asset.FolderName = new FString(propertyValue, Encoding.UTF8.GetByteCount(propertyValue) == propertyValue.Length ? Encoding.ASCII : Encoding.Unicode);
                                break;
                        }

                    }
                    break;
                case TableHandlerMode.NameMap:
                    asset.ClearNameIndexList();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string ourValue = (string)row.Cells[0].Value;
                        string encoding = (string)row.Cells[1].Value;
                        if (string.IsNullOrWhiteSpace(encoding)) encoding = "ascii";
                        if (!string.IsNullOrWhiteSpace(ourValue)) asset.AddNameReference(new FString(ourValue, encoding.Equals("utf-16") ? Encoding.Unicode : Encoding.ASCII), true);
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
                        if (val1 == null || val2 == null || val3 == null || val4 == null) continue;
                        if (!(val1 is string) || !(val2 is string) || !(val4 is string)) continue;
                        if (!(val3 is string) && !(val3 is int)) continue;

                        int realVal3;
                        if (val3 is string)
                        {
                            if (!int.TryParse((string)val3, out realVal3)) continue;
                        }
                        else
                        {
                            realVal3 = Convert.ToInt32(val3);
                        }

                        string realVal1 = (string)val1;
                        string realVal2 = (string)val2;
                        string realVal4 = (string)val4;
                        if (string.IsNullOrWhiteSpace(realVal1) || string.IsNullOrWhiteSpace(realVal2) || string.IsNullOrWhiteSpace(realVal4)) continue;

                        FName parsedVal1 = FName.FromString(realVal1);
                        FName parsedVal2 = FName.FromString(realVal2);
                        FName parsedVal4 = FName.FromString(realVal4);
                        asset.AddNameReference(parsedVal1.Value);
                        asset.AddNameReference(parsedVal2.Value);
                        asset.AddNameReference(parsedVal4.Value);
                        Import newLink = new Import(parsedVal1, parsedVal2, realVal3, parsedVal4);
                        asset.Imports.Add(newLink);
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    FieldInfo[] allExportDetailsFields = Export.GetAllObjectExportFields();
                    ExportDetailsParseType[] parsingTypes = new ExportDetailsParseType[]
                    {
                        ExportDetailsParseType.FName,
                        ExportDetailsParseType.Int,

                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.FPackageIndex,
                        ExportDetailsParseType.EObjectFlags,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Long,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Guid,
                        ExportDetailsParseType.UInt,
                        ExportDetailsParseType.Bool,
                        ExportDetailsParseType.Bool,

                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int
                    };

                    int rowNum = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        bool isNewExport = false;
                        if (asset.Exports.Count <= rowNum)
                        {
                            // If we add a new category, we'll make a new NormalExport (None-terminated UProperty list). If you want to make some other kind of export, you'll need to do it manually with UAssetAPI
                            var newCat = new NormalExport(asset, new byte[4]);
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
                                    settingVal = new FName();
                                    if (currentVal is string rawFName) // blah(0)
                                    {
                                        settingVal = FName.FromString(rawFName);
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
                                        settingVal = ((string)currentVal).Equals("1") || ((string)currentVal).ToLower().Equals("true");
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
                                        Guid.TryParse((string)currentVal, out x);
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
                            }

                            allExportDetailsFields[i].SetValue(asset.Exports[rowNum], settingVal);
                        }

                        if (isInvalidRow && isNewExport)
                        {
                            asset.Exports.RemoveAt(asset.Exports.Count - 1);
                        }
                        rowNum++;
                    }
                    break;
                case TableHandlerMode.DependsMap:
                    asset.DependsMap = new List<int[]>();
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

                        if (asset.DependsMap.Count > vals[0])
                        {
                            var arr = asset.DependsMap[vals[0]];
                            Array.Resize(ref arr, arr.Length + 1);
                            arr[arr.Length - 1] = vals[1];
                            asset.DependsMap[vals[0]] = arr;
                        }
                        else
                        {
                            asset.DependsMap.Insert(vals[0], new int[]{ vals[1] });
                        }
                    }
                    break;
                case TableHandlerMode.SoftPackageReferences:
                    asset.SoftPackageReferenceList = new List<string>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string strVal = (string)row.Cells[0].Value;
                        if (!string.IsNullOrEmpty(strVal)) asset.SoftPackageReferenceList.Add(strVal);
                    }
                    break;
                case TableHandlerMode.PreloadDependencies:
                    asset.PreloadDependencies = new List<FPackageIndex>();
                    int rowN = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        int intVal;
                        if (row.Cells[0].Value == null) continue;
                        if (row.Cells[0].Value is string)
                        {
                            string strVal = (string)row.Cells[0].Value;
                            if (string.IsNullOrEmpty(strVal)) continue;
                            bool result = int.TryParse(strVal, out int x);
                            if (!result) continue;
                            intVal = x;
                        }
                        else
                        {
                            intVal = Convert.ToInt32(row.Cells[0].Value);
                        }

                        if (asset.PreloadDependencies.Count > rowN)
                        {
                            asset.PreloadDependencies[rowN] = FPackageIndex.FromRawIndex(intVal);
                        }
                        else
                        {
                            asset.PreloadDependencies.Insert(rowN, FPackageIndex.FromRawIndex(intVal));
                        }

                        rowN++;
                    }
                    break;
                case TableHandlerMode.WorldTileInfo:
                    // Modification is disabled

                    break;
                case TableHandlerMode.CustomVersionContainer:
                    asset.CustomVersionContainer = new List<CustomVersion>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string rawCustomVersion = (string)row.Cells[0].Value;
                        Guid customVersionKey = CustomVersion.UnusedCustomVersionKey;
                        if (!Guid.TryParse(rawCustomVersion, out customVersionKey))
                        {
                            customVersionKey = CustomVersion.GetCustomVersionGuidFromFriendlyName(rawCustomVersion);
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
                    if (listView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Pointer is StringTable usStrTable)
                        {
                            usStrTable.Clear();
                            List<DataGridViewRow> rows = new List<DataGridViewRow>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                DataGridViewRow row = dataGridView1.Rows[i];
                                object transformB = row.Cells[2].Value;
                                object value1B = row.Cells[3].Value;
                                if (transformB == null || value1B == null || !(transformB is string) || !(value1B is string)) continue;

                                usStrTable.Add(new FString(((string)value1B).Replace("\\n", "\n").Replace("\\r", "\r"), ((string)transformB).Equals("utf-16") ? Encoding.Unicode : Encoding.ASCII));
                            }

                            pointerNode.Text = usStrTable.Name + " (" + usStrTable.Count + ")";
                        }
                        else if (pointerNode.Pointer is MapPropertyData usMap)
                        {
                            FName mapKeyType = usMap.Value.Keys.ElementAt(0).PropertyType;
                            FName mapValueType = usMap.Value[0].PropertyType;

                            var newData = new TMap<PropertyData, PropertyData>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                DataGridViewRow row = dataGridView1.Rows[i];
                                if (row.Cells.Count <= 1 || row.Cells[1].Value == null || !row.Cells[1].Value.Equals("MapEntry")) continue;

                                if (row.Tag is KeyValuePair<PropertyData, PropertyData> dictBit)
                                {
                                    newData.Add(dictBit.Key, dictBit.Value);
                                }
                                else
                                {
                                    newData.Add(MainSerializer.TypeToClass(mapKeyType, usMap.Name, asset), MainSerializer.TypeToClass(mapValueType, usMap.Name, asset));
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
                                PropertyData val = RowToPD(i, usStruct.Value.ElementAtOrDefault(i));
                                if (val == null)
                                {
                                    newData.Add(null);
                                    continue;
                                }
                                newData.Add(val);
                                newCount++;
                            }
                            usStruct.Value = newData;

                            string decidedName = usStruct.Name.Value.Value;
                            if (((PointingTreeNode)pointerNode.Parent).Pointer is PropertyData && ((PropertyData)((PointingTreeNode)pointerNode.Parent).Pointer).Name.Equals(decidedName)) decidedName = usStruct.StructType.Value.Value;
                            pointerNode.Text = decidedName + " (" + newCount + ")";
                        }
                        else if (pointerNode.Pointer is ClassExport usBGCCat)
                        {
                            // No writing here
                        }
                        else if (pointerNode.Pointer is NormalExport usCat)
                        {
                            switch(pointerNode.Type)
                            {
                                case PointingTreeNodeType.Normal:
                                    List<PropertyData> newData = new List<PropertyData>();
                                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                    {
                                        PropertyData val = RowToPD(i, usCat.Data.ElementAtOrDefault(i));
                                        if (val == null)
                                        {
                                            newData.Add(null);
                                            continue;
                                        }
                                        newData.Add(val);
                                    }
                                    if (newData[newData.Count - 1] == null) newData.RemoveAt(newData.Count - 1);
                                    usCat.Data = newData;
                                    pointerNode.Text = (usCat.ClassIndex.IsImport() ? usCat.ClassIndex.ToImport(asset).ObjectName.Value.Value : usCat.ClassIndex.Index.ToString()) + " (" + usCat.Data.Count + ")";
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
                                                FName enumEntryName = FName.FromString(enumFrontValue);
                                                if (enumEntryName != null) enumCat.Enum.Names.Add(new Tuple<FName, long>(enumEntryName, enumValue));
                                            }
                                        }
                                    }
                                    break;
                            }
                            
                        }
                        else if (pointerNode.Pointer is DataTable dtUs)
                        {
                            List<StructPropertyData> newData = new List<StructPropertyData>();
                            var numTimesNameUses = new Dictionary<string, int>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, dtUs.Data.ElementAtOrDefault(i));
                                if (val == null || !(val is StructPropertyData)) continue;
                                if (numTimesNameUses.ContainsKey(val.Name.Value.Value))
                                {
                                    numTimesNameUses[val.Name.Value.Value]++;
                                }
                                else
                                {
                                    numTimesNameUses.Add(val.Name.Value.Value, 0);
                                }
                                val.Name.Number = numTimesNameUses[val.Name.Value.Value];
                                newData.Add((StructPropertyData)val);
                            }
                            dtUs.Data = newData;
                            pointerNode.Text = "Table Info (" + dtUs.Data.Count + ")";
                            break;
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData usArr)
                        {
                            List<PropertyData> newData = new List<PropertyData>();
                            List<PropertyData> origArr = usArr.Value.ToList();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i));
                                if (val == null) continue;
                                newData.Add(val);
                            }
                            usArr.Value = newData.ToArray();
                            pointerNode.Text = usArr.Name.Value.Value + " (" + usArr.Value.Length + ")";
                        }
                        else if (pointerNode.Pointer is GameplayTagContainerPropertyData usArr2)
                        {
                            List<NamePropertyData> newData = new List<NamePropertyData>();
                            List<NamePropertyData> origArr = usArr2.Value.ToList();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i));
                                if (val == null || !(val is NamePropertyData)) continue;
                                newData.Add((NamePropertyData)val);
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

                            PropertyData desiredKey = RowToPD(0, usDictEntry.Entry.Key);
                            PropertyData desiredValue = RowToPD(1, usDictEntry.Entry.Value);

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
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i));
                                if (val == null) continue;
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
                ((Form1)dataGridView1.Parent).SetUnsavedChanges(true);
            }
        }

        public TableHandler(DataGridView dataGridView1, UAsset asset, TreeView listView1)
        {
            this.asset = asset;
            this.dataGridView1 = dataGridView1;
            this.listView1 = listView1;
            this.mode = 0;
        }
    }
}
