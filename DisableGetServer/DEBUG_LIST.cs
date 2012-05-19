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
    public partial class DEBUG_LIST : Form
    {
        DisableGetObjects.Setting_Type_Switch[] ss;
        public DEBUG_LIST(ref DisableGetObjects.Setting_Type_Switch[]  swi)
        {
            InitializeComponent();
            ss = swi;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = "LAST_FLUSH:" + DateTime.Now.ToString();
            var p = listBox1.SelectedItem;
            listBox1.Items.Clear();
            foreach (var psss in ss)
            {
                listBox1.Items.Add(psss);
            }
            listBox1.SelectedItem = p;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                var a = listBox1.SelectedItem as DisableGetObjects.Setting_Type_Switch;
                textBox1.Text = a.LastFlushTime.ToString();
                richTextBox1.Text = a.FlushResult;
            }
            else
            {
            }
        }

        private void DEBUG_LIST_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}
