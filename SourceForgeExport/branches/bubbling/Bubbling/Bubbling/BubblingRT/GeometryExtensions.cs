using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BubblingRT;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Bubbling
{
    static class GeometryExtensions
    {
        public static bool InPolygon(this Point testPoint,IList<Point> polygon)
        {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static double SqDistance(this Point src,Point dst)
        {
            var dx = src.X - dst.X;
            var dy = src.Y - dst.Y;
            return dx*dx + dy*dy;
        }

        public static double SqDistance(this Point src, double dstx,double dsty)
        {
            var dx = src.X - dstx;
            var dy = src.Y - dsty;
            return dx * dx + dy * dy;
        }

        public static WriteableBitmap Paint(WriteableBitmap wbm, List<Circle> circles, double xScale, double yScale,out bool isTrial)
        {
            isTrial = new LicenseInformation().IsTrial;
            var res = new WriteableBitmap(wbm.PixelWidth,wbm.PixelHeight);
            Color color = BubblingSettings.Instance.Color;
            var icolor = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B; 

            

            foreach (var c in circles)
            {
                res.FillEllipse((int)((c.X - c.R) / xScale), (int)((c.Y - c.R) / yScale), (int)((c.X + c.R) / xScale), (int)((c.Y + c.R) / yScale), 12345);
                res.DrawEllipse((int)((c.X - c.R) / xScale), (int)((c.Y - c.R) / yScale), (int)((c.X + c.R) / xScale), (int)((c.Y + c.R) / yScale), 12346);
            }
            using (BitmapContext rc = WriteableBitmapContextExtensions.GetBitmapContext(res))
            using (BitmapContext wc = WriteableBitmapContextExtensions.GetBitmapContext(wbm))
            for (int i = 0; i < wbm.PixelBuffer.Length; i++)
            {
                if (rc.Pixels[i] == 12345)
                {
                    rc.Pixels[i] = wc.Pixels[i];
                }
                else if (rc.Pixels[i] == 12346)
                {
                    var c = wc.Pixels[i];
                    var cl = Color.FromArgb((byte) (c >> 24), (byte) (c >> 16), (byte) (c >> 8), (byte) (c));
                    rc.Pixels[i] = (((color.A + cl.A) / 2) << 24) | (((color.R + cl.R) / 2) << 16) | (((color.G + cl.G) / 2) << 8) | ((color.B + cl.B) / 2);
                }
                else
                {
                    rc.Pixels[i] = icolor;
                }
            }

            if (isTrial)
            {
                res.DrawText(new Point(0, wbm.PixelHeight - 28), "Bubbled with trial BUBBLING UNDRESSER WP App. Please consider buying app", 24, Colors.Blue);
            }

            res.Invalidate();
            UpdateTile(res);
            return res;
        }

        internal static void UpdateTile(WriteableBitmap bmp)
        {/*
            var firstTile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri.ToString() == "/");
            if (firstTile != null)
            {
                String fileName = String.Format("/Shared/ShellContent/Tile.jpg");

                var myStore = IsolatedStorageFile.GetUserStoreForApplication();

                if (myStore.FileExists(fileName))
                {
                    myStore.DeleteFile(fileName);
                }

                var myFileStream = myStore.CreateFile(fileName);
                double hw = bmp.PixelHeight/(double) bmp.PixelWidth;
                int w, h;
                if (hw > 1)
                {
                    h = 336;
                    w = (int) (h/hw);
                }
                else
                {
                    w = 336;
                    h = (int) (w*hw);
                }

                bmp.SaveJpeg(myFileStream, w, h, 0, 95);
                myFileStream.Close();

                Uri uri = new Uri("isostore:" + fileName, UriKind.Absolute);
                
                firstTile.Update(new FlipTileData
                {
                    Title = "",
                    BackgroundImage = uri,
                    BackContent = AppResources.GeometryExtensions_UpdateTile_BUBBLING_UNDRESSER
                });
            }*/
        }

        public static void DrawText(this WriteableBitmap wBmp, Point at, string text, double fontSize, Color textColor)
        {
            TextBlock lbl = new TextBlock();
            lbl.Text = text;
            lbl.FontSize = fontSize;
            lbl.Foreground = new SolidColorBrush(textColor);
            WriteableBitmap tBmp = new WriteableBitmap((int) lbl.ActualWidth, (int) lbl.ActualHeight);
            wBmp.Blit(at, tBmp, new Rect(0, 0, tBmp.PixelWidth, tBmp.PixelHeight), Colors.White, WriteableBitmapExtensions.BlendMode.Alpha);
        }
    }
}