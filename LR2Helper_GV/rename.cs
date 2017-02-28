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
//ini 사용을 위한 include지만 안 쓰므로 주석처리
//using System.Runtime.InteropServices;

namespace LR2Helper_GV {
    public partial class mainForm : Form {
        public string rename_template = "";
        public string simple_rename_template = "";
        public int screenshot_save_mode = 0;

        void InitRenameScreenshot() {
            RegisterHotKey(this.Handle, 2, (int)KeyModifier.None, Keys.F9.GetHashCode()); // F9 핫키 등록
            RegisterHotKey(this.Handle, 3, (int)KeyModifier.Alt, Keys.F9.GetHashCode()); // ALT+F9 핫키 등록
        }
        void SetEventRenameScreenshot() {
            if (LR2value.scene != 5) { return; }
            if (flag_run_rename != 0) { return; }

            if (flag_already_screenshoted == 0) { //스크린샷을 안찍었다면
                flag_already_screenshoted = 1;
                keybd_event((byte)Keys.F6, 0x00, 0x00, 0);
                delay(200);
                keybd_event((byte)Keys.F6, 0x00, 0x02, 0);
                delay(1000);
            }
            flag_run_rename = 1;
            toolStripStatusLabel1.Text = "Screenshot will be renamed after result screen.";
        }
        void RunRenameScreenshot() {
            if (flag_run_rename == 0) { return; }
            //트윗 내용이랑 똑같은 방법으로 치환해준다
            getLR2value();

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

            String text = rename_template;
            String text_simple = simple_rename_template;
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
                switch (screenshot_save_mode) {
                    case 1: //일반 스크린샷만
                        System.IO.Directory.Move(@before_image, screenshot_path + "\\" + text + ".png");
                        break;
                    case 2: //심플 스크린샷만
                        pictureBoxSimpleresult.Image.Save(simple_screenshot_path + "\\" + text_simple + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default: //둘 다
                        System.IO.Directory.Move(@before_image, screenshot_path + "\\" + text + ".png");  //얘는 옮기고
                        pictureBoxSimpleresult.Image.Save(simple_screenshot_path + "\\" + text_simple + ".png", System.Drawing.Imaging.ImageFormat.Png); //얘는 만들고
                        break;
                }
            } catch (Exception) {
                toolStripStatusLabel1.Text = "Screenshot rename failed.";
            }

            //끝.
            toolStripStatusLabel1.Text = "Screenshot rename complete.";

            flag_run_rename = 0;
        }
    }
}
