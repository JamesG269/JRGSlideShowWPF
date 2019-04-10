using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                PauseSave();
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }
                DisplayFileInfo();
                PauseRestore();
                Interlocked.Exchange(ref OneInt, 0);
            }
            else if (e.Key == Key.Delete || e.Key == Key.D)
            {
                PauseSave();
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }                
                DeleteNoInterlock();
                PauseRestore();
                Interlocked.Exchange(ref OneInt, 0);                
            }
        }

        private void DisplayFileInfo(Boolean DpiError = false)
        {
            if (!ImageListReady)
            {
                return;
            }            
            PauseSave();
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {                
                FileInfo imageInfo = null;

                try
                {
                    imageInfo = new FileInfo(ImageList[ImageIdxList[ImageIdxListDeletePtr]]);
                    var imageName = imageInfo.Name;
                    DisplayPicInfoDpiX = (int)bitmapImage.DpiX;
                    DisplayPicInfoDpiY = (int)bitmapImage.DpiY;
                    DisplayPicInfoHeight = bitmapImage.PixelHeight;
                    DisplayPicInfoWidth = bitmapImage.PixelWidth;

                    MessageBox.Show((DpiError == true ? "DPI ERROR" + Environment.NewLine : "")
                        + "      JRGSlideShowWPF Ver: " + version + System.Environment.NewLine
                        + "                     Name: " + imageName + System.Environment.NewLine
                        + "                   Length: " + imageInfo.Length + Environment.NewLine
                        + "           Current Height: " + DisplayPicInfoHeight + Environment.NewLine
                        + "            Current Width: " + DisplayPicInfoWidth + Environment.NewLine
                        + "          Original Height: " + imageOriginalHeight + Environment.NewLine
                        +"            Original Width: " + imageOriginalWidth + Environment.NewLine
                        + "                     DpiX: " + DisplayPicInfoDpiX + Environment.NewLine
                        + "                     DpiY: " + DisplayPicInfoDpiY + Environment.NewLine
                        + "        Mouse Wheel Count: " + MouseWheelCount + Environment.NewLine
                        + "Mouse Wheel missed OneInt: " + MouseOneIntCount + Environment.NewLine
                        + "          ImageIdxListPtr: " + ImageIdxListPtr + Environment.NewLine
                        + "             Total Images: " + ImageList.Length
                        );
                }
                catch
                {
                    MessageBox.Show("Error: could not execute FileInfo on image.");
                }                
            }            
            PauseRestore();            
        }
    }
}