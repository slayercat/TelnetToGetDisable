using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer
{
    public class ApplicationStatics
    {
        static DisableGetObjects.ApplicationSettings setting;

        static ApplicationStatics()
        {
            readFromConfigureFile();
        }

        public static DisableGetObjects.ApplicationSettings Settings { get { return setting; } }

        private static void readFromConfigureFile()
        {
            setting = new DisableGetObjects.ApplicationSettings();
            LogInToEvent.WriteDebug("读取配置文件");
            setting.ReadFromConfigureFile();
            string i = DisableGetObjects.Log.OverallLog.GetErr();
            LogInToEvent.WriteDebug("读取配置文件完毕");

            LogInToEvent.WriteDebug("记录信息 Info:" + DisableGetObjects.Log.OverallLog.GetLog());
            LogInToEvent.WriteDebug("记录信息 Error:" + i);

            if (i.Trim().Length != 0)
            {
                LogInToEvent.WriteDebug("配置文件错误：\n" + i);
                throw new Exception("读取配置文件过程中出现问题，记录如下：\n" + i);
            }
        }
    }
}
