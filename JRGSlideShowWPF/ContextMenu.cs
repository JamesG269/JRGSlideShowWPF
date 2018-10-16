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

        private void ContextMenuOpenFolder(object sender, RoutedEventArgs e)
        {
            OpenDirCheckCancel();
        }
        private void OpenDirCheckCancel()
        {
            if (StartGetFilesBW.IsBusy)
            {
                StartGetFilesBW.CancelAsync();
                return;
            }
            OpenDir();
        }
        private void OpenDir()
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            dialog.SelectedPath = SlideShowDirectory;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SlideShowDirectory = dialog.SelectedPath;
                StartGetFilesBW.RunWorkerAsync();
            }
            else
            {
                Interlocked.Exchange(ref OneInt, 0);
                Play();
            }
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
            PauseSave();
            if (0 == Interlocked.Exchange(ref OneInt, 1))
            {
                if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] == -1)
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
                Interlocked.Exchange(ref OneInt, 0);
            }
            PauseRestore();
        }

        private void ContextMenuDelete(object sender, RoutedEventArgs e)
        {
            PauseSave();
            if (0 == Interlocked.Exchange(ref OneInt, 1))
            {
                DeleteNoInterlock();
                Interlocked.Exchange(ref OneInt, 0);
            }
            PauseRestore();

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

            SlideShowTimerWindow.TimerTextBox.Text = dispatcherTimerSlow.Interval.Seconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherTimerSlow.Interval = new TimeSpan(0, 0, 0, i, c);

            Activate();

            PauseRestore();
        }

        private void ContextMenuFullScreen(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
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
            PauseSave();
            ImageListReady = false;
            CreateIdxListCode();
            ImageListReady = true;
            ResizeImageCode();
        }
        private void RandomizeBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Interlocked.Exchange(ref OneInt, 0);
            PauseRestore();
        }
    }
}