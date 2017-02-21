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

        public void initSimpleresult() {

            string image_name = "clear";

            //클리어했는지, 풀콤했는지 확인
            switch (LR2value.play_clear_type) {
                case 5: //풀콤
                    image_name = "clear";
                    break;
                case 1: //페일
                    image_name = "fail";
                    break;
                default: //그 외
                    image_name = "clear";
                    break;

            }
            var image_list = Directory.GetFiles(Application.StartupPath+"\\images","normal_"+image_name+"_*.png");
            //이미지를 랜덤으로 고르기 위해..
            var image = image_list[new Random().Next(0, image_list.Length)];
        

            pictureBoxSimpleresult.Load(image);
            simple_result = (Bitmap)pictureBoxSimpleresult.Image;

            String font_name = "Arial";

            //DrawImage(font_image, simple_result, 5, 150, 255,0);
            DrawBox(10, 81, 270, 15, 200, Color.Black); // 장르
            DrawImage((Bitmap)GetImageText(LR2value.music_genre, new Font(font_name, 8f), Color.White, Color.Black), simple_result, 143, 86, 200, 0, 1, 1);
            DrawBox(10, 98, 270, 30, 200, Color.Black); // 곡명
            DrawImage((Bitmap)GetImageText(LR2value.music_name, new Font(font_name, 13f), Color.White, Color.Black), simple_result, 143, 111, 200, 0, 1, 1);
            DrawBox(10, 130, 270, 15, 200, Color.Black); // 아티스트
            DrawImage((Bitmap)GetImageText(LR2value.music_artist, new Font(font_name, 8f), Color.White, Color.Black), simple_result, 143, 135, 200, 0, 1, 1);
            DrawBox(10, 147, 200, 24, 200, Color.Black); // 클리어
            DrawImage((Bitmap)GetImageText(LR2value.str_clear_type[LR2value.play_clear_type], new Font(font_name, 15f), Color.White, Color.Black), simple_result, 104, 155, 200, 0, 1, 1);

            var level = "";
            if (Convert.ToInt16(LR2value.music_diff_level) > 0) {
                level = "☆" + LR2value.music_diff_level;
            }
            if (Convert.ToInt16(LR2value.music_diff_hakko) > 0) {
                level = "★" + LR2value.music_diff_hakko;
            }

            DrawBox(285, 81, 110, 30, 200, Color.Black); // 장르
            DrawImage((Bitmap)GetImageText(level, new Font(font_name, 15f), Color.White, Color.Black), simple_result, 339, 92, 200, 0, 1, 1);

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
                DrawBox(10, 377 + i * 26, 200, 24, 200, Color.Black); // DJ LEVEL 등
                DrawImage((Bitmap)GetImageText(box_name[i], new Font(font_name, 16f), Color.White, Color.Black), simple_result, 8, 386 + i * 26, 200, 0, 0, 1);
                DrawImage((Bitmap)GetImageText(box_value[i], new Font(font_name, 16f), Color.White, Color.Black), simple_result, 205, 386 + i * 26, 200, 0, 2, 1);
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

            drawing.SmoothingMode = SmoothingMode.AntiAlias;
            //drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;

            SolidBrush brushWhite = new SolidBrush(Color.Black);
            drawing.FillRectangle(brushWhite, 0, 0,
            this.ClientSize.Width, this.ClientSize.Height);

            FontFamily fontFamily = new FontFamily("Arial");
            StringFormat strformat = new StringFormat();

            GraphicsPath path = new GraphicsPath();
            path.AddString(text, fontFamily,
                (int)FontStyle.Bold, font.Size, new Point(7, 7), strformat);
            Pen pen = new Pen(Color.FromArgb(0, 0, 0), 5);
            pen.LineJoin = LineJoin.Round;
            drawing.DrawPath(pen, path);
            LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(10, 10, 30, 70),
                Color.FromArgb(245, 245, 245),
                Color.FromArgb(254, 254, 254), LinearGradientMode.Vertical);
            drawing.FillPath(brush, path);

            //그린 폰트의 사이즈를 다시 구한다
            var maxHeight = 0;
            var maxWidth = 0;
            Color clr;
            for (int py = 0; py < img.Height; py++) {
                for (int px = 0; px < img.Width; px++) {
                    clr = img.GetPixel(px, py);

                    if ((clr.R != 0) || (clr.B != 0) || (clr.B != 0)) {
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
            brushWhite.Dispose();
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
    }

}
