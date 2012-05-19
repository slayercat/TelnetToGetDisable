using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer
{
    public class Helper
    {
        public static string FirstStrN(string input, int count)
        {
            if (input == null)
                return "";
            if (input.Length <= count)
                return input;
            else
            {
                return input.Substring(0, count);
            }
        }

        public static List<DisableGetObjects.Setting_Type_Switch> GetSwitchByName(List<DisableGetObjects.Setting_Type_Switch> input, string name)
        {
            
            var t = input.Where(a => a.Name.ToLower().Contains(name.ToLower()));
            return t.ToList();
        }

        public static string ToHtmlShow(string i)
        {
            if (i == null) return "";
            return i.Replace("\n", "<br/>");
        }
    }
}
