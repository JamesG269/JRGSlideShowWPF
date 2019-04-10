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
        Boolean StartGetFiles_Cancel = false;
        private async Task<Boolean> OpenDirCheckCancel()
        {            
            StartGetFiles_Cancel = true;
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(1);
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            await OpenDir();
            Interlocked.Exchange(ref OneInt, 0);
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
        private async void ImageInfo_Click(object sender, RoutedEventArgs e)
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
        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async void ContextMenuNext(object sender, RoutedEventArgs e)
        {
            await DisplayGetNextImage(1);
        }

        private async void ContextMenuPrev(object sender, RoutedEventArgs e)
        {
            await DisplayGetNextImage(-1);
        }
        private void ContextMenuPause(object sender, RoutedEventArgs e)
        {
            PauseSave();
        }

        private void ContextMenuPlay(object sender, RoutedEventArgs e)
        {
            PauseRestore();
        }

        private async void ContextMenuCopyDelete(object sender, RoutedEventArgs e)
        {
            await CopyDeleteCode();
        }
        private async Task<Boolean> CopyDeleteCode()
        {
            PauseSave();
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }            
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {                
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
            return true;
        }

        private async void ContextMenuDelete(object sender, RoutedEventArgs e)
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
        private async void ChangeTimerCode()
        {
            PauseSave();
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
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
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void ContextMenuFullScreen(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private async void CheckedRandomize(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }            
            Randomize = ContextMenuCheckBox.IsChecked;
            PauseSave();
            await Task.Run(() => RandomizeBW_DoWork());
            PauseRestore();
            DisplayCurrentImage();           
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void RandomizeBW_DoWork()
        {            
            ImageListReady = false;
            CreateIdxListCode();            
            ResizeImageCode();
            ImageListReady = true;            
        }        
    }
}