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
        DisableGetObjects.ApplicationSettings settings = new DisableGetObjects.ApplicationSettings();

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
        /// 扫描过程中出错的交换机
        /// </summary>
        System.Collections.Generic.HashSet<DisableGetObjects.Setting_Type_Switch> hashset_switchs_error = new HashSet<DisableGetObjects.Setting_Type_Switch>();

        /// <summary>
        /// 有被disable的端口的交换机
        /// </summary>
        System.Collections.Generic.HashSet<DisableGetObjects.Setting_Type_Switch> hashset_switchs_disables = new HashSet<DisableGetObjects.Setting_Type_Switch>();


        /// <summary>
        /// WEB访问端口
        /// </summary>
        const int PORT = 9999;

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
            catch (System.Net.Sockets.SocketException e)
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
                    lock (lockdisablehashset)
                    {
                        contentbuilder.AppendLine("<table>");
                        foreach (var t in hashset_switchs_disables)
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
                    contentbuilder.AppendLine("<hr/>");
                    contentbuilder.AppendLine("<h1>Error</h1>");

                    lock (lockerrorhashset)
                    {
                        contentbuilder.AppendLine("<table>");
                        foreach (var t in hashset_switchs_error)
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
                }
                else if (location.ToUpper().StartsWith("/ALLITEMS/"))
                {

                    lock (lockServQueue)
                    {
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
        Queue<DisableGetObjects.Setting_Type_Switch> servQueue = new Queue<DisableGetObjects.Setting_Type_Switch>();

        /// <summary>
        /// 交换机列表，给出报告时使用
        /// </summary>
        List<DisableGetObjects.Setting_Type_Switch> servList = new List<DisableGetObjects.Setting_Type_Switch>();

        /// <summary>
        /// 当前正在扫描的项，为线程个数
        /// </summary>
        DisableGetObjects.Setting_Type_Switch[] servItem = new DisableGetObjects.Setting_Type_Switch[THREAD_COUNTS];

        /// <summary>
        /// 交换机队列锁
        /// </summary>
        private object lockServQueue = 1;
        /// <summary>
        /// disable交换机列表锁
        /// </summary>
        private object lockdisablehashset = 1;
        /// <summary>
        /// 错误列表锁
        /// </summary>
        private object lockerrorhashset = 1;
        /// <summary>
        /// servItem锁，锁定当前正在扫描的项
        /// </summary>
        private object lockservItem = 1;

        /// <summary>
        /// 初始化本程序
        /// </summary>
        public void InitAndStart()
        {

            LogInToEvent.WriteDebug("读取配置文件");
            settings.ReadFromConfigureFile();
            string i = DisableGetObjects.Log.OverallLog.GetErr();
            LogInToEvent.WriteDebug("读取配置文件完毕");

            LogInToEvent.WriteDebug("记录信息 Info:" + DisableGetObjects.Log.OverallLog.GetLog());
            LogInToEvent.WriteDebug("记录信息 Error:" + i);

            if (i.Trim().Length != 0)
            {
                LogInToEvent.WriteDebug("配置文件错误：\n" + i);
                throw new Exception("读取配置文件过程中出现问题，记录如下：\n" + i);
            }
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
                servQueue = new Queue<DisableGetObjects.Setting_Type_Switch>(servList);
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
                    DisableGetObjects.Setting_Type_Switch nowUsingItem = null;
                    lock (lockServQueue)
                    {
                        if (servQueue.Count != 0)
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
                        LogInToEvent.WriteDebug("取得待操作对象成功" + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                        TimeSpan p = DateTime.Now - nowUsingItem.LastFlushTime;
                        LogInToEvent.WriteDebug("上次刷新时间：" + nowUsingItem.LastFlushTime + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                        if (p.TotalMinutes > settings.FlushTime)
                        {
                            //需要刷新
                            LogInToEvent.WriteDebug("需要并进行刷新" + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                            Scan_FlushSwitchAndScanForResult(nowUsingItem);
                        }
                        else
                        {
                            //不需要刷新，等待
                            LogInToEvent.WriteDebug("不需要，等待刷新" + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                            //等待一段时间
                            TimeSpan var_target = new TimeSpan(0, settings.FlushTime, 0);
                            System.Threading.Thread.Sleep(var_target - p);
                            //刷新
                            LogInToEvent.WriteDebug("等待结束，进行刷新" + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                            Scan_FlushSwitchAndScanForResult(nowUsingItem);
                        }
                        LogInToEvent.WriteDebug("处理结束，加入队列" + "，NAME=" + nowUsingItem.Name + " IP=" + nowUsingItem.IpAddress);
                        lock (lockServQueue)
                        {
                            nowUsingItem.LastFlushLog += "\n\n" + "加入队列" + "\n\n";
                            servQueue.Enqueue(nowUsingItem);
                            nowUsingItem.LastFlushLog += "\n\n" + "加入队列结束" + "\n\n";
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
        /// 用于刷新指定交换机，并报告是否被disable
        /// </summary>
        /// <param name="nowUsingItem">被刷新的项</param>
        private void Scan_FlushSwitchAndScanForResult(DisableGetObjects.Setting_Type_Switch nowUsingItem)
        {

            const string nowProgressing = "正在处理...";
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog = "开始处理";
                nowUsingItem.FlushResult = nowProgressing;
            }
            //进行刷新操作
            const int DieTime = 5;//second
            Telnet.Terminal telnetSwitch = new Telnet.Terminal(nowUsingItem.IpAddress, 23, DieTime, 800, 600);

            /////////////////
            LogInToEvent.WriteDebug("试图连接");
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\n试图连接\n";
            }
            if (telnetSwitch.Connect())
            {
                LogInToEvent.WriteDebug("连接成功");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n连接成功\n";
                }
                var whatTypeOfSwitch = DisableGetObjects.ApplicationSettings.GetSwitchTypeByName(settings.SwitchTypeItems, nowUsingItem.SwitchTypeNameBelongTo);
                if (whatTypeOfSwitch != null)
                {
                    LogInToEvent.WriteDebug("类型：" + whatTypeOfSwitch.ToString());
                }
                else
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "错误的类型" + nowUsingItem.SwitchTypeNameBelongTo;
                    }
                    goto errorProcress;
                }

                telnetSwitch.WaitForChangedScreen();
                System.Threading.Thread.Sleep(1000);
                //登录用户名
                if (whatTypeOfSwitch.IsThisSwitchNeedsOfUserName)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录用户名");
                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录用户名\n";
                    }
                    try
                    {
                        telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForUserName);
                    }
                    catch (Exception e)
                    {
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "没有要求登录用户名\n";
                        }
                        LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                        goto errorProcress;
                    }

                    telnetSwitch.SendResponse(nowUsingItem.UserName, true);
                    telnetSwitch.WaitForChangedScreen();
                    System.Threading.Thread.Sleep(1000);
                }

                //登录密码
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录密码");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录密码\n";
                }
                try
                {
                    telnetSwitch.WaitForString
                        (whatTypeOfSwitch.PromptForPassword);
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "没有要求密码或密码请求字符串不正确";
                    }
                    goto errorProcress;
                }
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送登录密码");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送登录密码\n";
                }
                telnetSwitch.SendResponse(nowUsingItem.Password, true);
                telnetSwitch.WaitForChangedScreen();
                System.Threading.Thread.Sleep(1000);
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取得结果\n";
                }

                //确认登录密码正确性
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认登录密码正确性");
                nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认登录密码正确性\n";
                try
                {
                    telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForCommandBeforeEnable);
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "登录密码错误";
                    }
                    goto errorProcress;
                }

                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable\n";
                }
                telnetSwitch.SendResponse("enable", true);
                telnetSwitch.WaitForChangedScreen();
                System.Threading.Thread.Sleep(1000);

                //enable登录过程
                if (whatTypeOfSwitch.IsThisSwitchNeedsOfEnableUserName)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待要求enable用户名登录");
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待要求enable用户名登录\n";
                    try
                    {
                        telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForEnableUserName);
                    }
                    catch (Exception e)
                    {
                        LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "没有提示输入enable用户名";
                        }
                        goto errorProcress;
                    }
                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable用户名\n";
                    }
                    telnetSwitch.SendResponse(nowUsingItem.EnableUsername, true);
                    telnetSwitch.WaitForChangedScreen();
                    System.Threading.Thread.Sleep(1000);
                }


                //enable密码
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待要求enable密码");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待要求enable密码\n";
                }
                try
                {
                    telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForEnablePassword);
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "没有提示输入enable密码/enable用户名错误？"; }
                    goto errorProcress;
                }
                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable密码\n"; }
                if (!telnetSwitch.SendResponse(nowUsingItem.EnablePassword, true))
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "错误：发送enable密码失败"; }
                    goto errorProcress;
                }
                //telnetSwitch.Send("\r\n"); telnetSwitch.Send("\r\n"); telnetSwitch.Send("\r\n");
                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "错误：WaitForChangedScreen返回false"; }
                    goto errorProcress;
                }
                //System.Threading.Thread.Sleep(1000);

                //enable 成功？
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认enable成功");

                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认enable成功\n"; }
                try
                {
                    if (telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForCommandAfterEnable) == null)
                    {
                        
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "错误：在试图确认命令成功时发生错误。请确认enable密码。WaitForString返回了NULL"; }
                        goto errorProcress;
                    }
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "没有提示enable提示符，enable 密码错误？"; }
                    goto errorProcress;
                }

                //发送查询状态信息
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令" + whatTypeOfSwitch.CommandForFindStatus);
                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令\n"; }


                //清除屏幕，因为之前已经有enable态的提示符了。

                telnetSwitch.VirtualScreen.CleanScreen();




                telnetSwitch.SendResponse(whatTypeOfSwitch.CommandForFindStatus, true);

                telnetSwitch.WaitForChangedScreen();
                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令完成\n"; }

                System.Threading.Thread.Sleep(1000);
                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令完成+1s\n"; }
                try
                {
                    //筛选项目
                    //若没有commandline，则不断发送空格，直到有为止
                    /*string dataContains = telnetSwitch.WorkingData;
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取到ORA1项目：\n" + dataContains);
                    while (!dataContains.ToLower().Contains(whatTypeOfSwitch.PromptForCommandAfterEnable.ToLower()))
                    {
                        telnetSwitch.Send(" ");
                        System.Threading.Thread.Sleep(5000);//等待5s
                        dataContains += telnetSwitch.WorkingData;
                        LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取到ORA2项目：\n" + dataContains);
                    }*/




                    /* telnetSwitch.SendResponse(" ",true);
                     LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待命令行，发送空格1\n");
                     nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送空格\n";
                     telnetSwitch.WaitForChangedScreen();
                     System.Threading.Thread.Sleep(1000);
                     nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送空格+2s\n";
                     string wegets = telnetSwitch.GetHistory.Replace(beforeSendSearchCommand, "");

                     while (!wegets.Trim().ToLower().Contains(whatTypeOfSwitch.PromptForCommandAfterEnable.ToLower()))
                     {
                         //一直到命令行出来为止
                         nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待命令行："+wegets+"\n";
                         LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "等待命令行，发送空格：" + wegets + "\n");
                         telnetSwitch.SendResponse(" ", true);
                         telnetSwitch.WaitForChangedScreen();
                         System.Threading.Thread.Sleep(1000);
                        
                     }*/








                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //ora:telnetSwitch.SendResponse(" \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n", true);
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //大量交换机显示==more==信息，因此需要等待命令行
                    //added @ 2012 05 20 by sc

                    for (int counter = 0; counter < 20; ++counter)
                    {
                        telnetSwitch.SendResponse(" ", false);
                    }

                    ///最大尝试次数
                    const int MAXIMUM_TRY_COUNT = 20;

                    for (int currectTry = 0; currectTry <= MAXIMUM_TRY_COUNT; ++currectTry)
                    {
                        telnetSwitch.WaitForChangedScreen();
                        //若是屏幕上没有出现命令提示符
                        if (telnetSwitch.VirtualScreen.FindOnScreen(whatTypeOfSwitch.PromptForCommandAfterEnable, false) != null)
                        {
                            break;
                        }

                        if (currectTry == MAXIMUM_TRY_COUNT)
                        {
                            //到达最大重试门限
                            lock (nowUsingItem)
                            {
                                nowUsingItem.LastFlushLog += "\n到达取得数据最大门限\n";
                            }
                        }
                        else
                        {

                            //发送空格以及两个回车
                            telnetSwitch.SendResponse(" ", true);
                        }

                    }



                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////






                    string dataContains = telnetSwitch.VirtualScreen.Hardcopy().Trim(); ;

                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取到项目：\n" + dataContains);
                    var lines = dataContains.Split('\n');
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "开始检查项目列表：共" + lines.Length + "项");
                    lock (nowUsingItem)
                    { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "开始检查项目列表：共" + lines.Length.ToString() + "项\n"; }
                    bool isthisswitchhasdisable = false;
                    //保存disable；
                    string flushResult = "";
                    foreach (var t in lines)
                    {
                        LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "检查项目_disable\n" + t + "查找" + whatTypeOfSwitch.ScreeningStringForDisableItem);
                        if (t.ToLower().Contains(whatTypeOfSwitch.ScreeningStringForDisableItem.ToLower()))
                        {
                            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "检查项目_port\n" + t + "查找" + whatTypeOfSwitch.ScreeningStringForPortLine);
                            if (t.ToLower().Contains(whatTypeOfSwitch.ScreeningStringForPortLine.ToLower()))
                            {
                                //找到了disable的项
                                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "DISABLE:" + t);
                                flushResult += t + "\n";
                                isthisswitchhasdisable = true;
                            }
                        }

                    }
                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "检查结束";
                    }
                    if (isthisswitchhasdisable)
                    {
                        //有disable
                        //增加disable
                        lock (nowUsingItem)
                        {
                            nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发现disable\n";
                            nowUsingItem.FlushResult = flushResult;
                        }
                        lock (lockdisablehashset)
                        {
                            if (!hashset_switchs_disables.Contains(nowUsingItem))
                            {
                                lock (nowUsingItem)
                                {
                                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "增加到disable列表\n";
                                }
                                hashset_switchs_disables.Add(nowUsingItem);
                            }
                        }
                    }
                    else
                    {
                        //检查有无disable，有则消除
                        lock (nowUsingItem)
                        {
                            nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "未发现disable\n";
                        }
                        lock (lockdisablehashset)
                        {
                            if (hashset_switchs_disables.Contains(nowUsingItem))
                            {
                                lock (nowUsingItem)
                                {
                                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "从disable列表去除\n";
                                }
                                hashset_switchs_disables.Remove(nowUsingItem);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "无Fa提示符"; }
                    goto errorProcress;
                }

            }
            else
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "无法连接到该交换机"; }
                goto errorProcress;
            }

            nowUsingItem.LastFlushTime = DateTime.Now;

            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "成功，日志：\n" + telnetSwitch.GetHistory);
            lock (nowUsingItem)
            { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "成功\n"; }
            //成功
            lock (lockerrorhashset)
            {
                if (hashset_switchs_error.Contains(nowUsingItem))
                {
                    hashset_switchs_error.Remove(nowUsingItem);
                }
            }
            if (nowProgressing == nowUsingItem.FlushResult)
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.FlushResult = "";
                }

            }


            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\n\n SESSIONLOG:\n\n" + telnetSwitch.GetHistory;
            }

            if (telnetSwitch.IsOpenConnection())
            {
                telnetSwitch.SendLogout();
                telnetSwitch.Close();
            }
            return;


        errorProcress:
            //失败
            //todo:hardcopy可能为空，需要被注意
            string infomationToWriteInfo = nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + nowUsingItem.FlushResult;
            if (telnetSwitch.VirtualScreen != null)
            {
                infomationToWriteInfo += telnetSwitch.VirtualScreen.Hardcopy().Trim();
            }
            else
            {
                infomationToWriteInfo += "=====NULL=====";
            }
            LogInToEvent.WriteInfo(infomationToWriteInfo);
            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "失败，日志：\n" + telnetSwitch.GetHistory);

            lock (lockdisablehashset)
            {
                if (hashset_switchs_disables.Contains(nowUsingItem))
                {
                    //错误的时候消除disable
                    hashset_switchs_disables.Remove(nowUsingItem);
                }
            }
            lock (lockerrorhashset)
            {
                if (!hashset_switchs_error.Contains(nowUsingItem))
                {
                    hashset_switchs_error.Add(nowUsingItem);
                }
            }
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog = telnetSwitch.GetHistory;
            }
            nowUsingItem.LastFlushTime = DateTime.Now;

            if (telnetSwitch.IsOpenConnection())
            {
                telnetSwitch.SendLogout();
                telnetSwitch.Close();
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
