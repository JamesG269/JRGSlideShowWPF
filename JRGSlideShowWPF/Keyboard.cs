using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                DisplayFileInfo();
            }
            if (e.Key == Key.Delete)
            {
                PauseSave();
                if (0 == Interlocked.Exchange(ref OneInt, 1))
                {
                    DeleteNoInterlock();
                    Interlocked.Exchange(ref OneInt, 0);
                }
                PauseRestore();
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

                    MessageBox.Show((DpiError == true ? "DPI ERROR" + Environment.NewLine : "")
                        + "                     Name: " + imageName + System.Environment.NewLine
                        + "                   Length: " + imageInfo.Length + Environment.NewLine
                        + "                   Height: " + DisplayPicInfoHeight + Environment.NewLine
                        + "                    Width: " + DisplayPicInfoWidth + Environment.NewLine
                        + "                     DpiX: " + DisplayPicInfoDpiX + Environment.NewLine
                        + "                     DpiY: " + DisplayPicInfoDpiY + Environment.NewLine
                        + "        Mouse Wheel Count: " + MouseWheelCount + Environment.NewLine
                        + "Mouse Wheel missed OneInt: " + MouseOneIntCount + Environment.NewLine
                        + "          ImageIdxListPtr: " + ImageIdxListPtr + Environment.NewLine
                        + "             Total Images: " + ImageList.Count
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