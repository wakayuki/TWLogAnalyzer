using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TWLogAnalyzer.common
{
    class Logger
    {
        private static Logger instance;

        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        public static void init(string logFilePath, bool logEnabled)
        {
            File.Delete(logFilePath);
            instance = new Logger(logFilePath, logEnabled);
        }

        private bool logEnabled;
        private string logFilePath;

        private Logger(string logFilePath, bool logEnabled)
        {
            this.logEnabled = logEnabled;
            this.logFilePath = logFilePath;
        }

        public void PutMessage(string message)
        {
            if (!logEnabled) return;

            File.AppendAllText(logFilePath, message + Environment.NewLine);
        }
    }
}
