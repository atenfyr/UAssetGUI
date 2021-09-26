using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UAssetAPI.PropertyTypes;

namespace UAssetGUI
{
    public static class UAGUtils
    {
        public static T TryGetElement<T>(this T[] array, int index)
        {
            if (array != null && index < array.Length)
            {
                return array[index];
            }
            return default(T);
        }

        public static object ArbitraryTryParse(this string input, Type type)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter != null)
                {
                    return converter.ConvertFromString(input);
                }
            }
            catch (NotSupportedException) { }
            return null;
        }

        public static void UpdateObjectPropertyValues(DataGridViewRow row, DataGridView dgv, ObjectPropertyData objData)
        {
            if (dgv == null || row == null || objData == null) return;

            bool underlineStyle = false;
            if (objData.Value.IsImport() && objData.Value == null)
            {
                row.Cells[3].Value = string.Empty;
            }
            else
            {
                row.Cells[3].Value = objData.Value.IsExport() ? "Jump" : (objData.Value.IsImport() ? objData.ToImport().ObjectName?.ToString() : string.Empty);
                row.Cells[3].Tag = "CategoryJump";
                if (objData.Value.IsExport()) underlineStyle = true;
            }

            DataGridViewCellStyle sty = new DataGridViewCellStyle();
            if (underlineStyle)
            {
                Font styFont = new Font(dgv.Font.Name, dgv.Font.Size, FontStyle.Underline);
                sty.Font = styFont;
                sty.ForeColor = Color.Blue;
            }
            row.Cells[3].Style = sty;
        }

        public static string ConvertByteArrayToString(this byte[] val)
        {
            if (val == null) return "";
            return BitConverter.ToString(val).Replace("-", " ");
        }

        public static byte[] ConvertStringToByteArray(this string val)
        {
            if (val == null) return new byte[0];
            string[] rawStringArr = val.Split(' ');
            byte[] byteArr = new byte[rawStringArr.Length];
            for (int i = 0; i < rawStringArr.Length; i++) byteArr[i] = Convert.ToByte(rawStringArr[i], 16);
            return byteArr;
        }

        private static Control internalForm;
        public static void InitializeInvoke(Control control)
        {
            internalForm = control;
        }

        public static void InvokeUI(Action act)
        {
            if (internalForm.InvokeRequired)
            {
                internalForm.Invoke(act);
            }
            else
            {
                act();
            }
        }
    }
}
