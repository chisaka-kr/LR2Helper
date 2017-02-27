using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using Tweetinvi.Parameters;

namespace LR2Helper_GV.program {
    public class main {

        internal string prog_version = "L2.0.6";
        internal string prog_build = "170228:0 alpha";

        internal IntPtr prog_baseaddr; // 보통 0x400000;
        internal IntPtr vmem_getbaseaddr_asm; // base address를 빼올 코드 
        internal IntPtr vmem_getbaseaddr_reg; // base address를 저장할 공간
        internal IntPtr vmem_dstnumber_300_asm;
        internal IntPtr vmem_dstnumber_300_reg;

        internal ushort flag_interrupt = 0;
        internal ushort flag_unsupportedskinmode = 0;
        internal ushort flag_resolution_manual_mode = 0;
        internal ushort flag_debug = 0;
        internal ushort flag_already_tweeted = 0;
        internal ushort flag_already_screenshoted = 0;
        internal ushort flag_run_rename = 0;
        internal string process_name;
        internal string process_path;

        //여기까지 디폴트

        private MemorySharp sharp;
        public LR2value LR2value = new LR2value();

        /* ini로 설정 저장. 사용하지 않음
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        */
        //private string setting_path = Application.StartupPath + "\\lr2helper.ini";

        //xml에 저장
        public string setting_path = Application.StartupPath + "\\lr2helper-setting.xml";
        public Dictionary<String, String> skin_template_dstx = new Dictionary<String, String>();
        public Dictionary<String, String> skin_template_dsty = new Dictionary<String, String>();
        
        internal void initFirstprocess() {
            while (true) {
                try {
                    //ini로 설정 저장. 사용하지 않음
                    //WritePrivateProfileString("setting", "DSTY", Program.lr2helper.GetFormText(textboxDSTY).ToString(), iniPath);
                    Process[] process_id = Process.GetProcessesByName("");

                    String[] process_name_list = {
                          "LR2body",
                          "LRHbody",
                          "LR2body_FS10",
                          "LRHbody_FS10",
                          "LR2body_FS9.1",
                          "LRHbody_FS9.1"
                    };
                    for (var i = 0; i < process_name_list.Length; i++) {
                        process_id = Process.GetProcessesByName(process_name_list[i]);
                        if (process_id.Length > 0) {
                            process_name = process_name_list[i];
                            break;
                        }
                    }
                    //var process_id = ApplicationFinder.FromProcessName("LRHbody_FS9.1").First();
                    if (process_id.Length > 0) {//process id가 0이 아닐 시

                        //IntPtr address = new IntPtr(0x400000);
                        //IntPtr address2 = new IntPtr(0x400010);
                        //var sharp = new MemorySharp(); //MemorySharp로 process hook
                        sharp = new MemorySharp(process_id[0]);
                        if (sharp.IsRunning == true) {
                            

                            Program.runningForm.SetTooltipStrip("LR2 detected.");

                            process_path = process_id[0].MainModule.FileName.Replace(process_name + ".exe", ""); ;

                            prog_baseaddr = new IntPtr(sharp.Modules.RemoteModules.First().BaseAddress.ToInt32());

                            //이하 LR2 base address (Hi-SPEED address) 값을 빼오기 위한 code injection

                            vmem_getbaseaddr_asm = sharp.Memory.Allocate(256).BaseAddress; //asmcode memory alloc
                            vmem_getbaseaddr_reg = sharp.Memory.Allocate(4).BaseAddress; //register memory alloc

                            //Program.lr2helper.SetTooltipStrip(vmem_getbaseaddr_asm.ToString(); 디버깅
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

                            Program.runningForm.SetTooltipStrip("LR2 detected.");

                            //sharp.Windows.MainWindow.X = 1280; LR2 윈도우 위치를 옮길 수 있는 명령이지만 별다른 메리트는 없어보이니 보류
                            //sharp.Windows.MainWindow.Y = 720;

                            //sharp.Windows.MainWindow.Title = sharp.Windows.MainWindow.Title+" + LR2Helper injected";

                            Thread th_getBaseaddr = new Thread(new ThreadStart(GetBaseAddress));
                            th_getBaseaddr.Start();
                            Thread th_getGreenvalue = new Thread(new ThreadStart(StartWork));
                            th_getGreenvalue.Start();
                            break;
                        } else {
                            for (uint i = 0; i < 5; i++) {
                                //attach 실패한 경우 (isRunning = false)
                                Program.runningForm.SetTooltipStrip("LR2body is running but is not responding. Try to reattach in " + (5 - i) + "s");
                                Thread.Sleep(1000);
                            }
                        }

                    } else {
                        for (uint i = 0; i < 5; i++) {
                            //LR2body를 찾지 못한 경우 (process_id = null)
                            Program.runningForm.SetTooltipStrip("Can't find LR2body process. Try to reattach in " + (5 - i) + "s");
                            Thread.Sleep(1000);
                        }
                    }

                    if (flag_interrupt == 1) {
                        break;
                    }
                } catch (Exception e) {
                    //뭔지 모를 에러가 났지만 그래도 재시도
                    Program.runningForm.SetTooltipStrip("Attach process failed..");
                    WriteLog(e.ToString());
                    break;
                }
            }

        }
        public void GetBaseAddress() {
            Thread.Sleep(1500);

            Program.runningForm.SetTooltipStrip("waiting for load music select screen.");
            int baseaddr = 0;
            var stripText = Program.runningForm.GetTooltipStrip();

            while (true) {
                try {
                    

                    //ini로 설정 저장. 사용하지 않음
                    //WritePrivateProfileString("setting", "DSTY", Program.lr2helper.GetFormText(textboxDSTY).ToString(), setting_path); 

                    try {
                        baseaddr = sharp.Read<int>(vmem_getbaseaddr_reg, false);
                    } catch (Exception) {

                    }
                   
                    if (stripText.Length > 50) { Program.runningForm.SetTooltipStrip("waiting for load music select screen."); }
                    Program.runningForm.SetTooltipStrip(stripText + ".");
                    if (baseaddr > 0) {
                        LR2value.baseaddr = baseaddr;

                        Program.runningForm.SetTooltipStrip("LR2 attach complete.");
                        Thread.Sleep(1000);
                        break;
                    }
                    if (!sharp.IsRunning) //LR2 프로세스 체크
                    {
                        Thread th_initFirstprocess = new Thread(new ThreadStart(initFirstprocess));
                        Program.runningForm.SetTooltipStrip("LR2 process is terminated!");
                        Thread.Sleep(2000);
                        th_initFirstprocess.Start();
                        break;
                    }
                    if (flag_interrupt == 1) {
                        break;
                    }
                    Thread.Sleep(1000);
                } catch (Exception e) {
                    WriteLog(e.ToString());
                    Thread.Sleep(1000);
                }
            }
        }
        public void StartWork() { // 모든 리프레시 작업은 여기서 (스레드를 나눌 필요가 있으면 다른 방법으로)
            InitGreenvalue();
            int now_scene;
            while (true) {
                if (LR2value.baseaddr > 0) {

                    try {
                        now_scene = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x23db4, false);

                        int dst_x;
                        int dst_y;

                        if (!int.TryParse(Program.runningForm.GetFormText("textboxDSTX"), out dst_x)) {
                            dst_x = 288;
                        }

                        if (!int.TryParse(Program.runningForm.GetFormText("textboxDSTY"), out dst_y)) {
                            dst_y = 482;
                        }

                        //xml로 설정 저장. 
                        if ((LR2value.dst_x != dst_x) || (LR2value.dst_y != dst_y)) {
                            XmlDocument setting_update = new XmlDocument();
                            setting_update.Load(@setting_path);
                            setting_update.GetElementsByTagName("dst_x")[0].InnerText = dst_x.ToString();
                            setting_update.GetElementsByTagName("dst_y")[0].InnerText = dst_y.ToString();
                            LR2value.dst_x = dst_x;
                            LR2value.dst_y = dst_y;

                            setting_update.Save(@setting_path);
                        }

                        if (now_scene == 4) {
                            GetGreenvalue();
                        }
                    } catch (Exception e) {
                        Program.runningForm.SetTooltipStrip("Critical error occured. Please restart LR2Helper.");
                        Thread.Sleep(1000);
                        WriteLog(e.ToString());
                        Program.runningForm.SetTooltipStrip("");
                    }
                    try {
                        now_scene = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x23db4, false);
                        if ((now_scene == 2) && (flag_run_rename == 1)) {
                            RunRenameScreenshot(); //스크린샷을 옮긴다
                        }
                        if ((now_scene == 5) && (now_scene != LR2value.scene)) { //리절트 화면에 진입했을 경우
                            flag_already_tweeted = 0; //플래그 초기화
                            flag_already_screenshoted = 0; //플래그 초기화

                            GetSongstatus(this.tweet_template);
                        }
                        LR2value.scene = now_scene;
                    } catch (Exception e) {
                        WriteLog(e.ToString());
                    }
                    if (flag_interrupt == 1) {
                        break;
                    }
                    if (!sharp.IsRunning) //LR2 프로세스 체크
{
                        Thread th_initFirstprocess = new Thread(new ThreadStart(initFirstprocess));
                        Program.runningForm.SetTooltipStrip("LR2 process is terminated!");
                        Thread.Sleep(2000);
                        th_initFirstprocess.Start();
                        break;
                    }
                    Thread.Sleep(16);
                } else {
                    Thread.Sleep(1000);
                }
            }
        }
        internal void GetXML() {
            try {
                XmlDocument setting_file = new XmlDocument();
                setting_file.Load(@setting_path);
            } catch (Exception e) {
                Program.runningForm.SetTooltipStrip("Failed load setting file. Try to create it.");
                if (MakeXML() == false) {
                    Program.runningForm.SetTooltipStrip("Failed create setting file.");
                    WriteLog(e.ToString());
                }

            } finally {
                skin_template_dstx["---"] = Program.runningForm.GetFormText("textboxDSTX");
                skin_template_dsty["---"] = Program.runningForm.GetFormText("textboxDSTY");
                using (XmlReader setting_root = XmlReader.Create(@setting_path)) {
                    while (setting_root.Read()) {
                        if (setting_root.IsStartElement()) {
                            switch (setting_root.Name) {
                                case "resolution_manual_mode":
                                    if (setting_root.Read()) {
                                        ushort.TryParse(setting_root.Value.Trim(), out flag_resolution_manual_mode);
                                    }
                                    break;
                                case "resolution_height":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out LR2value.resolution_height);
                                    }
                                    break;
                                case "resolution_width":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out LR2value.resolution_width);
                                    }
                                    break;
                                case "monitor_height":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out LR2value.window_height);
                                    }
                                    break;
                                case "monitor_width":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out LR2value.window_width);
                                    }
                                    break;
                                case "tweet_template":
                                    if (setting_root.Read()) {
                                        tweet_template = setting_root.Value.Trim();
                                    }
                                    break;
                                case "tweet_template_sub":
                                    if (setting_root.Read()) {
                                        tweet_template_sub = setting_root.Value.Trim();
                                    }
                                    break;
                                case "debug_mode":
                                    if (setting_root.Read()) {
                                        ushort.TryParse(setting_root.Value.Trim(), out flag_debug);
                                    }
                                    break;
                                case "dst_x":
                                    if (setting_root.Read()) {
                                        Program.runningForm.SetFormText("textboxDSTX", setting_root.Value.Trim());
                                    }
                                    break;
                                case "dst_y":
                                    if (setting_root.Read()) {
                                        Program.runningForm.SetFormText("textboxDSTY", setting_root.Value.Trim());
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
                                case "tweet_upload_mode":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out tweet_upload_mode);
                                    }
                                    break;
                                case "rename_template":
                                    if (setting_root.Read()) {
                                        renameTemplate = setting_root.Value.Trim();
                                    }
                                    break;
                                case "simple_rename_template":
                                    if (setting_root.Read()) {
                                        renameSimpleTemplate = setting_root.Value.Trim();
                                    }
                                    break;
                                case "screenshot_save_mode":
                                    if (setting_root.Read()) {
                                        int.TryParse(setting_root.Value.Trim(), out screenshotSaveMode);
                                    }
                                    break;
                                case "simple_result":
                                    var item_list = new SimpleResultItem();

                                    item_list.box_color = System.Drawing.ColorTranslator.FromHtml(setting_root["box_color"]);
                                    item_list.font_color = System.Drawing.ColorTranslator.FromHtml(setting_root["font_color"]);
                                    item_list.font_name = setting_root["font_name"];
                                    if (!byte.TryParse(setting_root["box_alpha"], out item_list.box_alpha)) {
                                        item_list.box_alpha = 200;
                                    }
                                    if (!byte.TryParse(setting_root["font_alpha"], out item_list.font_alpha)) {
                                        item_list.font_alpha = 200;
                                    }

                                    if (setting_root.Read()) {
                                        var item = setting_root.Value.Trim();
                                        simple_result_items[item] = item_list;

                                    }
                                    break;
                                case "simple_box_color":
                                    if (setting_root.Read()) {
                                        simple_box_color = System.Drawing.ColorTranslator.FromHtml(setting_root.Value.Trim());
                                    }
                                    break;
                                case "simple_box_alpha":
                                    if (setting_root.Read()) {
                                        if (!byte.TryParse(setting_root.Value.Trim(), out simple_box_alpha)) {
                                            simple_box_alpha = 200;
                                        }
                                    }
                                    break;
                                case "simple_font_color":
                                    if (setting_root.Read()) {
                                        simple_font_color = System.Drawing.ColorTranslator.FromHtml(setting_root.Value.Trim());
                                    }
                                    break;
                                case "simple_font_alpha":
                                    if (setting_root.Read()) {
                                        if (!byte.TryParse(setting_root.Value.Trim(), out simple_font_alpha)) {
                                            simple_font_alpha = 200;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
        private bool MakeXML() {
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
                    setting_make.WriteElementString("debug_mode", "0");
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
                    setting_make.WriteAttributeString("dst_x", "236");
                    setting_make.WriteAttributeString("dst_y", "385");
                    setting_make.WriteString("DRFlat(SD)");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("skin");
                    setting_make.WriteAttributeString("dst_x", "194");
                    setting_make.WriteAttributeString("dst_y", "320");
                    setting_make.WriteString("spaaaarking!!(SD)");
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

                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("twitter");
                    setting_make.WriteElementString("auth_key", "");
                    setting_make.WriteElementString("auth_secret", "");
                    setting_make.WriteElementString("tweet_template", "#MUSIC_NAME# (#MUSIC_DIFF_LEVEL#)を#CLEAR_TYPE#しました!");
                    setting_make.WriteElementString("tweet_template_sub", "@null #MUSIC_NAME# (#MUSIC_DIFF_LEVEL#)を#CLEAR_TYPE#しました!");
                    setting_make.WriteElementString("tweet_upload_mode", "0");
                    setting_make.WriteEndElement();

                    setting_make.WriteStartElement("screenshot");
                    setting_make.WriteElementString("rename_template", "[#DATE#-LR2Result] #MUSIC_NAME# (#MUSIC_DIFF_LEVEL#) #CLEAR_TYPE#");
                    setting_make.WriteElementString("simple_rename_template", "[#DATE#-LR2Simple] #MUSIC_NAME# (#MUSIC_DIFF_LEVEL#) #CLEAR_TYPE#");
                    setting_make.WriteElementString("screenshot_save_mode", "0");

                    setting_make.WriteStartElement("simple_result");
                    setting_make.WriteAttributeString("box_color", "#000000");
                    setting_make.WriteAttributeString("box_alpha", "200");
                    setting_make.WriteAttributeString("font_color", "#FFFFFF");
                    setting_make.WriteAttributeString("font_alpha", "200");
                    setting_make.WriteAttributeString("font_name", "Arial");
                    setting_make.WriteString("1");
                    setting_make.WriteEndElement();

                    setting_make.WriteEndElement();

                    setting_make.WriteEndElement();
                    setting_make.WriteEndDocument();
                    setting_make.Flush();
                    setting_make.Close();
                    return true;
                }
            } catch (Exception e) {
                WriteLog(e.ToString());
                return false;
            }

        }
        public void WriteLog(string str) {
            if (flag_debug != 1) return;
            string FilePath = Application.StartupPath + "\\lr2helper-error.log";
            string DirPath = Application.StartupPath;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try {
                if (di.Exists != true) Directory.CreateDirectory(DirPath);

                if (fi.Exists != true) {
                    using (StreamWriter sw = new StreamWriter(FilePath)) {
                        temp = string.Format("[{0}] : {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                } else {
                    using (StreamWriter sw = File.AppendText(FilePath)) {
                        temp = string.Format("[{0}] : {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            } catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
        }
        private string GetActiveWindowTitle() {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (Program.GetWindowText(Program.GetForegroundWindow(), Buff, nChars) > 0) {
                return Buff.ToString();
            }
            return null;
        }
        public void GetLR2Value() {
            try {
                LR2value.music_name = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21F20, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21F24, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff_level = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f3c, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff_hakko = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f44, false), Encoding.GetEncoding(932), false);
                LR2value.music_genre = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f2c, false), Encoding.GetEncoding(932), false);
                LR2value.music_artist = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f30, false), Encoding.GetEncoding(932), false);
                LR2value.music_artist2 = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f34, false), Encoding.GetEncoding(932), false);
                LR2value.play_clear_type = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97b88, false);
                LR2value.play_gauge_type = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x8, false);
                LR2value.play_djlevel = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97a48, false) * 9 / 32;

                LR2value.play_pgreat = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97988, false);
                LR2value.play_great = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97984, false);
                LR2value.play_good = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97980, false);
                LR2value.play_bad = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x9797c, false);
                LR2value.play_poor = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97978, false);
                LR2value.play_combo = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97998, false);
                LR2value.play_maximum_combo = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x979BC, false);
                LR2value.play_score = LR2value.play_pgreat * 2 + LR2value.play_great;

                if (LR2value.play_djlevel > 8) {
                    LR2value.play_djlevel = (LR2value.play_score * 100 / (LR2value.play_maximum_combo * 2)) / 11;
                }
            } catch (Exception) { return; }

        }
        
        //rename.cs
        public string renameTemplate = "";
        public string renameSimpleTemplate = "";
        public int screenshotSaveMode = 0;

        internal void InitRenameScreenshot() {
            Program.RegisterHotKey(Program.runningForm.GetHandle(), 2, (int)Program.KeyModifier.None, Keys.F9.GetHashCode()); // F9 핫키 등록
            Program.RegisterHotKey(Program.runningForm.GetHandle(), 3, (int)Program.KeyModifier.Alt, Keys.F9.GetHashCode()); // ALT+F9 핫키 등록
        }
        internal void SetEventRenameScreenshot() {
            if (LR2value.scene != 5) { return; }
            if (flag_run_rename != 0) { return; }

            if (flag_already_screenshoted == 0) { //스크린샷을 안찍었다면
                Program.KeyboardEvent((byte)Keys.F6, 0x00, 0x00, 0);
                Program.Delay(200);
                Program.KeyboardEvent((byte)Keys.F6, 0x00, 0x02, 0);
                Program.Delay(1000);
                flag_already_screenshoted = 1;
            }
            flag_run_rename = 1;
            Program.runningForm.SetTooltipStrip("Screenshot will be renamed after result screen.");
        }
        internal void RunRenameScreenshot() {
            if (flag_run_rename == 0) { return; }
            //트윗 내용이랑 똑같은 방법으로 치환해준다
            GetLR2Value();

            var level = "";
            if (Convert.ToInt16(LR2value.music_diff_level) > 0) {
                level = "☆" + LR2value.music_diff_level;
            }
            if (Convert.ToInt16(LR2value.music_diff_hakko) > 0) {
                level = "★" + LR2value.music_diff_hakko;
            }
            //#MUSIC_NAME# (#MUSIC_LEVEL#)を #CLEAR_TYPE#し
            Dictionary<String, String> replace_list = new Dictionary<String, String>();

            var date = DateTime.Now.Year.ToString();
            string time;
            if (DateTime.Now.Month < 10) {
                date = date + "-0" + DateTime.Now.Month.ToString();
            } else {
                date = date + "-" + DateTime.Now.Month.ToString();
            }
            if (DateTime.Now.Day < 10) {
                date = date + "-0" + DateTime.Now.Day.ToString();
            } else {
                date = date + "-" + DateTime.Now.Day.ToString();
            }
            if (DateTime.Now.Hour < 10) {
                time = "0" + DateTime.Now.Hour.ToString();
            } else {
                time = DateTime.Now.Hour.ToString();
            }
            if (DateTime.Now.Minute < 10) {
                time = time + "-0" + DateTime.Now.Minute.ToString();
            } else {
                time = time + "-" + DateTime.Now.Minute.ToString();
            }
            if (DateTime.Now.Second < 10) {
                time = time + "-0" + DateTime.Now.Second.ToString();
            } else {
                time = time + "-" + DateTime.Now.Second.ToString();
            }

            //뭔가 아닌거같은데


            replace_list["#MUSIC_NAME#"] = LR2value.music_name;
            replace_list["#MUSIC_DIFF_LEVEL#"] = level;
            replace_list["#MUSIC_DIFF#"] = LR2value.music_diff;
            replace_list["#MUSIC_GENRE#"] = LR2value.music_genre;
            replace_list["#MUSIC_ARTIST#"] = LR2value.music_artist;
            replace_list["#MUSIC_ARTIST2#"] = LR2value.music_artist2;
            replace_list["#CLEAR_TYPE#"] = LR2value.str_clear_type[LR2value.play_clear_type];
            replace_list["#GAUGE_TYPE#"] = LR2value.str_gauge_type[LR2value.play_gauge_type];
            replace_list["#DJ_LEVEL#"] = LR2value.str_djlevel[LR2value.play_djlevel];
            replace_list["#DATE#"] = date;
            replace_list["#TIME#"] = time;

            //파일명에 들어갈 수 없는 글자 전각으로 치환
            replace_list["\\"] = "＼";
            replace_list["/"] = "／";
            replace_list[":"] = "：";
            replace_list["*"] = "＊";
            replace_list["?"] = "？";
            replace_list["<"] = "＜";
            replace_list["\""] = "''";
            replace_list[">"] = "＞";
            replace_list["|"] = "｜";

            String text = renameTemplate;
            String text_simple = renameSimpleTemplate;
            foreach (var replace_key in replace_list.Keys) {
                text = text.Replace(replace_key, replace_list[replace_key]);
                text_simple = text_simple.Replace(replace_key, replace_list[replace_key]);
            }

            //옮기기 전에 폴더가 있는지 확인 후, 폴더를 만든다 
            var screenshot_path = @process_path + "\\screenshot";
            var simple_screenshot_path = @process_path + "\\screenshot\\simple";

            if (!System.IO.Directory.Exists(screenshot_path)) {
                System.IO.Directory.CreateDirectory(screenshot_path);
            }
            if (!System.IO.Directory.Exists(simple_screenshot_path)) {
                System.IO.Directory.CreateDirectory(simple_screenshot_path);
            }

            //경로 가져오고
            var before_image = Directory.GetFiles(@process_path, "LR2 *.png").Last();
            try {
                switch (screenshotSaveMode) {
                    case 1: //일반 스크린샷만
                        System.IO.Directory.Move(@before_image, screenshot_path + "\\" + text + ".png");
                        break;
                    case 2: //심플 스크린샷만
                        Program.runningForm.SaveSimpleResultPicture(simple_screenshot_path + "\\" + text_simple + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default: //둘 다
                        System.IO.Directory.Move(@before_image, screenshot_path + "\\" + text + ".png");  //얘는 옮기고
                        Program.runningForm.SaveSimpleResultPicture(simple_screenshot_path + "\\" + text_simple + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
            } catch (Exception) {
                Program.runningForm.SetTooltipStrip("Screenshot rename failed.");
            }

            //끝.
            Program.runningForm.SetTooltipStrip("Screenshot rename complete.");

            flag_run_rename = 0;
        }

        //greenvalue.cs
        private void InitGreenvalue() {
            vmem_dstnumber_300_asm = sharp.Memory.Allocate(256).BaseAddress;
            vmem_dstnumber_300_reg = sharp.Memory.Allocate(128).BaseAddress;
            String[] str_vmem_dstnumber_300_asm =
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
                    "mov eax,["+ vmem_dstnumber_300_reg+"+eax*0x4]",
                    "pop edi",
                    "pop esi",
                    "add esp,0x20",
                    "retn"
                };
            String[] str_vmem_dstnumber_300_jmp =
            {
                    "jmp "+vmem_dstnumber_300_asm
                };

            sharp.Assembly.Inject(str_vmem_dstnumber_300_asm, vmem_dstnumber_300_asm); // injection
            sharp.Assembly.Inject(str_vmem_dstnumber_300_jmp, prog_baseaddr + 0x24d0); // injection
        }
        private void GetGreenvalue() {

            //계산에 필요한 변수 긁어오기
            LR2value.bpm = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x97950), false);
            LR2value.lanecover = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x20), false);
            LR2value.hispeed = sharp.Read<int>((IntPtr)LR2value.baseaddr, false);
            LR2value.hispeed_option = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x48), false);
            LR2value.bpm_power = sharp.Read<double>((IntPtr)(LR2value.baseaddr + 0x9FCE0), false);
            LR2value.scrollspeed = sharp.Read<int>((IntPtr)(LR2value.baseaddr + 0x98), false);
            //여기까지 녹숫 계산을 위한 변수들

            if (flag_resolution_manual_mode == 0) {
                LR2value.resolution_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x61700), false);
                LR2value.resolution_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f), false);
                LR2value.window_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f0), false);
                LR2value.window_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616ec), false);
                LR2value.fullscreen_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x60890), false);
                LR2value.fullscreen_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x6088c), false);
                //보험용
                if ((LR2value.window_width == 0) || (LR2value.window_height == 0)) {
                    LR2value.resolution_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x61700 - 0x60), false);
                    LR2value.resolution_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616fc - 0x60), false);
                    LR2value.window_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616f0 - 0x60), false);
                    LR2value.window_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x616ec - 0x60), false);
                    LR2value.fullscreen_width = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x60890 - 0x60), false);
                    LR2value.fullscreen_height = sharp.Read<int>((IntPtr)(LR2value.baseaddr - 0x6088c - 0x60), false);
                }
            }
            //여기까지 흰숫 계산을 위한 변수들

            //만약 window_width나 height가 0일 경우 풀스크린으로 계산
            if ((LR2value.window_height == 0) || (LR2value.window_width == 0)) {
                LR2value.window_width = LR2value.fullscreen_width;
                LR2value.window_height = LR2value.fullscreen_height;
            }
            //하이스피드 고정 옵션이 있다면 보정한다
            if ((LR2value.hispeed_option > 0) && (LR2value.hispeed_option < 4)) {
                LR2value.cal_bpm = Convert.ToInt32(LR2value.bpm_power * (int)LR2value.bpm);
            } else if (LR2value.hispeed_option == 4) {
                LR2value.cal_bpm = 150;
            } else {
                LR2value.cal_bpm = (int)LR2value.bpm;
            }

            if (LR2value.cal_bpm == 0) {
                LR2value.cal_bpm = 150;
            }
            LR2value.green_number = 10 * ((2173.0 / 725.0) * 1000) * (LR2value.dst_y) / (LR2value.hispeed * LR2value.scrollspeed) * (150.0 / LR2value.cal_bpm) * (1.0 - (LR2value.lanecover / 100.0));
            //게산식 : 10*((2173/725)*1000)*(DST_Y)/(HISPD*SCRSPD)*(150/BPM)*(1-(LANECOVER/100))
            //white_number = (1.0 - ((288.0 * dst_y * (LR2value.window_width * 1.0 / LR2value.window_height * 1.0)) / (482.0 * dst_x * (LR2value.resolution_width * 1.0 / LR2value.resolution_height * 1.0))) * (1.0 - (LR2value.lanecover * 1.0 / dst_y * 1.0))) * 1000.0;
            LR2value.white_number = (1.0 - ((288.0 * LR2value.dst_y * (LR2value.window_width * 1.0 / LR2value.window_height * 1.0)) / (482.0 * LR2value.dst_x * (LR2value.resolution_width * 1.0 / LR2value.resolution_height * 1.0))) * (1.0 - (LR2value.lanecover * 1.0 / 100 * 1.0))) * 1000.0;
            //계산식 : ((1-((288*DST_Y*(WINDOW_W/WINDOW_H))/(482*DST_X*(RESOL_W/RESOL_H)))*(1-(LANECOVER/DST_Y)))*1000)

            if (LR2value.scene == 4) {
                Program.runningForm.SetTooltipStrip("Green Number: " + Convert.ToInt32(LR2value.green_number).ToString() + " White Number: " + Convert.ToInt32(LR2value.white_number).ToString());
            }

            //302와 303에 녹숫과 흰숫을 쓴다. 참고로 vmem_dstnumber_300_reg는 300번 주소를 가리킨다.
            sharp.Write<int>(vmem_dstnumber_300_reg + 0x8, Convert.ToInt32(LR2value.green_number), false);
            sharp.Write<int>(vmem_dstnumber_300_reg + 0xC, Convert.ToInt32(LR2value.white_number), false);

            //unsupported skin mode가 on이면 fps에 녹숫을 덮어씌운다
            if (flag_unsupportedskinmode == 1) {
                sharp.Write<double>((IntPtr)(LR2value.baseaddr + 0x20E08), Convert.ToInt32(LR2value.green_number), false);
            }
        }

        //simpleresult.cs
        Bitmap simple_result;
        //속성값. 기본으로 지정
        Color simple_box_color;
        Byte simple_box_alpha;
        Color simple_font_color;
        Byte simple_font_alpha;
        String simple_font_name;

        static Dictionary<String, SimpleResultItem> simple_result_items = new Dictionary<string, SimpleResultItem>();



        public void InitSimpleresult() {

            simple_box_color = System.Drawing.ColorTranslator.FromHtml("#000000");
            simple_box_alpha = 200;
            simple_font_color = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            simple_font_alpha = 200;
            simple_font_name = "Arial";

            string image_clear_type = "clear";

            //클리어했는지, 풀콤했는지 확인
            switch (LR2value.play_clear_type) {
                case 5: //풀콤
                    image_clear_type = "clear";
                    break;
                case 1: //페일
                    image_clear_type = "fail";
                    break;
                default: //그 외
                    image_clear_type = "clear";
                    break;

            }
            var image_list = Directory.GetFiles(Application.StartupPath + "\\images", "normal_" + image_clear_type + "_*.png");
            //이미지를 랜덤으로 고르기 위해..
            var image = image_list[new Random().Next(0, image_list.Length)];


            //이미지 이름 꺼내오기
            var image_name = image.Replace(Application.StartupPath + "\\images\\normal_" + image_clear_type + "_", "");
            image_name = image_name.Replace(".png", "");

            //속성값 가져오기
            foreach (var name_key in simple_result_items.Keys) {
                if (name_key == image_name) {
                    simple_box_alpha = simple_result_items[name_key].box_alpha;
                    simple_box_color = simple_result_items[name_key].box_color;
                    simple_font_alpha = simple_result_items[name_key].font_alpha;
                    simple_font_color = simple_result_items[name_key].font_color;
                    simple_font_name = simple_result_items[name_key].font_name;
                }
            }

            Program.runningForm.LoadSimpleResultPicture(image);
            simple_result = Program.runningForm.GetSimpleResultPicture();
            
            //DrawImage(font_image, simple_result, 5, 150, 255,0);
            DrawBox(10, 81, 270, 15, simple_box_alpha, simple_box_color); // 장르
            DrawImage((Bitmap)GetImageText(LR2value.music_genre, new Font(simple_font_name, 8f), simple_font_color, simple_box_color), simple_result, 143, 86, simple_font_alpha, 0, 1, 1);
            DrawBox(10, 98, 270, 30, simple_box_alpha, simple_box_color); // 곡명
            DrawImage((Bitmap)GetImageText(LR2value.music_name, new Font(simple_font_name, 13f), simple_font_color, simple_box_color), simple_result, 143, 111, simple_font_alpha, 0, 1, 1);
            DrawBox(10, 130, 270, 15, simple_box_alpha, simple_box_color); // 아티스트
            DrawImage((Bitmap)GetImageText(LR2value.music_artist, new Font(simple_font_name, 8f), simple_font_color, simple_box_color), simple_result, 143, 135, simple_font_alpha, 0, 1, 1);
            DrawBox(10, 147, 200, 24, simple_box_alpha, simple_box_color); // 클리어
            DrawImage((Bitmap)GetImageText(LR2value.str_clear_type[LR2value.play_clear_type], new Font(simple_font_name, 15f), simple_font_color, simple_box_color), simple_result, 104, 155, simple_font_alpha, 0, 1, 1);

            var level = "";
            if (Convert.ToInt16(LR2value.music_diff_level) > 0) {
                level = "☆" + LR2value.music_diff_level;
            }
            if (Convert.ToInt16(LR2value.music_diff_hakko) > 0) {
                level = "★" + LR2value.music_diff_hakko;
            }

            DrawBox(285, 81, 110, 30, simple_box_alpha, simple_box_color); // 장르
            DrawImage((Bitmap)GetImageText(level, new Font(simple_font_name, 15f), simple_font_color, simple_box_color), simple_result, 339, 92, simple_font_alpha, 0, 1, 1);

            string[] box_name = {
                "DJ LEVEL",
                "SCORE",
                "PGREAT",
                "GREAT",
                "GOOD",
                "BAD",
                "POOR",
                "MAX COMBO",
            };
            string[] box_value = {
                LR2value.str_djlevel[LR2value.play_djlevel],
                LR2value.play_score.ToString(),
                LR2value.play_pgreat.ToString(),
                LR2value.play_great.ToString(),
                LR2value.play_good.ToString(),
                LR2value.play_bad.ToString(),
                LR2value.play_poor.ToString(),
                LR2value.play_combo.ToString()
            };
            for (var i = 0; i < 8; i++) {
                DrawBox(10, 377 + i * 26, 200, 24, simple_box_alpha, simple_box_color); // DJ LEVEL 등
                DrawImage((Bitmap)GetImageText(box_name[i], new Font(simple_font_name, 16f), simple_font_color, simple_box_color), simple_result, 8, 386 + i * 26, simple_font_alpha, 0, 0, 1);
                DrawImage((Bitmap)GetImageText(box_value[i], new Font(simple_font_name, 16f), simple_font_color, simple_box_color), simple_result, 205, 386 + i * 26, simple_font_alpha, 0, 2, 1);
            }

            Program.runningForm.SetSimpleResultPicture(simple_result);
            
        }
        void DrawBox(int x, int y, int box_x, int box_y, byte transparent, Color color) {


            Bitmap box = new Bitmap(box_x, box_y);

            for (int py = 0; py < box.Height; py++) {
                for (int px = 0; px < box.Width; px++) {
                    box.SetPixel(px, py, color);
                }
            }
            using (Graphics gr = Graphics.FromImage(box)) {
                gr.DrawImage(box, 0, 0);
            }
            DrawImage(box, simple_result, x, y, transparent, 1, 0, 0);
        }
        private Bitmap GetImageText(String text, Font font, Color textColor, Color backColor) {
            backColor = Color.FromArgb(80, backColor.R, backColor.G, backColor.B);

            //first, create a dummy bitmap just to get a graphics object
            Bitmap img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)(textSize.Width * 2), (int)(textSize.Height * 2));


            drawing = Graphics.FromImage(img);
            Color tr;
            try {
                tr = Color.FromArgb(255, backColor.R + 1, backColor.G, backColor.B);
            } catch (Exception) {
                tr = Color.FromArgb(255, backColor.R - 1, backColor.G, backColor.B);
            }

            drawing.Clear(tr);



            drawing.SmoothingMode = SmoothingMode.AntiAlias;
            //drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;


            //SolidBrush brushWhite = new SolidBrush(Color.Black);
            //drawing.FillRectangle(brushWhite, 0, 0,
            //this.ClientSize.Width, this.ClientSize.Height);

            FontFamily fontFamily = font.FontFamily;
            StringFormat strformat = new StringFormat();

            GraphicsPath path = new GraphicsPath();
            path.AddString(text, fontFamily,
                (int)FontStyle.Bold, font.Size, new Point(7, 7), strformat);
            Pen pen = new Pen(backColor, 4);
            pen.LineJoin = LineJoin.Round;
            //drawing.DrawPath(pen, path);
            LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(10, 10, 30, 70),
                textColor,
                textColor, LinearGradientMode.Vertical);
            drawing.FillPath(brush, path);

            //그린 폰트의 사이즈를 다시 구한다
            var maxHeight = 0;
            var maxWidth = 0;
            Color clr;
            for (int py = 0; py < img.Height; py++) {
                for (int px = 0; px < img.Width; px++) {
                    clr = img.GetPixel(px, py);

                    if ((clr.A != tr.A) || (clr.R != tr.R) || (clr.B != tr.B) || (clr.B != tr.B)) {
                        if (maxHeight <= py) {
                            maxHeight = py;
                        }
                        if (maxWidth <= px) {
                            maxWidth = px;
                        }
                    } else {

                    }
                }
            }
            if (text == "1") { // 1만 나오면 옆으로 붙는거 보정
                maxWidth = maxWidth + (maxWidth / 8);
            }
            Bitmap newImg = new Bitmap(maxWidth + 3, maxHeight + 4);

            using (Graphics g = Graphics.FromImage(newImg)) {
                g.DrawImage(img, 0, 0, img.Width, img.Height);
            }
            //brushWhite.Dispose();
            fontFamily.Dispose();
            path.Dispose();
            pen.Dispose();
            brush.Dispose();
            drawing.Dispose();

            return newImg;

        }
        private void DrawImage(Bitmap watermark_bm, Bitmap result_bm, int x, int y, byte alpha, byte flag, byte align_type_x, byte align_type_y) {

            Color clr;
            for (int py = 0; py < watermark_bm.Height; py++) {
                for (int px = 0; px < watermark_bm.Width; px++) {
                    clr = watermark_bm.GetPixel(px, py);

                    watermark_bm.SetPixel(px, py,
                        Color.FromArgb(alpha, clr.R, clr.G, clr.B));
                }
            }
            if (align_type_x == 2) {
                x = x - watermark_bm.Width;
            }
            if (align_type_x == 1) {
                x = x - (watermark_bm.Width / 2);
            }

            if (align_type_y == 2) {
                y = y - watermark_bm.Height;
            }
            if (align_type_y == 1) {
                y = y - (watermark_bm.Height / 2);
            }
            // Set the watermark's transparent color.
            if (flag == 0) {
                watermark_bm.MakeTransparent(watermark_bm.GetPixel(0, 0));
            }
            // Copy onto the result image.
            using (Graphics gr = Graphics.FromImage(result_bm)) {
                gr.DrawImage(watermark_bm, x, y, watermark_bm.Width, watermark_bm.Height);
            }
        }
        public class SimpleResultItem {
            public Color box_color;
            public Byte box_alpha;
            public Color font_color;
            public Byte font_alpha;
            public String font_name;
        }

        //트위터
        public TwitterCredentials appCredentials;
        public IAuthenticationContext authenticationContext;
        public ITwitterCredentials userCredentials;
        public IAuthenticatedUser authenticatedUser;
        public string consumer_key = "3zhsR0xs7sd5HkTi4S68XDwyD";
        public string consumer_secret = "MGXli4ikRcPbM2nA8Cf2D7vYRuzTTj89MhR3rzfU9gGIRQ7wJ0";
        public string auth_key = "";
        public string auth_secret = "";
        public string tweet_template = "#MUSIC_NAME# (#MUSIC_DIFF_LEVEL#)を #CLEAR_TYPE#しました!";
        public string tweet_template_sub = "@null #MUSIC_NAME# (#MUSIC_DIFF_LEVEL#)を #CLEAR_TYPE#しました!";
        public int tweet_upload_mode = 0;
        

        internal void GetTwitterid() {
            appCredentials = new TwitterCredentials(consumer_key, consumer_secret);
            // Init the authentication process and store the related `AuthenticationContext`.
            authenticationContext = AuthFlow.InitAuthentication(appCredentials);

            Program.RegisterHotKey(Program.runningForm.GetHandle(), 0, (int)Program.KeyModifier.None, Keys.F11.GetHashCode()); // F11 핫키 등록
            Program.RegisterHotKey(Program.runningForm.GetHandle(), 1, (int)Program.KeyModifier.Alt, Keys.F11.GetHashCode()); // ALT+F11 핫키 등록

            if ((auth_key.Length > 0) && (auth_secret.Length > 0)) {
                try {
                    Auth.SetUserCredentials(consumer_key, consumer_secret, auth_key, auth_secret);

                    authenticatedUser = User.GetAuthenticatedUser();

                    if (authenticatedUser != null) {

                        Program.runningForm.SetFormTwitterLoggedIn("Already logged in", "@" + authenticatedUser.ScreenName);

                    }
                    /*
                    var SendTweet = authenticatedUser.PublishTweet("★6 Engine [Insane]をハードクリアしました。");
                    if (SendTweet == null) // Something went wrong but we don't know what
                    {

                        // We can get the latest exception received by Tweetinvi
                        var latestException = ExceptionHandler.GetLastException();
                        Console.WriteLine("The following error occured : '{0}'", latestException.TwitterDescription);
                    }
                    */
                } catch (Exception e) {
                    WriteLog(e.ToString());
                } finally { }

            }
        }
        internal void GetSongstatus(string text) {
            var now_scene = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x23db4, false);
            if (now_scene == 5) {
                Program.Delay(500); // 진입 후 500ms만 기다린다 (NO PLAY라고 뜨는 걸 막기위해)

                GetLR2Value();

                var level = "";
                if (Convert.ToInt16(LR2value.music_diff_level) > 0) {
                    level = "☆" + LR2value.music_diff_level;
                }
                if (Convert.ToInt16(LR2value.music_diff_hakko) > 0) {
                    level = "★" + LR2value.music_diff_hakko;
                }
                //#MUSIC_NAME# (#MUSIC_LEVEL#)を #CLEAR_TYPE#し
                Dictionary<String, String> replace_list = new Dictionary<String, String>();
                replace_list["#MUSIC_NAME#"] = LR2value.music_name;
                replace_list["#MUSIC_DIFF_LEVEL#"] = level;
                replace_list["#MUSIC_DIFF#"] = LR2value.music_diff;
                replace_list["#MUSIC_GENRE#"] = LR2value.music_genre;
                replace_list["#MUSIC_ARTIST#"] = LR2value.music_artist;
                replace_list["#MUSIC_ARTIST2#"] = LR2value.music_artist2;
                replace_list["#CLEAR_TYPE#"] = LR2value.str_clear_type[LR2value.play_clear_type];
                replace_list["#GAUGE_TYPE#"] = LR2value.str_gauge_type[LR2value.play_gauge_type];
                replace_list["#DJ_LEVEL#"] = LR2value.str_djlevel[LR2value.play_djlevel];
                InitSimpleresult();

                String tweet_text = text;
                foreach (var replace_key in replace_list.Keys) {
                    tweet_text = tweet_text.Replace(replace_key, replace_list[replace_key]);
                }
                Program.runningForm.SetFormText("textBoxTweettext", tweet_text);
                
            }
        }
        internal void SendTweet(string text) {
            String window_title = GetActiveWindowTitle();
            if (flag_already_tweeted == 1) {
                Program.runningForm.SetTooltipStrip("Already tweeted");
            }
            if ((authenticatedUser != null) && (LR2value.scene == 5) && (window_title == sharp.Windows.MainWindow.Title)) {
                if (text.Length < 140) {
                    var window = sharp.Windows.MainWindow;

                    //업로드 모드에 따라 
                    // 0 - 기본값, (혹은 1~3 이외의 값) : 둘 다 올린다. 심플 리절트를 먼저 올린다.
                    // 1 - 둘 다 올린다. 노멀 리절트를 먼저 올린다.
                    // 2 - 심플 리절트만 올린다.
                    // 3 - 노멀 리절트만 올린다.
                    // 4 - 이미지를 올리지 않는다.
                    IMedia media;
                    IMedia media2;
                    ITweet tweet;
                    if (flag_already_screenshoted == 0) { //스크린샷을 안찍었다면
                        if (!((tweet_upload_mode == 4) || (tweet_upload_mode == 2))) {
                            flag_already_screenshoted = 1;
                            Program.KeyboardEvent((byte)Keys.F6, 0x00, 0x00, 0);
                            Program.Delay(200);
                            Program.KeyboardEvent((byte)Keys.F6, 0x00, 0x02, 0);
                            Program.Delay(1000);

                        }
                    }
                    var file = Directory.GetFiles(@process_path, "LR2 *.png").Last();
                    var image = File.ReadAllBytes(file);
                    var simpleImage = Program.runningForm.GetSimpleResultPicture();

                    switch (tweet_upload_mode) {
                        case 4:
                            tweet = Tweet.PublishTweet(text);
                            break;
                        case 3:
                            media = Upload.UploadImage(image);
                            tweet = Tweet.PublishTweet(text, new PublishTweetOptionalParameters {
                                Medias = { media }
                            });
                            break;
                        case 2:
                            media = Upload.UploadImage(ImageToByteArray(simpleImage));
                            tweet = Tweet.PublishTweet(text, new PublishTweetOptionalParameters {
                                Medias = { media }
                            });
                            break;
                        case 1:
                            media = Upload.UploadImage(image);
                            media2 = Upload.UploadImage(ImageToByteArray(simpleImage));
                            tweet = Tweet.PublishTweet(text, new PublishTweetOptionalParameters {
                                Medias = { media, media2 }
                            });
                            break;
                        default:
                            media = Upload.UploadImage(image);
                            media2 = Upload.UploadImage(ImageToByteArray(simpleImage));
                            tweet = Tweet.PublishTweet(text, new PublishTweetOptionalParameters {
                                Medias = { media2, media }
                            });
                            break;
                    }

                    if (tweet != null) {
                        Program.runningForm.SetTooltipStrip("Tweet succeeded");
                        flag_already_tweeted = 1;
                    } else {
                        Program.runningForm.SetTooltipStrip("Tweet failed");
                    }
                } else {
                    Program.runningForm.SetTooltipStrip("Tweet is too long");
                }
            } else {
                Program.runningForm.SetTooltipStrip("Please run on result screen or twitter isn't authenticated.");
            }
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn) {
            using (var ms = new MemoryStream()) {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        
        public void DelFPSLogic() {
            try {
                Program.lr2helper.sharp.Assembly.Inject(
                new[]
                    {
                                            "nop",
                                            "nop",
                                            "nop",
                                            "nop",
                                            "nop",
                                            "nop"
                    },
                Program.lr2helper.prog_baseaddr + 0xb6ab7); // FPS 숫자 갱신을 무효화
            } catch (Exception) {
                return;
            }
        }


        public static DateTime Delay(int MS) {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment) {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
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

        public int play_djlevel;
        public int play_gauge_type;
        public int play_freq_val;
        public int play_freq_type;
        public int play_freq_bool;
        public int play_random_type;
        public int play_clear_type;

        public int play_score;

        public int play_pgreat;
        public int play_great;
        public int play_good;
        public int play_bad;
        public int play_poor;
        public int play_combo;
        public int play_maximum_combo;

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
        public string[] str_djlevel = {
            "F",
            "F",
            "E",
            "D",
            "C",
            "B",
            "A",
            "AA",
            "AAA",
            "MAX"
        };
        public int play_count;

        public int scene;

    }
}
