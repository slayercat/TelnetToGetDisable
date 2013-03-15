using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer.Datastructs.Commands
{
    /// <summary>
    /// 表示交换机指令集，如：扫描Disable端口、恢复Disable端口等。
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 指令名
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// 常规指令？不会被删除？
        /// </summary>
        bool IsImmuable { get; }

        /// <summary>
        /// 指令实现代码
        /// </summary>
        void DoCommand();

        /// <summary>
        /// 目标交换机
        /// </summary>
        DisableGetObjects.Setting_Type_Switch SwitchItem { set; get; }

        /// <summary>
        /// 当前是否需要执行
        /// </summary>
        bool IfNeedExecution { get; }

    }
}
