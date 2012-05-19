using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Security.Permissions;

namespace DisableGetServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        //[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string [] config)
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            if (config.Length == 0)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new GetSwitchDisableStatus() 
			    };
                ServiceBase.Run(ServicesToRun);
            }

            else if (config[0].ToLower() == "/debug")
            {
                //进入调试模式

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DEBUG());
                //t.InitAndStart();


            }
            else if (config[0].ToLower() == "/run")
            {
                (new GetSwitchDisableStatus()).InitAndStart();
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                MessageBox.Show("参数：\n/run:立即执行\n/debug：调试模式");
            }

        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //写入到磁盘下
            string what = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().StartInfo.FileName)+"\\"+"error."+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".log";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(what, true);
            Exception ef = e.Exception;
            while (ef != null)
            {
                sw.WriteLine(ef.ToString());
                sw.WriteLine("==================================");
                ef = ef.InnerException;
            }
            sw.Flush();
            sw.Close();

        }
    }
}
