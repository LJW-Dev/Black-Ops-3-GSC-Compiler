using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BO3_GSC_Compiler_XBOX
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string dirname = Path.GetDirectoryName(Application.ExecutablePath) + "\\CompiledScripts";
            if (!Directory.Exists(dirname))
                Directory.CreateDirectory(dirname);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new XBOXUI());
        }
    }
}
