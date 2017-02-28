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
    public partial class LR2helper {

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
            replace_list["#CLEAR_TYPE#"] = LR2value.text_clear_type[LR2value.play_clear_type];
            replace_list["#GAUGE_TYPE#"] = LR2value.text_gauge_type[LR2value.play_gauge_type];
            replace_list["#DJ_LEVEL#"] = LR2value.text_djlevel[LR2value.play_djlevel];
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

    }
}
