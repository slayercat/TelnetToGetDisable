using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetObjects.Log
{
    public class OverallLog
    {
        private static StringBuilder LogOverAllContent = new StringBuilder();
        private static StringBuilder LogErr = new StringBuilder();

        private static object ContentLock = 0;
        private static object ErrLock = 0;

        public static void LogForErr(string errcontent)
        {
            lock (ErrLock)
            {
                LogErr.AppendFormat("{0} - {1} \n", DateTime.Now, errcontent);
            }
        }

        public static String GetErr()
        {
            String i="";
            lock (ErrLock)
            {
                i = LogErr.ToString();
            }
            return i;
        }

        public static void Log(string logcontent)
        {
            lock (ContentLock)
            {
                LogOverAllContent.AppendFormat("{0} - {1} \n", DateTime.Now, logcontent);
            }
        }
        public static String GetLog()
        {
            String i;
            lock (ContentLock)
            {
                i = LogOverAllContent.ToString();
            }
            return i;
        }

        public static void ClearLog()
        {
            lock (ContentLock)
            {
                LogOverAllContent.Clear();
            }
        }
    }
}
