using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetObjects
{
    /// <summary>
    /// 用于表示一个交换机组
    /// </summary>
    [Serializable]
    public class Setting_Type_Group:IConfigSwitchOrGroup
    {
        
        /// <summary>
        /// IConfigSwitchOrGroup类型的List，用于建立树形结构
        /// </summary>
        public IConfigSwitchOrGroup[] ItemList { get; set; }

        /// <summary>
        /// 当前分组的名称
        /// </summary>
        public String Name { get; set; }



        public TypeOfSwitchOrGroup GetTypeOfThisItem()
        {
            return TypeOfSwitchOrGroup.分组;
        }

        public string GetNowItemName()
        {
            return Name;
        }

        public bool IfHaveNextItems()
        {
            return ItemList!=null && ItemList.Length != 0;
        }

        public IConfigSwitchOrGroup[] NextItems()
        {
            return ItemList.ToArray<IConfigSwitchOrGroup>();
        }


        public override string ToString()
        {
            return GetNowItemName();
        }


        public void AddSubItem(IConfigSwitchOrGroup input)
        {

            List<IConfigSwitchOrGroup> a;
            if (ItemList == null)
            {
                a = new List<IConfigSwitchOrGroup>();
            }
            else
            {
                a = new List<IConfigSwitchOrGroup>(ItemList);
            }
            a.Add(input);
            ItemList = a.ToArray();

        }


        public void SetItemList(IConfigSwitchOrGroup[] List)
        {
            ItemList = List;
        }
    }
}
