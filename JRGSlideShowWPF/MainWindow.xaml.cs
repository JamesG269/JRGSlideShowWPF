/*
JRGSlideShowWPF is a slide show program written in C# targetting WPF by James Gentile (jamesraymondgentile@gmail.com), that runs on Windows.
I wrote this app because other slide show software I tried was lacking in several key areas,
I wanted the monitor not to sleep (this is like 1 line of code, I can't understand why 
this is not an option in other slide show apps) and I wanted the ability to delete the
currently displayed picture which I couldn't find in the others I tried. It is simple to use:
open the app, select a folder, and it starts playing. 

Functions:
F1 Key - Picture and technical info.
Del key - prompt to delete current picture.
Double click window - maximize or de-maximize.
Right click window - options such as open folder, randomize/sequential play, change timer, etc.
Mouse wheel up/down - scroll through picture list.
Screen will not sleep if the app is full screen.

*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Interop;
using System.Threading;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Diagnostics;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        const string version = "1.1";

        System.Windows.Threading.DispatcherTimer dispatcherImageTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherMouseTimer = new System.Windows.Threading.DispatcherTimer();

        string SlideShowDirectory;

        Boolean Randomize = true;
        Boolean ImageReady = false;
        Boolean StartUp = true;

        int ImagesNotNull = 0;
        int OneInt = 0;
        int imagesDisplayed = 0;

        FolderBrowserDialog dialog = new FolderBrowserDialog();

        IntPtr thisHandle = IntPtr.Zero;

        PresentationSource PSource = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            PSource = PresentationSource.FromVisual(this);
            thisHandle = new WindowInteropHelper(this).Handle;

            progressBar.Visibility = Visibility.Hidden;
            TextBlockControl.Visibility = Visibility.Hidden;

            dispatcherImageTimer.Tick += DisplayNextImageTimer;
            dispatcherMouseTimer.Tick += MouseHide;
            dispatcherMouseTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);

            LoadSettings();
            StartUp = false;
            NotifyStart();

            if (SlideShowDirectory == null || !Directory.Exists(SlideShowDirectory))
            {
                await OpenDirCheckCancel();
            }
            else
            {
                while (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    await Task.Delay(1);
                }                
                await Task.Run(() => StartGetFilesNoInterlock());
                DisplayCurrentImage();
                Interlocked.Exchange(ref OneInt, 0);
            }                        
            Play();
        }
        
        private async Task DisplayGetNextImage(int i)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            if (ImageListReady == true && Paused == 0)
            {
                PauseSave();
                await Task.Run(() => LoadNextImage(i));
                DisplayCurrentImage();
                PauseRestore();
            }
            Interlocked.Exchange(ref OneInt, 0);
        }

        private void LoadNextImage(int i)
        {            
            do
            {
                if (Randomize == true)
                {
                    if (ImageIdxListPtr == 0 && i == -1)
                    {
                        DecryptIdxListCode();
                    }
                    else if (ImageIdxListPtr == (ImageIdxList.Length - 1) && i == 1)
                    {
                        EncryptIdxListCode();
                    }
                }
                ImageIdxListPtr += i;
                ImageIdxListPtr = ((ImageIdxListPtr % ImageIdxList.Length) + ImageIdxList.Length) % ImageIdxList.Length;

            } while (ImageIdxList[ImageIdxListPtr] == -1);
            imageTimeToDecode.Restart();
            ResizeImageCode();            
        }
        
        private void DisplayCurrentImage()
        {
            if (ImageReady == true)
            {
                ImageReady = false;
                Boolean timerenabled = dispatcherImageTimer.IsEnabled;
                dispatcherImageTimer.Stop();                
                if (ImageError == false)
                {
                    ImageIdxListDeletePtr = -1;
                    ImageControl.Source = displayPhoto;                   
                    ImageIdxListDeletePtr = ImageIdxListPtr;
                    imageTimeToDecode.Stop();
                    imagesDisplayed++;
                    if (displayingInfo)
                    {
                        updateInfo();
                    }
                }
                else
                {                    
                    MessageBox.Show(ErrorMessage);                                       
                }                
                if (timerenabled)
                {
                    dispatcherImageTimer.Start();
                }
            }
        }

        private async void DisplayNextImageTimer(object sender, EventArgs e)
        {
            await DisplayGetNextImage(1);
        }

        private async void StartGetFiles()
        {
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(1);
            }
            StartGetFilesNoInterlock();
            Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 0);
        }
        private async void StartGetFilesNoInterlock()
        { 
            Boolean ImageListReadyBackup = ImageListReady;
            ImageListReady = false;
            StartGetFiles_Cancel = false;
            GetFilesCode();
            if (StartGetFiles_Cancel != true && NewImageList != null && NewImageList.Count > 0)
            {
                ImageList = null;
                ImageList = new FileInfo[NewImageList.Count];
                int i = 0;
                foreach (var n in NewImageList)
                {
                    ImageList[i] = n;
                    i++;
                }
                ImagesNotNull = ImageList.Length;
                CreateIdxListCode();
                imageTimeToDecode.Restart();
                ResizeImageCode();
                ImageListReady = true;                                
            }
            else
            {
                ImageListReady = ImageListReadyBackup;
            }
        }

        private void GetMaxSize()
        {
            var bounds = Screen.FromHandle(thisHandle).Bounds;
            ScreenMaxHeight = bounds.Height;
            ScreenMaxWidth = bounds.Width;
        }

        Boolean OldSlow;
        int Paused = 0;
        private void PauseSave()
        {            
            if (Paused > 0)
            {
                return;
            }
            Paused++;
            OldSlow = dispatcherImageTimer.IsEnabled;
            Stop();
        }
        private void PauseRestore()
        {
            Paused--;
            if (Paused > 0)
            {
                return;
            }
            Paused = 0;
            if (OldSlow == true)
            {
                Play();
            }
        }
        private void Stop()
        {
            dispatcherImageTimer.Stop();
            DisplayNotRequired();                     
        }
        
        private void Play()
        {
            if (ImageListReady == false)
            {
                Stop();
                return;
            }
            dispatcherImageTimer.Stop();
            dispatcherImageTimer.Start();
            DisplayRequired();                           
        }
        private void DisplayRequired()
        {
            if (isMaximized && dispatcherImageTimer.IsEnabled)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                if (!MouseHidden)
                {
                    dispatcherMouseTimer.Stop();
                    dispatcherMouseTimer.Start();
                }
            }
        }
        private void DisplayNotRequired()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            _cancelTokenSource.Cancel();
            Stop();
            if (NIcon != null)
            {
                NIcon.Dispose();
                NIcon = null;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            if (StartUp == false)
            {
                SaveSettings();
            }                        
            base.OnClosing(e);
        }

        
    }
}
