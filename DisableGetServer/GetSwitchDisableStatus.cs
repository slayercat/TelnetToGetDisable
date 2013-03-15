using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DisableGetServer
{
    /// <summary>
    /// 这是一个服务，用于运行期间检查交换机被disable的情况，并在用户访问时提供web界面。
    /// </summary>
    public partial class GetSwitchDisableStatus : ServiceBase
    {
        public GetSwitchDisableStatus()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当前程序的设置
        /// </summary>
        DisableGetObjects.ApplicationSettings settings = ApplicationStatics.Settings;

        /// <summary>
        /// 用于在交换机类型的List和Array间相互转换。
        /// </summary>
        /// <returns></returns>
        public DisableGetObjects.Setting_Type_Switch[] GetListItems()
        {
            return servList.ToArray();
        }

        /// <summary>
        /// 同时进行扫描交换机disable的线程个数
        /// </summary>
        const int THREAD_COUNTS = 10;

        /// <summary>
        /// 线程列表/线程池（称为线程池似乎有点不太妥当，因为他们只有sleep，而不停息）
        /// </summary>
        System.Threading.Thread[] pool = new System.Threading.Thread[THREAD_COUNTS];

        /// <summary>
        /// 用于等待WEB访问的线程
        /// </summary>
        System.Threading.Thread itemThredOfWaitWeb = null;






        /// <summary>
        /// WEB访问端口
        /// </summary>
        const int PORT = 9999;

        /// <summary>
        /// 默认的低优先级队列的优先级
        /// </summary>
        const int DEFAULT_LOW_PRI = 10000;

        /// <summary>
        /// 默认的高优先级队列的优先级
        /// </summary>
        const int DEFAULT_HIGH_PRI = 10;

        /// <summary>
        /// 用于等待web访问，web访问线程的执行方
        /// </summary>
        void Web_WaitWebConnection()
        {

            System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, PORT);
            try
            {
                listener.Start();
            }
            catch (System.Net.Sockets.SocketException)
            {
                //已经被占用
                LogInToEvent.WriteError("端口" + PORT.ToString() + "已经被占用");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            while (true)
            {
                var comein = listener.AcceptTcpClient();
                var dealing = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Web_AfterConnectionGetReportAndSendToUser));
                dealing.Start(comein);

            }
        }

        /// <summary>
        /// 在web访问到来时取得访问报告，并返回给用户
        /// </summary>
        /// <param name="a"></param>
        void Web_AfterConnectionGetReportAndSendToUser(object a)
        {
            System.Net.Sockets.TcpClient socketcomein = a as System.Net.Sockets.TcpClient;

            if (socketcomein == null)
            {
                LogInToEvent.WriteError("未知错误：socketcomein == null");
                return;
            }
            try
            {
                var stream = socketcomein.GetStream();
                System.IO.StreamReader rstream = new System.IO.StreamReader(stream);
                string i = rstream.ReadLine();
                var where = i.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (!
                    (where.Length == 3 &&
                    where[1].Trim().StartsWith("/"))
                    )
                {
                    string errorMessage = "请求格式错误，请发送 GET / HTTP1.1 \n" + i;
                    foreach (var p in where)
                    {
                        errorMessage += "\n" + p + "\n";
                    }
                    var error_gs = System.Text.Encoding.UTF8.GetBytes(errorMessage);
                    stream.Write(error_gs, 0, error_gs.Length);
                    stream.Close();
                    socketcomein.Close();
                    return;
                }
                string report = Web_GetReport(where[1]);
                string head = "http/1.1 200 ok\n" +
                            "Content-Type: text/html\n";
                byte[] content = System.Text.Encoding.UTF8.GetBytes(report);
                head += "Content-Length:" + content.Length + "\n\n";
                var headstr = System.Text.Encoding.UTF8.GetBytes(head);
                stream.Write(headstr, 0, headstr.Length);
                stream.Write(content, 0, content.Length);
                stream.Close();
                socketcomein.Close();
            }
            catch (Exception e)
            {
                LogInToEvent.WriteError(e.ToString());
                return;
            }
        }


        /// <summary>
        /// 用于取得报告
        /// </summary>
        /// <param name="location">指明位置的字符串，所有包含该字符串的说明交换机均会被取到</param>
        /// <returns></returns>
        string Web_GetReport(string location)
        {
            string result = "";
            try
            {
                StringBuilder contentbuilder = new StringBuilder();

                if (contentbuilder == null)
                {
                    return "error:contentbuilder has null point";
                }

                contentbuilder.AppendLine("<!DOCTYPE html>");
                contentbuilder.AppendLine("<head>");
                contentbuilder.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                contentbuilder.AppendLine("</head>");
                contentbuilder.AppendLine("<body>");

                contentbuilder.AppendLine("<a href=\"/\">");
                contentbuilder.AppendLine("关注列表");
                contentbuilder.AppendLine("</a>");
                contentbuilder.AppendLine("<br/>");
                contentbuilder.AppendLine("<a href=\"/ALLITEMS/\">");
                contentbuilder.AppendLine("全部项目列表");
                contentbuilder.AppendLine("</a>");


                location = location.Trim();
                if (location == "/")
                {

                    contentbuilder.AppendLine("<h1>全局错误信息</h1>");
                    contentbuilder.AppendLine("<p>");
                    string errlog = DisableGetObjects.Log.OverallLog.GetErr();
                    if (errlog == null)
                    {
                        contentbuilder.AppendLine("ERROR:Errlog eq null");
                    }
                    else
                    {
                        contentbuilder.AppendLine(errlog.Replace("\n", "<br/>"));
                    }
                    contentbuilder.AppendLine("</p>");
                    contentbuilder.AppendLine("<hr/>");
                    //输出disable和错误项
                    contentbuilder.AppendLine("<h1>DISABLE</h1>");
                    ApplicationStatics.SwitchsDisables_EnterLock();
                    {
                        contentbuilder.AppendLine("<table>");
                        foreach (var t in ApplicationStatics.SwitchsDisables)
                        {
                            lock (t)
                            {
                                contentbuilder.AppendLine("<tr>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine("<a href=\"/ITEM/" + t.Name + " \">");
                                contentbuilder.AppendLine(t.Name);
                                contentbuilder.AppendLine("</a>");
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine(t.LastFlushTime.ToString());
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("<td>");

                                contentbuilder.AppendLine(Helper.FirstStrN(t.FlushResult, 30));
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("</tr>");
                            }
                        }
                        contentbuilder.AppendLine("</table>");
                    }
                    ApplicationStatics.SwitchsDisables_ExitLock();
                    contentbuilder.AppendLine("<hr/>");
                    contentbuilder.AppendLine("<h1>Error</h1>");

                    ApplicationStatics.SwitchsErrors_EnterLock();
                    {
                        contentbuilder.AppendLine("<table>");
                        foreach (var t in ApplicationStatics.SwitchsErrors)
                        {
                            lock (t)
                            {
                                contentbuilder.AppendLine("<tr>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine("<a href=\"/ITEM/" + t.Name + " \">");
                                contentbuilder.AppendLine(t.Name);
                                contentbuilder.AppendLine("</a>");
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine(t.LastFlushTime.ToString());
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("<td>");

                                contentbuilder.AppendLine(Helper.FirstStrN(t.FlushResult, 30));
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("</tr>");
                            }
                        }
                        contentbuilder.AppendLine("</table>");
                    }
                    ApplicationStatics.SwitchsErrors_ExitLock();
                }
                else if (location.ToUpper().StartsWith("/ALLITEMS/"))
                {

                    lock (lockServQueue)
                    {
                        contentbuilder.AppendLine("<p><a href=\"/REFLUSH/ALL" + "\"> 刷新 </a></p>");
                        contentbuilder.AppendLine("<table>");
                        foreach (var t in servList)
                        {
                            lock (t)
                            {
                                contentbuilder.AppendLine("<tr>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine("<a href=\"/ITEM/" + t.Name.Trim() + "\">");
                                contentbuilder.AppendLine(t.Name);
                                contentbuilder.AppendLine("</a>");
                                contentbuilder.AppendLine("</td>");
                                contentbuilder.AppendLine("<td>");
                                contentbuilder.AppendLine(t.LastFlushTime.ToString());
                                contentbuilder.AppendLine("</td>");

                                contentbuilder.AppendLine("<td>");

                                contentbuilder.AppendLine(Helper.FirstStrN(t.FlushResult, 30));
                                contentbuilder.AppendLine("</td>");

                                contentbuilder.AppendLine("</tr>");
                            }
                        }
                        contentbuilder.AppendLine("</table>");
                    }
                }
                else if (location.ToUpper().StartsWith("/ITEM/"))
                {
                    string contains = location.Split(new string[] { "/ITEM/" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    contentbuilder.AppendLine("<h1>");
                    //Server
                    contains = System.Web.HttpUtility.UrlDecode(contains, System.Text.Encoding.UTF8);
                    contentbuilder.AppendLine("Contains=" + contains);
                    contentbuilder.AppendLine("</h1>");
                    contentbuilder.AppendLine("<p><a href=\"/REFLUSH/" + contains + "\"> 刷新 </a></p>");
                    var resultSearched = Helper.GetSwitchByName(servList, contains);

                    contentbuilder.AppendLine("<table>");
                    foreach (var t in resultSearched)
                    {
                        lock (t)
                        {
                            contentbuilder.AppendLine("<tr>");
                            contentbuilder.AppendLine("<td>");
                            contentbuilder.AppendLine("<a href=\" " + t.Name + " \">");
                            contentbuilder.AppendLine(t.Name);
                            contentbuilder.AppendLine("</a>");
                            contentbuilder.AppendLine("</td>");
                            contentbuilder.AppendLine("<td>");
                            contentbuilder.AppendLine(t.LastFlushTime.ToString());
                            contentbuilder.AppendLine("</td>");

                            contentbuilder.AppendLine("<td>");
                            contentbuilder.AppendLine(Helper.FirstStrN(t.FlushResult, 30));
                            contentbuilder.AppendLine("</td>");

                            contentbuilder.AppendLine("</tr>");
                        }
                    }
                    contentbuilder.AppendLine("</table>");


                    contentbuilder.AppendLine("<hr/>");


                    foreach (var t in resultSearched)
                    {
                        lock (t)
                        {
                            contentbuilder.AppendLine("<p>");
                            contentbuilder.AppendLine(t.Name + "结果");
                            contentbuilder.AppendLine(t.LastFlushTime.ToString());
                            contentbuilder.AppendLine(Helper.ToHtmlShow(t.FlushResult));
                            contentbuilder.AppendLine(Helper.ToHtmlShow(t.LastFlushLog));
                            contentbuilder.AppendLine("</p>");
                        }
                    }

                }
                else if (location.ToUpper().StartsWith("/REFLUSH/"))
                {
                    if (location.ToUpper().StartsWith("/REFLUSH/ALL"))
                    {
                        lock (lockServQueue)
                        {
                            foreach (var t in servList)
                            {
                                lock (t)
                                {
                                    // 优先处理
                                    servQueue.Enqueue(new Datastructs.Commands.SearchForDisableNonImmuable(t), DEFAULT_HIGH_PRI);
                                }
                            }
                        }
                        contentbuilder.AppendLine("已经提交处理，请<a href=\"/\">返回</a>");
                    }
                    else
                    {
                        string contains = location.Split(new string[] { "/REFLUSH/" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        contains = System.Web.HttpUtility.UrlDecode(contains, System.Text.Encoding.UTF8);
                        var resultSearched = Helper.GetSwitchByName(servList, contains);
                        lock (lockServQueue)
                        {
                            foreach (var t in resultSearched)
                            {
                                lock (t)
                                {
                                    // 优先处理
                                    servQueue.Enqueue(new Datastructs.Commands.SearchForDisableNonImmuable(t), DEFAULT_HIGH_PRI);
                                }
                            }
                        }
                        contentbuilder.AppendLine("已经提交处理，请<a href=\"/\">返回</a>");
                    }
                }
                else
                {
                    contentbuilder.AppendLine("错误的请求：目标" + location);
                }

                contentbuilder.AppendLine("</body>");
                contentbuilder.AppendLine("</html>");
                result = contentbuilder.ToString();
            }
            catch (Exception e)
            {

                result += e.Source;
                result += "<br/>";
                result += e.Message;
                result += "<br/>";
                result += e.StackTrace;
                result += "<br/>";
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(e, true);
                result += "POSITION:<br/>";
                result += "LINE:" + st.GetFrame(0).GetFileLineNumber();
                result += "Column:" + st.GetFrame(0).GetFileColumnNumber();
                result += "FILE :" + st.GetFrame(0).GetFileName();


            }
            return result;
        }

        /// <summary>
        /// 交换机队列，扫描时使用
        /// </summary>
        Datastructs.PriorityQueue<Datastructs.Commands.ICommand> servQueue;

        /// <summary>
        /// 交换机列表，给出报告时使用
        /// </summary>
        List<DisableGetObjects.Setting_Type_Switch> servList = new List<DisableGetObjects.Setting_Type_Switch>();

        /// <summary>
        /// 当前正在扫描的项，为线程个数
        /// </summary>
        Datastructs.Commands.ICommand[] servItem = new Datastructs.Commands.ICommand[THREAD_COUNTS];

        /// <summary>
        /// 交换机队列锁
        /// </summary>
        private object lockServQueue = 1;

        /// <summary>
        /// servItem锁，锁定当前正在扫描的项
        /// </summary>
        private object lockservItem = 1;

        /// <summary>
        /// 初始化本程序
        /// </summary>
        public void InitAndStart()
        {


            if (
                settings.FlushTime == 0
                ||
                settings.ItemConfigWoods == null
                ||
                settings.SwitchTypeItems == null
                )
            {
                LogInToEvent.WriteDebug("没有初始化内容");
                throw new Exception("没有初始化内容？");
            }

            //列出待服务队列
            LogInToEvent.WriteDebug("列出待服务队列");
            turnItemIntoList(settings.ItemConfigWoods, ref servList);
            lock (lockServQueue)
            {
                servQueue = new Datastructs.PriorityQueue<Datastructs.Commands.ICommand>();
                foreach (var t in servList)
                {
                    // 约定为DEFAULT_LOW_PRI ，普通的为低优先级
                    servQueue.Enqueue(new Datastructs.Commands.SearchForDisable(t), DEFAULT_LOW_PRI);
                }

            }

            LogInToEvent.WriteDebug("列出待服务队列完成，共" + servList.Count + "项");

            LogInToEvent.WriteDebug("开始初始化线程");
            for (int c = 0; c < pool.Length; ++c)
            {
                pool[c] = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Scan_DecideIfNeedsScanAndDispatchScan));
                pool[c].Start(c);
            }
            LogInToEvent.WriteDebug("初始化线程结束");

            LogInToEvent.WriteDebug("初始化WEB访问线程");
            itemThredOfWaitWeb = new System.Threading.Thread(new System.Threading.ThreadStart(Web_WaitWebConnection));
            itemThredOfWaitWeb.Start();
            LogInToEvent.WriteDebug("初始化WEB访问线程结束");
        }


        /// <summary>
        /// 只在初始化时调用：将交换机的配置树转换为列表
        /// </summary>
        /// <param name="iConfigSwitchOrGroup">传入的配置树</param>
        /// <param name="servList">传出的列表</param>
        private void turnItemIntoList(DisableGetObjects.IConfigSwitchOrGroup[] iConfigSwitchOrGroup, ref List<DisableGetObjects.Setting_Type_Switch> servList)
        {
            foreach (var t in iConfigSwitchOrGroup)
            {
                if (t is DisableGetObjects.Setting_Type_Switch)
                {
                    servList.Add(t as DisableGetObjects.Setting_Type_Switch);
                }
                else
                {
                    if (t.IfHaveNextItems())
                    {
                        turnItemIntoList(t.NextItems(), ref servList);
                    }
                }
            }
        }

        /// <summary>
        /// 如果需要的话，调度刷新。刷新线程调度项目
        /// </summary>
        /// <param name="count">当前执行该函数的为第几个线程</param>
        void Scan_DecideIfNeedsScanAndDispatchScan(object count)
        {
            int currItemCount = (int)count;

            while (true)
            {
                try
                {
                    LogInToEvent.WriteDebug("试图取得待操作对象");
                    Datastructs.Commands.ICommand nowUsingItem = null;
                    lock (lockServQueue)
                    {
                        if (!servQueue.IsEmpty())
                        {
                            nowUsingItem = servQueue.Dequeue();
                        }
                    }
                    if (nowUsingItem != null)
                    {
                        lock (lockservItem)
                        {
                            servItem[currItemCount] = nowUsingItem;
                        }

                        //执行操作
                        if (nowUsingItem.IfNeedExecution)
                        {
                            nowUsingItem.DoCommand();
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        if (nowUsingItem.IsImmuable)
                        {
                            lock (lockServQueue)
                            {
                                servQueue.Enqueue(nowUsingItem, DEFAULT_LOW_PRI);
                            }
                        }
                        lock (lockservItem)
                        {
                            servItem[currItemCount] = null;
                        }
                    }
                    else
                    {
                        //等着看看
                        lock (lockservItem)
                        {
                            servItem[currItemCount] = null;
                        }
                        LogInToEvent.WriteDebug("取得待操作对象失败，休眠100s");
                        System.Threading.Thread.Sleep(100000);
                    }
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteError(e.ToString());
                    DisableGetObjects.Log.OverallLog.LogForErr(e.ToString());
                }
            }
        }



        /// <summary>
        /// 启动本服务，调度初始化
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            AutoLog = true;
            try
            {
                LogInToEvent.WriteInfo("尝试启动");
                InitAndStart();
            }
            catch (Exception e)
            {
                LogInToEvent.WriteError(e.ToString());

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }


        /// <summary>
        /// 停止本服务，清理
        /// </summary>
        protected override void OnStop()
        {
            //停止全部线程
            LogInToEvent.WriteDebug("开始停止进程");
            LogInToEvent.WriteDebug("停止线程池");
            foreach (var t in pool)
            {
                t.Abort();
            }
            LogInToEvent.WriteDebug("准备结束应用程序");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
