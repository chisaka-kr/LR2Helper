using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
//ini 사용을 위한 include지만 안 쓰므로 주석처리
//using System.Runtime.InteropServices;

namespace LR2Helper_GV {
    public partial class mainForm : Form {

        delegate void SetTextCallback(string formName, string text);
        delegate Control RecursiveFindCallback(Control ParentCntl, string NameToSearch);
        public mainForm() {

            InitializeComponent();

            Thread th_initFirstprocess = new Thread(new ThreadStart(Program.lr2helper.initFirstprocess));
            th_initFirstprocess.Start();

            this.Text = "LR2Helper" + Program.lr2helper.prog_version + " [build:" + Program.lr2helper.prog_build + "]";
            buttonUnsupportskinmode.Enabled = true;
            /* ini로 설정 저장. 사용하지 않음
            int Program.lr2helper.getini_status = GetPrivateProfileString("setting", "DSTY", "",temp,255, setting_path);
            if (Program.lr2helper.getini_status > 0)
            {
                textBoxDSTY.Text = temp.ToString();
            }
            */
        }

        internal void init() {
            Program.lr2helper.GetXML();

            //form setting (설정값 등)
            comboBoxDSTtemplate.DisplayMember = "Key";
            comboBoxDSTtemplate.ValueMember = "Value";
            comboBoxDSTtemplate.DataSource = new BindingSource(Program.lr2helper.skin_template_dsty, null);

            Program.lr2helper.GetTwitterid();
            Program.lr2helper.InitRenameScreenshot();

        }
        internal IntPtr GetHandle() {
            return this.Handle;
        }

        public void SetFormTwitterLoggedIn(string buttonText, string TwitterID) {
            buttonGettwittertoken.Enabled = false;
            buttonOpentwittertoken.Enabled = false;
            textBoxTwittertoken.Enabled = false;
            //buttonTweetsend.Enabled = true;
            this.SetFormText("buttonOpentwittertoken", "Already logged in");
            this.SetFormText("textBoxTwittertoken", TwitterID);
        }

        public Bitmap GetSimpleResultPicture() {
            return (Bitmap)pictureBoxSimpleresult.Image;
        }
        public void LoadSimpleResultPicture(string path) {
            pictureBoxSimpleresult.Load(path);
        }
        public void SetSimpleResultPicture(Bitmap image) {
            pictureBoxSimpleresult.Image = image;
        }
        public void SaveSimpleResultPicture(string path, System.Drawing.Imaging.ImageFormat format) {
            pictureBoxSimpleresult.Image.Save(path, format);
        }

        public void SetFormText(string formName, string text) {

            var form = this.Controls.Find(formName, true)[0];
            
            if (form == null) { return; }
            if (form.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetFormText);
                this.Invoke(d, new object[] { formName, text });
            } else {
                form.Text = text;
            }

        }

        public string GetFormText(string formName) {
            var form = this.Controls[formName];

            if (form == null) { return ""; }

            return form.Text;
        }
        public void SetTooltipStrip(string text) {
            try {
                toolStripStatusLabel1.Text = text;
            } catch (Exception) { return; }

        }
        public string GetTooltipStrip() {
            return toolStripStatusLabel1.Text;
        }

        //다양한 이벤트들
        private void mainForm_Load(object sender, EventArgs e) {

        }
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e) {
            Program.lr2helper.flag_interrupt = 1;
            Program.UnregisterHotKey(Program.runningForm.GetHandle(), 0);
            Program.UnregisterHotKey(Program.runningForm.GetHandle(), 1);
        }
        private void labelDSTYtemplate_Click(object sender, EventArgs e) {

        }
        private void comboBoxDSTtemplate_TextChanged(object sender, EventArgs e) {
            try {
                if (comboBoxDSTtemplate.Text != "---") {
                    textBoxDSTY.Text = Program.lr2helper.skin_template_dsty[comboBoxDSTtemplate.Text];
                    textBoxDSTX.Text = Program.lr2helper.skin_template_dstx[comboBoxDSTtemplate.Text];
                }
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Key not found!!"; //사용자가 임의로 텍스트를 입력하였을 때 발생
            }

        }
        private void buttonUnsupportskinmode_Click(object sender, EventArgs e) {
            if (Program.lr2helper.LR2value.baseaddr > 0) // 성공적으로 LR2가 로드되었을 때만 작동하도록
            {
                Program.lr2helper.DelFPSLogic();

                Program.lr2helper.flag_unsupportedskinmode = 1;
                buttonUnsupportskinmode.Enabled = false;
                buttonUnsupportskinmode.Text = "Enabled. check LR2 FPS value (toggle F7)";
            }
        }
        private void buttonOpentwittertoken_Click(object sender, EventArgs e) {
            // Go to the URL so that Twitter authenticates the user and gives him a PIN code.
            Process.Start(Program.lr2helper.authenticationContext.AuthorizationURL);
            buttonOpentwittertoken.Enabled = false;
            buttonGettwittertoken.Enabled = true;
        }
        private void buttonGettoken_Click(object sender, EventArgs e) {
            if (textBoxTwittertoken.Text.Length == 7) {
                try {
                    Program.lr2helper.userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(textBoxTwittertoken.Text, Program.lr2helper.authenticationContext);
                    Auth.SetCredentials(Program.lr2helper.userCredentials);
                    Program.lr2helper.authenticatedUser = User.GetAuthenticatedUser();
                    if (Convert.ToInt64(Program.lr2helper.authenticatedUser.IdStr) > 0) {
                        buttonGettwittertoken.Enabled = false;
                        buttonOpentwittertoken.Enabled = false;
                        textBoxTwittertoken.Enabled = false;
                        //buttonTweetsend.Enabled = true;
                        buttonOpentwittertoken.Text = "Login Sucess";
                        textBoxTwittertoken.Text = "@" + Program.lr2helper.authenticatedUser.ScreenName;

                        //세팅값 저장
                        XmlDocument setting_update = new XmlDocument();
                        setting_update.Load(@Program.lr2helper.setting_path);
                        setting_update.GetElementsByTagName("auth_key")[0].InnerText = Program.lr2helper.userCredentials.AccessToken;
                        setting_update.GetElementsByTagName("auth_secret")[0].InnerText = Program.lr2helper.userCredentials.AccessTokenSecret;
                        setting_update.Save(@Program.lr2helper.setting_path);
                    }
                } catch (Exception err) {
                    Program.lr2helper.WriteLog(err.ToString());
                }

            }
        }
        private void tabControl1_TabIndexChanged(object sender, EventArgs e) {

        }
        private void tabControl1_Selected(object sender, TabControlEventArgs e) {
            try {
                if (e.TabPageIndex == 2) {
                    mainForm.ActiveForm.Width = 423;
                    mainForm.ActiveForm.Height = 690;
                    tabControl1.Width = 423;
                    tabControl1.Height = 630;
                } else {
                    mainForm.ActiveForm.Width = 423;
                    mainForm.ActiveForm.Height = 180;
                    tabControl1.Width = 423;
                    tabControl1.Height = 120;
                }
            } catch (Exception) {

            }

        }
        protected override void WndProc(ref System.Windows.Forms.Message m) { // 단축키 등록
            base.WndProc(ref m);

            if (m.Msg == 0x0312) {
                try {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                    Program.KeyModifier modifier = (Program.KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                    int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.
                    if (id == 0) {
                        Program.lr2helper.SendTweet(this.textBoxTweettext.Text);
                    } else if (id == 1) {
                        Program.lr2helper.GetSongstatus(Program.lr2helper.tweet_template_sub);
                        Program.lr2helper.SendTweet(this.textBoxTweettext.Text);
                    } else if (id == 2) {
                        Program.lr2helper.SetEventRenameScreenshot();
                    } else if (id == 3) {
                        Program.lr2helper.SetEventRenameScreenshot();
                    }
                } catch (Exception) { return; }
            }
        }

    }

}

