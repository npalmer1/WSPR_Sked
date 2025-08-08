using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Org.BouncyCastle.Asn1.Ocsp;
using ZstdSharp.Unsafe;

namespace Wspr_Encode
{
    internal class WsprMsg
    {

        public WsprMsg()
        {
            

        }
        public string output = "";
        int OpSystem = 0;
        public byte[] getWsprLevels(string filepath, string callsign, string loc, double power, int slotNo, int opsys)
        {
            byte[] levels = null;
            //string output;
            string content;
            string commandline;
            if (Path.Exists(filepath))
            {
                string slash = "\\";
                if (opsys != 0) // eg. Linux or MacOS
                {
                    slash = "/";
                }
                if (filepath.EndsWith(slash))
                {
                    slash = "";
                }
                filepath = filepath + slash;
                string filename = "echo";
                try
                {
                   
                    content = callsign + " " + loc + " " + power;
                    runWsprMsg(filepath, content,opsys);
                    if (output != "")
                    {                      
                            levels = findslotLevels(output, slotNo);                        
                    }
                        
                    return levels;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
       
        public async void runWsprMsg(string filepath, string content, int opsys)
        {

            output = "";
            string c = "/c ";
            if (opsys != 0)
            {
                c = "-c ";
            }
            //string args = "/c echo " + content + " | " + filepath + "wsprmsg.exe";
            string args = c + filepath + "wspr_enc.exe " + content;
            
            //await Task.Run(() =>
            //{
                runAsynProcess(args,opsys);

            //});            

        }

        public async Task runAsynProcess(string args, int opsys)
        {
            string cmd = "cmd.exe";
            if (opsys != 0)
            {
                cmd = "/bin/bash";
            }
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo()
                {
                  
                    FileName = cmd, // Command to run
                        
                                     //Arguments = args, // Arguments for the command
                    Arguments = args,
                    RedirectStandardOutput = true, // Redirect output if needed
                    RedirectStandardError = false,  // Redirect error stream if needed
                    UseShellExecute = false,       // Necessary for redirection
                    CreateNoWindow = true,          // Run without a visible window
                                                    //WorkingDirectory
                };


                Process process = new Process
                {
                    StartInfo = processInfo
                };

                // Start the process
                process.Start();
                Task.Delay(100);
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
              
            }
            catch (Exception ex)
            {                
                
            }

        }
     

        public byte[] findslotLevels(string fileoutput, int slotNo)
        {
            byte[] levels = new byte[162];
            string symbols = "";
            //string symbolstart = "Symbol message #" + slotNo.ToString() + ":";
            string symbolstart = "Symbol codes: ";
            try
            {
                int e = fileoutput.IndexOf(symbolstart);
 
                if (e > -1)
                {
                    e = e + symbolstart.Length;
                    symbols = fileoutput.Substring(e, fileoutput.Length - e);
                    
                       
                        symbols = symbols.Trim();
                        string[] sym = symbols.Split(',');
                        for (int i = 0; i < sym.Length; i++)
                        {
                            sym[i] = sym[i].Trim('\r');
                            sym[i] = sym[i].Trim('\n');
                            sym[i] = sym[i].Trim();
                        }
                        if (sym.Length > 161)
                        {
                            for (int i = 0; i < 162; i++)
                            {
                                levels[i] = Convert.ToByte(sym[i]);
                            }
                        }
                }
            }
            catch
            {
                levels = null;
            }

            return levels;
        }

    }
}
