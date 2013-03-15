using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer.Datastructs.Commands
{
    public class SearchForDisable : ICommand
    {
        DisableGetObjects.Setting_Type_Switch nowUsingItem;

        public delegate void DelegateCommand(DisableGetObjects.Setting_Type_Switch e);

        private DelegateCommand dc;


        public SearchForDisable(DisableGetObjects.Setting_Type_Switch dsni, DelegateCommand dcommand)
        {
            nowUsingItem = dsni;
            dc = dcommand;
        }



        string ICommand.CommandName
        {
            get { return "查找交换机Disable端口"; }
        }

        bool ICommand.IsImmuable
        {
            get { return true; }
        }

        void ICommand.DoCommand()
        {
            // todo: 重构到这个类里头
            if (dc != null)
                dc(nowUsingItem);
        }


        DisableGetObjects.Setting_Type_Switch ICommand.SwitchItem
        {
            set { nowUsingItem = value; }
            get { return nowUsingItem; }
        }


        bool ICommand.IfNeedExecution
        {
            get { return (DateTime.Now - nowUsingItem.LastFlushTime).TotalMinutes > ApplicationStatics.Settings.FlushTime; }
        }
    }
}
