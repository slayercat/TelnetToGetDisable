using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace DisableGetServer
{
    public class LogInToEvent
    {
        const string sEventSource="Telnet到交换机并测试端口是否被Disable";
        const string sEventLog = "应用程序";

        public delegate void WriteToLog(string message);
        public static event WriteToLog OnWrite;


        private static string strickStr(string stext)
        {
            const int i = 32760;
            if (stext.Length > i)
                stext = stext.Substring(0, i);
            return stext;
        }

        public static void WriteError(string sText)
        {
            sText = strickStr(sText);
            if (!EventLog.SourceExists(sEventSource))
                EventLog.CreateEventSource(sEventSource, sEventLog);
            
            EventLog.WriteEntry(sEventSource, sText, EventLogEntryType.Error);

            if (OnWrite != null)
            {
                OnWrite(sText);
            }
        }

        public static void WriteInfo(string sText)
        {
            sText = strickStr(sText);
            if (!EventLog.SourceExists(sEventSource))
                EventLog.CreateEventSource(sEventSource, sEventLog);
            EventLog.WriteEntry(sEventSource, sText, EventLogEntryType.Information);

            if (OnWrite != null)
            {
                OnWrite(sText);
            }
        }

        public static void WriteDebug(string sText)
        {

            sText = strickStr(sText);
            if (OnWrite != null)
            {
                OnWrite(sText);
            }
        }
    }
}
