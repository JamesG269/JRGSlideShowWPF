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
using System.Runtime.InteropServices;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        private const int SW_SHOW = 5;

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
                await DisplayCurrentImage();
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
        int benchmarkRunning = 0;
        bool benchmarkStop = false;
        private async void Benchmark_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => BenchMarkWorker());
        }

        private void BenchMarkWorker()
        {
            if (0 != Interlocked.Exchange(ref benchmarkRunning, 1))
            {
                benchmarkStop = true;
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                Task.Delay(1);
            }
            PauseSave();
            benchmarkStop = false;
            int imagesLimit = ImageList.Length;

            Stopwatch benchmark = new Stopwatch();
            ImageIdxListPtr = 0;
            imagesDisplayed = 0;
            var backuprandomize = RandomizeImages;
            if (RandomizeImages == true)
            {
                RandomizeImages = false;
                CreateIdxListCode();
            }
            if (ImageListReady == true)
            {
                benchmark.Start();
                while (imagesDisplayed < imagesLimit && benchmarkStop == false)
                {
                    LoadNextImage(1);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync((new Action(async () => {
                        await DisplayCurrentImage();
                    })), System.Windows.Threading.DispatcherPriority.SystemIdle);                   
                }
                benchmark.Stop();
            }
            if (backuprandomize == true)
            {
                RandomizeImages = backuprandomize;
                CreateIdxListCode();
            }
            PauseRestore();
            MessageBox.Show("Benchmark - Images displayed: " + imagesDisplayed + " Milliseconds: " + benchmark.ElapsedMilliseconds + " Ticks: " + benchmark.ElapsedTicks);
            Interlocked.Exchange(ref benchmarkRunning, 0);
            Interlocked.Exchange(ref OneInt, 0);
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

        private async Task DeleteNoInterlock(bool GetNextImage = false)
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
                        StartTurnOffTextBoxDisplayTimer("Deleted: " + fileName, 5);
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
            if (GetNextImage)
            {
                await DisplayGetNextImageWithoutCheck(1);
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
            RandomizeImages = ContextMenuCheckBox.IsChecked;
            if (!Starting)
            {
                PauseSave();
                await Task.Run(() => RandomizeBW_DoWork());
                PauseRestore();
                await DisplayCurrentImage();
            }
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void RandomizeBW_DoWork()
        {            
            ImageListReady = false;
            CreateIdxListCode();            
            ResizeImageCode();
            ImageListReady = true;            
        }
        private void OpenInExplorer(object sender, RoutedEventArgs e)
        {
            if (ImageIdxListDeletePtr == -1)
            {
                return;
            }
            FileInfo imageInfo = ImageList[ImageIdxList[ImageIdxListDeletePtr]];
            var info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>();
            info.lpVerb = "explore";
            info.nShow = SW_SHOW;
            info.lpFile = imageInfo.DirectoryName;
            ShellExecuteEx(ref info);
            return;
        }
    }
}