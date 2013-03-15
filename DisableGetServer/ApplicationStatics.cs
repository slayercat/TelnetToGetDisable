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


        /// <summary>
        /// 有被disable的端口的交换机
        /// </summary>
        static HashSet<DisableGetObjects.Setting_Type_Switch> hashset_switchs_disables = new HashSet<DisableGetObjects.Setting_Type_Switch>();

        /// <summary>
        /// 有被disable的端口的交换机, 进入前必须先获取锁。SwitchsDisables_EnterLock进入，SwitchsDisables_ExitLock退出
        /// </summary>
        public static HashSet<DisableGetObjects.Setting_Type_Switch> SwitchsDisables { get { return hashset_switchs_disables; } }

        private static System.Threading.SpinLock hashset_switchs_disables_lock = new System.Threading.SpinLock();
        /// <summary>
        /// 为SwitchsDisables加锁
        /// </summary>
        public static void SwitchsDisables_EnterLock() 
        {
            bool e = false;
            hashset_switchs_disables_lock.Enter(ref e); 
        }
        /// <summary>
        /// 退出SwitchsDisables的锁
        /// </summary>
        public static void SwitchsDisables_ExitLock()
        {
            hashset_switchs_disables_lock.Exit();
        }

        /// <summary>
        /// 扫描过程中出错的交换机
        /// </summary>
        static System.Collections.Generic.HashSet<DisableGetObjects.Setting_Type_Switch> hashset_switchs_error = new HashSet<DisableGetObjects.Setting_Type_Switch>();
        /// <summary>
        /// 扫描过程中出错的交换机，进入前必须先获取锁。SwitchsErrors_EnterLock进入，SwitchsErrorss_ExitLock退出
        /// </summary>
        public static HashSet<DisableGetObjects.Setting_Type_Switch> SwitchsErrors { get { return hashset_switchs_error; } }

        private static System.Threading.SpinLock hashset_switchs_error_lock = new System.Threading.SpinLock();
        /// <summary>
        /// 为SwitchsErrors加锁
        /// </summary>
        public static void SwitchsErrors_EnterLock()
        {
            bool e = false;
            hashset_switchs_error_lock.Enter(ref e);
        }
        /// <summary>
        /// 退出SwitchsErrors的锁
        /// </summary>
        public static void SwitchsErrors_ExitLock()
        {
            hashset_switchs_error_lock.Exit();
        }


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
