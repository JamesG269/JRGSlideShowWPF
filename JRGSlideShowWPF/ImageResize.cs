
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

        public MemoryStream memStream;
        public string ErrorMessage = "";

        double widthAspect;
        double heightAspect;

        Stopwatch imageTimeToDecode = new Stopwatch();
        
        BitmapFrame displayPhoto = null;
        TransformedBitmap target = null;
        BitmapDecoder decoder = null;
        BitmapFrame photo = null;

        byte[] readBuf = new byte[1000000];

        public void ResizeImageCode()
        {            
            ImageReadyToDisplay = true;
            ImageError = false;            
            GetMaxSize();
            bool DisplayedMessage = false;
            try
            {
                DisplayedMessage = false;
                ErrorMessage = "Resize Error.";
                FileInfo fileInfo = ImageList[ImageIdxList[ImageIdxListPtr]];
                decoder = null;
                
                if (fileInfo.Length > 20000000)
                {
                    DisplayedMessage = true;
                    topTextBoxClass.messageDisplayStart("Loading large image : " + fileInfo.Name + " Length: " + fileInfo.Length.ToString("N0") + " Bytes", -1, false, true);
                    decoder = LoadLargeImage(fileInfo);
                }
                else
                {
                    memStream = new MemoryStream(File.ReadAllBytes(fileInfo.FullName));
                    memStream.Seek(0,SeekOrigin.Begin);

                    decoder = BitmapDecoder.Create(memStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand);
                }                
                photo = decoder.Frames[0];
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
                target = new TransformedBitmap(photo, new ScaleTransform(widthAspect, heightAspect,0,0));                
                displayPhoto = BitmapFrame.Create(target);
                photo = null;
                decoder = null;
                target = null;
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
                if (memStream != null)
                {
                    memStream.Dispose();
                }
                memStream = null;
                ImageError = true;                
                displayPhoto = null;
                photo = null;
                decoder = null;
                target = null;
                string srcName = Path.GetFileName(ImageList[ImageIdxList[ImageIdxListPtr]].FullName);
                string destName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), srcName );
                try
                {
                    //File.Copy(ImageList[ImageIdxList[ImageIdxListPtr]].FullName, destName);
                    ErrorMessage = srcName + " " + ErrorMessage + " Exception details: " + e.Message;
                }
                catch
                {
                    ErrorMessage = srcName + " " + ErrorMessage + " Exception details: " + e.Message;
                }                
            }
            finally
            {
                if (DisplayedMessage)
                {
                    topTextBoxClass.messageDisplayEndUninterruptable(new Action(() =>
                    {
                        progressBar.Visibility = Visibility.Hidden;
                    }));                    
                }
            }
        }

        private BitmapDecoder LoadLargeImage(FileInfo fileInfo)
        {
            BitmapDecoder decoder;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progressBar.Value = 0;                
                progressBar.Visibility = Visibility.Visible;                
            }));            
            GC.Collect();
            FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

            memStream = new MemoryStream();
            int readLen = 1000000;
            int readTotal = 0;
            while (readTotal < fileInfo.Length && readLen > 0)
            {
                int n = fileStream.Read(readBuf, 0, readLen);
                memStream.Write(readBuf, 0, n);
                readTotal += n;
                if ((readTotal + readLen) >= fileInfo.Length)
                {
                    readLen = (int)fileInfo.Length - readTotal;
                }
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progressBar.Value = Convert.ToInt32(Math.Ceiling(100d * readTotal / (int)fileInfo.Length));
                }));
            }
            fileStream.Close();
            fileStream.Dispose();
            GC.Collect();
            memStream.Seek(0, SeekOrigin.Begin);
            decoder = BitmapDecoder.Create(memStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);                        
            return decoder;
        }
    }
}
 