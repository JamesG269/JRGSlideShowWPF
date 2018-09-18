using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        int DisplayPicInfoHeight = 0;
        int DisplayPicInfoWidth = 0;
        int DisplayPicInfoDPIx = 0;
        int DisplayPicInfoDPIy = 0;
        int ResizeMaxWidth = 0;
        int ResizeMaxHeight = 0;

        Boolean ImageError = false;
        
        public void ResizeImageCode()
        {
            string imageFileName = ImageList[ImageIdxList[ImageIdxListPtr]];
            bitmapImage = new BitmapImage();
            try
            {                
                bitmapImage.BeginInit();                
                bitmapImage.StreamSource = new FileStream(imageFileName, FileMode.Open, FileAccess.Read);
                bitmapImage.DecodePixelWidth = ResizeMaxWidth;                             
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                DisplayPicInfoHeight = bitmapImage.PixelHeight;
                DisplayPicInfoWidth = bitmapImage.PixelWidth;
                DisplayPicInfoDPIx = (int)bitmapImage.DpiX;
                DisplayPicInfoDPIy = (int)bitmapImage.DpiY;

                ImageError = false;
            }
            catch
            {
                bitmapImage = null;                
                string destName = @"c:\users\jgentile\desktop\broke\" + Path.GetFileName(imageFileName);
                try
                {
                    File.Copy(imageFileName, destName);
                }
                catch { }
                if (File.Exists(destName))
                {
                    MessageBox.Show(destName + " is broken, copied successfully");
                }
                else
                {
                    MessageBox.Show(destName + " is broken, FAILED to copy.");
                }
            }
        }

    }
}