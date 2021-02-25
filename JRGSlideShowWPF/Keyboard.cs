using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }
                await DisplayFileInfo();
                Interlocked.Exchange(ref OneInt, 0);
            }
            else if (e.Key == Key.Delete || e.Key == Key.D)
            {
                PauseSave(true);
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }
                await DeleteNoInterlock();
                PauseRestore();
                Interlocked.Exchange(ref OneInt, 0);
            }
            else if (e.Key == Key.Insert)
            {
                PauseSave(true);
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }
                Undelete();
                PauseRestore();
                Interlocked.Exchange(ref OneInt, 0);
            }
            else if (e.Key == Key.Enter)
            {
                await DisplayGetNextImage(1);
            }
        }
        public bool displayingInfo = false;
        private async Task DisplayFileInfo()
        {
            if (!ImageListReady)
            {
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }            
            if (displayingInfo == true)
            {
                topTextBoxClass.messageDisplayEndUninterruptable(new Action(() => { }));
                displayingInfo = false;
            }
            else
            {                
                displayingInfo = true;
                updateInfo();
            }
            Interlocked.Exchange(ref OneInt, 0);
        }        
        public void updateInfo()
        {
            if (displayingInfo == false)
            {
                return;
            }
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {
                FileInfo imageInfo = ImageList[ImageIdxList[ImageIdxListDeletePtr]];
                var s = string.Format("{0:0.0}", (decimal)((decimal)imageTimeToDecode.ElapsedTicks / (decimal)10000));
                StringBuilder sb = new StringBuilder();
                sb.Append(
                      "         JRGSlideShowWPF Ver: " + version + System.Environment.NewLine
                    + "                        Name: " + imageInfo.Name + System.Environment.NewLine
                    + "                   Directory: " + imageInfo.DirectoryName + System.Environment.NewLine
                    + "                      Length: " + imageInfo.Length.ToString("N0") + " Bytes" + Environment.NewLine
                    + "          Current Resolution: " + DisplayPicInfoWidth + " x " + DisplayPicInfoHeight + Environment.NewLine
                    + "         Original Resolution: " + imageOriginalWidth + " x " + imageOriginalHeight + Environment.NewLine
                    + "                        DpiX: " + DisplayPicInfoDpiX + Environment.NewLine
                    + "                        DpiY: " + DisplayPicInfoDpiY + Environment.NewLine
                    + "           Mouse Wheel Count: " + MouseWheelCount + Environment.NewLine
                    + "   Mouse Wheel missed OneInt: " + MouseOneIntCount + Environment.NewLine
                    + "             ImageIdxListPtr: " + ImageIdxListPtr + Environment.NewLine
                    + "Image time to decode (ticks): " + imageTimeToDecode.ElapsedTicks + Environment.NewLine
                    + "   Image time to decode (ms): " + s + Environment.NewLine
                    + "                Total Images: " + ImagesNotNull + Environment.NewLine
                    + "     Pictures undelete count: " + DeletedFiles.Count + Environment.NewLine
                    + "             Last Sleep Mode: " + (LastDisplayMode == 0 ? "Display not required." : "Display Required.")
                    );

                topTextBoxClass.messageDisplayStart(sb.ToString(), -1, false, true);
            }
            else
            {
                displayingInfo = false;
            }
        }
    }
}