using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAMSHelper
{
    public class Logger
    {
        public void AddLogToTXT(string logstring,string filePath)
        {
            if (!File.Exists(filePath))
            {
                FileStream stream = File.Create(filePath);
                stream.Close();
                stream.Dispose();
            }
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(logstring);
            }
        }
    }
}
