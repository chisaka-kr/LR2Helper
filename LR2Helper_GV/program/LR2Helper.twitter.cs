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
    public partial class LR2helper {

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
                replace_list["#CLEAR_TYPE#"] = LR2value.text_clear_type[LR2value.play_clear_type];
                replace_list["#GAUGE_TYPE#"] = LR2value.text_gauge_type[LR2value.play_gauge_type];
                replace_list["#DJ_LEVEL#"] = LR2value.text_djlevel[LR2value.play_djlevel];
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

    }
}
