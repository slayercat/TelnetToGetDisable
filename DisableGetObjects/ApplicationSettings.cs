using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

namespace DisableGetObjects
{
    [Serializable]
    public class ApplicationSettings
    {
        /// <summary>
        /// 单次刷新时间，以分钟计算
        /// </summary>
        public int FlushTime { get; set; }

        /// <summary>
        /// 交换机树林，每个Item都是一个交换机树树根，可以包括交换机组（树枝）和交换机（树叶）
        /// </summary>
        public IConfigSwitchOrGroup[] ItemConfigWoods { get; set; }

        /// <summary>
        /// 全部交换机类型组
        /// </summary>
        public Setting_SwitchType[] SwitchTypeItems { get; set; }

        /// <summary>
        /// 用于根据名称取得交换机类型组信息
        /// </summary>
        /// <param name="switchtypename">交换机类型组名称</param>
        /// <returns>交换机类型组，如果没有找到，返回null</returns>
        public static Setting_SwitchType GetSwitchTypeByName(Setting_SwitchType[]i,string switchtypename)
        {
            foreach (var t in i)
            {
                if (t.SwitchTypeName.ToLower() == switchtypename.ToLower())
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// 取得配置文件路径
        /// </summary>
        /// <returns></returns>
        private string GetConfigPath()
        {
            string currectModel=System.Reflection.Assembly.GetExecutingAssembly().Location;

            string path=System.IO.Path.GetDirectoryName(currectModel);
            string pathOfConfigFile=path+"\\config.cfg";
            return pathOfConfigFile;
        }

        public ApplicationSettings()
        {
            FlushTime = 60;
            ItemConfigWoods = new IConfigSwitchOrGroup[0];
            SwitchTypeItems = new Setting_SwitchType[0];
        }

        /// <summary>
        /// 读取配置文件到当前类
        /// </summary>
        public void ReadFromConfigureFile()
        {
            //从配置文件中读
            Log.OverallLog.Log("读取配置文件");
            IFormatter formatter = new SoapFormatter();
            //当前dll的目录下的config.cfg
            string pathOfConfigFile = GetConfigPath();
            System.IO.Stream stream=null;
            try
            {
                 stream = new System.IO.FileStream(pathOfConfigFile, FileMode.Open,
                        FileAccess.Read, FileShare.Read);
                ApplicationSettings obj = (ApplicationSettings)formatter.Deserialize(stream);

                this.FlushTime = obj.FlushTime;
                this.ItemConfigWoods = obj.ItemConfigWoods;
                this.SwitchTypeItems = obj.SwitchTypeItems;

                stream.Close();
            }
            catch (FileNotFoundException)
            {

                Log.OverallLog.LogForErr("配置文件不存在");
                return;
            }
            catch (SerializationException)
            {
                if (stream != null)
                    stream.Dispose();
                System.IO.File.Delete(pathOfConfigFile);
                Log.OverallLog.LogForErr("配置文件存在错误，试图删除");
            }

        }
        /// <summary>
        /// 将配置写入配置文件中
        /// </summary>
        public void SetToConfigureFile()
        {
            //实现写入到配置文件中
            Log.OverallLog.Log("保存配置文件");
            IFormatter formatter = new SoapFormatter();
            //当前dll的目录下的config.cfg
            string pathOfConfigFile = GetConfigPath();
            System.IO.Stream stream = new System.IO.FileStream(pathOfConfigFile, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }
    }
}
