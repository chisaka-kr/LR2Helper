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
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBoxDSTY = new System.Windows.Forms.TextBox();
            this.labelDSTY = new System.Windows.Forms.Label();
            this.comboBoxDSTtemplate = new System.Windows.Forms.ComboBox();
            this.labelDSTYtemplate = new System.Windows.Forms.Label();
            this.buttonUnsupportskinmode = new System.Windows.Forms.Button();
            this.textBoxDSTX = new System.Windows.Forms.TextBox();
            this.labelDSTX = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 72);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(393, 22);
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
            // textBoxDSTY
            // 
            this.textBoxDSTY.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxDSTY.Location = new System.Drawing.Point(185, 36);
            this.textBoxDSTY.Name = "textBoxDSTY";
            this.textBoxDSTY.Size = new System.Drawing.Size(55, 22);
            this.textBoxDSTY.TabIndex = 2;
            this.textBoxDSTY.Text = "321";
            // 
            // labelDSTY
            // 
            this.labelDSTY.AutoSize = true;
            this.labelDSTY.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDSTY.Location = new System.Drawing.Point(135, 42);
            this.labelDSTY.Name = "labelDSTY";
            this.labelDSTY.Size = new System.Drawing.Size(38, 12);
            this.labelDSTY.TabIndex = 3;
            this.labelDSTY.Text = "DST_Y";
            // 
            // comboBoxDSTtemplate
            // 
            this.comboBoxDSTtemplate.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.comboBoxDSTtemplate.FormattingEnabled = true;
            this.comboBoxDSTtemplate.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.comboBoxDSTtemplate.Location = new System.Drawing.Point(79, 12);
            this.comboBoxDSTtemplate.Name = "comboBoxDSTtemplate";
            this.comboBoxDSTtemplate.Size = new System.Drawing.Size(161, 20);
            this.comboBoxDSTtemplate.TabIndex = 4;
            this.comboBoxDSTtemplate.Text = "Select skin..";
            this.comboBoxDSTtemplate.TextChanged += new System.EventHandler(this.comboBoxDSTtemplate_TextChanged);
            // 
            // labelDSTYtemplate
            // 
            this.labelDSTYtemplate.AutoSize = true;
            this.labelDSTYtemplate.Location = new System.Drawing.Point(15, 16);
            this.labelDSTYtemplate.Name = "labelDSTYtemplate";
            this.labelDSTYtemplate.Size = new System.Drawing.Size(58, 12);
            this.labelDSTYtemplate.TabIndex = 5;
            this.labelDSTYtemplate.Text = "Template";
            this.labelDSTYtemplate.Click += new System.EventHandler(this.labelDSTYtemplate_Click);
            // 
            // buttonUnsupportskinmode
            // 
            this.buttonUnsupportskinmode.Enabled = false;
            this.buttonUnsupportskinmode.Location = new System.Drawing.Point(246, 12);
            this.buttonUnsupportskinmode.Name = "buttonUnsupportskinmode";
            this.buttonUnsupportskinmode.Size = new System.Drawing.Size(134, 46);
            this.buttonUnsupportskinmode.TabIndex = 6;
            this.buttonUnsupportskinmode.Text = "Enable unsupported skin mode";
            this.buttonUnsupportskinmode.UseVisualStyleBackColor = true;
            this.buttonUnsupportskinmode.Click += new System.EventHandler(this.buttonUnsupportskinmode_Click);
            // 
            // textBoxDSTX
            // 
            this.textBoxDSTX.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxDSTX.Location = new System.Drawing.Point(66, 36);
            this.textBoxDSTX.Name = "textBoxDSTX";
            this.textBoxDSTX.Size = new System.Drawing.Size(55, 22);
            this.textBoxDSTX.TabIndex = 7;
            this.textBoxDSTX.Text = "321";
            // 
            // labelDSTX
            // 
            this.labelDSTX.AutoSize = true;
            this.labelDSTX.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDSTX.Location = new System.Drawing.Point(16, 42);
            this.labelDSTX.Name = "labelDSTX";
            this.labelDSTX.Size = new System.Drawing.Size(38, 12);
            this.labelDSTX.TabIndex = 3;
            this.labelDSTX.Text = "DST_X";
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(393, 94);
            this.Controls.Add(this.textBoxDSTX);
            this.Controls.Add(this.buttonUnsupportskinmode);
            this.Controls.Add(this.labelDSTYtemplate);
            this.Controls.Add(this.comboBoxDSTtemplate);
            this.Controls.Add(this.labelDSTX);
            this.Controls.Add(this.labelDSTY);
            this.Controls.Add(this.textBoxDSTY);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "mainForm";
            this.Text = "LR2Helper ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_FormClosing);
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

                private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox textBoxDSTY;
        private System.Windows.Forms.Label labelDSTY;
        private System.Windows.Forms.Label labelDSTYtemplate;
        public System.Windows.Forms.ComboBox comboBoxDSTtemplate;
        private System.Windows.Forms.Button buttonUnsupportskinmode;
        private System.Windows.Forms.TextBox textBoxDSTX;
        private System.Windows.Forms.Label labelDSTX;
    }
}

