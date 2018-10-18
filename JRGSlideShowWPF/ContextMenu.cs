using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Shell32;
using System.Threading.Tasks;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {

        public static Shell shell = new Shell();

        public static Folder RecyclingBin = shell.NameSpace(10);

        private async void ContextMenuOpenFolder(object sender, RoutedEventArgs e)
        {
            await OpenDirCheckCancel();
        }

        int StartGetFilesBW_IsBusy = 0;
        Boolean StartGetFilesBW_Cancel = false;
        private async Task<Boolean> OpenDirCheckCancel()
        {            
            StartGetFilesBW_Cancel = true;
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(25);
            }
            if (0 == Interlocked.Exchange(ref OneInt, 1))
            {
                await OpenDir();
                Interlocked.Exchange(ref OneInt, 0);
            }
            Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 0);            
            return true;
        }
        private async Task<Boolean> OpenDir()
        {
            if (SlideShowDirectory != null && SlideShowDirectory != "" && Directory.Exists(SlideShowDirectory))
            {
                dialog.SelectedPath = SlideShowDirectory;
            }            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SlideShowDirectory = dialog.SelectedPath;
                await Task.Run(() => StartGetFiles());
                DisplayCurrentImage();
            }
            return true;
        }
        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async void ContextMenuNext(object sender, RoutedEventArgs e)
        {
            await DisplayNextImage();
        }

        private async void ContextMenuPrev(object sender, RoutedEventArgs e)
        {
            await DisplayPrevImage();
        }
        private void ContextMenuPause(object sender, RoutedEventArgs e)
        {
            PauseSave();
        }

        private void ContextMenuPlay(object sender, RoutedEventArgs e)
        {
            PauseRestore();
        }

        private void ContextMenuCopyDelete(object sender, RoutedEventArgs e)
        {
            CopyDeleteCode();
        }
        private void CopyDeleteCode()
        {

            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            PauseSave();
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] == -1)
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
                if (bitmapImage != null && bitmapImage.StreamSource != null)
                {
                    bitmapImage.StreamSource.Dispose();
                }
                string destPath = "";
                string sourcePath = ImageList[ImageIdxList[ImageIdxListDeletePtr]];
                try
                {
                    destPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    destPath = Path.Combine(destPath, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, destPath);
                    MessageBox.Show("Image copied to " + destPath);
                    DeleteNoInterlock();
                }
                catch
                {
                    MessageBox.Show("Error: image not copied to " + destPath);
                }
            }
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void ContextMenuDelete(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            PauseSave();
            DeleteNoInterlock();
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void DeleteNoInterlock()
        {
            if (ImageIdxListDeletePtr == -1 || ImageIdxList[ImageIdxListDeletePtr] == -1)
            {
                return;
            }

            var fileName = ImageList[ImageIdxList[ImageIdxListDeletePtr]];
            var result = MessageBox.Show("Confirm delete: " + fileName, "Confirm delete image.", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                    }
                    if (bitmapImage != null && bitmapImage.StreamSource != null)
                    {
                        bitmapImage.StreamSource.Dispose();
                    }
                    bitmapImage = null;
                    RecyclingBin.MoveHere(fileName);
                    if (!File.Exists(fileName))
                    {
                        ImageIdxList[ImageIdxListDeletePtr] = -1;
                        ImagesNotNull--;
                        ImageIdxListDeletePtr = -1;
                        MessageBox.Show("Image deleted.");
                    }
                    else
                    {
                        MessageBox.Show("Error: Could not delete image.");
                    }
                }
                catch
                {
                    MessageBox.Show("Error: Could not delete image.");
                }
            }
            if (ImagesNotNull <= 0)
            {
                ImageListReady = false;
            }
        }

        private void ContextMenuChangeTimer(object sender, RoutedEventArgs e)
        {
            ChangeTimerCode();
        }
        private void ChangeTimerCode()
        {
            PauseSave();

            SlideShowTimer SlideShowTimerWindow = new SlideShowTimer
            {
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
            };

            SlideShowTimerWindow.TimerTextBox.Text = dispatcherImageTimer.Interval.Seconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherImageTimer.Interval = new TimeSpan(0, 0, 0, i, c);

            Activate();

            PauseRestore();
        }

        private void ContextMenuFullScreen(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private async void CheckedRandomize(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }            
            Randomize = ContextMenuCheckBox.IsChecked;
            await Task.Run(() => RandomizeBW_DoWork());
            DisplayCurrentImage();           
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void RandomizeBW_DoWork()
        {
            PauseSave();
            ImageListReady = false;
            CreateIdxListCode();
            ImageListReady = true;
            ResizeImageCode();
            PauseRestore();
        }        
    }
}