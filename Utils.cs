using System;
using System.Windows.Forms;

namespace UAssetGUI
{
    public static class Utils
    {
        public static T TryGetElement<T>(this T[] array, int index)
        {
            if (array != null && index < array.Length)
            {
                return array[index];
            }
            return default(T);
        }

        public static string ConvertByteArrayToString(this byte[] val)
        {
            return BitConverter.ToString(val).Replace("-", " ");
        }

        public static byte[] ConvertStringToByteArray(this string val)
        {
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
