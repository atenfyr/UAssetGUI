using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UAssetGUI
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 f1 = new Form1
            {
                Size = new System.Drawing.Size(1000, 700)
            };
            Application.Run(f1);
        }
    }
}
