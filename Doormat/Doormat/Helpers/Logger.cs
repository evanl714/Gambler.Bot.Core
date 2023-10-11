using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoormatCore.Helpers
{
    public class Logger
    {

        /// <summary>
        /// logging levels: -1: exceptions. 0: 1: 2: 3: 4: 5: 6: 7: 8: 9: 10:
        /// </summary>

        private static int loggingLevel = 0;

        public static int LoggingLevel
        {
            get
            {
                return loggingLevel;
            }
            set { loggingLevel = value; }
        }

        public static void DumpLog(Exception E)
        {
            try
            {
                string Message = E.ToString();
                Message+= "\r\n\r\n";
                while (E.InnerException != null)
                {
                    E = E.InnerException;
                    try
                    {
                        Message += E.ToString() + "\r\n\r\n";
                        
                    }
                    catch { }
                }
                DumpLog(Message, -1);
            }
            catch
            { }
        }

        public static void DumpLog(string Message, int Level)
        {
            try
            {
                using (StreamWriter sw = System.IO.File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dicebotlog.log")))
                {
                    sw.WriteLine(string.Format(@"{1} {0}
------------------------------------------------------------------------------------------------------", Message, DateTime.UtcNow));


                }
            }
            catch
            {

            }
        }
    }
}
