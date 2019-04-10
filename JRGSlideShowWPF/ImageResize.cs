using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        int DisplayPicInfoHeight = 0;
        int DisplayPicInfoWidth = 0;
        int DisplayPicInfoDpiX = 0;
        int DisplayPicInfoDpiY = 0;

        double ScreenMaxWidth = 0;
        double ScreenMaxHeight = 0;
        int imageOriginalWidth = 0;
        int imageOriginalHeight = 0;

        Boolean ImageError = false;

        FileStream fileStream;
        
        public string ErrorMessage = "";

        public void ResizeImageCode()
        {
            ImageReady = true;
            ImageError = false;            
            bitmapImage = new BitmapImage();
            GetMaxSize();
            try
            {
                ErrorMessage = "Resize Error.";
                fileStream = new FileStream(ImageList[ImageIdxList[ImageIdxListPtr]], FileMode.Open, FileAccess.Read);
                var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                double iHeight = decoder.Frames[0].PixelHeight;
                double iWidth = decoder.Frames[0].PixelWidth;
                imageOriginalHeight = (int)iHeight;
                imageOriginalWidth = (int)iWidth;
                fileStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fileStream;
                if (iWidth > ScreenMaxWidth || iHeight > ScreenMaxHeight)
                {
                    double aspect = iWidth / iHeight;                    
                    if (iWidth > ScreenMaxWidth)
                    {
                        iWidth = ScreenMaxWidth;
                        iHeight = iWidth / aspect;
                    }
                    if (iHeight > ScreenMaxHeight)
                    {
                        aspect = iWidth / iHeight;
                        iHeight = ScreenMaxHeight;
                        iWidth = iHeight * aspect;
                    }
                    bitmapImage.DecodePixelHeight = (int)iHeight;
                    bitmapImage.DecodePixelWidth = (int)iWidth;
                }
                if (fileStream.Length > 20000000)
                {
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                }                
                bitmapImage.EndInit();
                bitmapImage.Freeze();                
                if (bitmapImage.DpiX != bitmapImage.DpiY)
                {
                    ErrorMessage = "DPI Error.";
                    throw new Exception();
                } 
            }
            catch
            {
                ImageError = true;                
                if (bitmapImage != null && bitmapImage.StreamSource != null)
                {
                    bitmapImage.StreamSource.Dispose();
                }
                bitmapImage = null;

                string destName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.GetFileName(ImageList[ImageIdxList[ImageIdxListPtr]]));
                try
                {                    
                    File.Copy(ImageList[ImageIdxList[ImageIdxListPtr]], destName);
                    ErrorMessage = destName + " " + ErrorMessage + " Copied successfully.";
                }
                catch
                {
                    ErrorMessage = destName + " " + ErrorMessage + " Copy error.";
                }
            }
            
        }        
    }
}