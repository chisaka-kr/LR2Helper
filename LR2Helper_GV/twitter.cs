using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
//ini 사용을 위한 include지만 안 쓰므로 주석처리
//using System.Runtime.InteropServices;

namespace LR2Helper_GV {
    public partial class mainForm {

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
        delegate void SetTextCallback(string text);

        // Create a new set of credentials for the application.

        public void getTwitterid() {
            appCredentials = new TwitterCredentials(consumer_key, consumer_secret);
            // Init the authentication process and store the related `AuthenticationContext`.
            authenticationContext = AuthFlow.InitAuthentication(appCredentials);

            RegisterHotKey(this.Handle, 0, (int)KeyModifier.None, Keys.F11.GetHashCode()); // F11 핫키 등록
            RegisterHotKey(this.Handle, 1, (int)KeyModifier.Alt, Keys.F11.GetHashCode()); // ALT+F11 핫키 등록

            if ((auth_key.Length > 0) && (auth_secret.Length > 0)) {
                try {
                    Auth.SetUserCredentials(consumer_key, consumer_secret, auth_key, auth_secret);

                    authenticatedUser = User.GetAuthenticatedUser();

                    if (authenticatedUser != null) {
                        flag_twitterlogon = 1;
                        buttonGettwittertoken.Enabled = false;
                        buttonOpentwittertoken.Enabled = false;
                        textBoxTwittertoken.Enabled = false;
                        //buttonTweetsend.Enabled = true;
                        buttonOpentwittertoken.Text = "Already logged in";
                    }
                    /*
                    var sendTweet = authenticatedUser.PublishTweet("★6 Engine [Insane]をハードクリアしました。");
                    if (sendTweet == null) // Something went wrong but we don't know what
                    {

                        // We can get the latest exception received by Tweetinvi
                        var latestException = ExceptionHandler.GetLastException();
                        Console.WriteLine("The following error occured : '{0}'", latestException.TwitterDescription);
                    }
                    */
                } catch (Exception e) {
                    writeLog(e.ToString());
                } finally { }

            }
        }
        public void getSongstatus(string text,int flag) {

            var now_scene = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x23db4, false);
            if (((now_scene != LR2value.scene) && (now_scene == 5))||((now_scene == 5)&&(flag == 1))) { //리절트 화면에 진입했을 경우
                delay(500); // 진입 후 500ms만 기다린다 (NO PLAY라고 뜨는 걸 막기위해)

                LR2value.music_name = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21F20, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21F24, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff_level = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f3c, false), Encoding.GetEncoding(932), false);
                LR2value.music_diff_hakko = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f44, false), Encoding.GetEncoding(932), false);
                LR2value.music_genre = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f2c, false), Encoding.GetEncoding(932), false);
                LR2value.music_artist = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f30, false), Encoding.GetEncoding(932), false);
                LR2value.music_artist2 = sharp.ReadString((IntPtr)sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x21f34, false), Encoding.GetEncoding(932), false);
                LR2value.clear_type = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x97b88, false);
                LR2value.gauge_type = sharp.Read<int>((IntPtr)LR2value.baseaddr + 0x8, false);

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
                replace_list["#CLEAR_TYPE#"] = LR2value.str_clear_type[LR2value.clear_type];
                replace_list["#GAUGE_TYPE#"] = LR2value.str_gauge_type[LR2value.gauge_type];


                String tweet_text = text;
                foreach (var replace_key in replace_list.Keys) {
                    tweet_text = tweet_text.Replace(replace_key, replace_list[replace_key]);
                }
                setTweettext(tweet_text);
            }
            LR2value.scene = now_scene;



        }
        private void setTweettext(string text) {
            if (this.textBoxTweettext.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(setTweettext);
                this.Invoke(d, new object[] { text });
            } else {
               
                this.textBoxTweettext.Text = text;
            }
        }
        private void sendTweet(string text) {
            String window_title = getActiveWindowTitle();
            if ((authenticatedUser != null) && (LR2value.scene == 5) && (window_title == "LR2 beta3 version 100201")) {
                if (text.Length < 140) {
                    var window = sharp.Windows.MainWindow;
                    keybd_event((byte)Keys.F6, 0x00, 0x00, 0);
                    delay(200);
                    keybd_event((byte)Keys.F6, 0x00, 0x02, 0);
                    delay(1000);
                    var file = Directory.GetFiles(@process_path, "LR2 *.png").Last();
                    var image = File.ReadAllBytes(file);
                    var media = Upload.UploadImage(image);
                    var tweet = Tweet.PublishTweet(text, new PublishTweetOptionalParameters {
                        Medias = { media }
                    });
                    if (tweet != null) {
                        toolStripStatusLabel1.Text = "Tweet succeeded";
                    } else {
                        toolStripStatusLabel1.Text = "Tweet failed";
                    }
                } else {
                    toolStripStatusLabel1.Text = "Tweet is too long";
                }
            } else {
                toolStripStatusLabel1.Text = "Please run on result screen or twitter isn't authenticated.";
            }
        }
        private void buttonTweetsend_Click(object sender, EventArgs e) {
            if (authenticatedUser != null) {
                if (this.textBoxTweettext.Text.Length < 140) {
                    var file = Directory.GetFiles(@process_path, "*.png").Last();
                    var image = File.ReadAllBytes(file);
                    var media = Upload.UploadImage(image);
                    var tweet = Tweet.PublishTweet(this.textBoxTweettext.Text, new PublishTweetOptionalParameters {
                        Medias = { media }
                    });
                    if (tweet != null) {
                        toolStripStatusLabel1.Text = "Tweet succeeded";
                    } else {
                        toolStripStatusLabel1.Text = "Tweet failed";
                    }
                }
            }
        }
        protected override void WndProc(ref System.Windows.Forms.Message m) {
            base.WndProc(ref m);

            if (m.Msg == 0x0312) {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.
                if (id == 0) {
                    sendTweet(this.textBoxTweettext.Text);
                } else if (id == 1) {
                    getSongstatus(tweet_template_sub,1);
                    sendTweet(this.textBoxTweettext.Text);
                }
            }
        }
        private static DateTime delay(int MS) {
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

}