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
        Bitmap simple_result;
        //속성값. 기본으로 지정
        Color simple_box_color;
        Byte simple_box_alpha;
        Color simple_font_color;
        Byte simple_font_alpha;
        String simple_font_name;

        static Dictionary<String, SimpleResultItem> simple_result_items = new Dictionary<string, SimpleResultItem>();



        public void initSimpleresult() {

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
            var image_list = Directory.GetFiles(Application.StartupPath+"\\images","normal_"+image_clear_type+"_*.png");
            //이미지를 랜덤으로 고르기 위해..
            var image = image_list[new Random().Next(0, image_list.Length)];


            //이미지 이름 꺼내오기
            var image_name = image.Replace(Application.StartupPath + "\\images\\normal_" + image_clear_type + "_","");
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

            pictureBoxSimpleresult.Load(image);
            simple_result = (Bitmap)pictureBoxSimpleresult.Image;

           

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

            pictureBoxSimpleresult.Image = simple_result;


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
               tr =  Color.FromArgb(255, backColor.R+1, backColor.G, backColor.B);
            } catch (Exception) {
                tr = Color.FromArgb(255, backColor.R-1, backColor.G, backColor.B);
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
            if (text =="1") { // 1만 나오면 옆으로 붙는거 보정
                maxWidth = maxWidth+(maxWidth/8);
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
    }

}
