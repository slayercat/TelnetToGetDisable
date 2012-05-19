using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DisableGetObjects
{
    /// <summary>
    /// 用于表示一台交换机
    /// </summary>
    [Serializable]
    public class Setting_Type_Switch : IConfigSwitchOrGroup
    {
        /// <summary>
        /// 当前交换机的描述名
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 当前交换机所属的交换机类型名称
        /// </summary>
        public string SwitchTypeNameBelongTo { get; set; }

        /// <summary>
        /// 交换机若有用户名的话，该交换机的用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 该交换机的登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 该交换机的Enable密码
        /// </summary>
        public string EnablePassword { get; set; }

        /// <summary>
        /// 交换机若有Enable用户名的话，写在这里
        /// </summary>
        public string EnableUsername { get; set; }

        /// <summary>
        /// 交换机的IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 上次被刷新时间
        /// </summary>
        [NonSerialized]
        public DateTime LastFlushTime = DateTime.MinValue;

        /// <summary>
        /// 刷新结果
        /// </summary>
        [NonSerialized]
        public string FlushResult = "";

        /// <summary>
        /// 上次刷新的日志文件
        /// </summary>
        [NonSerialized]
        public string LastFlushLog = "";

        public TypeOfSwitchOrGroup GetTypeOfThisItem()
        {
            return TypeOfSwitchOrGroup.交换机;
        }

        public string GetNowItemName()
        {
            return Name;
        }

        public bool IfHaveNextItems()
        {
            return false;
        }

        public IConfigSwitchOrGroup[] NextItems()
        {
            return null;
        }

        public override string ToString()
        {
            return GetNowItemName();
        }


        public void AddSubItem(IConfigSwitchOrGroup input)
        {
            throw new NotSupportedException("交换机无法增加子元素");
        }


        public void SetItemList(IConfigSwitchOrGroup[] List)
        {
            throw new NotSupportedException("交换机无法拥有子元素");
        }
    }
}
