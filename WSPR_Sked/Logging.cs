using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class Log
    {
        public Log(string filepath, string filename, string content)
        {            
            if (Path.Exists(filepath))
            {
                string slash = "\\";
                if (filepath.EndsWith("\\"))
                {
                    slash = "";
                }
                filepath = filepath + slash + filename;
                try
                {
                    using (StreamWriter writer = new StreamWriter(filepath, true)) //true = append to file
                    {
                        writer.WriteLine(content);
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

    }
}


