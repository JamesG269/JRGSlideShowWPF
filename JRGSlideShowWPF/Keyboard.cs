﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                DisplayFileInfo();    
            }
            if (e.Key == Key.Delete)
            {
                if (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    return;
                }
                DeleteCode();
                Interlocked.Exchange(ref OneInt, 0);
            }
        }

        private void DisplayFileInfo(Boolean DpiError = false)
        {
            FileInfo imageInfo = null;
            try
            {
                imageInfo = new FileInfo(ImageList[ImageIdxList[ImageIdxListPtr]]);
            }
            catch {
                MessageBox.Show("Error: could not execute FileInfo on image.");
            }
            var imageName = imageInfo.Name;

            MessageBox.Show((DpiError == true ? "DPI ERROR" + Environment.NewLine : "") + "Name: " + imageName + System.Environment.NewLine
                + "Length: " + imageInfo.Length + Environment.NewLine
                + "Height: " + DisplayPicInfoHeight + Environment.NewLine
                + " Width: " + DisplayPicInfoWidth + Environment.NewLine
                + "  DPIx: " + DisplayPicInfoDPIx + Environment.NewLine
                + "  DPIy: " + DisplayPicInfoDPIy + Environment.NewLine
                + "Mouse Wheel Count: " + MouseWheenCount + Environment.NewLine
                + "Mouse Wheel with OneInt: " + MouseOneIntCount
                );            
        }
    }
}