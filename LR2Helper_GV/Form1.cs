using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
//ini 사용을 위한 include지만 안 쓰므로 주석처리
//using System.Runtime.InteropServices;

namespace LR2Helper_GV {
    public partial class mainForm : Form {
        public static string prog_version = "L2.0.0a";
        public static string prog_build = "170211:2 release";

        public IntPtr prog_baseaddr; // 보통 0x400000;
        public IntPtr vmem_getbaseaddr_asm; // base address를 빼올 코드 
        public IntPtr vmem_getbaseaddr_reg; // base address를 저장할 공간
        public IntPtr vmem_dstnumber_302_asm;
        public IntPtr vmem_dstnumber_302_reg;
        public ushort flag_interrupt = 0;
        public ushort flag_unsupportedskinmode = 0;
        public MemorySharp sharp;
        public LR2value LR2value = new LR2value();

        /* ini로 설정 저장. 사용하지 않음
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        */
        //private string setting_path = Application.StartupPath + "\\lr2helper.ini";

        //xml에 저장
        private string setting_path = Application.StartupPath + "\\lr2helper-setting.xml";
        static Dictionary<String, String> skin_template = new Dictionary<String, String>();

        public mainForm() {
            InitializeComponent();
            Thread th_initFirstprocess = new Thread(new ThreadStart(initFirstprocess));
            th_initFirstprocess.Start();
            StringBuilder temp = new StringBuilder(255);
            //form setting (설정값 등)
            try {
                XmlDocument setting_file = new XmlDocument();
                setting_file.Load(@setting_path);
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Failed load setting file. Try to create it.";
                XmlWriterSettings setting_make_settings = new XmlWriterSettings();
                setting_make_settings.Indent = true;
                setting_make_settings.IndentChars = "\t";
                using (XmlWriter setting_make = XmlWriter.Create(@setting_path, setting_make_settings)) {
                    setting_make.WriteStartDocument();
                    setting_make.WriteStartElement("lr2helper");
                    setting_make.WriteStartElement("Setting");
                    setting_make.WriteElementString("DST_Y", "482");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("SkinTemplate");

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("value", "321");
                    setting_make.WriteString("OADX+ AC,CS SKIN(SD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("value", "482");
                    setting_make.WriteString("WMIX_HD AC/EC AC(HD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("value", "482");
                    setting_make.WriteString("WMIX_HD WIDE(HD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("value", "836");
                    setting_make.WriteString("LITONE5(FHD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteEndElement();
                    setting_make.WriteEndElement();
                    setting_make.WriteEndDocument();
                    setting_make.Flush();
                    setting_make.Close();
                }
            } finally {
                skin_template["default"] = textBoxDSTY.Text;
                using (XmlReader setting_root = XmlReader.Create(@setting_path)) {
                    while (setting_root.Read()) {
                        if (setting_root.IsStartElement()) {
                            switch (setting_root.Name) {
                                case "DST_Y":
                                    if (setting_root.Read()) {
                                        textBoxDSTY.Text = setting_root.Value.Trim();
                                    }
                                    break;
                                case "skin":
                                    String skin_value = setting_root["value"];
                                    if (setting_root.Read()) {
                                        skin_template[setting_root.Value.Trim()] = skin_value;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            comboBoxDSTYtemplate.DisplayMember = "Key";
            comboBoxDSTYtemplate.ValueMember = "Value";
            comboBoxDSTYtemplate.DataSource = new BindingSource(skin_template, null);

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
                    setting_update.GetElementsByTagName("DST_Y")[0].InnerText = textBoxDSTY.Text;
                    setting_update.Save(@setting_path);

                    var process_id = Process.GetProcessesByName("LRHbody_FS9.1"); // process id 체크
                                                                                  //var process_id = ApplicationFinder.FromProcessName("LRHbody_FS9.1").First();
                    if (process_id.Length > 0) //process id가 0이 아닐 시
                    {
                        //IntPtr address = new IntPtr(0x400000);
                        //IntPtr address2 = new IntPtr(0x400010);
                        //var sharp = new MemorySharp(); //MemorySharp로 process hook
                        sharp = new MemorySharp(process_id[0]);
                        if (sharp.IsRunning == true) {
                            toolStripStatusLabel1.Text = "LR2 detected.";

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
                            Thread th_getGreenvalue = new Thread(new ThreadStart(getGreenvalue));
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
                    toolStripStatusLabel1.Text = "Unexpected error";
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
                } catch (Exception e) { }
            }
        }
        public void getGreenvalue() {
            //녹숫자를 출력한다. 쓰레드로 동작
            var cal_bpm = 0;
            var dst_y = 0;
            double green_number = 0;
            vmem_dstnumber_302_asm = sharp.Memory.Allocate(256).BaseAddress;
            vmem_dstnumber_302_reg = sharp.Memory.Allocate(128).BaseAddress;
            String[] str_vmem_dstnumber_302_asm =
            {
                    "mov eax,[esp+0x8]",
                    "sub esp,0x20",
                    "sub eax,0xa",
                    "push esi",
                    "push edi",
                    "cmp eax,0x122", //demical 290
                    "jge @new_number1",
                    "jmp "+(prog_baseaddr+0x24dc),
                    "@new_number1:",
                    "mov ecx,[esp+0x2c]",
                    "cmp eax,0x127", //decmial 295
                    "jl @new_number2",
                    "mov eax,[ecx+eax*4+0x1eb1c]",
                    "@new_number3:",
                    "pop edi",
                    "pop esi",
                    "add esp,0x20",
                    "retn",
                    "@new_number2:",
                    "cmp eax, 0x123", //decimal 291
                    "je @new_number3",
                    "sub eax, 0x122",
                    "mov eax,["+ vmem_dstnumber_302_reg+"+eax*0x4]",
                    "pop edi",
                    "pop esi",
                    "add esp,0x20",
                    "retn"
                };
            String[] str_vmem_dstnumber_302_jmp =
            {
                    "jmp "+vmem_dstnumber_302_asm
                };

            sharp.Assembly.Inject(str_vmem_dstnumber_302_asm, vmem_dstnumber_302_asm); // injection
            sharp.Assembly.Inject(str_vmem_dstnumber_302_jmp, prog_baseaddr + 0x24d0); // injection


            while (true) {
                if (LR2value.baseaddr > 0) {
                    try {
                        //계산에 필요한 변수 긁어오기
                        LR2value.bpm = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x97950), false);
                        LR2value.lanecover = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x20), false);
                        LR2value.hispeed = sharp.Read<int>((IntPtr)LR2value.baseaddr, false);
                        LR2value.hispeed_option = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x48), false);
                        LR2value.bpm_power = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x9FCE0), false);
                        LR2value.scrollspeed = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x98), false);


                        //하이스피드 고정 옵션이 있다면 보정한다
                        if ((LR2value.hispeed_option > 0) && (LR2value.hispeed_option < 4)) {
                            cal_bpm = Convert.ToInt32(LR2value.bpm_power * (int)LR2value.bpm);
                        } else if (LR2value.hispeed_option == 4) {
                            cal_bpm = 150;
                        } else {
                            cal_bpm = (int)LR2value.bpm;
                        }

                        if (textBoxDSTY.Text != null) {
                            try {
                                if (dst_y != Int32.Parse(textBoxDSTY.Text)) {
                                    dst_y = Int32.Parse(textBoxDSTY.Text);
                                }
                            } catch (Exception) { }
                        } else {
                            dst_y = 482;
                        }
                        if (cal_bpm == 0) {
                            cal_bpm = 150;
                        }
                        green_number = 10 * ((2173.0 / 725.0) * 1000) * (dst_y) / (LR2value.hispeed * LR2value.scrollspeed) * (150.0 / cal_bpm) * (1.0 - (LR2value.lanecover / 100.0));
                        //게산식 : 10*((2173/725)*1000)*(DST_Y)/(HISPD*SCRSPD)*(150/BPM)*(1-(LANECOVER/100))

                        toolStripStatusLabel1.Text = "Green Number: " + Convert.ToInt32(green_number).ToString();

                        //unsupported skin mode가 on이면 fps에 녹숫을 덮어씌운다
                        sharp.Write<int>(vmem_dstnumber_302_reg + 0x8, Convert.ToInt32(green_number), false);
                        if (flag_unsupportedskinmode == 1) {
                            sharp.Write<double>((IntPtr)(LR2value.baseaddr + 0x20E08), Convert.ToInt32(green_number), false);
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
                        Thread.Sleep(16);
                    } catch (Exception) {
                        toolStripStatusLabel1.Text = "Critical error occured. Please restart LR2Helper.";
                    }
                }
            }
        }

        private void mainForm_Load(object sender, EventArgs e) {

        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e) {
            flag_interrupt = 1;
        }

        private void labelDSTYtemplate_Click(object sender, EventArgs e) {

        }

        private void comboBoxDSTYtemplate_TextChanged(object sender, EventArgs e) {
            try {
                if (comboBoxDSTYtemplate.Text != "default") {
                    textBoxDSTY.Text = skin_template[comboBoxDSTYtemplate.Text];
                }
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Key not found!!"; //사용자가 임의로 텍스트를 입력하였을 때 발생
            }

        }

        private void buttonUnsupportskinmode_Click(object sender, EventArgs e) {
            if (LR2value.baseaddr > 0) // 성공적으로 LR2가 로드되었을 때만 작동하도록
            {
                buttonUnsupportskinmode.Enabled = false;
                buttonUnsupportskinmode.Text = "Enabled. check LR2 FPS value (toggle F7)";
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

    }
}

