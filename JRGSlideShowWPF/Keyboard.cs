﻿using System;
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
                DisplayFileInfo();
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
        public bool displayingInfo = false;
        private void DisplayFileInfo()
        {
            if (!ImageListReady)
            {
                return;
            }
            if (displayingInfo == true && TextBlockControl.Visibility != Visibility.Hidden)
            {
                TextBlockControl.Visibility = Visibility.Hidden;
                displayingInfo = false;
            }
            else
            {
                TextBlockControl.Visibility = Visibility.Visible;
                displayingInfo = true;
                updateInfo();
            }
        }
        public void updateInfo()
        {
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {
                FileInfo imageInfo = ImageList[ImageIdxList[ImageIdxListDeletePtr]];

                StringBuilder sb = new StringBuilder();
                sb.Append(
                      "      JRGSlideShowWPF Ver: " + version + System.Environment.NewLine
                    + "                     Name: " + imageInfo.Name + System.Environment.NewLine
                    + "                   Length: " + imageInfo.Length.ToString("N0") + " Bytes" + Environment.NewLine
                    + "       Current Resolution: " + DisplayPicInfoWidth + " x " + DisplayPicInfoHeight + Environment.NewLine
                    + "      Original Resolution: " + imageOriginalWidth + " x " + imageOriginalHeight + Environment.NewLine
                    + "                     DpiX: " + DisplayPicInfoDpiX + Environment.NewLine
                    + "                     DpiY: " + DisplayPicInfoDpiY + Environment.NewLine
                    + "        Mouse Wheel Count: " + MouseWheelCount + Environment.NewLine
                    + "Mouse Wheel missed OneInt: " + MouseOneIntCount + Environment.NewLine
                    + "          ImageIdxListPtr: " + ImageIdxListPtr + Environment.NewLine
                    + "     Image time to decode: " + imageTimeToDecode.ElapsedTicks + Environment.NewLine
                    + "             Total Images: " + ImagesNotNull);

                TextBlockControl.Text = sb.ToString();
            }
        }
    }
}