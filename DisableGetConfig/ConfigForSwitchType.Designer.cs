using System.Collections.Generic;
namespace DisableGetConfig
{
    partial class ConfigForSwitchType
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btn_add = new System.Windows.Forms.Button();
            this.btn_modify = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tb_disableStr = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tb_PortStr = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_findPort = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_enablePassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cb_EnableNeedusername = new System.Windows.Forms.CheckBox();
            this.tb_enableUsername = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_afterenable = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cb_NeeduserName = new System.Windows.Forms.CheckBox();
            this.tb_beforeenable = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_username = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_TypeName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(202, 448);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // btn_add
            // 
            this.btn_add.Location = new System.Drawing.Point(247, 12);
            this.btn_add.Name = "btn_add";
            this.btn_add.Size = new System.Drawing.Size(75, 23);
            this.btn_add.TabIndex = 1;
            this.btn_add.Text = "增加";
            this.btn_add.UseVisualStyleBackColor = true;
            this.btn_add.Click += new System.EventHandler(this.btn_add_Click);
            // 
            // btn_modify
            // 
            this.btn_modify.Location = new System.Drawing.Point(374, 12);
            this.btn_modify.Name = "btn_modify";
            this.btn_modify.Size = new System.Drawing.Size(75, 23);
            this.btn_modify.TabIndex = 2;
            this.btn_modify.Text = "修改";
            this.btn_modify.UseVisualStyleBackColor = true;
            this.btn_modify.Click += new System.EventHandler(this.btn_modify_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.Location = new System.Drawing.Point(491, 12);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(75, 23);
            this.btn_delete.TabIndex = 3;
            this.btn_delete.Text = "删除";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_disableStr);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.tb_PortStr);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.tb_findPort);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.tb_enablePassword);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.cb_EnableNeedusername);
            this.groupBox1.Controls.Add(this.tb_enableUsername);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tb_afterenable);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.cb_NeeduserName);
            this.groupBox1.Controls.Add(this.tb_beforeenable);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tb_password);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tb_username);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tb_TypeName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(247, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 419);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "交换机类型信息";
            // 
            // tb_disableStr
            // 
            this.tb_disableStr.Location = new System.Drawing.Point(166, 309);
            this.tb_disableStr.Name = "tb_disableStr";
            this.tb_disableStr.Size = new System.Drawing.Size(274, 21);
            this.tb_disableStr.TabIndex = 27;
            this.tb_disableStr.Text = "disable";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(29, 312);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(131, 12);
            this.label10.TabIndex = 26;
            this.label10.Text = "筛选disable特征字符串";
            // 
            // tb_PortStr
            // 
            this.tb_PortStr.Location = new System.Drawing.Point(166, 282);
            this.tb_PortStr.Name = "tb_PortStr";
            this.tb_PortStr.Size = new System.Drawing.Size(274, 21);
            this.tb_PortStr.TabIndex = 25;
            this.tb_PortStr.Text = "fa";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(47, 285);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 12);
            this.label9.TabIndex = 24;
            this.label9.Text = "筛选端口特征字符串";
            // 
            // tb_findPort
            // 
            this.tb_findPort.Location = new System.Drawing.Point(166, 255);
            this.tb_findPort.Name = "tb_findPort";
            this.tb_findPort.Size = new System.Drawing.Size(274, 21);
            this.tb_findPort.TabIndex = 23;
            this.tb_findPort.Text = "show interface status";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(83, 258);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 22;
            this.label7.Text = "查询端口指令";
            // 
            // tb_enablePassword
            // 
            this.tb_enablePassword.Location = new System.Drawing.Point(166, 228);
            this.tb_enablePassword.Name = "tb_enablePassword";
            this.tb_enablePassword.Size = new System.Drawing.Size(137, 21);
            this.tb_enablePassword.TabIndex = 21;
            this.tb_enablePassword.Text = "password";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 231);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 12);
            this.label6.TabIndex = 20;
            this.label6.Text = "enable密码提示特征字符串";
            // 
            // cb_EnableNeedusername
            // 
            this.cb_EnableNeedusername.AutoSize = true;
            this.cb_EnableNeedusername.Location = new System.Drawing.Point(320, 203);
            this.cb_EnableNeedusername.Name = "cb_EnableNeedusername";
            this.cb_EnableNeedusername.Size = new System.Drawing.Size(120, 16);
            this.cb_EnableNeedusername.TabIndex = 19;
            this.cb_EnableNeedusername.Text = "enable需要用户名";
            this.cb_EnableNeedusername.UseVisualStyleBackColor = true;
            // 
            // tb_enableUsername
            // 
            this.tb_enableUsername.Location = new System.Drawing.Point(166, 201);
            this.tb_enableUsername.Name = "tb_enableUsername";
            this.tb_enableUsername.Size = new System.Drawing.Size(137, 21);
            this.tb_enableUsername.TabIndex = 18;
            this.tb_enableUsername.Text = "username";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-1, 207);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "enable用户名提示特征字符串";
            // 
            // tb_afterenable
            // 
            this.tb_afterenable.Location = new System.Drawing.Point(166, 164);
            this.tb_afterenable.Name = "tb_afterenable";
            this.tb_afterenable.Size = new System.Drawing.Size(137, 21);
            this.tb_afterenable.TabIndex = 16;
            this.tb_afterenable.Text = "#";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 167);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(149, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "enable后提示符特征字符串";
            // 
            // cb_NeeduserName
            // 
            this.cb_NeeduserName.AutoSize = true;
            this.cb_NeeduserName.Location = new System.Drawing.Point(320, 78);
            this.cb_NeeduserName.Name = "cb_NeeduserName";
            this.cb_NeeduserName.Size = new System.Drawing.Size(84, 16);
            this.cb_NeeduserName.TabIndex = 14;
            this.cb_NeeduserName.Text = "需要用户名";
            this.cb_NeeduserName.UseVisualStyleBackColor = true;
            // 
            // tb_beforeenable
            // 
            this.tb_beforeenable.Location = new System.Drawing.Point(166, 137);
            this.tb_beforeenable.Name = "tb_beforeenable";
            this.tb_beforeenable.Size = new System.Drawing.Size(137, 21);
            this.tb_beforeenable.TabIndex = 7;
            this.tb_beforeenable.Text = ">";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "enable前提示符特征字符串";
            // 
            // tb_password
            // 
            this.tb_password.Location = new System.Drawing.Point(166, 112);
            this.tb_password.Name = "tb_password";
            this.tb_password.Size = new System.Drawing.Size(137, 21);
            this.tb_password.TabIndex = 5;
            this.tb_password.Text = "password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "密码提示特征字符串";
            // 
            // tb_username
            // 
            this.tb_username.Location = new System.Drawing.Point(166, 79);
            this.tb_username.Name = "tb_username";
            this.tb_username.Size = new System.Drawing.Size(137, 21);
            this.tb_username.TabIndex = 3;
            this.tb_username.Text = "username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名提示特征字符串";
            // 
            // tb_TypeName
            // 
            this.tb_TypeName.Location = new System.Drawing.Point(166, 31);
            this.tb_TypeName.Name = "tb_TypeName";
            this.tb_TypeName.Size = new System.Drawing.Size(274, 21);
            this.tb_TypeName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(107, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "类型名称";
            // 
            // ConfigForSwitchType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 483);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_modify);
            this.Controls.Add(this.btn_add);
            this.Controls.Add(this.listBox1);
            this.Name = "ConfigForSwitchType";
            this.Text = "配置交换机类型";
            this.Load += new System.EventHandler(this.ConfigForSwitchType_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btn_add;
        private System.Windows.Forms.Button btn_modify;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_NeeduserName;
        private System.Windows.Forms.TextBox tb_beforeenable;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_username;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_TypeName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_PortStr;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tb_findPort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_enablePassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cb_EnableNeedusername;
        private System.Windows.Forms.TextBox tb_enableUsername;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_afterenable;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_disableStr;
        private System.Windows.Forms.Label label10;
    }
}