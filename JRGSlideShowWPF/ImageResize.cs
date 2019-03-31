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
        int ResizeMaxWidth = 0;
        int ResizeMaxHeight = 0;

        Boolean ImageError = false;

        FileStream fileStream;
        
        public string ErrorMessage = "";

        public void ResizeImageCode()
        {
            ImageReady = true;
            ImageError = false;            
            bitmapImage = new BitmapImage();
            try
            {                
                ErrorMessage = "Resize Error.";
                bitmapImage.BeginInit();
                fileStream = new FileStream(ImageList[ImageIdxList[ImageIdxListPtr]], FileMode.Open, FileAccess.Read);
                bitmapImage.StreamSource = fileStream;                
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