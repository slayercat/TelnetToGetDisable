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
    public partial class ConfigForSwitchType : Form
    {

        private List<DisableGetObjects.Setting_SwitchType> listofSwitchType;

        

        public ConfigForSwitchType(ref List<DisableGetObjects.Setting_SwitchType> input)
        {
            InitializeComponent();
            listofSwitchType = input;
        }

        void flushForDefault()
        {
            this.tb_TypeName.Text = "";
            this.tb_beforeenable.Text = ">";
            this.tb_password.Text = "password";
            this.tb_username.Text = "username";
            this.tb_afterenable.Text = "#";
            this.tb_enableUsername.Text = "username";
            this.tb_enablePassword.Text = "password";
            this.tb_PortStr.Text = "fa";
            this.tb_disableStr.Text = "disable";
            this.tb_findPort.Text = "show interface status";

            this.cb_NeeduserName.Checked = false;
            this.cb_EnableNeedusername.Checked = false;
        }
        void flushItem()
        {
            if (listBox1.SelectedItem == null)
                flushForDefault();
            else
            {
                var data = listBox1.SelectedItem as DisableGetObjects.Setting_SwitchType;
                tb_TypeName.Text=data.SwitchTypeName;
                tb_beforeenable.Text=data.PromptForCommandBeforeEnable;
                tb_password.Text=data.PromptForPassword;
                tb_username.Text=data.PromptForUserName;
                tb_afterenable.Text=data.PromptForCommandAfterEnable;
                tb_enableUsername.Text=data.PromptForEnableUserName;
                tb_enablePassword.Text=data.PromptForEnablePassword;
                tb_PortStr.Text=data.ScreeningStringForPortLine;
                tb_disableStr.Text=data.ScreeningStringForDisableItem;
                tb_findPort.Text=data.CommandForFindStatus;

                cb_NeeduserName.Checked=data.IsThisSwitchNeedsOfUserName;
                cb_EnableNeedusername.Checked = data.IsThisSwitchNeedsOfEnableUserName;
            }
        }

        private void ConfigForSwitchType_Load(object sender, EventArgs e)
        {
            flushForDefault();
            //载入全部的type

            foreach (var t in listofSwitchType)
            {
                listBox1.Items.Add(t);
            }
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            if (
                    tb_TypeName.Text.Trim() != ""
                    &&
                    notInList(tb_TypeName.Text.Trim())
                )
            {
                listBox1.Items.Add(new DisableGetObjects.Setting_SwitchType
                {
                    SwitchTypeName = tb_TypeName.Text.Trim(),
                    PromptForCommandBeforeEnable = tb_beforeenable.Text.Trim(),
                    PromptForPassword = tb_password.Text.Trim(),
                    PromptForUserName = tb_username.Text.Trim(),
                    PromptForCommandAfterEnable = tb_afterenable.Text.Trim(),
                    PromptForEnableUserName = tb_enableUsername.Text.Trim(),
                    PromptForEnablePassword = tb_enablePassword.Text.Trim(),
                    ScreeningStringForPortLine = tb_PortStr.Text.Trim(),
                    ScreeningStringForDisableItem = tb_disableStr.Text.Trim(),
                    CommandForFindStatus = tb_findPort.Text.Trim(),

                    IsThisSwitchNeedsOfUserName = cb_NeeduserName.Checked,
                    IsThisSwitchNeedsOfEnableUserName = cb_EnableNeedusername.Checked
                });
                flushToMem();
            }
            else
            {
                MessageBox.Show("必须填写typename，且typename不能重复");
            }
        }

        private void flushToMem()
        {
            listofSwitchType.Clear();
            foreach (var e in listBox1.Items)
            {
                var mm = e as DisableGetObjects.Setting_SwitchType;
                listofSwitchType.Add(mm);
            }
        }

        private bool notInList(string p)
        {
            p = p.Trim();
            foreach (var e in listBox1.Items)
            {
                var mm = e as DisableGetObjects.Setting_SwitchType;
                if (mm.SwitchTypeName.ToLower() == p.ToLower())
                {
                    return false;
                }
            }

            return true;
        }

        private void btn_modify_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("没有选择项目");
                return;
            }

            if (
                    tb_TypeName.Text.Trim() != ""
                    &&
                    notInListExcept(tb_TypeName.Text.Trim(), listBox1.SelectedItem)
                )
            {
                listBox1.Items[listBox1.SelectedIndex] = new DisableGetObjects.Setting_SwitchType
                {
                    SwitchTypeName = tb_TypeName.Text.Trim(),
                    PromptForCommandBeforeEnable = tb_beforeenable.Text.Trim(),
                    PromptForPassword = tb_password.Text.Trim(),
                    PromptForUserName = tb_username.Text.Trim(),
                    PromptForCommandAfterEnable = tb_afterenable.Text.Trim(),
                    PromptForEnableUserName = tb_enableUsername.Text.Trim(),
                    PromptForEnablePassword = tb_enablePassword.Text.Trim(),
                    ScreeningStringForPortLine = tb_PortStr.Text.Trim(),
                    ScreeningStringForDisableItem = tb_disableStr.Text.Trim(),
                    CommandForFindStatus = tb_findPort.Text.Trim(),

                    IsThisSwitchNeedsOfEnableUserName = cb_EnableNeedusername.Checked,
                    IsThisSwitchNeedsOfUserName = cb_NeeduserName.Checked
                };
                flushToMem();
            }
            else
            {
                MessageBox.Show("错误：该项名称已经存在");
            }
        }

        

        /// <summary>
        /// 除了选中的项目其他的项目不存在指定的值
        /// </summary>
        /// <param name="p">寻找的值</param>
        /// <param name="p_2">选中的项</param>
        /// <returns></returns>
        private bool notInListExcept(string p, object p_2)
        {
            p = p.Trim();
            foreach (var e in listBox1.Items)
            {
                if (e == p_2)
                    continue;

                var mm = e as DisableGetObjects.Setting_SwitchType;
                if (mm.SwitchTypeName.ToLower() == p.ToLower())
                {
                    return false;
                }
            }

            return true;
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("没有选择项目");
                return;
            }
            listBox1.Items.Remove(listBox1.SelectedItem);
            flushToMem();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            flushItem();
        }
    }
}
