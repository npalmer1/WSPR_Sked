using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSPR_Sked;

namespace RigControl
{
    public class RigCtl
    {
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public readonly string ErrorMessage;
        public string reply;// => replymsg;
        public readonly string replymsg;

        public readonly string ErrorMessageDetails = string.Empty;
        public RigCtl(string radio, string com, string baud)
        {
            //string cmdline = "rigctl -m " + radio + " -r " + com + " -s " + baud;
            string cmdline = @" -m " + radio + " -r " + com + " -s " + baud + " F 14010000";
            reply = SendCommand(cmdline);
            ErrorMessage = string.Empty;
            
        }
      
        private string SendCommand(string commandline)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string searchPattern = "rigctl.exe";
            string[] files = Directory.GetFiles(programFilesPath, searchPattern, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                startInfo.FileName = files[0] + "\\rigctl.exe";
                startInfo.WorkingDirectory = files[0];
            }
           
             
            startInfo.Arguments = commandline;        // Start the process.
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;                    // Start the process in a new window 
            startInfo.RedirectStandardOutput = true;            // This is required to get STDOUT
            startInfo.RedirectStandardError = true;               // This is required to get STDERR
            // Create a new Process object.
          
            Process process = Process.Start(startInfo);        // Set the StartInfo.FileName property to the path of the CMD executable.
                       // Do not use the OS shell to start the process
           
            process.Start();        // Wait for the process to finish.
           process.WaitForExit();        // Read the output of the process.
           
            string output = process.StandardOutput.ReadToEnd();        // Display the output of the process.
            return output;
            
        }
    }
        
        
}
