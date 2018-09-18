using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private void ContextMenuOpenFolder(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt,1))
            {
                return;
            }
            OpenImageDirectory();
            StartGetFilesBW.RunWorkerAsync();
        }
        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ContextMenuNext(object sender, RoutedEventArgs e)
        {
            displayNextImage();
        }

        private void ContextMenuPrev(object sender, RoutedEventArgs e)
        {
            displayPrevImage();
        }
        private void ContextMenuPause(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void ContextMenuPlay(object sender, RoutedEventArgs e)
        {
            Unpause();
        }

        private void ContextMenuCopyDelete(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            CopyDeleteCode();
            Interlocked.Exchange(ref OneInt, 0);                
        }
        private void CopyDeleteCode()
        {
            if (ImageListDeletePtr == -1)
            {
                return;
            }
            try
            {
                string destPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                destPath = Path.Combine(destPath, Path.GetFileName(ImageList[ImageListDeletePtr]));
                File.Copy(ImageList[ImageListDeletePtr], destPath);
                MessageBox.Show("Image copied to " + destPath);
                DeleteCode();
            }
            catch
            {
                MessageBox.Show("Error: image not copied to desktop.");
            }
        }

        private void ContextMenuDelete(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            DeleteCode();
            Interlocked.Exchange(ref OneInt, 0);

        }
        private void DeleteCode()
        {
            Pause();
            if (ImageListDeletePtr == -1)
            {
                return;
            }
            var result = MessageBox.Show("Delete " + ImageList[ImageListDeletePtr] + " ?", "Confirm delete image.", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (bitmapImage != null && bitmapImage.StreamSource != null)
                    {
                        bitmapImage.StreamSource.Dispose();                        
                        bitmapImage = null;
                        File.Delete(ImageList[ImageListDeletePtr]);
                        MessageBox.Show("Image deleted.");
                    }
                }
                catch
                {
                    MessageBox.Show("Error: Could not delete image.");
                }
            }
            Unpause();
        }

        private void ContextMenuChangeTimer(object sender, RoutedEventArgs e)
        {
            ChangeTimerCode();
        }
        private void ChangeTimerCode()
        {
            SlideShowTimer SlideShowTimerWindow = new SlideShowTimer
            {
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
            };
            SlideShowTimerWindow.Closed += new EventHandler(ContextMenuChangeTimerClosed);
            SlideShowTimerWindow.TimerTextBox.Text = dispatcherTimerSlow.Interval.Seconds.ToString();            
            SlideShowTimerWindow.Show();
        }
        
        private void ContextMenuChangeTimerClosed(object sender, EventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt,1))
            {
                return;
            }
            var SlideShowTimerWindow = sender as SlideShowTimer;
            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            Boolean b = dispatcherTimerSlow.IsEnabled;
            dispatcherTimerSlow.Stop();
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherTimerSlow.Interval = new TimeSpan(0, 0, 0, i, c);
            if (b == true)
            {
                dispatcherTimerSlow.Start();
            }
            this.Focus();
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void ContextMenuFullScreen(object sender, RoutedEventArgs e)
        {
            maximize();
        }

        private void CheckedRandomize(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            Randomize = ContextMenuCheckBox.IsChecked;            
            RandomizeBW.RunWorkerAsync();
        }
        
        private void RandomizeBW_DoWork(object sender, DoWorkEventArgs e)
        {
            Pause();            
            ImageListReady = false;
            ImageWhenReady = false;
            CreateIdxListCode();
            ResizeImageCode();
            ImageListReady = true;
            ImageWhenReady = true;   
            
            Interlocked.Exchange(ref OneInt, 0);
            Unpause();
        }
    }
}