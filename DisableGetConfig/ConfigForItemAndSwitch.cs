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
    public partial class ConfigForItemAndSwitch : Form
    {

        List<DisableGetObjects.IConfigSwitchOrGroup> data;
        DisableGetObjects.Setting_SwitchType[] dataSwitchTypes;
        public ConfigForItemAndSwitch(ref List<DisableGetObjects.IConfigSwitchOrGroup> obj, DisableGetObjects.Setting_SwitchType[] switchTypes)
        {
            InitializeComponent();
            data = obj;
            dataSwitchTypes = switchTypes;
            AddSelectionItems();
        }

        private void AddSelectionItems()
        {
            cb_itemType.Items.Clear();
            cb_itemType.Items.Add(DisableGetObjects.TypeOfSwitchOrGroup.分组);
            cb_itemType.Items.Add(DisableGetObjects.TypeOfSwitchOrGroup.交换机);
            cb_switchType.Items.Clear();
            foreach (var t in dataSwitchTypes)
            {
                cb_switchType.Items.Add(t);
            }
        }

        private void ConfigForItemAndSwitch_Load(object sender, EventArgs e)
        {
            flushTreeViewItems();
            cb_itemType_SelectedIndexChanged(sender, e);
        }

        private void flushTreeViewItems()
        {
            var bak = treeView1.SelectedNode;
            treeView1.Nodes.Clear();
            foreach (var t in data)
            {
                treeView1.Nodes.Add(buildtree(t));
            }

            if (treeView1.SelectedNode == null)
            {
                //clear up all item
                tb_TypeName.Clear();
                tb_username.Clear();
                tb_password.Clear();
                tb_enableUsername.Clear();
                tb_IPAddress.Clear();
                tb_enablePassword.Clear();
                cb_itemType.SelectedItem = null;
                cb_switchType.SelectedItem = null;
                return;
            }
            treeView1.SelectedNode = bak;
        }

        private TreeNode buildtree(DisableGetObjects.IConfigSwitchOrGroup datainput)
        {
            TreeNode result = new TreeNode(datainput.GetNowItemName());
            
            result.Tag = datainput;
            if (datainput.IfHaveNextItems())
            {
                var datanext = datainput.NextItems();
                foreach (var p in datanext)
                {
                    result.Nodes.Add(buildtree(p));
                }
            }
            return result;
        }

        private void cb_itemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_itemType.SelectedItem != null)
            {
                var t = (DisableGetObjects.TypeOfSwitchOrGroup)cb_itemType.SelectedItem;
                if (t == DisableGetObjects.TypeOfSwitchOrGroup.分组)
                {
                    cb_switchType.Enabled =
                    tb_enablePassword.Enabled =
                    tb_enableUsername.Enabled =
                    tb_password.Enabled =
                    tb_username.Enabled =
                    tb_IPAddress.Enabled=
                    false;
                }
                else
                {
                    cb_switchType.Enabled =
                    tb_password.Enabled =
                    tb_username.Enabled =
                    tb_enablePassword.Enabled =
                    tb_IPAddress.Enabled =
                    true;

                    cb_switchType_SelectedIndexChanged(this, e);
                }
            }
            else
            {
                cb_switchType.Enabled =
                tb_enablePassword.Enabled =
                tb_enableUsername.Enabled =
                tb_password.Enabled =
                tb_username.Enabled =
                false;
            }
        }

        private void cb_switchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_switchType.SelectedItem != null)
            {
                var t = cb_switchType.SelectedItem as DisableGetObjects.Setting_SwitchType;
                tb_enableUsername.Enabled = t.IsThisSwitchNeedsOfEnableUserName;
                tb_username.Enabled = t.IsThisSwitchNeedsOfUserName;
            }
            else
            {
                tb_enableUsername.Enabled = false;
                tb_username.Enabled = false;
            }
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            string typename = tb_TypeName.Text.Trim();
            if (typename.Length == 0)
            {
                MessageBox.Show("错误：您必须输入类型名称");
                return;
            }
            if (hasTypeName(typename))
            {
                MessageBox.Show("错误：该类型名已经存在");
                return;
            }
            TreeNode addToWhere = null;
            if (treeView1.SelectedNode != null)
            {
                var i = treeView1.SelectedNode.Tag as DisableGetObjects.IConfigSwitchOrGroup;
                if (i.GetTypeOfThisItem() == DisableGetObjects.TypeOfSwitchOrGroup.交换机)
                {
                    addToWhere = treeView1.SelectedNode.Parent;

                }
                else
                {
                    addToWhere = treeView1.SelectedNode;
                }
            }
            if (cb_itemType.SelectedItem == null)
            {
                MessageBox.Show("无效类型");
                return;
            }
            DisableGetObjects.IConfigSwitchOrGroup icsg;
            DisableGetObjects.TypeOfSwitchOrGroup typeofnewitem = (DisableGetObjects.TypeOfSwitchOrGroup)cb_itemType.SelectedItem;
            if (typeofnewitem == DisableGetObjects.TypeOfSwitchOrGroup.分组)
            {
                icsg = new DisableGetObjects.Setting_Type_Group
                {
                    Name = tb_TypeName.Text,
                    
                };
            }
            else
            {
                if (cb_switchType.SelectedItem == null)
                {
                    MessageBox.Show("请选择交换机类型");
                    return;
                }

                icsg = new DisableGetObjects.Setting_Type_Switch
                {
                    Name = tb_TypeName.Text,
                    Password = tb_password.Text,
                    UserName = tb_username.Text,
                    EnablePassword = tb_enablePassword.Text,
                    IpAddress=tb_IPAddress.Text,
                    EnableUsername = tb_enableUsername.Text,
                    SwitchTypeNameBelongTo = (cb_switchType.SelectedItem as DisableGetObjects.Setting_SwitchType).SwitchTypeName
                };
            }

            if (addToWhere == null)
            {
                //增加到顶级项
                var nodeadded = treeView1.Nodes.Add(icsg.GetNowItemName());
                nodeadded.Tag = icsg;
            }
            else
            {
                var tempitem = addToWhere.Tag as DisableGetObjects.IConfigSwitchOrGroup;
                tempitem.AddSubItem(icsg);
            }

            saveItemsToMem();
            flushTreeViewItems();

        }

        private void saveItemsToMem()
        {
            //rebuild variable data
            data.Clear();
            foreach (TreeNode tnodes in treeView1.Nodes)
            {
                var t = tnodes.Tag as DisableGetObjects.IConfigSwitchOrGroup;
                data.Add(t);
            }
        }

        private bool hasTypeName(string typename)
        {
            foreach (var t in data)
            {
                if (t.GetNowItemName().Trim().ToLower() == typename.Trim().ToLower())
                {
                    return true;
                }
                if (t.IfHaveNextItems())
                {
                    if (t.NextItems()
                        .Where(
                            a => a.GetNowItemName() ==
                                typename.Trim().ToLower()).Count() != 0
                      )
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (treeView1.GetChildAtPoint(new Point(e.X, e.Y)) == null)
                {
                    this.treeView1.SelectedNode = null;

                        //clear up all item
                        tb_TypeName.Clear();
                        tb_username.Clear();
                        tb_password.Clear();
                        tb_enableUsername.Clear();
                        tb_IPAddress.Clear();
                        tb_enablePassword.Clear();
                        cb_itemType.SelectedItem = null;
                        cb_switchType.SelectedItem = null;
                        return;

                }
            }
        }

        private void btn_modify_Click(object sender, EventArgs e)
        {
            string typename = tb_TypeName.Text.Trim();
            if (typename.Length == 0)
            {
                MessageBox.Show("错误：您必须输入类型名称");
                return;
            }
            if (hasTypeWithOutSelectedItemName(typename, treeView1.SelectedNode))
            {
                MessageBox.Show("错误：该类型名已经存在");
                return;
            }
            TreeNode modifyWhere = null;
            if (treeView1.SelectedNode != null)
            {
                var i = treeView1.SelectedNode.Tag as DisableGetObjects.IConfigSwitchOrGroup;
                
                modifyWhere = treeView1.SelectedNode;
            }
            else
            {
                MessageBox.Show("请先选定待修改项");
                return;
            }
            if (cb_itemType.SelectedItem == null)
            {
                MessageBox.Show("无效类型");
                return;
            }
            DisableGetObjects.IConfigSwitchOrGroup icsg;
            DisableGetObjects.TypeOfSwitchOrGroup typeofnewitem = (DisableGetObjects.TypeOfSwitchOrGroup)cb_itemType.SelectedItem;
            if (typeofnewitem == DisableGetObjects.TypeOfSwitchOrGroup.分组)
            {
                icsg = new DisableGetObjects.Setting_Type_Group
                {
                    Name = tb_TypeName.Text,
                    //ItemList = new List<DisableGetObjects.IConfigSwitchOrGroup>()
                };
            }
            else
            {
                if (cb_switchType.SelectedItem == null)
                {
                    MessageBox.Show("请选择交换机类型");
                    return;
                }

                icsg = new DisableGetObjects.Setting_Type_Switch
                {
                    Name = tb_TypeName.Text,
                    Password = tb_password.Text,
                    UserName = tb_username.Text,
                    IpAddress=tb_IPAddress.Text,
                    EnablePassword = tb_enablePassword.Text,
                    EnableUsername = tb_enableUsername.Text,
                    SwitchTypeNameBelongTo = (cb_switchType.SelectedItem as DisableGetObjects.Setting_SwitchType).SwitchTypeName
                };
            }

            var t=modifyWhere.Tag as DisableGetObjects.IConfigSwitchOrGroup;

            if (t.IfHaveNextItems() &&
                icsg.GetTypeOfThisItem() == DisableGetObjects.TypeOfSwitchOrGroup.交换机)
            {
                MessageBox.Show("有子项的分组项不能被修改为交换机");
                return;
            }

            if (t.IfHaveNextItems())
            {
                foreach (var tmp in t.NextItems())
                {
                    icsg.AddSubItem(tmp);
                }
            }
            findAndReplaceInData(t, icsg);
            flushTreeViewItems();
            

        }

        private void findAndReplaceInData(DisableGetObjects.IConfigSwitchOrGroup src, DisableGetObjects.IConfigSwitchOrGroup dstValue)
        {
            for(int i=0;i<data.Count;++i)
            {

                if (data[i] == src)
                {
                    data[i] = dstValue;
                }
                else
                {
                    var t = data[i];
                    findandReplaceInSub(data[i].NextItems(),ref t, src, dstValue);
                    data[i] = t;
                }
            }
        }

        private void findandReplaceInSub(DisableGetObjects.IConfigSwitchOrGroup[] iConfigSwitchOrGroup,ref DisableGetObjects.IConfigSwitchOrGroup father, DisableGetObjects.IConfigSwitchOrGroup src, DisableGetObjects.IConfigSwitchOrGroup dstValue)
        {
            if (iConfigSwitchOrGroup == null)
                return;
            for (int i = 0; i < iConfigSwitchOrGroup.Length; ++i)
            {
                if (iConfigSwitchOrGroup[i] == src)
                {
                    iConfigSwitchOrGroup[i] = dstValue;
                    father.SetItemList(iConfigSwitchOrGroup);
                    return;
                }
                else
                {
                    findandReplaceInSub(iConfigSwitchOrGroup[i].NextItems(),ref iConfigSwitchOrGroup[i], src, dstValue);
                }
            }
        }

        private bool hasTypeWithOutSelectedItemName(string typename, TreeNode treeNode)
        {
            if (treeNode.Text.Trim().ToLower() == typename.Trim().ToLower())
            {
                return false;
            }
            return 
                hasTypeName(typename);
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            //todo:delete logic
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选择要删除的项");
                return;
            }
            //find the father,and this one
            //first find in the root
            var t = treeView1.SelectedNode.Tag as DisableGetObjects.IConfigSwitchOrGroup;
            for (int ts = 0; ts < data.Count; ++ts)
            {
                if (data[ts] == t)
                {
                    data.RemoveAt(ts);
                    flushTreeViewItems();
                    return;
                }
            }
            //not find in root
            foreach (var p in data)
            {
                deleteItemInSub(p, t);
            }
            flushTreeViewItems();
            return;
        }

        private void deleteItemInSub(DisableGetObjects.IConfigSwitchOrGroup father, DisableGetObjects.IConfigSwitchOrGroup whattodel)
        {
            if (!father.IfHaveNextItems())
            {
                return;
            }
            foreach (var m in father.NextItems())
            {
                if (m == whattodel)
                {
                    List<DisableGetObjects.IConfigSwitchOrGroup> w = new List<DisableGetObjects.IConfigSwitchOrGroup>(father.NextItems());
                    w.Remove(whattodel);
                    father.SetItemList(w.ToArray());
                    return;
                }
                else
                {
                    deleteItemInSub(m, whattodel);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
            var toshow = e.Node.Tag as DisableGetObjects.IConfigSwitchOrGroup;
            tb_TypeName.Text = toshow.GetNowItemName();
            cb_itemType.SelectedItem = toshow.GetTypeOfThisItem();
            if (toshow.GetTypeOfThisItem() == DisableGetObjects.TypeOfSwitchOrGroup.交换机)
            {
                var toshowjhj = toshow as DisableGetObjects.Setting_Type_Switch;
                cb_switchType.SelectedItem = DisableGetObjects.ApplicationSettings.GetSwitchTypeByName(dataSwitchTypes, toshowjhj.SwitchTypeNameBelongTo);
                tb_enableUsername.Text = toshowjhj.EnableUsername;
                tb_enablePassword.Text = toshowjhj.EnablePassword;
                tb_username.Text = toshowjhj.UserName;
                tb_password.Text = toshowjhj.Password;
                tb_IPAddress.Text = toshowjhj.IpAddress;
            }
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                // Draw the background of the selected node. The NodeBounds
                // method makes the highlight rectangle large enough to
                // include the text of a node tag, if one is present.
                e.Graphics.FillRectangle(Brushes.Green, e.Bounds);

                // Retrieve the node font. If the node font has not been set,
                // use the TreeView font.
                Font nodeFont = e.Node.NodeFont;
                if (nodeFont == null) nodeFont = ((TreeView)sender).Font;

                // Draw the node text.
                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.White,
                    Rectangle.Inflate(e.Bounds, 2, 0));
            }

    // Use the default background and node text.
            else
            {
                e.DrawDefault = true;
            }
            if ((e.State & TreeNodeStates.Focused) != 0)
            {
                using (Pen focusPen = new Pen(Color.Black))
                {
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    Rectangle focusBounds = e.Bounds;
                    focusBounds.Size = new Size(focusBounds.Width - 1,
                    focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
            }
        }

        
    }
}
