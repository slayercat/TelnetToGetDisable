using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetObjects
{
    /// <summary>
    /// 交换机与分组的接口，必须实现
    /// </summary>
    public interface IConfigSwitchOrGroup
    {
        /// <summary>
        /// 取得当前项目的类型
        /// </summary>
        /// <returns>当前项的类型</returns>
        TypeOfSwitchOrGroup GetTypeOfThisItem();

        /// <summary>
        /// 取得当前项目的名称
        /// </summary>
        /// <returns>当前项名称</returns>
        string GetNowItemName();

        /// <summary>
        /// 是否有子项
        /// </summary>
        /// <returns>有子项则为true</returns>
        bool IfHaveNextItems();

        /// <summary>
        /// 子项列表
        /// </summary>
        /// <returns>当前项的所有子项</returns>
        IConfigSwitchOrGroup[] NextItems();

        /// <summary>
        /// 增加一项
        /// </summary>
        /// <param name="input"></param>
        void AddSubItem(IConfigSwitchOrGroup input);

        /// <summary>
        /// 设置子项列表（替换现有的）
        /// </summary>
        /// <param name="List"></param>
        void SetItemList(IConfigSwitchOrGroup[] List);
        

    }
}
