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

        MemoryStream memStream;
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

                memStream = new MemoryStream(File.ReadAllBytes(ImageList[ImageIdxList[ImageIdxListPtr]].FullName));
                memStream.Position = 0;

                var decoder = BitmapDecoder.Create(memStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand);
                
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
            catch (Exception e)
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
                    ErrorMessage = destName + " " + ErrorMessage + " Copied successfully. Exception details: " + e.Message;
                }
                catch
                {
                    ErrorMessage = destName + " " + ErrorMessage + " Copy error. Exception details: " + e.Message;
                }
            }
            //GC.Collect();
            /*
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.DecodePixelHeight = 1080;
                bitmapImage.StreamSource = memStream;
                bitmapImage.CacheOption = BitmapCacheOption.None;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                DisplayPicInfoDpiX = (int)bitmapImage.DpiX;
                DisplayPicInfoDpiY = (int)bitmapImage.DpiY;
                DisplayPicInfoHeight = bitmapImage.PixelHeight;
                DisplayPicInfoWidth = bitmapImage.PixelWidth;
                return;*/
        }
    }
}
 