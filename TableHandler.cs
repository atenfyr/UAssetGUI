using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.StructureSerializers;

namespace UAssetGUI
{
    public enum TableHandlerMode
    {
        None,
        HeaderList,
        LinkedSectors,
        CategoryInformation,
        CategoryStrings,
        CategoryInts,
        CategoryData
    }

    public class PointingTreeNode : TreeNode
    {
        public object Pointer;

        public PointingTreeNode(string label, object pointer)
        {
            Pointer = pointer;
            this.Text = label;
            this.Tag = Guid.NewGuid();
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
        public AssetWriter asset;
        public TreeView listView1;
        public DataGridView dataGridView1;

        private Dictionary<Guid, bool> treeState;
        public bool readyToSave = true;

        public void FillOutTree()
        {
            listView1.BeginUpdate();
            listView1.Nodes.Clear();
            listView1.BackColor = Color.White;
            listView1.Nodes.Add(new PointingTreeNode("Header List", null));
            listView1.Nodes.Add(new PointingTreeNode("Linked Sectors", null));
            listView1.Nodes.Add(new PointingTreeNode("Section Information", null));
            listView1.Nodes.Add(new PointingTreeNode("Category Data", null));

            TreeNode superTopNode = listView1.Nodes[listView1.Nodes.Count - 1];
            for (int i = 0; i < asset.data.categories.Count; i++)
            {
                Category baseUs = asset.data.categories[i];
                var categoryNode = new PointingTreeNode("Category " + (i + 1), null);
                superTopNode.Nodes.Add(categoryNode);
                if (baseUs is NormalCategory us)
                {
                    {
                        var parentNode = new PointingTreeNode(asset.data.GetHeaderReference(asset.data.GetLinkReference(us.ReferenceData.connection)) + " (" + us.Data.Count + ")", baseUs);
                        categoryNode.Nodes.Add(parentNode);

                        for (int j = 0; j < us.Data.Count; j++)
                        {
                            InterpretThing(us.Data[j], parentNode);
                        }
                    }

                    {
                        if (us.Extras.Length > 0 && us.Extras.Length != 4)
                        {
                            var parentNode = new PointingTreeNode("Extra Data (" + us.Extras.Length + " B)", us.Extras);
                            categoryNode.Nodes.Add(parentNode);
                        }
                    }
                }
            }
            listView1.EndUpdate();
        }

        private void InterpretThing(PropertyData me, PointingTreeNode ourNode)
        {
            if (me == null) return;
            switch (me.Type)
            {
                case "StructProperty":
                    var struc = (StructPropertyData)me;

                    string decidedName = struc.Name;
                    if (ourNode.Pointer is PropertyData && ((PropertyData)ourNode.Pointer).Name.Equals(decidedName)) decidedName = struc.StructType;

                    var structNode = new PointingTreeNode(decidedName + " (" + struc.Value.Count + ")", struc);
                    ourNode.Nodes.Add(structNode);
                    for (int j = 0; j < struc.Value.Count; j++)
                    {
                        InterpretThing(struc.Value[j], structNode);
                    }
                    break;
                case "ArrayProperty":
                    var arr = (ArrayPropertyData)me;

                    var arrNode = new PointingTreeNode(arr.Name + " (" + arr.Value.Length + ")", arr);
                    ourNode.Nodes.Add(arrNode);
                    for (int j = 0; j < arr.Value.Length; j++)
                    {
                        InterpretThing(arr.Value[j], arrNode);
                    }
                    break;
                case "MapProperty":
                    var mapp = (MapPropertyData)me;

                    var mapNode = new PointingTreeNode(mapp.Name + " (" + mapp.Value.Keys.Count + ")", mapp);
                    ourNode.Nodes.Add(mapNode);

                    foreach (DictionaryEntry entry in mapp.Value)
                    {
                        var softEntryNode = new PointingTreeNode(mapp.Name + " (2)", new PointingDictionaryEntry(entry, mapp));
                        mapNode.Nodes.Add(softEntryNode);
                        InterpretThing((PropertyData)entry.Key, softEntryNode);
                        InterpretThing((PropertyData)entry.Value, softEntryNode);
                    }
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

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                row.Cells[0].Value = thisPD.Name;
                row.Cells[1].Value = thisPD.Type;
                switch (thisPD.Type)
                {
                    case "BoolProperty":
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = ((BoolPropertyData)thisPD).Value ? 1 : 0;
                        break;
                    case "ObjectProperty":
                        var objData = (ObjectPropertyData)thisPD;
                        row.Cells[2].Value = objData.LinkValue;
                        row.Cells[3].Value = objData.LinkValue > 0 ? "Jump" : asset.data.GetHeaderReference((int)objData.Value.Property);
                        if (objData.LinkValue > 0)
                        {
                            DataGridViewCellStyle sty = new DataGridViewCellStyle();
                            Font styFont = new Font(dataGridView1.Font.Name, dataGridView1.Font.Size, FontStyle.Underline);
                            sty.Font = styFont;
                            sty.ForeColor = Color.Blue;
                            row.Cells[3].Style = sty;
                        }
                        break;
                    case "SoftObjectProperty":
                        var objData2 = (SoftObjectPropertyData)thisPD;
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = objData2.Value;
                        row.Cells[4].Value = objData2.Value2;
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
                            for (int z = 0; z < 4; z++)
                            {
                                row.Cells[3 + z].Value = txtData.Value.TryGetElement(z);
                            }
                        }
                        break;
                    case "NameProperty":
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = ((NamePropertyData)thisPD).Value;
                        row.Cells[4].Value = ((NamePropertyData)thisPD).Value2;
                        break;
                    case "EnumProperty":
                        var enumData = (EnumPropertyData)thisPD;
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = enumData.GetEnumBase();
                        row.Cells[3].Value = enumData.GetEnumFull();
                        break;
                    case "StructProperty":
                        row.Cells[2].Value = ((StructPropertyData)thisPD).StructType;
                        break;
                    case "ArrayProperty":
                        row.Cells[2].Value = ((ArrayPropertyData)thisPD).ArrayType;
                        break;
                    case "LinearColor":
                        var colorData = (LinearColorPropertyData)thisPD;
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = colorData.Value.R;
                        row.Cells[4].Value = colorData.Value.G;
                        row.Cells[5].Value = colorData.Value.B;
                        row.Cells[6].Value = colorData.Value.A;
                        break;
                    case "Vector":
                        var vectorData = (VectorPropertyData)thisPD;
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = vectorData.Value[0];
                        row.Cells[4].Value = vectorData.Value[1];
                        row.Cells[5].Value = vectorData.Value[2];
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
                    default:
                        row.Cells[2].Value = string.Empty;
                        row.Cells[3].Value = Convert.ToString(thisPD.RawValue);
                        break;
                }
                
                row.HeaderCell.Value = Convert.ToString(i);
                rows.Add(row);
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

                if (nameB == null || typeB == null) return null;
                if (!(nameB is string) || !(typeB is string)) return null;

                string name = (string)nameB;
                string type = (string)typeB;
                if (name.Equals(string.Empty) || type.Equals(string.Empty)) return null;

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
                            decidedTextData = new TextPropertyData(name, asset.data);
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
                                List<string> strAvailablesL = new List<string>();
                                if (value1B != null && value1B is string) strAvailablesL.Add((string)value1B);
                                if (value2B != null && value2B is string) strAvailablesL.Add((string)value2B);
                                if (value3B != null && value3B is string) strAvailablesL.Add((string)value3B);
                                if (value4B != null && value4B is string) strAvailablesL.Add((string)value4B);
                                if (strAvailablesL.Count == 0) return null;

                                decidedTextData.Value = strAvailablesL.ToArray();
                                break;
                            case TextHistoryType.StringTableEntry:
                                if (value1B == null || !(value1B is string)) return null;

                                decidedTextData.Value = new string[] { (string)value1B };
                                break;
                            case TextHistoryType.None:
                                decidedTextData.Value = null;
                                break;
                            default:
                                throw new FormatException("Unimplemented text history type " + histType.ToString());
                        }

                        return decidedTextData;
                    case "ObjectProperty":
                        ObjectPropertyData decidedObjData = null;
                        if (original != null && original is ObjectPropertyData)
                        {
                            decidedObjData = (ObjectPropertyData)original;
                        }
                        else
                        {
                            decidedObjData = new ObjectPropertyData(name, asset.data);
                        }

                        int objValue = int.MinValue;
                        if (transformB == null) return null;
                        if (transformB is string) int.TryParse((string)transformB, out objValue);
                        if (transformB is int) objValue = (int)transformB;
                        if (objValue == int.MinValue) return null;

                        decidedObjData.LinkValue = objValue;
                        decidedObjData.Value = asset.data.links.ElementAtOrDefault(UAssetAPI.Utils.GetNormalIndex(objValue));
                        return decidedObjData;
                    case "SoftObjectProperty":
                        SoftObjectPropertyData decidedObjData2 = null;
                        if (original != null && original is SoftObjectPropertyData)
                        {
                            decidedObjData2 = (SoftObjectPropertyData)original;
                        }
                        else
                        {
                            decidedObjData2 = new SoftObjectPropertyData(name, asset.data);
                        }

                        if (value1B == null || value2B == null) return null;
                        if (!(value1B is string)) return null;

                        string objValue2 = (string)value1B;
                        long objValue3 = 0;
                        if (value2B is string) long.TryParse((string)value2B, out objValue3);
                        if (value2B is int || value2B is long) objValue3 = (long)value2B;

                        decidedObjData2.Value = objValue2;
                        decidedObjData2.Value2 = objValue3;
                        return decidedObjData2;
                    case "NameProperty":
                        NamePropertyData decidedNameProp = null;
                        if (original != null && original is NamePropertyData)
                        {
                            decidedNameProp = (NamePropertyData)original;
                        }
                        else
                        {
                            decidedNameProp = new NamePropertyData(name, asset.data);
                        }

                        if (value1B == null || value2B == null) return null;
                        if (!(value1B is string)) return null;

                        string nameValue2 = (string)value1B;
                        int nameValue3 = 0;
                        if (value2B is string) int.TryParse((string)value2B, out nameValue3);
                        if (value2B is int) nameValue3 = (int)value2B;

                        decidedNameProp.Value = nameValue2;
                        decidedNameProp.Value2 = nameValue3;
                        return decidedNameProp;
                    case "LinearColor": // RGBA
                        LinearColorPropertyData decidedColorData = null;
                        if (original != null && original is LinearColorPropertyData)
                        {
                            decidedColorData = (LinearColorPropertyData)original;
                        }
                        else
                        {
                            decidedColorData = new LinearColorPropertyData(name, asset.data);
                        }

                        int[] colorData = new int[4] { -1, -1, -1, -1 };
                        if (value1B == null || value2B == null || value3B == null || value4B == null) return null;

                        if (value1B is string) int.TryParse((string)value1B, out colorData[0]);
                        if (value1B is int) colorData[0] = (int)value1B;
                        if (value2B is string) int.TryParse((string)value2B, out colorData[1]);
                        if (value2B is int) colorData[1] = (int)value2B;
                        if (value3B is string) int.TryParse((string)value3B, out colorData[2]);
                        if (value3B is int) colorData[2] = (int)value3B;
                        if (value4B is string) int.TryParse((string)value4B, out colorData[3]);
                        if (value4B is int) colorData[3] = (int)value4B;

                        if (colorData[0] < 0 || colorData[1] < 0 || colorData[2] < 0 || colorData[3] < 0) return null;
                        if (colorData[0] > 255 || colorData[1] > 255 || colorData[2] > 255 || colorData[3] > 255) return null;

                        decidedColorData.Value = Color.FromArgb(colorData[3], colorData[0], colorData[1], colorData[2]);
                        return decidedColorData;
                    default:
                        PropertyData newThing = MainSerializer.TypeToClass(type, name, asset.data);
                        if (original != null && original.GetType().Equals(newThing.GetType()))
                        {
                            newThing = original;
                        }

                        string[] existingStrings = new string[4];
                        if (value1B != null) existingStrings[0] = Convert.ToString(value1B);
                        if (value2B != null) existingStrings[1] = Convert.ToString(value2B);
                        if (value3B != null) existingStrings[2] = Convert.ToString(value3B);
                        if (value4B != null) existingStrings[3] = Convert.ToString(value4B);

                        newThing.FromString(existingStrings);
                        return newThing;
                }
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public void SaveTreeState()
        {
            Dictionary<Guid, bool> nodeStates = new Dictionary<Guid, bool>();
            foreach (TreeNode node in Collect(listView1.Nodes))
            {
                nodeStates.Add((Guid)node.Tag, node.IsExpanded);
            }
            treeState = nodeStates;
        }

        public void ExpandAll()
        {
            Dictionary<Guid, bool> nodeStates = new Dictionary<Guid, bool>();
            foreach (TreeNode node in Collect(listView1.Nodes))
            {
                nodeStates.Add((Guid)node.Tag, true);
            }
            treeState = nodeStates;
            RestoreTreeState();
        }

        public void CollapseAll()
        {
            Dictionary<Guid, bool> nodeStates = new Dictionary<Guid, bool>();
            foreach (TreeNode node in Collect(listView1.Nodes))
            {
                nodeStates.Add((Guid)node.Tag, false);
            }
            treeState = nodeStates;
            RestoreTreeState();
        }

        private void RestoreTreeState()
        {
            foreach (TreeNode node in Collect(listView1.Nodes))
            {
                if (treeState.Keys.Count != 0 && treeState[(Guid)node.Tag])
                    node.Expand();
                else
                    node.Collapse();
            }
        }

        IEnumerable<TreeNode> Collect(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;
                foreach (TreeNode child in Collect(node.Nodes))
                    yield return child;
            }
        }

        private void AddColumns(string[] ourColumns)
        {
            for (int i = 0; i < ourColumns.Length; i++)
            {
                DataGridViewColumn dgc = new DataGridViewColumn();
                dgc = new DataGridViewTextBoxColumn
                {
                    HeaderText = ourColumns[i]
                };

                dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (i >= (ourColumns.Length - 1))
                {
                    dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                dataGridView1.Columns.Add(dgc);
            }
        }

        private void ClearScreen()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.BackgroundColor = Color.FromArgb(211, 211, 211);
        }

        public void Load() // Updates the table with selected asset data
        {
            SaveTreeState();

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

            dataGridView1.BackgroundColor = Color.FromArgb(240, 240, 240);
            readyToSave = false;

            switch (mode)
            {
                case TableHandlerMode.HeaderList:
                    AddColumns(new string[] { "String" });

                    for (int num = 0; num < asset.data.headerIndexList.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.data.headerIndexList[num]);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num);
                    }
                    break;
                case TableHandlerMode.LinkedSectors:
                    AddColumns(new string[] { "Base", "Class", "Link", "Connection", "" });

                    for (int num = 0; num < asset.data.links.Count; num++)
                    {
                        dataGridView1.Rows.Add(asset.data.GetHeaderReference((int)asset.data.links[num].Base), asset.data.GetHeaderReference((int)asset.data.links[num].Class), asset.data.links[num].Linkage, asset.data.GetHeaderReference((int)asset.data.links[num].Property));
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(asset.data.links[num].Index);
                    }
                    break;
                case TableHandlerMode.CategoryInformation:
                    AddColumns(new string[] { "Connection", "Connect", "Category", "Link", "Type Index", "Type", "Length", "Offset", "" });

                    for (int num = 0; num < asset.data.categories.Count; num++)
                    {
                        CategoryReference refer = asset.data.categories[num].ReferenceData;
                        dataGridView1.Rows.Add(asset.data.GetHeaderReference(asset.data.GetLinkReference(refer.connection)), asset.data.GetHeaderReference(asset.data.GetLinkReference(refer.connect)), refer.category, refer.link, refer.typeIndex, refer.type, refer.lengthV, refer.startV);
                        dataGridView1.Rows[num].HeaderCell.Value = Convert.ToString(num + 1);
                    }
                    break;
                case TableHandlerMode.CategoryData:
                    if (listView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        AddColumns(new string[] { "Name", "Type", "Transform", "Value", "Value 2", "Value 3", "Value 4", "Other", "" });

                        dataGridView1.AllowUserToAddRows = true;

                        bool standardRendering = true;
                        PropertyData[] renderingArr = null;

                        if (pointerNode.Pointer is Category usCategory)
                        {
                            if (usCategory is NormalCategory normalUs)
                            {
                                renderingArr = normalUs.Data.ToArray();
                            }
                        }
                        else if (pointerNode.Pointer is StructPropertyData usStruct)
                        {
                            renderingArr = usStruct.Value.ToArray();
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData usArr)
                        {
                            renderingArr = usArr.Value;
                        }
                        else if (pointerNode.Pointer is PointingDictionaryEntry usDictEntry)
                        {
                            dataGridView1.AllowUserToAddRows = false;
                            var ourKey = (PropertyData)usDictEntry.Entry.Key;
                            var ourValue = (PropertyData)usDictEntry.Entry.Value;
                            ourKey.Name = "Key";
                            ourValue.Name = "Value";
                            renderingArr = new PropertyData[2] { ourKey, ourValue };
                        }
                        else if (pointerNode.Pointer is byte[])
                        {
                            dataGridView1.Visible = false;
                            byteView1.SetBytes(new byte[] { });
                            byteView1.SetBytes((byte[])pointerNode.Pointer);
                            byteView1.Visible = true;
                            origForm.ForceResize();
                            standardRendering = false;
                        }

                        if (standardRendering)
                        {
                            if (renderingArr != null && renderingArr.Length > 0)
                            {
                                AddRowsForArray(renderingArr);
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
            RestoreTreeState();
        }

        public void Save(bool forceNewLoad) // Reads from the table and updates the asset data as needed
        {
            if (!readyToSave) return;
            switch (mode)
            {
                case TableHandlerMode.HeaderList:
                    asset.data.headerIndexList = new List<string>();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        asset.data.headerIndexList.Add((string)(row.Cells[0].Value));
                    }
                    break;
                case TableHandlerMode.LinkedSectors:
                    asset.data.links = new List<Link>();
                    int nextIndex = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        object val1 = row.Cells[0].Value;
                        object val2 = row.Cells[1].Value;
                        object val3 = row.Cells[2].Value;
                        object val4 = row.Cells[3].Value;
                        if (val1 == null || val2 == null || val3 == null || val4 == null) return;
                        if (!(val1 is string) || !(val2 is string) || !(val4 is string)) return;
                        if (!(val3 is string) && !(val3 is int)) return;

                        int realVal3;
                        if (val3 is string)
                        {
                            bool result = int.TryParse((string)val3, out realVal3);
                            if (!result) return;
                        }
                        else
                        {
                            realVal3 = (int)val3;
                        }

                        string realVal1 = (string)val1;
                        string realVal2 = (string)val2;
                        string realVal4 = (string)val4;
                        if (string.IsNullOrEmpty(realVal1) || string.IsNullOrEmpty(realVal2) || string.IsNullOrEmpty(realVal4)) return;

                        asset.data.AddHeaderReference(realVal1);
                        asset.data.AddHeaderReference(realVal2);
                        asset.data.AddHeaderReference(realVal4);

                        Link newLink = new Link((ulong)asset.data.SearchHeaderReference(realVal1), (ulong)asset.data.SearchHeaderReference(realVal2), realVal3, (ulong)asset.data.SearchHeaderReference(realVal4), --nextIndex);
                        asset.data.links.Add(newLink);
                    }
                    break;
                case TableHandlerMode.CategoryInformation:
                    int rowNum = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells.Count != 4) return;

                        object[] vals = new object[8];
                        for (int i = 0; i < vals.Length; i++)
                        {
                            vals[i] = row.Cells[i].Value;
                            if (vals[i] == null) return;
                        }
                        if (!(vals[0] is string) || !(vals[1] is string)) return;

                        string val1 = (string)vals[0];
                        string val2 = (string)vals[1];

                        int[] restOfVals = new int[8];
                        for (int i = 2; i < vals.Length; i++)
                        {
                            if (vals[i] is string)
                            {
                                bool result = int.TryParse((string)vals[i], out int x);
                                if (!result) return;
                                restOfVals[i] = x;
                            }
                            else
                            {
                                restOfVals[i] = (int)vals[i];
                            }
                        }

                        asset.data.AddHeaderReference(val1);
                        asset.data.AddHeaderReference(val2);
                        if (asset.data.categories.Count > rowNum)
                        {
                            asset.data.categories[rowNum].ReferenceData.connection = asset.data.SearchHeaderReference(val1);
                            asset.data.categories[rowNum].ReferenceData.connect = asset.data.SearchHeaderReference(val2);
                            asset.data.categories[rowNum].ReferenceData.category = restOfVals[2];
                            asset.data.categories[rowNum].ReferenceData.link = restOfVals[3];
                            asset.data.categories[rowNum].ReferenceData.typeIndex = restOfVals[4];
                            asset.data.categories[rowNum].ReferenceData.type = (ushort)restOfVals[5];
                            asset.data.categories[rowNum].ReferenceData.lengthV = restOfVals[6];
                            asset.data.categories[rowNum].ReferenceData.startV = restOfVals[7];
                        }
                        else
                        {
                            CategoryReference refer = new CategoryReference(asset.data.SearchHeaderReference(val1), asset.data.SearchHeaderReference(val2), restOfVals[2], restOfVals[3], restOfVals[4], (ushort)restOfVals[5], restOfVals[6], restOfVals[7]);
                            asset.data.categories.Add(new Category(refer, asset.data, new byte[0]));
                        }
                        rowNum++;
                    }
                    break;
                case TableHandlerMode.CategoryData:
                    if (listView1.SelectedNode is PointingTreeNode pointerNode)
                    {
                        if (pointerNode.Pointer is NormalCategory usCat)
                        {
                            List<PropertyData> newData = new List<PropertyData>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, usCat.Data.ElementAtOrDefault(i));
                                if (val != null) newData.Add(val);
                            }
                            usCat.Data = newData;
                        }
                        else if (pointerNode.Pointer is StructPropertyData usStruct)
                        {
                            List<PropertyData> newData = new List<PropertyData>();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, usStruct.Value.ElementAtOrDefault(i));
                                if (val != null) newData.Add(val);
                            }
                            usStruct.Value = newData;
                        }
                        else if (pointerNode.Pointer is ArrayPropertyData usArr)
                        {
                            List<PropertyData> newData = new List<PropertyData>();
                            List<PropertyData> origArr = usArr.Value.ToList();
                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                PropertyData val = RowToPD(i, origArr.ElementAtOrDefault(i));
                                if (val != null) newData.Add(val);
                            }
                            usArr.Value = newData.ToArray();
                        }
                        else if (pointerNode.Pointer is PointingDictionaryEntry usDictEntry)
                        {
                            ((PointingDictionaryEntry)pointerNode.Pointer).Entry.Key = RowToPD(0, (PropertyData)usDictEntry.Entry.Key);
                            ((PointingDictionaryEntry)pointerNode.Pointer).Entry.Value = RowToPD(1, (PropertyData)usDictEntry.Entry.Value);
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

        public TableHandler(DataGridView dataGridView1, AssetWriter asset, TreeView listView1)
        {
            this.asset = asset;
            this.dataGridView1 = dataGridView1;
            this.listView1 = listView1;
        }
    }
}
