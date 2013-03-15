using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer.Datastructs.Commands
{
    public class SearchForDisable : ICommand
    {
        DisableGetObjects.Setting_Type_Switch nowUsingItem;

      

        DisableGetObjects.ApplicationSettings settings = ApplicationStatics.Settings;

        public SearchForDisable(DisableGetObjects.Setting_Type_Switch dsni)
        {
            nowUsingItem = dsni;
        }



        string ICommand.CommandName
        {
            get { return "查找交换机Disable端口"; }
        }

        public virtual bool IsImmuable
        {
            get { return true; }
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
                

                    //发送查询状态信息
                    LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令" + whatTypeOfSwitch.CommandForFindStatus);
                    lock (nowUsingItem)
                    { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令\n"; }


                    //清除屏幕，因为之前已经有enable态的提示符了。
                    ClearScreen(telnetSwitch);

                    if (!telnetSwitch.SendResponse(whatTypeOfSwitch.CommandForFindStatus, true))
                    {
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "发送查询端口指令失败"; }
                        throw new Exception();
                    }
                    if (!telnetSwitch.WaitForChangedScreen())
                    {
                        lock (nowUsingItem)
                        { nowUsingItem.FlushResult = "发送查询端口指令后，没有响应"; }
                        throw new Exception();
                    }
                    lock (nowUsingItem)
                    { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令完成\n"; }

                    //System.Threading.Thread.Sleep(1000);
                    lock (nowUsingItem)
                    { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发送状态查询指令完成+1s\n"; }

                    WaitForAllSwitchStatusTransmited(telnetSwitch, whatTypeOfSwitch);

                    string dataContains = GetAllDataTransmited(telnetSwitch);

                    bool isthisswitchhasdisable;
                    string flushResult;

                    CheckForDisableStringInData(whatTypeOfSwitch, dataContains, out isthisswitchhasdisable, out flushResult);

                    lock (nowUsingItem)
                    {
                        nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "检查结束";
                    }

                    if (isthisswitchhasdisable)
                    {
                        AddsInToDisableList(flushResult);
                    }
                    else
                    {
                        RemoveFromDisabledList();
                    }
                }
                catch
                {
                    goto errorProcress;
                }


            }
            else
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "无法连接到该交换机"; }
                goto errorProcress;
            }

            try
            {
                DealWithSuccessStuff(nowProgressing, telnetSwitch);
            }
            catch
            {
                goto errorProcress;
            }
            return;


        errorProcress:
            //失败

            DealWithErrorStuff(telnetSwitch);

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

            RemoveFromDisableListForErrorOcour();

            AddsToErrorList();

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
                }
                telnetSwitch.Close();
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

        private void DealWithSuccessStuff(string nowProgressing, Telnet.Terminal telnetSwitch)
        {
            nowUsingItem.LastFlushTime = DateTime.Now;

            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "成功，日志：\n" + telnetSwitch.GetHistory);
            lock (nowUsingItem)
            { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "成功\n"; }
            //成功

            RemoveFromErrorList();

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
                    throw new Exception();
                }
                telnetSwitch.Close();
            }
        }

        private void RemoveFromErrorList()
        {
            ApplicationStatics.SwitchsErrors_EnterLock();
            if (ApplicationStatics.SwitchsErrors.Contains(nowUsingItem))
            {
                ApplicationStatics.SwitchsErrors.Remove(nowUsingItem);
            }
            ApplicationStatics.SwitchsErrors_ExitLock();
        }

        private void RemoveFromDisabledList()
        {
            //检查有无disable，有则消除
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "未发现disable\n";
            }
            ApplicationStatics.SwitchsDisables_EnterLock();
            if (ApplicationStatics.SwitchsDisables.Contains(nowUsingItem))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "从disable列表去除\n";
                }
                ApplicationStatics.SwitchsDisables.Remove(nowUsingItem);
            }
            ApplicationStatics.SwitchsDisables_ExitLock();
        }

        private void AddsInToDisableList(string flushResult)
        {
            //有disable
            //增加disable
            lock (nowUsingItem)
            {
                nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "发现disable\n";
                nowUsingItem.FlushResult = flushResult;
            }
            ApplicationStatics.SwitchsDisables_EnterLock();
            if (!ApplicationStatics.SwitchsDisables.Contains(nowUsingItem))
            {
                lock (nowUsingItem)
                {
                    nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "增加到disable列表\n";
                }
                ApplicationStatics.SwitchsDisables.Add(nowUsingItem);
            }
            ApplicationStatics.SwitchsDisables_ExitLock();
        }

        private void CheckForDisableStringInData(DisableGetObjects.Setting_SwitchType whatTypeOfSwitch, string dataContains, out bool isthisswitchhasdisable, out string flushResult)
        {
            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "取到项目：\n" + dataContains);
            var lines = dataContains.Split('\n');
            LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "开始检查项目列表：共" + lines.Length + "项");
            lock (nowUsingItem)
            { nowUsingItem.LastFlushLog += "\n" + nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + "开始检查项目列表：共" + lines.Length.ToString() + "项\n"; }
            isthisswitchhasdisable = false;
            //保存disable
            flushResult = "";
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
        }

        private string GetAllDataTransmited(Telnet.Terminal telnetSwitch)
        {
            string dataContains;
            try
            {
                dataContains = telnetSwitch.VirtualScreen.Hardcopy().Trim(); ;
            }
            catch (NullReferenceException)
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "发送等待命令行参数的请求过程中断开连接"; }
                throw new Exception();
            }
            catch (Exception e)
            {
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "发送等待命令行参数的请求过程中发生了异常，" + e.ToString(); }
                throw new Exception();
            }
            return dataContains;
        }

        private void WaitForAllSwitchStatusTransmited(Telnet.Terminal telnetSwitch, DisableGetObjects.Setting_SwitchType whatTypeOfSwitch)
        {
            //大量交换机显示==more==信息，因此需要等待命令行
            //added @ 2012 05 20 by sc
            for (int counter = 0; counter < 20; ++counter)
            {
                if (!telnetSwitch.SendResponse(" ", false))
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "发送等待命令行参数的请求失败"; }
                    throw new Exception();
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
                    throw new Exception();
                }
                catch (Exception)
                {
                    lock (nowUsingItem)
                    { nowUsingItem.FlushResult = "发送请求命令行参数的请求失败"; }
                    throw new Exception();
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
                        throw new Exception();
                    }
                }

            }
        }

        private void ClearScreen(Telnet.Terminal telnetSwitch)
        {
            try
            {
                telnetSwitch.VirtualScreen.CleanScreen();
            }
            catch (Exception e)
            {
                LogInToEvent.WriteDebug(nowUsingItem.Name + "/" + nowUsingItem.IpAddress + "-" + e.ToString());
                lock (nowUsingItem)
                { nowUsingItem.FlushResult = "试图清屏幕失败，可能是连接被断开" + e.ToString(); }
                throw new Exception();
            }
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


        DisableGetObjects.Setting_Type_Switch ICommand.SwitchItem
        {
            set { nowUsingItem = value; }
            get { return nowUsingItem; }
        }


        public virtual bool IfNeedExecution
        {
            get { return (DateTime.Now - nowUsingItem.LastFlushTime).TotalMinutes > ApplicationStatics.Settings.FlushTime; }
        }
    }
}
