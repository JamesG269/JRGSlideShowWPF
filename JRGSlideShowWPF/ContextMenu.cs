using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Shell32;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Linq;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {

        public static Shell shell = new Shell();

        public static Folder RecyclingBin = shell.NameSpace(10);

        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        private async void GoogleImageSearch_Click(object sender, RoutedEventArgs e)
        {
            if (PrivateModeCheckBox.IsChecked == true)
            {
                MessageBox.Show("Private Mode is enabled, google search not done.");
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            var result = MessageBox.Show("Confirm google look up, Private Mode is DISABLED. ", "Confirm google look up.", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                PauseSave();
                await Task.Run(() => GoogleImageSearch(ImageList[ImageIdxList[ImageIdxListPtr]].FullName, true, _cancelTokenSource.Token));
                PauseRestore();
            }
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void PrivateModeClick(object sender, RoutedEventArgs e)
        {

        }
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
            StartGetFiles_Cancel = false;
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
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                dialog.SelectedPath = SlideShowDirectory;
            }            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SlideShowDirectory = dialog.SelectedPath;
                await Task.Run(() => StartGetFilesNoInterlock());                
                DisplayCurrentImage(ref ImageIdxListDeletePtr, ref ImageIdxListPtr);
                Play();
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
        private async void Benchmark_Click(object sender, RoutedEventArgs e)
        {
            int imagesLimit = 400;
            if (ImageList.Length < imagesLimit)
            {
                imagesLimit = ImageList.Length;
            }
            Stopwatch benchmark = new Stopwatch();
            ImageIdxListPtr = 0;
            imagesDisplayed = 0;
            var backuprandomize = RandomizeNotFinishedIHaveToLOL;
            if (RandomizeNotFinishedIHaveToLOL == true)
            {
                RandomizeNotFinishedIHaveToLOL = false;
                await Task.Run(() => CreateIdxListCode());
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            PauseSave();
            if (ImageListReady == true)
            {                
                benchmark.Start();
                while (imagesDisplayed < imagesLimit)
                {
                    await Task.Run(() => LoadNextImage(1, ref ImageIdxListPtr, ImageIdxList, ImageList));
                    DisplayCurrentImage(ref ImageIdxListDeletePtr, ref ImageIdxListPtr);
                }
                benchmark.Stop();                
            }
            if (backuprandomize == true)
            {
                RandomizeNotFinishedIHaveToLOL = backuprandomize;
                await Task.Run(() => CreateIdxListCode());
            }
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
            MessageBox.Show("Benchmark - Images displayed: " + imagesDisplayed + " Milliseconds: " + benchmark.ElapsedMilliseconds + " Ticks: " + benchmark.ElapsedTicks);
        }
        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async void ContextMenuNext(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            await DisplayGetNextImageWithoutCheck(1);            
            Interlocked.Exchange(ref OneInt, 0);
        }

        private async void ContextMenuPrev(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            await DisplayGetNextImageWithoutCheck(-1);
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void ContextMenuPause(object sender, RoutedEventArgs e)
        {
            if (Paused == false)
            {
                PauseSave();
            }
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
            await CopyDeleteWorker();
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
            return true;
        }

        private async Task CopyDeleteWorker()
        {
            if (PrivateModeCheckBox.IsChecked == true)
            {
                MessageBox.Show("Private Mode is enabled, copy not done.");
                return;
            }
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {
                string destPath = "";
                string sourcePath = ImageList[ImageIdxList[ImageIdxListDeletePtr]].FullName;
                try
                {
                    destPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    destPath = Path.Combine(destPath, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, destPath);
                    MessageBox.Show("Image copied to " + destPath);
                    await DeleteNoInterlock();
                }
                catch
                {
                    MessageBox.Show("Error: image not copied to " + destPath);
                }
            }
        }

        private async void ContextMenuDelete(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            PauseSave();            
            await DeleteNoInterlock();
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
        }        
        
        private bool Undelete()
        {
            TextBlockControl.Visibility = Visibility.Visible;
            if (DeletedFiles.Count == 0)
            {
                TextBlockControl.Text = "No more files to undelete.";
                return false;
            }
            string LastDeleted = DeletedFiles.Pop();
            if (LastDeleted == "")
            {
                TextBlockControl.Text = LastDeleted + " UNDELETE ERROR.";
                return false;
            }
            FolderItems folderItems = RecyclingBin.Items();
            for (int i = 0; i < folderItems.Count; i++) 
            {
                FolderItem FI = folderItems.Item(i);
                string FileName = RecyclingBin.GetDetailsOf(FI, 0);
                if (Path.GetExtension(FileName) == "")
                {
                    FileName += Path.GetExtension(FI.Path);
                }
                //Necessary for systems with hidden file extensions.
                string FilePath = RecyclingBin.GetDetailsOf(FI, 1);                
                if (String.Compare(LastDeleted, Path.Combine(FilePath, FileName),true) == 0)
                {
                    FileInfo undelFile;                    
                    try
                    {
                        DoVerb(FI, "ESTORE");
                        undelFile = new FileInfo(LastDeleted);
                    }
                    catch
                    {                        
                        StartTurnOffTextBoxDisplayTimer(LastDeleted + " Could not be undeleted, file not found.", 5);
                        return false;
                    }                                       
                    StartTurnOffTextBoxDisplayTimer(LastDeleted + " Restored.", 5);
                    Array.Resize(ref ImageList, ImageList.Length + 1);
                    ImageList[ImageList.Length - 1] = undelFile;
                    Array.Resize(ref ImageIdxList, ImageIdxList.Length + 1);
                    ImageIdxList[ImageIdxList.Length - 1] = ImageIdxList.Length - 1;
                    ImagesNotNull++;
                    return true;
                }
            }
            return false;
        }
        private bool DoVerb(FolderItem Item, string Verb)
        {
            foreach (FolderItemVerb FIVerb in Item.Verbs())
            {
                if (FIVerb.Name.ToUpper().Contains(Verb.ToUpper()))
                {
                    FIVerb.DoIt();
                    return true;
                }
            }
            return false;
        }
        
        public System.Collections.Generic.Stack<string> DeletedFiles = new System.Collections.Generic.Stack<string>();

        private async Task DeleteNoInterlock()
        {
            if (ImageIdxListDeletePtr == -1 || ImageIdxList[ImageIdxListDeletePtr] == -1)
            {
                return;
            }            
            var fileName = ImageList[ImageIdxList[ImageIdxListDeletePtr]].FullName;
            bool result = true;
            if (!IsUserjgentile)
            {
                result = MessageBox.Show("Confirm delete: " + fileName, "Confirm delete image.", MessageBoxButton.YesNo) == MessageBoxResult.Yes;                
            }
            if (result)
            {
                try
                {
                    if (memStream != null)
                    {
                        memStream.Dispose();
                    }
                    memStream = null;                    
                    displayPhoto = null;
                    ImageControl.Source = null;
                    RecyclingBin.MoveHere(fileName);                    
                    if (!File.Exists(fileName))
                    {                                                
                        DeletedFiles.Push(fileName);                        
                        ImageIdxList[ImageIdxListDeletePtr] = -1;
                        ImagesNotNull--;
                        ImageIdxListDeletePtr = -1;                                                
                        StartTurnOffTextBoxDisplayTimer(fileName + " deleted.", 5);
                    }
                    else
                    {
                        MessageBox.Show("Error: Could not delete image.");
                    }
                }
                catch
                {
                    MessageBox.Show("Exception: Could not delete image.");
                }
            }
            if (ImagesNotNull <= 0)
            {
                ImageListReady = false;
            }
            await DisplayGetNextImageWithoutCheck(1);                        
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

            SlideShowTimerWindow.TimerTextBox.Text = dispatcherE10E.Interval.Seconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherE10E.Interval = new TimeSpan(0, 0, 0, i, c);

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
            RandomizeNotFinishedIHaveToLOL = ContextMenuCheckBox.IsChecked;
            if (!Starting)
            {
                PauseSave();
                await Task.Run(() => RandomizeBW_DoWork());
                PauseRestore();
                DisplayCurrentImage(ref ImageIdxListDeletePtr, ref ImageIdxListPtr);
            }
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void RandomizeBW_DoWork()
        {            
            ImageListReady = false;
            CreateIdxListCode();            
            ResizeImageCode(ImageList, ImageIdxList, ImageIdxListPtr);
            ImageListReady = true;            
        }        
    }
}