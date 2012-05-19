namespace DisableGetConfig
{
    partial class ConfigForItemAndSwitch
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_switchType = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cb_itemType = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tb_enablePassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_enableUsername = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_username = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_TypeName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_delete = new System.Windows.Forms.Button();
            this.btn_modify = new System.Windows.Forms.Button();
            this.btn_add = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tb_IPAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_IPAddress);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.cb_switchType);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.cb_itemType);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.tb_enablePassword);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.tb_enableUsername);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tb_password);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tb_username);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tb_TypeName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(257, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 296);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "交换机类型信息";
            // 
            // cb_switchType
            // 
            this.cb_switchType.FormattingEnabled = true;
            this.cb_switchType.Location = new System.Drawing.Point(127, 106);
            this.cb_switchType.Name = "cb_switchType";
            this.cb_switchType.Size = new System.Drawing.Size(274, 20);
            this.cb_switchType.TabIndex = 31;
            this.cb_switchType.SelectedIndexChanged += new System.EventHandler(this.cb_switchType_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(56, 109);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 12);
            this.label12.TabIndex = 30;
            this.label12.Text = "交换机类型";
            // 
            // cb_itemType
            // 
            this.cb_itemType.FormattingEnabled = true;
            this.cb_itemType.Location = new System.Drawing.Point(127, 69);
            this.cb_itemType.Name = "cb_itemType";
            this.cb_itemType.Size = new System.Drawing.Size(274, 20);
            this.cb_itemType.TabIndex = 29;
            this.cb_itemType.SelectedIndexChanged += new System.EventHandler(this.cb_itemType_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(56, 72);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 28;
            this.label11.Text = "选中项类型";
            // 
            // tb_enablePassword
            // 
            this.tb_enablePassword.Location = new System.Drawing.Point(172, 272);
            this.tb_enablePassword.Name = "tb_enablePassword";
            this.tb_enablePassword.Size = new System.Drawing.Size(137, 21);
            this.tb_enablePassword.TabIndex = 21;
            this.tb_enablePassword.Text = "password";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(101, 275);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 20;
            this.label6.Text = "enable密码";
            // 
            // tb_enableUsername
            // 
            this.tb_enableUsername.Location = new System.Drawing.Point(172, 245);
            this.tb_enableUsername.Name = "tb_enableUsername";
            this.tb_enableUsername.Size = new System.Drawing.Size(137, 21);
            this.tb_enableUsername.TabIndex = 18;
            this.tb_enableUsername.Text = "username";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(89, 248);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "enable用户名";
            // 
            // tb_password
            // 
            this.tb_password.Location = new System.Drawing.Point(172, 206);
            this.tb_password.Name = "tb_password";
            this.tb_password.Size = new System.Drawing.Size(137, 21);
            this.tb_password.TabIndex = 5;
            this.tb_password.Text = "password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 209);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "密码";
            // 
            // tb_username
            // 
            this.tb_username.Location = new System.Drawing.Point(172, 173);
            this.tb_username.Name = "tb_username";
            this.tb_username.Size = new System.Drawing.Size(137, 21);
            this.tb_username.TabIndex = 3;
            this.tb_username.Text = "username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(125, 176);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名";
            // 
            // tb_TypeName
            // 
            this.tb_TypeName.Location = new System.Drawing.Point(127, 17);
            this.tb_TypeName.Name = "tb_TypeName";
            this.tb_TypeName.Size = new System.Drawing.Size(274, 21);
            this.tb_TypeName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "类型名称";
            // 
            // btn_delete
            // 
            this.btn_delete.Location = new System.Drawing.Point(465, 12);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(75, 23);
            this.btn_delete.TabIndex = 8;
            this.btn_delete.Text = "删除";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // btn_modify
            // 
            this.btn_modify.Location = new System.Drawing.Point(384, 12);
            this.btn_modify.Name = "btn_modify";
            this.btn_modify.Size = new System.Drawing.Size(75, 23);
            this.btn_modify.TabIndex = 7;
            this.btn_modify.Text = "修改";
            this.btn_modify.UseVisualStyleBackColor = true;
            this.btn_modify.Click += new System.EventHandler(this.btn_modify_Click);
            // 
            // btn_add
            // 
            this.btn_add.Location = new System.Drawing.Point(303, 12);
            this.btn_add.Name = "btn_add";
            this.btn_add.Size = new System.Drawing.Size(75, 23);
            this.btn_add.TabIndex = 6;
            this.btn_add.Text = "增加";
            this.btn_add.UseVisualStyleBackColor = true;
            this.btn_add.Click += new System.EventHandler(this.btn_add_Click);
            // 
            // treeView1
            // 
            this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeView1.FullRowSelect = true;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(12, 12);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(239, 325);
            this.treeView1.TabIndex = 28;
            this.treeView1.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView1_DrawNode);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            // 
            // tb_IPAddress
            // 
            this.tb_IPAddress.Location = new System.Drawing.Point(172, 141);
            this.tb_IPAddress.Name = "tb_IPAddress";
            this.tb_IPAddress.Size = new System.Drawing.Size(137, 21);
            this.tb_IPAddress.TabIndex = 33;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(125, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 32;
            this.label4.Text = "IP地址";
            // 
            // ConfigForItemAndSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 374);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_modify);
            this.Controls.Add(this.btn_add);
            this.Name = "ConfigForItemAndSwitch";
            this.Text = "配置交换机（及组）";
            this.Load += new System.EventHandler(this.ConfigForItemAndSwitch_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_modify;
        private System.Windows.Forms.Button btn_add;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ComboBox cb_switchType;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cb_itemType;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tb_enablePassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_enableUsername;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_username;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_TypeName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_IPAddress;
        private System.Windows.Forms.Label label4;
    }
}