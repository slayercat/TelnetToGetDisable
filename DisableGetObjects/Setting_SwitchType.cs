using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        [OptionalField(VersionAdded = 2)]
        bool m_IsSwitchSupportDisableRecovering;

        [OptionalField(VersionAdded = 2)]
        string m_SwitchCommandForConfigMode;

        [OptionalField(VersionAdded = 2)]
        string m_SwitchCommandForRecoving;

        [OptionalField(VersionAdded = 2)]
        string m_SwitchCommandForEndConfig;

        /// <summary>
        /// 交换机是否支持Disable恢复
        /// </summary>
        public bool IsSwitchSupportDisableRecovering { get { return m_IsSwitchSupportDisableRecovering; } set { m_IsSwitchSupportDisableRecovering = value; } }

        /// <summary>
        /// 交换机进入配置模式的命令
        /// </summary>
        public string SwitchCommandForConfigMode { get { return m_SwitchCommandForConfigMode; } set { m_SwitchCommandForConfigMode = value; } }

        /// <summary>
        /// 交换机恢复指令
        /// </summary>
        public string SwitchCommandForRecoving { get { return m_SwitchCommandForRecoving; } set { m_SwitchCommandForRecoving = value; } }

        /// <summary>
        /// 交换机退出配置模式指令
        /// </summary>
        public string SwitchCommandForEndConfig { get { return m_SwitchCommandForEndConfig; } set { m_SwitchCommandForEndConfig = value; } }

        public override string ToString()
        {
            return SwitchTypeName;
        }
    }
}
