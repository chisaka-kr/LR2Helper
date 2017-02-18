﻿using Binarysharp.MemoryManagement;
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
        static string prog_version = "L2.0.2";
        static string prog_build = "170218:0 alpha";

        IntPtr prog_baseaddr; // 보통 0x400000;
        IntPtr vmem_getbaseaddr_asm; // base address를 빼올 코드 
        IntPtr vmem_getbaseaddr_reg; // base address를 저장할 공간
        IntPtr vmem_dstnumber_300_asm;
        IntPtr vmem_dstnumber_300_reg;

        ushort flag_interrupt = 0;
        ushort flag_unsupportedskinmode = 0;
        ushort flag_twitterlogon = 0;
        ushort flag_resolution_manual_mode = 0;

        string process_name;
        string process_path;
        string[] screenshot_files;

        MemorySharp sharp;
        LR2value LR2value = new LR2value();

        /* ini로 설정 저장. 사용하지 않음
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        */
        //private string setting_path = Application.StartupPath + "\\lr2helper.ini";

        //xml에 저장
        private string setting_path = Application.StartupPath + "\\lr2helper-setting.xml";
        static Dictionary<String, String> skin_template_dstx = new Dictionary<String, String>();
        static Dictionary<String, String> skin_template_dsty = new Dictionary<String, String>();

        //글로벌 단축키 지정을 위한 dllimport + 키보드 입력
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void keybd_event(uint vk, uint scan, uint flags, uint extraInfo);

        enum KeyModifier {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public mainForm() {
            InitializeComponent();
            Thread th_initFirstprocess = new Thread(new ThreadStart(initFirstprocess));
            th_initFirstprocess.Start();

            //form setting (설정값 등)
            getXML();

            comboBoxDSTtemplate.DisplayMember = "Key";
            comboBoxDSTtemplate.ValueMember = "Value";
            comboBoxDSTtemplate.DataSource = new BindingSource(skin_template_dsty, null);

            getTwitterid();

            this.Text = "LR2Helper" + prog_version + " [build:" + prog_build + "]";
            buttonUnsupportskinmode.Enabled = true;
            /* ini로 설정 저장. 사용하지 않음
            int getini_status = GetPrivateProfileString("setting", "DSTY", "",temp,255, setting_path);
            if (getini_status > 0)
            {
                textBoxDSTY.Text = temp.ToString();
            }
            */
        }


        void initFirstprocess() {
            while (true) {
                try {
                    //ini로 설정 저장. 사용하지 않음
                    //WritePrivateProfileString("setting", "DSTY", textBoxDSTY.Text.ToString(), iniPath);
                    //xml로 설정 저장
                    XmlDocument setting_update = new XmlDocument();
                    setting_update.Load(@setting_path);
                    setting_update.GetElementsByTagName("dst_y")[0].InnerText = textBoxDSTY.Text;
                    setting_update.GetElementsByTagName("dst_x")[0].InnerText = textBoxDSTX.Text;
                    setting_update.Save(@setting_path);
                    var process_id = Process.GetProcessesByName("LR2body");
                    
                    String[] process_name_list = {
                          "LRHbody",
                          "LR2body_FS10",
                          "LRHbody_FS10",
                          "LR2body_FS9.1",
                          "LRHbody_FS9.1"
                    };
                    if (process_id.Length == 0) {
                        for (var i = 0; i < process_name_list.Length; i++) {
                            process_id = Process.GetProcessesByName(process_name_list[i]);
                            if (process_id.Length > 0) {
                                process_name = process_name_list[i];
                                break;
                            }
                        }
                    }
                    //var process_id = ApplicationFinder.FromProcessName("LRHbody_FS9.1").First();
                    if (process_id.Length > 0) {//process id가 0이 아닐 시

                        //IntPtr address = new IntPtr(0x400000);
                        //IntPtr address2 = new IntPtr(0x400010);
                        //var sharp = new MemorySharp(); //MemorySharp로 process hook
                        sharp = new MemorySharp(process_id[0]);
                        if (sharp.IsRunning == true) {
                            toolStripStatusLabel1.Text = "LR2 detected.";
                            
                            process_path = process_id[0].MainModule.FileName.Replace(process_name+".exe",""); ;

                            prog_baseaddr = new IntPtr(sharp.Modules.RemoteModules.First().BaseAddress.ToInt32());

                            //이하 LR2 base address (Hi-SPEED address) 값을 빼오기 위한 code injection

                            vmem_getbaseaddr_asm = sharp.Memory.Allocate(256).BaseAddress; //asmcode memory alloc
                            vmem_getbaseaddr_reg = sharp.Memory.Allocate(4).BaseAddress; //register memory alloc

                            //toolStripStatusLabel1.Text = vmem_getbaseaddr_asm.ToString(); 디버깅
                            //sharp.Write<int>(virtual_addr, 123456, false); 디버깅
                            //int integers = sharp.Read<int>(address,false); 디버깅

                            string[] str_vmem_getbaseaddr_asm =
                            {
                        "mov ebx,1",
                        "cmp eax,100000",
                        "jl "+(prog_baseaddr+0xe79b),
                        "cmp eax,2000000",
                        "jg "+(prog_baseaddr+0xe79b),
                        "mov ["+vmem_getbaseaddr_reg+"],eax",
                        "jmp "+(prog_baseaddr+0xe79b)
                    }; // 삽입할 code
                            string[] str_vmem_getbaseaddr_jmp =
                            {
                        "jmp "+vmem_getbaseaddr_asm
                    }; // 원래 코드에서 vmem_getbaseaddr_asm으로 점프하기 위한 코드
                            sharp.Assembly.Inject(str_vmem_getbaseaddr_asm, vmem_getbaseaddr_asm); // injection
                            sharp.Assembly.Inject(str_vmem_getbaseaddr_jmp, prog_baseaddr + 0xe796); // injection

                            toolStripStatusLabel1.Text = "LR2 detected.";

                            Thread th_getBaseaddr = new Thread(new ThreadStart(getBaseaddr));
                            th_getBaseaddr.Start();
                            Thread th_getGreenvalue = new Thread(new ThreadStart(startWork));
                            th_getGreenvalue.Start();
                            break;
                        } else {
                            for (uint i = 0; i < 5; i++) {
                                //attach 실패한 경우 (isRunning = false)
                                toolStripStatusLabel1.Text = "LR2body is running but is not responding. Try to reattach in " + (5 - i) + "s";
                                Thread.Sleep(1000);
                            }
                        }

                    } else {
                        for (uint i = 0; i < 5; i++) {
                            //LR2body를 찾지 못한 경우 (process_id = null)
                            toolStripStatusLabel1.Text = "Can't find LR2body process. Try to reattach in " + (5 - i) + "s";
                            Thread.Sleep(1000);
                        }
                    }

                    if (flag_interrupt == 1) {
                        break;
                    }
                } catch (Exception) {
                    toolStripStatusLabel1.Text = "Unexpected error occured. program load failed.";
                    Thread.Sleep(1000);
                    break;
                }
            }

        }

        public void getBaseaddr() {
            Thread.Sleep(1500);

            toolStripStatusLabel1.Text = "waiting for load music select screen.";

            while (true) {
                try {
                    //ini로 설정 저장. 사용하지 않음
                    //WritePrivateProfileString("setting", "DSTY", textBoxDSTY.Text.ToString(), setting_path); 

                    int baseaddr = sharp.Read<int>(vmem_getbaseaddr_reg, false);
                    if (toolStripStatusLabel1.Text.Length > 50) { toolStripStatusLabel1.Text = "waiting for load music select screen."; }
                    toolStripStatusLabel1.Text += ".";
                    if (baseaddr > 0) {
                        LR2value.baseaddr = baseaddr;

                        toolStripStatusLabel1.Text = "LR2 attach complete.";
                        Thread.Sleep(1000);
                        break;
                    }
                    if (!sharp.IsRunning) //LR2 프로세스 체크
                    {
                        Thread th_initFirstprocess = new Thread(new ThreadStart(initFirstprocess));
                        toolStripStatusLabel1.Text = "LR2 process is terminated!";
                        Thread.Sleep(2000);
                        th_initFirstprocess.Start();
                        break;
                    }
                    if (flag_interrupt == 1) {
                        break;
                    }
                    Thread.Sleep(1000);
                } catch (Exception) { }
            }
        }
        public void startWork() { // 모든 리프레시 작업은 여기서 (스레드를 나눌 필요가 있으면 다른 방법으로)
            initGreenvalue();

            while (true) {
                try {
                    if (LR2value.baseaddr > 0) {
                        getGreenvalue();
                        getSongstatus();
                    }
                } catch (Exception) {
                    toolStripStatusLabel1.Text = "Critical error occured. Please restart LR2Helper.";
                }
                if (flag_interrupt == 1) {
                    break;
                }
                Thread.Sleep(16);
            }
        }
        private void getXML() {
            try {
                XmlDocument setting_file = new XmlDocument();
                setting_file.Load(@setting_path);
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Failed load setting file. Try to create it.";
                if (makeXML() == false) {
                    toolStripStatusLabel1.Text = "Failed create setting file.";
                }

            } finally {
                skin_template_dstx["---"] = textBoxDSTX.Text;
                skin_template_dsty["---"] = textBoxDSTY.Text;
                using (XmlReader setting_root = XmlReader.Create(@setting_path)) {
                    while (setting_root.Read()) {
                        if (setting_root.IsStartElement()) {
                            switch (setting_root.Name) {
                                case "resolution_manual_mode":
                                    if (setting_root.Read()) {
                                        flag_resolution_manual_mode = Convert.ToUInt16(setting_root.Value.Trim());
                                    }
                                    break;
                                case "resolution_height":
                                    if (setting_root.Read()) {
                                        LR2value.resolution_height = Convert.ToUInt16(setting_root.Value.Trim());
                                    }
                                    break;
                                case "resolution_width":
                                    if (setting_root.Read()) {
                                        LR2value.resolution_width = Convert.ToUInt16(setting_root.Value.Trim());
                                    }
                                    break;
                                case "monitor_height":
                                    if (setting_root.Read()) {
                                        LR2value.window_height = Convert.ToUInt16(setting_root.Value.Trim());
                                    }
                                    break;
                                case "monitor_width":
                                    if (setting_root.Read()) {
                                        LR2value.window_width = Convert.ToUInt16(setting_root.Value.Trim());
                                    }
                                    break;
                                case "tweet_template":
                                    if (setting_root.Read()) {
                                        tweet_template = setting_root.Value.Trim();
                                    }
                                    break;
                                    break;
                                case "dst_x":
                                    if (setting_root.Read()) {
                                        textBoxDSTX.Text = setting_root.Value.Trim();
                                    }
                                    break;
                                case "dst_y":
                                    if (setting_root.Read()) {
                                        textBoxDSTY.Text = setting_root.Value.Trim();
                                    }
                                    break;
                                case "skin":
                                    String skin_dstx = setting_root["dst_x"];
                                    String skin_dsty = setting_root["dst_y"];
                                    if (setting_root.Read()) {
                                        skin_template_dstx[setting_root.Value.Trim()] = skin_dstx;
                                        skin_template_dsty[setting_root.Value.Trim()] = skin_dsty;
                                    }
                                    break;
                                case "auth_key":
                                    if (setting_root.Read()) {
                                        auth_key = setting_root.Value.Trim();
                                    }
                                    break;
                                case "auth_secret":
                                    if (setting_root.Read()) {
                                        auth_secret = setting_root.Value.Trim();
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private bool makeXML() {
            try {
                XmlWriterSettings setting_make_settings = new XmlWriterSettings();
                setting_make_settings.Indent = true;
                setting_make_settings.IndentChars = "\t";
                using (XmlWriter setting_make = XmlWriter.Create(@setting_path, setting_make_settings)) {
                    setting_make.WriteStartDocument();
                    setting_make.WriteStartElement("lr2helper");
                    setting_make.WriteStartElement("setting");
                    setting_make.WriteElementString("dst_x", "288");
                    setting_make.WriteElementString("dst_y", "482");
                    setting_make.WriteElementString("resolution_manual_mode", "1");
                    setting_make.WriteElementString("resolution_width", "1280");
                    setting_make.WriteElementString("resolution_height", "720");
                    setting_make.WriteElementString("monitor_width", "1280");
                    setting_make.WriteElementString("monitor_height", "720");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin_template");

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "194");
                    setting_make.WriteAttributeString("dst_y", "315");
                    setting_make.WriteString("LR2 Default");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "144");
                    setting_make.WriteAttributeString("dst_y", "321");
                    setting_make.WriteString("OADX+ AC SKIN(SD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "233");
                    setting_make.WriteAttributeString("dst_y", "321");
                    setting_make.WriteString("OADX+ CS SKIN(SD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "288");
                    setting_make.WriteAttributeString("dst_y", "482");
                    setting_make.WriteString("WMIX_HD AC (HD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "375");
                    setting_make.WriteAttributeString("dst_y", "482");
                    setting_make.WriteString("WMIX_HD WIDE(HD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "288");
                    setting_make.WriteAttributeString("dst_y", "482");
                    setting_make.WriteString("Endless Circulation SE(HD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "431");
                    setting_make.WriteAttributeString("dst_y", "713");
                    setting_make.WriteString("LITONE5 AC(FHD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "513");
                    setting_make.WriteAttributeString("dst_y", "836");
                    setting_make.WriteString("LITONE5 HALF WIDE(FHD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("twitter");
                    setting_make.WriteElementString("auth_key", "");
                    setting_make.WriteElementString("auth_secret", "");
                    setting_make.WriteElementString("tweet_template", "#MUSIC_NAME# (#MUSIC_DIFF_LEVEL#)を #CLEAR_TYPE#しました!");
                    
                    setting_make.WriteEndElement();

                    setting_make.WriteEndElement();
                    setting_make.WriteEndElement();
                    setting_make.WriteEndDocument();
                    setting_make.Flush();
                    setting_make.Close();
                    return true;
                }
            } catch (Exception) {
                return false;
            }

        }

        //다양한 이벤트들
        private void mainForm_Load(object sender, EventArgs e) {

        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e) {
            flag_interrupt = 1;
            //좀 더 좋은 방법을 찾는게 좋겠다
        }

        private void labelDSTYtemplate_Click(object sender, EventArgs e) {

        }

        private void comboBoxDSTtemplate_TextChanged(object sender, EventArgs e) {
            try {
                if (comboBoxDSTtemplate.Text != "---") {
                    textBoxDSTY.Text = skin_template_dsty[comboBoxDSTtemplate.Text];
                    textBoxDSTX.Text = skin_template_dstx[comboBoxDSTtemplate.Text];
                }
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Key not found!!"; //사용자가 임의로 텍스트를 입력하였을 때 발생
            }

        }
        private void buttonUnsupportskinmode_Click(object sender, EventArgs e) {
            if (LR2value.baseaddr > 0) // 성공적으로 LR2가 로드되었을 때만 작동하도록
            {
                sharp.Assembly.Inject(
                new[]
                    {
                                "nop",
                                "nop",
                                "nop",
                                "nop",
                                "nop",
                                "nop"
                    },
                prog_baseaddr + 0xb6ab7); // FPS 숫자 갱신을 무효화
                flag_unsupportedskinmode = 1;
                buttonUnsupportskinmode.Enabled = false;
                buttonUnsupportskinmode.Text = "Enabled. check LR2 FPS value (toggle F7)";
            }
        }

        private void buttonOpentwittertoken_Click(object sender, EventArgs e) {
            // Go to the URL so that Twitter authenticates the user and gives him a PIN code.
            Process.Start(authenticationContext.AuthorizationURL);
            buttonOpentwittertoken.Enabled = false;
            buttonGettwittertoken.Enabled = true;
        }

        private void buttonGettoken_Click(object sender, EventArgs e) {
            if (textBoxTwittertoken.Text.Length == 7) {


                    userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(textBoxTwittertoken.Text, authenticationContext);
                    Auth.SetCredentials(userCredentials);
                    authenticatedUser = User.GetAuthenticatedUser();
                    if (Convert.ToInt64(authenticatedUser.IdStr) > 0) {
                        flag_twitterlogon = 1;
                        buttonGettwittertoken.Enabled = false;
                        buttonOpentwittertoken.Enabled = false;
                        textBoxTwittertoken.Enabled = false;
                        buttonTweetsend.Enabled = true;
                        textBoxTwittertoken.Text = "";
                        buttonOpentwittertoken.Text = "Login Sucess";

                        //세팅값 저장
                        XmlDocument setting_update = new XmlDocument();
                        setting_update.Load(@setting_path);
                        setting_update.GetElementsByTagName("auth_key")[0].InnerText = userCredentials.AccessToken;
                        setting_update.GetElementsByTagName("auth_secret")[0].InnerText = userCredentials.AccessTokenSecret;
                        setting_update.Save(@setting_path);
                    }


            }
        }
    }
    public class LR2value {
        public int baseaddr = 0;
        public int hispeed = 0;
        public int lanecover = 0;
        public int hispeed_option = 0;
        public double bpm_power = 1;
        public double bpm = 0;
        public int scrollspeed = 100;

        public int cal_bpm = 0;
        public int dst_x = 0; //흰숫 계산을 위해
        public int dst_y = 0;
        public double green_number = 0;
        public double white_number = 0; //흰숫 계산을 위해

        public int window_width;
        public int window_height;
        public int resolution_width;
        public int resolution_height;
        public int fullscreen_width;
        public int fullscreen_height;

        public string music_name;
        public string music_diff;
        public string music_diff_level;
        public string music_diff_grade;
        public string music_diff_hakko;
        public string music_name_full;
        public string music_genre;
        public string music_artist;
        public string music_artist2;

        public int gauge_type;
        public int freq_val;
        public int freq_type;
        public int freq_bool;
        public int random_type;
        public int clear_type;
        public string[] str_random_type = {
                                          
                                          };
        public string[] str_gauge_type = {
                                           "NORMAL",
                                           "HARD",
                                           "HAZARD",
                                           "EASY",
                                           "P-ATTACK",
                                           "G-ATTACK"
                                          };
        public string[] str_clear_type = {
                                            "NO PLAY",
                                            "FAILED",
                                            "EASY CLEAR",
                                            "NORMAL CLEAR",
                                            "HARD CLEAR",
                                            "FULL COMBO",
                                          };

        public int play_count;
               
        public int scene;

    }
}
