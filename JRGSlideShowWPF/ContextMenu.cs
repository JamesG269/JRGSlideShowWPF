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
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

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
                PauseSave(true);
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
            await DisplayFileInfo();            
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
            MenuPause();
        }
        private void MenuPause()
        {
            if (Paused == false)
            {
                PlayXaml.IsChecked = false;
                PauseXaml.IsChecked = true;
                PauseSave();
            }
        }
        private void ContextMenuPlay(object sender, RoutedEventArgs e)
        {
            MenuPlay();
        }
        private void MenuPlay()
        {
            PauseRestore();
            PlayXaml.IsChecked = true;
            PauseXaml.IsChecked = false;
        }
        private async void ContextMenuCopyDelete(object sender, RoutedEventArgs e)
        {
            await CopyDeleteCode();
        }
        private async Task<Boolean> CopyDeleteCode()
        {
            PauseSave(true);
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
            PauseSave(true);            
            await DeleteNoInterlock();
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);
        }        
        
        private bool Undelete()
        {
            TextBlockControl.Visibility = Visibility.Visible;
            if (DeletedFiles.Count == 0)
            {
                topTextBoxClass.messageDisplayStart("No more files to undelete.", 5);
                return false;
            }
            string LastDeleted = DeletedFiles.Pop();
            if (LastDeleted == "")
            {
                topTextBoxClass.messageDisplayStart(LastDeleted + " UNDELETE ERROR.", 5);
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
                        topTextBoxClass.messageDisplayStart(LastDeleted + " Could not be undeleted, file not found.", 5);
                        return false;
                    }                                       
                    topTextBoxClass.messageDisplayStart(LastDeleted + " Restored.", 5);
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
                        topTextBoxClass.messageDisplayStart("Deleted: " + fileName, 5);
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

            SlideShowTimerWindow.TimerTextBox.Text = dispatcherPlaying.Interval.Seconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherPlaying.Interval = new TimeSpan(0, 0, 0, i, c);

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
            OpenFolderAndSelectItem(imageInfo.DirectoryName, imageInfo.Name);
            return;            
        }
        public static void OpenFolderAndSelectItem(string folderPath, string file)
        {
            IntPtr nativeFolder;
            uint psfgaoOut;
            SHParseDisplayName(folderPath, IntPtr.Zero, out nativeFolder, 0, out psfgaoOut);

            if (nativeFolder == IntPtr.Zero)
            {
                // Log error, can't find folder
                return;
            }

            IntPtr nativeFile;
            SHParseDisplayName(Path.Combine(folderPath, file), IntPtr.Zero, out nativeFile, 0, out psfgaoOut);

            IntPtr[] fileArray;
            if (nativeFile == IntPtr.Zero)
            {
                // Open the folder without the file selected if we can't find the file
                fileArray = new IntPtr[0];
            }
            else
            {
                fileArray = new IntPtr[] { nativeFile };
            }

            SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

            Marshal.FreeCoTaskMem(nativeFolder);
            if (nativeFile != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(nativeFile);
            }
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            PlayXaml.IsChecked = !Paused;
            PauseXaml.IsChecked = Paused;
        }
    }
}