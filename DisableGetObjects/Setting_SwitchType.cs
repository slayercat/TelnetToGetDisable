using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetObjects
{
    /// <summary>
    /// 用于表明交换机的类型，及针对这个类型的设置
    /// </summary>
    [Serializable]
    public class Setting_SwitchType
    {
        /// <summary>
        /// 交换机的类型名称
        /// </summary>
        public string SwitchTypeName { get; set; }

        /// <summary>
        /// 当前款式的交换机是否需要用户名
        /// </summary>
        public bool IsThisSwitchNeedsOfUserName { get; set; }

        /// <summary>
        /// 交换机提示需要用户名时的特征字符串
        /// </summary>
        public string PromptForUserName { get; set; }

        /// <summary>
        /// 该交换机Enable的时候时候需要用户名
        /// </summary>
        public bool IsThisSwitchNeedsOfEnableUserName { get; set; }

        /// <summary>
        /// 交换机提示需要Enable用户名时的特征字符串
        /// </summary>
        public string PromptForEnableUserName { get; set; }

        /// <summary>
        /// 交换机提示需要密码时的特征字符串
        /// </summary>
        public string PromptForPassword { get; set; }

        /// <summary>
        /// 交换机Enable前的命令提示符特征字符串
        /// </summary>
        public string PromptForCommandBeforeEnable { get; set; }

        /// <summary>
        /// 交换机Enable后的命令提示符特征字符串（特权状态下）
        /// </summary>
        public string PromptForCommandAfterEnable { get; set; }

        /// <summary>
        /// enable密码提示特征字符串
        /// </summary>
        public string PromptForEnablePassword { get; set; }

        /// <summary>
        /// 查看端口状态需要的指令
        /// </summary>
        public string CommandForFindStatus { get; set; }


        /// <summary>
        /// 用于筛选端口的特征字符串，如Fa fastethernet等
        /// </summary>
        public string ScreeningStringForPortLine { get; set; }

        /// <summary>
        /// 用于筛选Disable项的字符串
        /// </summary>
        public string ScreeningStringForDisableItem { get; set; }

        public override string ToString()
        {
            return SwitchTypeName;
        }
    }
}
