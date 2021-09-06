using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        NameMap,
        Imports,
        ExportInformation,
        SoftPackageReferences,
        DependsMap,
        PreloadDependencyMap,
        CustomVersionContainer,
        ExportData
    }

    public enum ExportDetailsParseType
    {
        None = -1,
        Int,
        FName,
        EObjectFlags,
        Long,
        Bool,
        Guid,
        UInt
    }

    public class PointingTreeNode : TreeNode
    {
        public object Pointer;
        public int Type;

        public PointingTreeNode(string label, object pointer, int type = 0)
        {
            Pointer = pointer;
            Type = type;
            this.Text = label;
        }
    }

    public class PointingDictionaryEntry
    {
        public DictionaryEntry Entry;
        public object Pointer;

        public PointingDictionaryEntry(DictionaryEntry entry, object pointer)
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
            listView1.Nodes.Add(new PointingTreeNode("Name Map", null));
            listView1.Nodes.Add(new PointingTreeNode("Import Data", null));
            listView1.Nodes.Add(new PointingTreeNode("Export Information", null));
            listView1.Nodes.Add(new PointingTreeNode("Depends Map", null));
            listView1.Nodes.Add(new PointingTreeNode("Soft Package References", null));
            if (asset.UseSeparateBulkDataFiles) listView1.Nodes.Add(new PointingTreeNode("Preload Dependency Map", null));
            if (asset.CustomVersionContainer.Count > 0) listView1.Nodes.Add(new PointingTreeNode("Custom Version Container", null));
            listView1.Nodes.Add(new PointingTreeNode("Export Data", null));

            PointingTreeNode superTopNode = (PointingTreeNode)listView1.Nodes[listView1.Nodes.Count - 1];
            for (int i = 0; i < asset.Exports.Count; i++)
            {
                Export baseUs = asset.Exports[i];
                var categoryNode = new PointingTreeNode("Export " + (i + 1) + " (" + baseUs.ReferenceData.ObjectName.Value.Value + ")", null);
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
                        var parentNode = new PointingTreeNode(asset.GetImportObjectName(baseUs.ReferenceData.ClassIndex) + " (" + us.Data.Count + ")", us);
                        categoryNode.Nodes.Add(parentNode);

                        for (int j = 0; j < us.Data.Count; j++) InterpretThing(us.Data[j], parentNode);

                        if (us is StringTableExport us2)
                        {
                            var parentNode2 = new PointingTreeNode(us2.Data2.Name + " (" + us2.Data2.Count + ")", us2.Data2);
                            categoryNode.Nodes.Add(parentNode2);
                        }

                        if (us is BlueprintGeneratedClassExport us3)
                        {
                            var parentNode2 = new PointingTreeNode("Export Layout Data", us3, 1);
                            categoryNode.Nodes.Add(parentNode2);
                        }

                        if (us is DataTableExport us4)
                        {
                            var parentNode2 = new PointingTreeNode("Table Info (" + us4.Data2.Table.Count + ")", us4.Data2);
                            categoryNode.Nodes.Add(parentNode2);
                            foreach (DataTableEntry entry in us4.Data2.Table)
                            {
                                string decidedName = entry.Data.Name.Value.Value;
                                if (entry.DuplicateIndex > 0) decidedName += " [" + entry.DuplicateIndex + "]";

                                var structNode = new PointingTreeNode(decidedName + " (" + entry.Data.Value.Count + ")", entry.Data);
                                parentNode2.Nodes.Add(structNode);
                                for (int j = 0; j < entry.Data.Value.Count; j++)
                                {
                                    InterpretThing(entry.Data.Value[j], structNode);
                                }
                            }
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
            switch (me.Type.Value.Value)
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

                    var arrNode = new PointingTreeNode(arr.Name + " (" + arr.Value.Length + ")", arr);
                    ourNode.Nodes.Add(arrNode);
                    for (int j = 0; j < arr.Value.Length; j++)
                    {
                        InterpretThing(arr.Value[j], arrNode);
                    }
                    break;
                case "GameplayTagContainer":
                    var arr2 = (GameplayTagContainerPropertyData)me;

                    var arrNode2 = new PointingTreeNode(arr2.Name + " (" + arr2.Value.Length + ")", arr2);
                    ourNode.Nodes.Add(arrNode2);
                    for (int j = 0; j < arr2.Value.Length; j++)
                    {
                        InterpretThing(arr2.Value[j], arrNode2);
                    }
                    break;
                case "MapProperty":
                    var mapp = (MapPropertyData)me;

                    var mapNode = new PointingTreeNode(mapp.Name + " (" + mapp.Value.Keys.Count + ")", mapp);
                    ourNode.Nodes.Add(mapNode);

                    foreach (DictionaryEntry entry in mapp.Value)
                    {
                        ((PropertyData)entry.Key).Name = new FName("Key");
                        ((PropertyData)entry.Value).Name = new FName("Value");

                        var softEntryNode = new PointingTreeNode(mapp.Name + " (2)", new PointingDictionaryEntry(entry, mapp));
                        mapNode.Nodes.Add(softEntryNode);
                        InterpretThing((PropertyData)entry.Key, softEntryNode);
                        InterpretThing((PropertyData)entry.Value, softEntryNode);
                    }
                    break;
                case "Box":
                    var box = (BoxPropertyData) me;

                    ourNode.Nodes.Add(new PointingTreeNode( box.Name + " (2)", box.Value));
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
                    row.Cells[0].Value = thisPD.Name.Value.Value;
                    row.Cells[1].Value = thisPD.Type.Value.Value;
                    if (thisPD is UnknownPropertyData)
                    {
                        row.Cells[2].Value = "Unknown ser.";
                        row.Cells[3].Value = ((UnknownPropertyData)thisPD).Value.ConvertByteArrayToString();
                    }
                    else
                    {
                        switch (thisPD.Type.Value.Value)
                        {
                            case "BoolProperty":
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = ((BoolPropertyData)thisPD).Value.ToString();
                                break;
                            case "ObjectProperty":
                                var objData = (ObjectPropertyData)thisPD;
                                row.Cells[2].Value = objData.CurrentIndex;
                                if (objData.CurrentIndex != 0)
                                {
                                    row.Cells[3].Value = objData.CurrentIndex > 0 ? "Jump" : objData.Value.ObjectName.ToString();
                                    row.Cells[3].Tag = "CategoryJump";
                                    if (objData.CurrentIndex > 0)
                                    {
                                        DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                        Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                        sty.Font = styFont;
                                        sty.ForeColor = Color.Blue;
                                        row.Cells[3].Style = sty;
                                    }
                                }
                                break;
                            case "SoftObjectProperty":
                                var objData2 = (SoftObjectPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = objData2.Value;
                                break;
                            case "RichCurveKey":
                                var curveData = (RichCurveKeyProperty)thisPD;
                                row.Cells[2].Value = curveData.InterpMode;
                                row.Cells[3].Value = curveData.TangentMode;
                                row.Cells[4].Value = curveData.Time;
                                row.Cells[5].Value = curveData.Value;
                                row.Cells[6].Value = curveData.ArriveTangent;
                                row.Cells[7].Value = curveData.LeaveTangent;
                                break;
                            case "TextProperty":
                                var txtData = (TextPropertyData)thisPD;
                                row.Cells[2].Value = (sbyte)txtData.HistoryType;
                                if (txtData.Value == null)
                                {
                                    row.Cells[3].Value = "null";
                                }
                                else
                                {
                                    int maxValuesToDisplay = 3;
                                    for (int z = 0; z < maxValuesToDisplay; z++)
                                    {
                                        row.Cells[3 + z].Value = txtData.Value.TryGetElement(z);
                                    }
                                    row.Cells[3 + maxValuesToDisplay + 0].Value = txtData.Flag;
                                    if (txtData.Extras != null) row.Cells[3 + maxValuesToDisplay + 1].Value = txtData.Extras.ConvertByteArrayToString();
                                }
                                break;
                            case "NameProperty":
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = ((NamePropertyData)thisPD).Value.Value;
                                row.Cells[4].Value = ((NamePropertyData)thisPD).Value.Number;
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
                                row.Cells[3].Value = enumData.EnumType.Value.Value;
                                row.Cells[4].Value = enumData.Value.Value.Value;
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
                                row.Cells[2].Value = ((StructPropertyData)thisPD).StructType.Value.Value;
                                break;
                            case "ArrayProperty":
                            case "SetProperty":
                                row.Cells[2].Value = ((ArrayPropertyData)thisPD).ArrayType.Value.Value;
                                break;
                            case "GameplayTagContainer":
                            case "MapProperty":
                                break;
                            case "Box":
                                var boxData = (BoxPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = boxData.IsValid;
                                break;
                            case "MulticastDelegateProperty":
                                var mdpData = (MulticastDelegatePropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = mdpData.Value[0];
                                row.Cells[4].Value = mdpData.Value[1];
                                row.Cells[5].Value = mdpData.Value2.Value.Value;
                                break;
                            case "LinearColor":
                                var colorData = (LinearColorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[2].ReadOnly = true;
                                row.Cells[2].Style.BackColor = ARGBtoRGB(LinearHelpers.Convert(colorData.Value));
                                row.Cells[3].Value = colorData.Value.R;
                                row.Cells[4].Value = colorData.Value.G;
                                row.Cells[5].Value = colorData.Value.B;
                                row.Cells[6].Value = colorData.Value.A;
                                break;
                            case "Color":
                                var colorData2 = (ColorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[2].ReadOnly = true;
                                if (colorData2.RawValue != null) row.Cells[2].Style.BackColor = colorData2.Value;
                                row.Cells[3].Value = colorData2.Value.R;
                                row.Cells[4].Value = colorData2.Value.G;
                                row.Cells[5].Value = colorData2.Value.B;
                                row.Cells[6].Value = colorData2.Value.A;
                                break;
                            case "Vector":
                                var vectorData = (VectorPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vectorData.Value[0];
                                row.Cells[4].Value = vectorData.Value[1];
                                row.Cells[5].Value = vectorData.Value[2];
                                break;
                            case "Vector2D":
                                var vector2DData = (Vector2DPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vector2DData.Value[0];
                                row.Cells[4].Value = vector2DData.Value[1];
                                break;
                            case "Vector4":
                                var vector4DData = (Vector4PropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = vector4DData.Value[0];
                                row.Cells[4].Value = vector4DData.Value[1];
                                row.Cells[5].Value = vector4DData.Value[2];
                                row.Cells[6].Value = vector4DData.Value[3];
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
                                row.Cells[3].Value = rotatorData.Value[0];
                                row.Cells[4].Value = rotatorData.Value[1];
                                row.Cells[5].Value = rotatorData.Value[2];
                                break;
                            case "Quat":
                                var quatData = (QuatPropertyData)thisPD;
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = quatData.Value[0];
                                row.Cells[4].Value = quatData.Value[1];
                                row.Cells[5].Value = quatData.Value[2];
                                row.Cells[6].Value = quatData.Value[3];
                                break;
                            case "StrProperty":
                                var strPropData = (StrPropertyData)thisPD;
                                row.Cells[2].Value = strPropData.Encoding.HeaderName;
                                row.Cells[3].Value = Convert.ToString(strPropData.Value);
                                break;
                            default:
                                row.Cells[2].Value = string.Empty;
                                row.Cells[3].Value = Convert.ToString(thisPD.RawValue);
                                break;
                        }
                    }

                    row.Cells[8].Value = thisPD.DuplicationIndex;
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
                    var res = new UnknownPropertyData(new FName(name), asset)
                    {
                        Value = ((string)value1B).ConvertStringToByteArray()
                    };
                    res.Type = new FName(type);
                    return res;
                }

                switch (type)
                {
                    case "TextProperty":
                        TextPropertyData decidedTextData = null;
                        if (original != null && original is TextPropertyData)
                        {
                            decidedTextData = (TextPropertyData)original;
                        }
                        else
                        {
                            decidedTextData = new TextPropertyData(new FName(name), asset);
                        }

                        TextHistoryType histType;
                        int histTypeInt = -100;
                        if (transformB == null || value1B == null || !(value1B is string)) return null;
                        if (transformB is string) int.TryParse((string)transformB, out histTypeInt);
                        if (transformB is sbyte) histTypeInt = (sbyte)transformB;
                        if (transformB is int) histTypeInt = unchecked((sbyte)(int)transformB);
                        histType = (TextHistoryType)histTypeInt;

                        decidedTextData.HistoryType = histType;
                        switch (histType)
                        {
                            case TextHistoryType.Base:
                            case TextHistoryType.None:
                                List<string> strAvailablesL = new List<string>();
                                if (value1B != null && value1B is string) strAvailablesL.Add((string)value1B);
                                if (value2B != null && value2B is string) strAvailablesL.Add((string)value2B);
                                if (value3B != null && value3B is string) strAvailablesL.Add((string)value3B);
                                if (strAvailablesL.Count == 0 && histType == TextHistoryType.Base) return null;

                                decidedTextData.Value = strAvailablesL.ToArray();
                                break;
                            case TextHistoryType.StringTableEntry:
                                if (value1B == null || !(value1B is string)) return null;

                                decidedTextData.Value = new string[] { (string)value1B };
                                break;
                            /*case TextHistoryType.None:
                                decidedTextData.Value = null;
                                break;*/
                            default:
                                throw new FormatException("Unimplemented text history type " + histType);
                        }

                        if (value4B != null && value4B is string) int.TryParse((string)value4B, out decidedTextData.Flag);
                        if (value4B != null && value4B is int) decidedTextData.Flag = (int)value4B;
                        if (value5B != null && value5B is string) decidedTextData.Extras = ((string)value5B).ConvertStringToByteArray();

                        return decidedTextData;
                    case "ObjectProperty":
                        ObjectPropertyData decidedObjData = null;
                        if (original != null && original is ObjectPropertyData)
                        {
                            decidedObjData = (ObjectPropertyData)original;
                        }
                        else
                        {
                            decidedObjData = new ObjectPropertyData(new FName(name), asset);
                        }

                        int objValue = int.MinValue;
                        if (transformB == null) return null;
                        if (transformB is string) int.TryParse((string)transformB, out objValue);
                        if (transformB is int) objValue = (int)transformB;
                        if (objValue == int.MinValue) return null;

                        decidedObjData.CurrentIndex = objValue;
                        decidedObjData.Value = asset.Imports.ElementAtOrDefault(UAPUtils.GetNormalIndex(objValue));
                        return decidedObjData;
                    case "RichCurveKey":
                        RichCurveKeyProperty decidedRCKProperty = null;
                        if (original != null && original is RichCurveKeyProperty)
                        {
                            decidedRCKProperty = (RichCurveKeyProperty)original;
                        }
                        else
                        {
                            decidedRCKProperty = new RichCurveKeyProperty(new FName(name), asset);
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
                        PropertyData newThing = MainSerializer.TypeToClass(new FName(type), new FName(name), asset);
                        if (original != null && original.GetType() == newThing.GetType())
                        {
                            newThing = original;
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
                case TableHandlerMode.NameMap:
                    AddColumns(new string[] { "Name", "Encoding" });

                    IReadOnlyList<FString> headerIndexList = asset.GetNameMapIndexList();
                    for (int num = 0; num < headerIndexList.Count; num++)
                    {
                        dataGridView1.Rows.Add(headerIndexList[num].Value, headerIndexList[num].Encoding.HeaderName);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                    }
                    break;
                case TableHandlerMode.Imports:
                    AddColumns(new string[] { "ClassPackage", "N", "ClassName", "N", "OuterIndex", "ObjectName", "N", "" });

                    for (int num = 0; num < asset.Imports.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.Imports[num].ClassPackage.Value.Value, asset.Imports[num].ClassPackage.Number, asset.Imports[num].ClassName.Value.Value, asset.Imports[num].ClassName.Number, asset.Imports[num].OuterIndex, asset.Imports[num].ObjectName.Value.Value, asset.Imports[num].ObjectName.Number);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(asset.Imports[num].Index);
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    string[] allExportDetailsFields = ExportDetails.GetAllFieldNames();
                    string[] allExportDetailsFields2 = new string[allExportDetailsFields.Length + 1];
                    allExportDetailsFields.CopyTo(allExportDetailsFields2, 0);
                    allExportDetailsFields2[allExportDetailsFields2.Length - 1] = "";
                    AddColumns(allExportDetailsFields2);

                    for (int num = 0; num < asset.Exports.Count; num++)
                    {
                        ExportDetails refer = asset.Exports[num].ReferenceData;
                        string[] newCellsTooltips = new string[allExportDetailsFields.Length];
                        object[] newCells = new object[allExportDetailsFields.Length];
                        for (int num2 = 0; num2 < allExportDetailsFields.Length; num2++)
                        {
                            string cellTooltip = null;

                            object printingVal = refer.GetType().GetField(allExportDetailsFields[num2]).GetValue(refer);
                            if (printingVal is FName parsingName)
                            {
                                string actualName = parsingName?.Value?.Value;
                                if (actualName == null) actualName = "null";
                                newCells[num2] = "FName(\"" + actualName.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\", " + parsingName?.Number + ")";
                            }
                            else
                            {
                                newCells[num2] = printingVal;
                            }

                            if (printingVal is int testInt)
                            {
                                if (testInt < 0) cellTooltip = asset.GetImportObjectName(testInt).Value.Value;
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

                    for (int num = 0; num < asset.SoftPackageReferencesMap.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.SoftPackageReferencesMap[num]);
                    }
                    break;
                case TableHandlerMode.PreloadDependencyMap:
                    AddColumns(new string[] { "Value", "" });

                    for (int num = 0; num < asset.PreloadDependencyMap.Count; num++)
                    {
                        dataGridView1.Rows.Add(new object[] { asset.PreloadDependencyMap[num] });
                    }
                    break;
                case TableHandlerMode.CustomVersionContainer:
                    AddColumns(new string[] { "Key", "Version" });

                    for (int num = 0; num < asset.CustomVersionContainer.Count; num++)
                    {
                        dataGridView1.Rows.Add(new object[] { Convert.ToString(asset.CustomVersionContainer[num].Key), asset.CustomVersionContainer[num].Version });
                    }
                    break;
                case TableHandlerMode.ExportData:
                    if (listView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        AddColumns(new string[] { "Name", "Type", "Variant", "Value", "Value 2", "Value 3", "Value 4", "Value 5", "DupIndex", "WData", "" });

                        bool standardRendering = true;
                        PropertyData[] renderingArr = null;

                        switch (pointerNode.Pointer)
                        {
                            case NormalExport usCategory:
                                switch(pointerNode.Type)
                                {
                                    case 0:
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
                                    case 1:
                                        BlueprintGeneratedClassExport bgcCat = (BlueprintGeneratedClassExport)usCategory;
                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();

                                        {
                                            ObjectPropertyData testProperty = new ObjectPropertyData(new FName("Inherited Class"), asset);
                                            testProperty.SetCurrentIndex(bgcCat.BaseClass);

                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = "Inherited Class";
                                            row.Cells[1].Value = testProperty.CurrentIndex;
                                            row.Cells[2].Value = testProperty.CurrentIndex >= 0 ? "" : testProperty.Value.ObjectName.ToString();
                                            rows.Add(row);
                                        }

                                        // Header 1
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = "---";
                                            row.Cells[1].Value = "GLOBAL VARIABLE INDEX?";
                                            row.Cells[2].Value = "---";
                                            rows.Add(row);
                                        }

                                        for (int i = 0; i < bgcCat.IndexData.Count; i++)
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = bgcCat.IndexData[i];
                                            rows.Add(row);
                                        }

                                        // Header 2
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = "---";
                                            row.Cells[1].Value = "FUNCTION DATA";
                                            row.Cells[2].Value = "---";
                                            rows.Add(row);
                                        }

                                        for (int i = 0; i < bgcCat.FunctionData.Count; i++)
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = bgcCat.FunctionData[i].Name;
                                            row.Cells[2].Value = bgcCat.FunctionData[i].Category;
                                            if (bgcCat.FunctionData[i].Category != 0)
                                            {
                                                row.Cells[3].Value = "Jump";
                                                row.Cells[3].Tag = "CategoryJump";
                                                DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                                Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                                sty.Font = styFont;
                                                sty.ForeColor = Color.Blue;
                                                row.Cells[3].Style = sty;
                                            }
                                            row.Cells[4].Value = bgcCat.FunctionData[i].Flags;
                                            rows.Add(row);
                                        }

                                        // Header 3
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = "---";
                                            row.Cells[1].Value = "FOOTER DATA";
                                            row.Cells[2].Value = "---";
                                            rows.Add(row);
                                        }

                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = BitConverter.ToString(BitConverter.GetBytes(bgcCat.FooterSeparator));
                                            row.Cells[1].Value = bgcCat.FooterObject + " (" + asset.GetImportObjectName(bgcCat.FooterObject) + ")";
                                            row.Cells[2].Value = bgcCat.FooterEngine;
                                            rows.Add(row);
                                        }

                                        dataGridView1.Rows.AddRange(rows.ToArray());
                                        dataGridView1.ReadOnly = true;
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
                                    List<StructPropertyData> listOfStructs = new List<StructPropertyData>();
                                    foreach (DataTableEntry entry in dtUs.Table)
                                    {
                                        listOfStructs.Add(entry.Data);
                                    }
                                    renderingArr = listOfStructs.ToArray();
                                    break;
                                }
                            case MapPropertyData usMap:
                                {
                                    if (usMap.Value.Count > 0)
                                    {
                                        DictionaryEntry firstEntry = usMap.Value.Cast<DictionaryEntry>().ElementAt(0);
                                        FName mapKeyType = ((PropertyData)firstEntry.Key).Type;
                                        FName mapValueType = ((PropertyData)firstEntry.Value).Type;

                                        List<DataGridViewRow> rows = new List<DataGridViewRow>();
                                        for (int i = 0; i < usMap.Value.Count; i++)
                                        {
                                            DataGridViewRow row = new DataGridViewRow();
                                            row.CreateCells(dataGridView1);
                                            row.Cells[0].Value = usMap.Name.Value.Value;
                                            row.Cells[1].Value = "MapEntry";
                                            row.Cells[2].Value = string.Empty;

                                            row.Cells[3].Value = "Jump";
                                            row.Cells[3].Tag = "ChildJump";

                                            DataGridViewCellStyle sty = new DataGridViewCellStyle();
                                            Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                                            sty.Font = styFont;
                                            sty.ForeColor = Color.Blue;
                                            row.Cells[3].Style = sty;

                                            row.Cells[4].Value = mapKeyType.Value.Value;
                                            row.Cells[5].Value = mapValueType.Value.Value;
                                            row.HeaderCell.Value = Convert.ToString(i);
                                            row.Tag = usMap.Value.Cast<DictionaryEntry>().ElementAt(i);
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
                                var ourKey = (PropertyData)usDictEntry.Entry.Key;
                                var ourValue = (PropertyData)usDictEntry.Entry.Value;
                                if (ourKey != null) ourKey.Name = new FName("Key");
                                if (ourValue != null) ourValue.Name = new FName("Value");
                                renderingArr = new PropertyData[2] { ourKey, ourValue };
                                break;
                            case PropertyData[] usRealArr:
                                renderingArr = usRealArr;
                                dataGridView1.AllowUserToAddRows = false;
                                break;
                            case byte[] bytes:
                                dataGridView1.Visible = false;
                                byteView1.SetBytes(new byte[] { });
                                byteView1.SetBytes(bytes);
                                byteView1.Visible = true;
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
                case TableHandlerMode.NameMap:
                    asset.ClearNameIndexList();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string ourValue = (string)row.Cells[0].Value;
                        string encoding = (string)row.Cells[1].Value;
                        if (string.IsNullOrWhiteSpace(encoding)) encoding = "ascii";
                        if (!string.IsNullOrWhiteSpace(ourValue)) asset.AddNameReference(new FString(ourValue, encoding.Equals("utf-16") ? Encoding.Unicode : Encoding.ASCII));
                    }
                    break;
                case TableHandlerMode.Imports:
                    asset.Imports = new List<Import>();
                    int nextIndex = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        object val1 = row.Cells[0].Value;
                        object val1N = row.Cells[1].Value;
                        object val2 = row.Cells[2].Value;
                        object val2N = row.Cells[3].Value;
                        object val3 = row.Cells[4].Value;
                        object val4 = row.Cells[5].Value;
                        object val4N = row.Cells[6].Value;
                        if (val1 == null || val2 == null || val3 == null || val4 == null || val1N == null || val2N == null || val4N == null) continue;
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

                        int realVal1N;
                        if (val1N is string)
                        {
                            if (!int.TryParse((string)val1N, out realVal1N)) continue;
                        }
                        else
                        {
                            realVal1N = Convert.ToInt32(val1N);
                        }

                        int realVal2N;
                        if (val2N is string)
                        {
                            if (!int.TryParse((string)val2N, out realVal2N)) continue;
                        }
                        else
                        {
                            realVal2N = Convert.ToInt32(val2N);
                        }

                        int realVal4N;
                        if (val4N is string)
                        {
                            if (!int.TryParse((string)val4N, out realVal4N)) continue;
                        }
                        else
                        {
                            realVal4N = Convert.ToInt32(val4N);
                        }

                        string realVal1 = (string)val1;
                        string realVal2 = (string)val2;
                        string realVal4 = (string)val4;
                        if (string.IsNullOrWhiteSpace(realVal1) || string.IsNullOrWhiteSpace(realVal2) || string.IsNullOrWhiteSpace(realVal4)) continue;

                        asset.AddNameReference(new FString(realVal1));
                        asset.AddNameReference(new FString(realVal2));
                        asset.AddNameReference(new FString(realVal4));
                        Import newLink = new Import(new FName(realVal1, realVal1N), new FName(realVal2, realVal2N), realVal3, new FName(realVal4, realVal4N), --nextIndex);
                        asset.Imports.Add(newLink);
                    }
                    break;
                case TableHandlerMode.ExportInformation:
                    FieldInfo[] allExportDetailsFields = typeof(ExportDetails).GetFields();
                    ExportDetailsParseType[] parsingTypes = new ExportDetailsParseType[]
                    {
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.Int,
                        ExportDetailsParseType.FName,
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
                        ExportDetails newRef = new ExportDetails();
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
                                case ExportDetailsParseType.FName:
                                    settingVal = new FName();
                                    if (currentVal is string rawFName) // FName("blah", 0)
                                    {
                                        if (rawFName.Length < 8) break;
                                        string[] fNameData = rawFName.Substring(6, rawFName.Length - 7).Split(','); // {"\"blah\"", 0}
                                        if (fNameData.Length != 2) break;
                                        int.TryParse(fNameData[1], out int x);
                                        string newStrVal = fNameData[0].Substring(1, fNameData[0].Length - 2).Replace(@"\""", @"""").Replace(@"\\", @"\");
                                        settingVal = new FName(newStrVal, x);
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
                            allExportDetailsFields[i].SetValue(newRef, settingVal);
                        }

                        if (isInvalidRow) continue;

                        if (asset.Exports.Count > rowNum)
                        {
                            asset.Exports[rowNum].ReferenceData = newRef;
                        }
                        else
                        {
                            // If we add a new category, we'll make a new NormalExport (None-terminated UProperty list). If you want to make some other kind of export, you'll need to do it manually with UAssetAPI
                            var newCat = new NormalExport(newRef, asset, new byte[4]);
                            newCat.Data = new List<PropertyData>();
                            asset.Exports.Add(newCat);
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
                    asset.SoftPackageReferencesMap = new List<string>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string strVal = (string)row.Cells[0].Value;
                        if (!string.IsNullOrEmpty(strVal)) asset.SoftPackageReferencesMap.Add(strVal);
                    }
                    break;
                case TableHandlerMode.PreloadDependencyMap:
                    asset.PreloadDependencyMap = new List<int>();
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

                        if (asset.PreloadDependencyMap.Count > rowN)
                        {
                            asset.PreloadDependencyMap[rowN] = intVal;
                        }
                        else
                        {
                            asset.PreloadDependencyMap.Insert(rowN, intVal);
                        }

                        rowN++;
                    }
                    break;
                case TableHandlerMode.CustomVersionContainer:
                    asset.CustomVersionContainer = new List<CustomVersion>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!Guid.TryParse((string)row.Cells[0].Value, out Guid customVersionKey)) continue;

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
                            DictionaryEntry firstEntry = usMap.Value.Cast<DictionaryEntry>().ElementAt(0);
                            FName mapKeyType = ((PropertyData)firstEntry.Key).Type;
                            FName mapValueType = ((PropertyData)firstEntry.Value).Type;

                            OrderedDictionary newData = new OrderedDictionary();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                DataGridViewRow row = dataGridView1.Rows[i];
                                if (row.Cells.Count <= 1 || row.Cells[1].Value == null || !row.Cells[1].Value.Equals("MapEntry")) continue;

                                if (row.Tag is DictionaryEntry dictBit)
                                {
                                    newData.Add(dictBit.Key, dictBit.Value);
                                }
                                else
                                {
                                    newData.Add(MainSerializer.TypeToClass(mapKeyType, usMap.Name, asset), MainSerializer.TypeToClass(mapValueType, usMap.Name, asset));
                                }
                            }
                            usMap.Value = newData;

                            pointerNode.Text = usMap.Name + " (" + usMap.Value.Count + ")";
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
                        else if (pointerNode.Pointer is BlueprintGeneratedClassExport usBGCCat)
                        {
                            // No writing here
                        }
                        else if (pointerNode.Pointer is NormalExport usCat)
                        {
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
                            usCat.Data = newData;
                            pointerNode.Text = asset.GetImportObjectName(usCat.ReferenceData.ClassIndex) + " (" + usCat.Data.Count + ")";
                        }
                        else if (pointerNode.Pointer is DataTable dtUs)
                        {
                            List<DataTableEntry> newData = new List<DataTableEntry>();
                            var numTimesNameUses = new Dictionary<FName, int>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, dtUs.Table.ElementAtOrDefault(i).Data);
                                if (val == null || !(val is StructPropertyData)) continue;
                                if (numTimesNameUses.ContainsKey(val.Name))
                                {
                                    numTimesNameUses[val.Name]++;
                                }
                                else
                                {
                                    numTimesNameUses.Add(val.Name, 0);
                                }
                                newData.Add(new DataTableEntry((StructPropertyData)val, numTimesNameUses[val.Name]));
                            }
                            dtUs.Table = newData;
                            pointerNode.Text = "Table Info (" + dtUs.Table.Count + ")";
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
                            pointerNode.Text = usArr.Name + " (" + usArr.Value.Length + ")";
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
                            pointerNode.Text = usArr2.Name + " (" + usArr2.Value.Length + ")";
                        }
                        else if (pointerNode.Pointer is PointingDictionaryEntry usDictEntry)
                        {
                            ((PointingDictionaryEntry)pointerNode.Pointer).Entry.Key = RowToPD(0, (PropertyData)usDictEntry.Entry.Key);
                            ((PointingDictionaryEntry)pointerNode.Pointer).Entry.Value = RowToPD(1, (PropertyData)usDictEntry.Entry.Value);
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
        }
    }
}
