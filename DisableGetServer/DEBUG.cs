using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DisableGetServer
{
    public partial class DEBUG : Form
    {
        GetSwitchDisableStatus mointerObject=new GetSwitchDisableStatus();
        public DEBUG()
        {
            InitializeComponent();
            //注册写入
            LogInToEvent.OnWrite += new LogInToEvent.WriteToLog(write);
            
        }
        private object m_SyncObjectForListBox1 = new object(); 
        void write(string str)
        {
            if (listBox1.InvokeRequired)
            {
                
                this.Invoke(new LogInToEvent.WriteToLog(write), new object[] { str });
            }
            else
            {
                lock (m_SyncObjectForListBox1)
                {

                    listBox1.Items.Add(str);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = listBox1.SelectedItem as string;
        }

        private void DEBUG_Load(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                mointerObject.InitAndStart();
            }
            catch (Exception err)
            {
                string p = "";
                while (err != null)
                {
                    p += err.ToString();
                    err = err.InnerException;
                }
                MessageBox.Show(p);
            }

        }

        private void DEBUG_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogInToEvent.OnWrite -= new LogInToEvent.WriteToLog(write);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var t=mointerObject.GetListItems();
            DEBUG_LIST a = new DEBUG_LIST(
                ref t
            );
            a.Show();
        }
    }
}
