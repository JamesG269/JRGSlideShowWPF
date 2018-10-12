using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        int DisplayPicInfoHeight = 0;
        int DisplayPicInfoWidth = 0;
        int DisplayPicInfoDpiX = 0;
        int DisplayPicInfoDpiY = 0;
        int ResizeMaxWidth = 0;
        int ResizeMaxHeight = 0;

        Boolean ImageError = false;

        public string ErrorMessage = "";

        public void ResizeImageCode()
        {
            string imageFileName = ImageList[ImageIdxList[ImageIdxListPtr]];
            bitmapImage = new BitmapImage();
            GetMaxSize();
            try
            {                
                ImageWhenReady = true;
                ImageError = false;
                bitmapImage.BeginInit();                
                bitmapImage.StreamSource = new FileStream(imageFileName, FileMode.Open, FileAccess.Read);
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.DecodePixelHeight = ResizeMaxHeight;                
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                DisplayPicInfoHeight = bitmapImage.PixelHeight;
                DisplayPicInfoWidth = bitmapImage.PixelWidth;
                DisplayPicInfoDpiX = (int)bitmapImage.DpiX;
                DisplayPicInfoDpiY = (int)bitmapImage.DpiY;
                GC.Collect();
                
                if (DisplayPicInfoDpiX == DisplayPicInfoDpiY)
                {
                    return;
                }
                ImageError = true;
                ErrorMessage = " DPI Error.";                
            }
            catch
            {
                ImageError = true;
                ErrorMessage = " Resize Error.";                
            }                                    
            if (bitmapImage != null && bitmapImage.StreamSource != null)
            {
                bitmapImage.StreamSource.Dispose();
            }
            bitmapImage = null;
            string destName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.GetFileName(imageFileName));
            try
            {
                File.Copy(imageFileName, destName);
                ErrorMessage = destName + ErrorMessage + " Copied successfully.";
            }
            catch
            {
                ErrorMessage = destName + ErrorMessage + " Copy error.";
            }
        }        
    }
}