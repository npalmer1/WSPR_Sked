using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WSPR_Sked
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string logPath = @"C:\temp\crash_log.txt";

                // make sure the folder exists
                Directory.CreateDirectory(@"C:\temp");

                Application.ThreadException += (sender, e) =>
                {
                    File.WriteAllText(logPath, e.Exception.ToString());
                    MessageBox.Show("Error: " + e.Exception.Message + "\n\nDetails saved to " + logPath);
                };

                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    File.WriteAllText(logPath, e.ExceptionObject.ToString());
                };
                Application.Run(new Form1());
                File.AppendAllText(@"C:\temp\crash_log.txt", "App exited cleanly" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\temp");
                File.WriteAllText(@"C:\temp\crash_log.txt", ex.ToString());
                MessageBox.Show("Fatal error: " + ex.Message);
            }
          
         
        }
    }
}
