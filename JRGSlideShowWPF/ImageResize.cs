using System;
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
        int ResizeMaxWidth = 0;
        int ResizeMaxHeight = 0;

        Boolean ImageError = false;

        FileStream fileStream;

        public string ErrorMessage = "";

        public void ResizeImageCode()
        {
            string imageFileName = ImageList[ImageIdxList[ImageIdxListPtr]];
            bitmapImage = new BitmapImage();
            GetMaxSize();
            try
            {
                ImageReady = true;
                ImageError = false;
                ErrorMessage = " Resize Error.";
                bitmapImage.BeginInit();
                fileStream = new FileStream(imageFileName, FileMode.Open, FileAccess.Read);

                bitmapImage.StreamSource = fileStream;
                if (fileStream.Length > 20000000)
                {
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                }
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                GC.Collect();
                DisplayPicInfoHeight = bitmapImage.PixelHeight;
                DisplayPicInfoWidth = bitmapImage.PixelWidth;
                DisplayPicInfoDpiX = (int)bitmapImage.DpiX;
                DisplayPicInfoDpiY = (int)bitmapImage.DpiY;
                
                if (DisplayPicInfoDpiX != DisplayPicInfoDpiY)
                {
                    ErrorMessage = " DPI Error.";
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
}