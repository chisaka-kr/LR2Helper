namespace LR2Helper_GV
{
    partial class mainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            Program.Delay(500); // 아무래도 핫키 등록이 해제되기 전에 아래 명령이 실행되면 exception이 발생하는 모양이니 임시방편으로 이렇게 해두자
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.comboBoxDSTtemplate = new System.Windows.Forms.ComboBox();
            this.buttonUnsupportskinmode = new System.Windows.Forms.Button();
            this.textBoxDSTX = new System.Windows.Forms.TextBox();
            this.textBoxDSTY = new System.Windows.Forms.TextBox();
            this.labelDSTY = new System.Windows.Forms.Label();
            this.labelDSTYtemplate = new System.Windows.Forms.Label();
            this.labelDSTX = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.labelHotkeyhelp = new System.Windows.Forms.Label();
            this.buttonTweetsend = new System.Windows.Forms.Button();
            this.textBoxTweettext = new System.Windows.Forms.TextBox();
            this.textBoxTwittertoken = new System.Windows.Forms.TextBox();
            this.buttonGettwittertoken = new System.Windows.Forms.Button();
            this.buttonOpentwittertoken = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pictureBoxSimpleresult = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSimpleresult)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 120);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(407, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Tag = "";
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(153, 17);
            this.toolStripStatusLabel1.Text = "Support: Twitter @Sena_JN";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(410, 120);
            this.tabControl1.TabIndex = 8;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            this.tabControl1.TabIndexChanged += new System.EventHandler(this.tabControl1_TabIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.comboBoxDSTtemplate);
            this.tabPage1.Controls.Add(this.buttonUnsupportskinmode);
            this.tabPage1.Controls.Add(this.textBoxDSTX);
            this.tabPage1.Controls.Add(this.textBoxDSTY);
            this.tabPage1.Controls.Add(this.labelDSTY);
            this.tabPage1.Controls.Add(this.labelDSTYtemplate);
            this.tabPage1.Controls.Add(this.labelDSTX);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(402, 94);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Green Number";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // comboBoxDSTtemplate
            // 
            this.comboBoxDSTtemplate.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.comboBoxDSTtemplate.FormattingEnabled = true;
            this.comboBoxDSTtemplate.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.comboBoxDSTtemplate.Location = new System.Drawing.Point(81, 13);
            this.comboBoxDSTtemplate.Name = "comboBoxDSTtemplate";
            this.comboBoxDSTtemplate.Size = new System.Drawing.Size(161, 20);
            this.comboBoxDSTtemplate.TabIndex = 11;
            this.comboBoxDSTtemplate.Text = "Select skin..";
            this.comboBoxDSTtemplate.TextChanged += new System.EventHandler(this.comboBoxDSTtemplate_TextChanged);
            // 
            // buttonUnsupportskinmode
            // 
            this.buttonUnsupportskinmode.Enabled = false;
            this.buttonUnsupportskinmode.Location = new System.Drawing.Point(248, 13);
            this.buttonUnsupportskinmode.Name = "buttonUnsupportskinmode";
            this.buttonUnsupportskinmode.Size = new System.Drawing.Size(134, 46);
            this.buttonUnsupportskinmode.TabIndex = 13;
            this.buttonUnsupportskinmode.Text = "Enable unsupported skin mode";
            this.buttonUnsupportskinmode.UseVisualStyleBackColor = true;
            this.buttonUnsupportskinmode.Click += new System.EventHandler(this.buttonUnsupportskinmode_Click);
            // 
            // textBoxDSTX
            // 
            this.textBoxDSTX.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxDSTX.Location = new System.Drawing.Point(68, 37);
            this.textBoxDSTX.Name = "textBoxDSTX";
            this.textBoxDSTX.Size = new System.Drawing.Size(55, 22);
            this.textBoxDSTX.TabIndex = 14;
            this.textBoxDSTX.Text = "321";
            // 
            // textBoxDSTY
            // 
            this.textBoxDSTY.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxDSTY.Location = new System.Drawing.Point(187, 37);
            this.textBoxDSTY.Name = "textBoxDSTY";
            this.textBoxDSTY.Size = new System.Drawing.Size(55, 22);
            this.textBoxDSTY.TabIndex = 8;
            this.textBoxDSTY.Text = "321";
            // 
            // labelDSTY
            // 
            this.labelDSTY.AutoSize = true;
            this.labelDSTY.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDSTY.Location = new System.Drawing.Point(137, 43);
            this.labelDSTY.Name = "labelDSTY";
            this.labelDSTY.Size = new System.Drawing.Size(38, 12);
            this.labelDSTY.TabIndex = 9;
            this.labelDSTY.Text = "DST_Y";
            // 
            // labelDSTYtemplate
            // 
            this.labelDSTYtemplate.AutoSize = true;
            this.labelDSTYtemplate.Location = new System.Drawing.Point(17, 17);
            this.labelDSTYtemplate.Name = "labelDSTYtemplate";
            this.labelDSTYtemplate.Size = new System.Drawing.Size(58, 12);
            this.labelDSTYtemplate.TabIndex = 12;
            this.labelDSTYtemplate.Text = "Template";
            // 
            // labelDSTX
            // 
            this.labelDSTX.AutoSize = true;
            this.labelDSTX.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDSTX.Location = new System.Drawing.Point(18, 43);
            this.labelDSTX.Name = "labelDSTX";
            this.labelDSTX.Size = new System.Drawing.Size(38, 12);
            this.labelDSTX.TabIndex = 10;
            this.labelDSTX.Text = "DST_X";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.labelHotkeyhelp);
            this.tabPage2.Controls.Add(this.buttonTweetsend);
            this.tabPage2.Controls.Add(this.textBoxTweettext);
            this.tabPage2.Controls.Add(this.textBoxTwittertoken);
            this.tabPage2.Controls.Add(this.buttonGettwittertoken);
            this.tabPage2.Controls.Add(this.buttonOpentwittertoken);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(402, 94);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Twitter";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // labelHotkeyhelp
            // 
            this.labelHotkeyhelp.AutoSize = true;
            this.labelHotkeyhelp.Location = new System.Drawing.Point(189, 46);
            this.labelHotkeyhelp.Name = "labelHotkeyhelp";
            this.labelHotkeyhelp.Size = new System.Drawing.Size(176, 12);
            this.labelHotkeyhelp.TabIndex = 26;
            this.labelHotkeyhelp.Text = "Tweet upload : F11 (on result)";
            // 
            // buttonTweetsend
            // 
            this.buttonTweetsend.Enabled = false;
            this.buttonTweetsend.Location = new System.Drawing.Point(343, 64);
            this.buttonTweetsend.Name = "buttonTweetsend";
            this.buttonTweetsend.Size = new System.Drawing.Size(41, 23);
            this.buttonTweetsend.TabIndex = 25;
            this.buttonTweetsend.Text = "▶";
            this.buttonTweetsend.UseVisualStyleBackColor = true;
            // 
            // textBoxTweettext
            // 
            this.textBoxTweettext.Location = new System.Drawing.Point(9, 65);
            this.textBoxTweettext.Name = "textBoxTweettext";
            this.textBoxTweettext.Size = new System.Drawing.Size(330, 21);
            this.textBoxTweettext.TabIndex = 24;
            // 
            // textBoxTwittertoken
            // 
            this.textBoxTwittertoken.Location = new System.Drawing.Point(9, 40);
            this.textBoxTwittertoken.Name = "textBoxTwittertoken";
            this.textBoxTwittertoken.Size = new System.Drawing.Size(97, 21);
            this.textBoxTwittertoken.TabIndex = 23;
            // 
            // buttonGettwittertoken
            // 
            this.buttonGettwittertoken.Enabled = false;
            this.buttonGettwittertoken.Location = new System.Drawing.Point(110, 39);
            this.buttonGettwittertoken.Name = "buttonGettwittertoken";
            this.buttonGettwittertoken.Size = new System.Drawing.Size(77, 23);
            this.buttonGettwittertoken.TabIndex = 22;
            this.buttonGettwittertoken.Text = "Enter";
            this.buttonGettwittertoken.UseVisualStyleBackColor = true;
            this.buttonGettwittertoken.Click += new System.EventHandler(this.buttonGettoken_Click);
            // 
            // buttonOpentwittertoken
            // 
            this.buttonOpentwittertoken.Location = new System.Drawing.Point(8, 6);
            this.buttonOpentwittertoken.Name = "buttonOpentwittertoken";
            this.buttonOpentwittertoken.Size = new System.Drawing.Size(179, 31);
            this.buttonOpentwittertoken.TabIndex = 21;
            this.buttonOpentwittertoken.Text = "Twitter Login";
            this.buttonOpentwittertoken.UseVisualStyleBackColor = true;
            this.buttonOpentwittertoken.Click += new System.EventHandler(this.buttonOpentwittertoken_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pictureBoxSimpleresult);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(402, 94);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Simple Result";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pictureBoxSimpleresult
            // 
            this.pictureBoxSimpleresult.Location = new System.Drawing.Point(0, 3);
            this.pictureBoxSimpleresult.Name = "pictureBoxSimpleresult";
            this.pictureBoxSimpleresult.Size = new System.Drawing.Size(400, 600);
            this.pictureBoxSimpleresult.TabIndex = 0;
            this.pictureBoxSimpleresult.TabStop = false;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(407, 142);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "mainForm";
            this.Text = "LR2Helper ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_FormClosing);
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSimpleresult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

                private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.ComboBox comboBoxDSTtemplate;
        private System.Windows.Forms.Button buttonUnsupportskinmode;
        private System.Windows.Forms.TextBox textBoxDSTX;
        private System.Windows.Forms.TextBox textBoxDSTY;
        private System.Windows.Forms.Label labelDSTY;
        private System.Windows.Forms.Label labelDSTYtemplate;
        private System.Windows.Forms.Label labelDSTX;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label labelHotkeyhelp;
        private System.Windows.Forms.Button buttonTweetsend;
        private System.Windows.Forms.TextBox textBoxTweettext;
        private System.Windows.Forms.TextBox textBoxTwittertoken;
        private System.Windows.Forms.Button buttonGettwittertoken;
        private System.Windows.Forms.Button buttonOpentwittertoken;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.PictureBox pictureBoxSimpleresult;
    }
}

