using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer.Datastructs.Commands
{
    public class RecoveryFromDisable:ICommand
    {
        DisableGetObjects.Setting_Type_Switch nowUsingItem;

        DisableGetObjects.ApplicationSettings settings = ApplicationStatics.Settings;

        public RecoveryFromDisable(DisableGetObjects.Setting_Type_Switch sw)
        {
            nowUsingItem = sw;
        }

        string ICommand.CommandName
        {
            get { return "恢复被disable端口"; }
        }

        bool ICommand.IsImmuable
        {
            get { return false; }
        }

        void ICommand.DoCommand()
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

                if (!whatTypeOfSwitch.IsSwitchSupportDisableRecovering)
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "交换机不支持Disable恢复！";
                    }
                    goto errorProcress;
                }

                telnetSwitch.WaitForChangedScreen();


                try
                {
                    // 发送登录用户名
                    SendLoginUserNameIfNeeded(telnetSwitch, whatTypeOfSwitch);

                    // 发送登录密码
                    SendLoginPassword_LowPriv(telnetSwitch, whatTypeOfSwitch);

                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取得结果\n";
                    }
                    // 确认低权限登录密码正确
                    MakeSureLowPrivIsGood(telnetSwitch, whatTypeOfSwitch);

                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable");
                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable\n";
                    }

                    // 发送Enable命令
                    SendEnableCommand(telnetSwitch);

                    // 若需要，发送Enable用户名

                    SendEnableUserNameIfNeeded(telnetSwitch, whatTypeOfSwitch);

                    // 发送Enable密码
                    SendLoginPassword_HighPriv(telnetSwitch, whatTypeOfSwitch);

                    //enable 成功？
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认enable成功");

                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "确认enable成功\n";
                    }

                    MakeSureEnableSuccess(telnetSwitch, whatTypeOfSwitch);

                    System.Threading.Thread.Sleep(100);

                    // 开始处理errdisable recovery事件

                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "开始试图恢复\n";
                    }

                    try
                    {
                        // 进入终端配置模式
                        GetIntoTerminalConfigMode(telnetSwitch, whatTypeOfSwitch);
                        telnetSwitch.WaitForChangedScreen();

                        // 发送终端配置字符串
                        SendErrdisableRecoveryCommand(telnetSwitch, whatTypeOfSwitch);
                        telnetSwitch.WaitForChangedScreen();

                        // 退出终端模式
                        SendEndToSuperModeCommand(telnetSwitch, whatTypeOfSwitch);
                        //telnetSwitch.WaitForChangedScreen();
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "")
                        {
                            lock (nowUsingItem)
                            {
                                nowUsingItem.LastFlushLog += "\n 在处理恢复命令时发生意外";
                            }
                            throw;
                        }
                        else
                        {
                            lock (nowUsingItem)
                            {
                                nowUsingItem.LastFlushLog += "\n" + e.ToString();
                            }
                            throw;
                        }
                           
                    }


                }
                catch
                {
                    goto errorProcress;
                }
                
                string infomationToWriteInfo = "";
                if (telnetSwitch.VirtualScreen != null)
                {
                    infomationToWriteInfo += telnetSwitch.VirtualScreen.Hardcopy().Trim();
                }
                else
                {
                    infomationToWriteInfo += "=====NULL=====";
                }
                LogInToEvent.WriteInfo(infomationToWriteInfo);
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "完毕，日志：\n" + telnetSwitch.GetHistory);

                if (telnetSwitch.IsOpenConnection())
                {
                    if (telnetSwitch.SendLogout() == false)
                    {
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "请求注销失败"; }
                    }
                    telnetSwitch.Close();
                }
                else
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + "已经关闭连接，可能失败";
                    }
                }

                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + "日志：\n" + telnetSwitch.GetHistory + "\n\n屏幕：\n" + infomationToWriteInfo;
                
                    nowUsingItem.FlushResult = "恢复结束，请单击刷新超链接以确认结果";
                }
                 return;

            errorProcress:
                DealWithErrorStuff(telnetSwitch);
            }
        
        }

        private void SendEndToSuperModeCommand(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\n SendEndToSuperModeCommand \r\n";
                nowUsingItem.LastFlushLog += "\n 发送："+ whatTypeOfSwitch.SwitchCommandForEndConfig+"\r\n";
            }
            if(!telnetSwitch.SendResponse(whatTypeOfSwitch.SwitchCommandForEndConfig, true))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + "回到普通命令提示模式失败\n";
                }
                throw new Exception();
            }
            for (int i = 0; i < 20; ++i)
            {
                telnetSwitch.SendResponse(" ", true);
            }
        }

        private void SendErrdisableRecoveryCommand(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
            nowUsingItem.LastFlushLog += "\n SendErrdisableRecoveryCommand ";
            nowUsingItem.LastFlushLog += "\n 发送：" + whatTypeOfSwitch.SwitchCommandForRecoving + "\r\n";
            if (!telnetSwitch.SendResponse(whatTypeOfSwitch.SwitchCommandForRecoving, true))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + "发送恢复命令失败\n";
                }
                throw new Exception();
            }
            for (int i = 0; i < 20; ++i)
            {
                telnetSwitch.SendResponse(" ", true);
            }
        }

        private void GetIntoTerminalConfigMode(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
            nowUsingItem.LastFlushLog += "\n GetIntoTerminalConfigMode ";
            nowUsingItem.LastFlushLog += "\n 发送：" + whatTypeOfSwitch.SwitchCommandForConfigMode + "\r\n";
            if (!telnetSwitch.SendResponse(whatTypeOfSwitch.SwitchCommandForConfigMode, true))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + "进入终端配置模式失败\n";
                }
                throw new Exception();
            }
            for (int i = 0; i < 20; ++i)
            {
                telnetSwitch.SendResponse(" ", true);
            }
        }

        DisableGetObjects.Setting_Type_Switch ICommand.SwitchItem
        {
            get
            {
                return nowUsingItem;
            }
            set
            {
                nowUsingItem = value;
            }
        }

        bool ICommand.IfNeedExecution
        {
            get { return true; }
        }




        private void MakeSureEnableSuccess(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
            try
            {
                telnetSwitch.WaitForString(whatTypeOfSwitch.PromptForCommandAfterEnable);

            }
            catch (Exception e)
            {
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "没有提示enable提示符，enable 密码错误？"; }
                throw new Exception();
            }
        }

        private void SendLoginPassword_HighPriv(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
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
                throw new Exception();
            }
            lock (nowUsingItem)
            { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送enable密码\n"; }
            if (!telnetSwitch.SendResponse(nowUsingItem.EnablePassword, true))
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "错误：发送enable密码失败"; }
                throw new Exception();
            }

            if (!telnetSwitch.WaitForChangedScreen())
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "错误：WaitForChangedScreen返回false"; }
                throw new Exception();
            }
        }

        private void SendEnableUserNameIfNeeded(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
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
                    throw new Exception();
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
                    throw new Exception();
                }
                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "发送enable用户名后没有响应";
                    }
                    throw new Exception();
                }
            }
        }

        private void SendEnableCommand(Telnet.Terminal telnetSwitch)
        {
            if (!telnetSwitch.SendResponse("enable", true))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.FlushResult = "发送enable指令失败";
                }
                throw new Exception();
            }
            if (!telnetSwitch.WaitForChangedScreen())
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.FlushResult = "发送enable指令后没有响应";
                }
                throw new Exception();
            }
        }

        private void MakeSureLowPrivIsGood(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
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
                throw new Exception();
            }
        }

        private void SendLoginPassword_LowPriv(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
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
                throw new Exception();
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
                throw new Exception();
            }
            if (!telnetSwitch.WaitForChangedScreen())
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.FlushResult = "发送密码后没有响应";
                }
                throw new Exception();
            }
        }

        private void SendLoginUserNameIfNeeded(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
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
                    throw new Exception();
                }

                if (!telnetSwitch.SendResponse(nowUsingItem.UserName, true))
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "未发送用户名";
                    }

                    throw new Exception();
                }

                if (!telnetSwitch.WaitForChangedScreen())
                {
                    lock (nowUsingItem)
                    {
                        nowUsingItem.FlushResult = "等待用户名结果失败";
                    }
                    throw new Exception();
                }

            }
        }
        private void DealWithErrorStuff(Telnet.Terminal telnetSwitch)
        {
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

            

            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n===========================\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
                nowUsingItem.LastFlushLog += infomationToWriteInfo;
                nowUsingItem.LastFlushLog += telnetSwitch.GetHistory;
                nowUsingItem.LastFlushLog += "\r\n错误返回";
                nowUsingItem.FlushResult = "在恢复过程中出现错误";
            }
            nowUsingItem.LastFlushTime = DateTime.Now;

            if (telnetSwitch.IsOpenConnection())
            {
                if (telnetSwitch.SendLogout() == false)
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "请求注销失败"; }
                }
                telnetSwitch.Close();
            }
            lock (nowUsingItem)
            {
                nowUsingItem.FlushResult = "在试图恢复时发生错误";
            }
        }

        private void AddsToErrorList()
        {
            ApplicationStatics.SwitchsErrors_EnterLock();
            if (!ApplicationStatics.SwitchsErrors.Contains(nowUsingItem))
            {
                ApplicationStatics.SwitchsErrors.Add(nowUsingItem);
            }
            ApplicationStatics.SwitchsErrors_ExitLock();
        }

        private void RemoveFromDisableListForErrorOcour()
        {
            ApplicationStatics.SwitchsDisables_EnterLock();
            if (ApplicationStatics.SwitchsDisables.Contains(nowUsingItem))
            {
                //错误的时候消除disable
                ApplicationStatics.SwitchsDisables.Remove(nowUsingItem);
            }
            ApplicationStatics.SwitchsDisables_ExitLock();
        }

    }
}
