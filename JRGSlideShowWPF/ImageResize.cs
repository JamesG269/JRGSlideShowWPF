using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        double ScreenMaxWidth = 0;
        double ScreenMaxHeight = 0;
        double imageOriginalWidth = 0;
        double imageOriginalHeight = 0;

        Boolean ImageError = false;

        FileStream fileStream;

        public string ErrorMessage = "";

        double widthAspect;
        double heightAspect;

        Stopwatch imageTimeToDecode = new Stopwatch();
        
        BitmapFrame displayPhoto;

        public void ResizeImageCode()
        {
            ImageReady = true;
            ImageError = false;
            
            GetMaxSize();
            try
            {
                ErrorMessage = "Resize Error.";
                fileStream = new FileStream(ImageList[ImageIdxList[ImageIdxListPtr]].FullName, FileMode.Open, FileAccess.Read);
                var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand);
                
                var photo = decoder.Frames[0];
                imageOriginalHeight = photo.PixelHeight;
                imageOriginalWidth = photo.PixelWidth;
                widthAspect = ScreenMaxWidth / imageOriginalWidth;
                heightAspect = ScreenMaxHeight / imageOriginalHeight;
                if (widthAspect > heightAspect)
                {
                    widthAspect = heightAspect;
                }
                else
                {
                    heightAspect = widthAspect;
                }
                var target = new TransformedBitmap(photo, new ScaleTransform(widthAspect, heightAspect,0,0));                
                displayPhoto = BitmapFrame.Create(target);
                displayPhoto.Freeze();
                DisplayPicInfoDpiX = (int)displayPhoto.DpiX;
                DisplayPicInfoDpiY = (int)displayPhoto.DpiY;
                DisplayPicInfoHeight = displayPhoto.PixelHeight;
                DisplayPicInfoWidth = displayPhoto.PixelWidth;
                
                if (DisplayPicInfoDpiX != DisplayPicInfoDpiY)
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

                string destName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.GetFileName(ImageList[ImageIdxList[ImageIdxListPtr]].FullName));
                try
                {
                    File.Copy(ImageList[ImageIdxList[ImageIdxListPtr]].FullName, destName);
                    ErrorMessage = destName + " " + ErrorMessage + " Copied successfully.";
                }
                catch
                {
                    ErrorMessage = destName + " " + ErrorMessage + " Copy error.";
                }
            }
            //GC.Collect();
        }
    }
}