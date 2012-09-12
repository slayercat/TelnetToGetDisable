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
            const int DieTime = 10;//second
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
                
                //System.Threading.Thread.Sleep(2000);
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
                            nowUsingItem.FlushResult = "要求用户名时遇到异常-" + e.ToString();
                        }
                        LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                        goto errorProcress;
                    }

                    if (!telnetSwitch.SendResponse(nowUsingItem.UserName, true))
                    {
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "未发送用户名";
                        }
                       
                        goto errorProcress;
                    }

                    if (!telnetSwitch.WaitForChangedScreen())
                    {
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "等待用户名结果失败";
                        }
                        goto errorProcress;
                    }
                    //System.Threading.Thread.Sleep(1000);
                }

                //登录密码
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录密码");
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "要求登录密码\n";
                }
                try
                {
                    telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForPassword);
                    
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
                if (!telnetSwitch.SendResponse(nowUsingItem.Password, true))
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "发送密码失败";
                    }
                    goto errorProcress;
                }
                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "发送密码后没有响应";
                    }
                    goto errorProcress;
                }
                //System.Threading.Thread.Sleep(1000);
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
                if (!telnetSwitch.SendResponse("enable", true))
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "发送enable指令失败";
                    }
                    goto errorProcress;
                }
                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "发送enable指令后没有响应";
                    }
                    goto errorProcress;
                }
                //System.Threading.Thread.Sleep(1000);

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
                    if (!telnetSwitch.SendResponse(nowUsingItem.EnableUsername, true))
                    {
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "发送enable用户名失败";
                        }
                        goto errorProcress;
                    }
                    if (!telnetSwitch.WaitForChangedScreen())
                    {
                        lock (nowUsingItem)
                        {
                            nowUsingItem.FlushResult = "发送enable用户名后没有响应";
                        }
                        goto errorProcress;
                    }
                    //System.Threading.Thread.Sleep(1000);
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
                    telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForCommandAfterEnable);
                    
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
                try
                {
                    telnetSwitch.VirtualScreen.CleanScreen();
                }
                catch (Exception e)
                {
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "试图清屏幕失败，可能是连接被断开"+e.ToString(); }
                    goto errorProcress;
                }




                if (!telnetSwitch.SendResponse(whatTypeOfSwitch.CommandForFindStatus, true))
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "发送查询端口指令失败"; }
                    goto errorProcress;
                }

                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "发送查询端口指令后，没有响应"; }
                    goto errorProcress;
                }
                lock (nowUsingItem)
                { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令完成\n"; }

                //System.Threading.Thread.Sleep(1000);
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
                        if (!telnetSwitch.SendResponse(" ", false))
                        {
                            lock (nowUsingItem)
                            { nowUsingItem.FlushResult = "发送等待命令行参数的请求失败"; }
                            goto errorProcress;
                        }
                    }

                    ///最大尝试次数
                    const int MAXIMUM_TRY_COUNT = 20;

                    for (int currectTry = 0; currectTry <= MAXIMUM_TRY_COUNT; ++currectTry)
                    {
                        telnetSwitch.WaitForChangedScreen();
                        
                        //若是屏幕上没有出现命令提示符
                        try
                        {
                            if (telnetSwitch.VirtualScreen.FindOnScreen(whatTypeOfSwitch.PromptForCommandAfterEnable, false) != null)
                            {
                                break;
                            }
                        }
                        catch (NullReferenceException)
                        {
                            lock (nowUsingItem)
                            { nowUsingItem.FlushResult = "发送请求命令行参数的请求中，连接断开"; }
                            goto errorProcress;
                        }
                        catch (Exception e)
                        {
                            lock (nowUsingItem)
                            { nowUsingItem.FlushResult = "发送请求命令行参数的请求失败"; }
                            goto errorProcress;
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
                            if (!telnetSwitch.SendResponse(" ", true))
                            {
                                lock (nowUsingItem)
                                { nowUsingItem.FlushResult = "发送等待命令行参数的请求失败-1"; }
                                goto errorProcress;
                            }
                        }

                    }



                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////




                    string dataContains;
                    try
                    {
                        dataContains = telnetSwitch.VirtualScreen.Hardcopy().Trim(); ;
                    }
                    catch (NullReferenceException)
                    {
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "发送等待命令行参数的请求过程中断开连接"; }
                        goto errorProcress;
                    }
                    catch (Exception e)
                    {
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "发送等待命令行参数的请求过程中发生了异常，"+e.ToString(); }
                        goto errorProcress;
                    }

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
                if (telnetSwitch.SendLogout() == false)
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "请求注销失败"; }
                    goto errorProcress;
                }
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
                if (telnetSwitch.SendLogout() == false)
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "请求注销失败"; }
                    goto errorProcress;
                }
                telnetSwitch.Close();
            }

        }

    }

}