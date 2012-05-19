using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DisableGetConfig
{
    public partial class Form1 : Form
    {
        DisableGetObjects.ApplicationSettings toConfigWhat = new DisableGetObjects.ApplicationSettings();

        public Form1()
        {
            InitializeComponent();
            toConfigWhat.ReadFromConfigureFile();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = toConfigWhat.FlushTime.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var t = new List<DisableGetObjects.Setting_SwitchType>(
                toConfigWhat.SwitchTypeItems);
            ConfigForSwitchType config = new ConfigForSwitchType(ref t);
            config.ShowDialog();
            toConfigWhat.SwitchTypeItems = t.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i;
            if (!int.TryParse(textBox1.Text, out i))
            {
                MessageBox.Show("错误！无法解析时间");
                return;
            }
            toConfigWhat.FlushTime = i;
            toConfigWhat.SetToConfigureFile();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var t=new List<DisableGetObjects.IConfigSwitchOrGroup>(toConfigWhat.ItemConfigWoods);
            ConfigForItemAndSwitch ediag = new ConfigForItemAndSwitch(ref t,toConfigWhat.SwitchTypeItems);
            ediag.ShowDialog();
            toConfigWhat.ItemConfigWoods = t.ToArray();
        }

        
    }
}
