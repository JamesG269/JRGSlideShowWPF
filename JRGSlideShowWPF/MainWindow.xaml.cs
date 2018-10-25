﻿using System;
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

        System.Windows.Threading.DispatcherTimer dispatcherImageTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherMouseTimer = new System.Windows.Threading.DispatcherTimer();

        string SlideShowDirectory;

        Boolean Randomize = true;
        Boolean ImageReady = false;
        Boolean StartUp = true;

        int ImagesNotNull = 0;
        int OneInt = 0;

        public static BitmapImage bitmapImage = null;

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
                await Task.Run(() => StartGetFiles());
                DisplayCurrentImage();
                Interlocked.Exchange(ref OneInt, 0);
            }
        }        
        private async Task<Boolean> DisplayPrevImage()
        {
            await DisplayGetNextImage(-1);
            return true;
        }
        private async Task<Boolean> DisplayNextImage()
        {
            await DisplayGetNextImage(1);
            return true;
        }        
        private async Task<Boolean> DisplayGetNextImage(int i)
        {
            if (ImageListReady == false || Paused > 0)
            {
                return false;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
                if (ImageListReady == false || Paused > 0)
                {
                    return false;
                }
            }                                
            await Task.Run(() => LoadNextImage(i));            
            DisplayCurrentImage();
            Interlocked.Exchange(ref OneInt, 0);
            return true;
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
                    else if (ImageIdxListPtr == (ImageIdxList.Count - 1) && i == 1)
                    {
                        EncryptIdxListCode();
                    }
                }
                ImageIdxListPtr += i;
                ImageIdxListPtr = ((ImageIdxListPtr % ImageIdxList.Count) + ImageIdxList.Count) % ImageIdxList.Count;

            } while (ImageIdxList[ImageIdxListPtr] == -1);

            ResizeImageCode();
        }

        private void DisplayCurrentImage()
        {
            if (ImageReady == true)
            {
                ImageReady = false;
                if (ImageError == false)
                {
                    ImageControl.Source = bitmapImage;
                    ImageIdxListDeletePtr = ImageIdxListPtr;
                }
                else
                {
                    MessageBox.Show(ErrorMessage);
                }
                if (dispatcherImageTimer.IsEnabled)
                {
                    dispatcherImageTimer.Stop();
                    dispatcherImageTimer.Start();
                }
            }
        }

        private async void DisplayNextImageTimer(object sender, EventArgs e)
        {
            await DisplayGetNextImage(1);
        }

        private void StartGetFiles()
        {
            Boolean ImageListReadyBackup = ImageListReady;
            ImageListReady = false;
            StartGetFilesBW_Cancel = false;
            GetFilesCode();
            if (StartGetFilesBW_Cancel != true && NewImageList != null && NewImageList.Count > 0)
            {
                ImageList.Clear();
                ImageList.AddRange(NewImageList);
                ImagesNotNull = ImageList.Count();
                CreateIdxListCode();                
                ResizeImageCode();
                ImageListReady = true;
                Play();                
            }
            else
            {
                ImageListReady = ImageListReadyBackup;
            }            
        }

        private void GetMaxSize()
        {
            var bounds = Screen.FromHandle(thisHandle).Bounds;
            ResizeMaxHeight = bounds.Height;
            ResizeMaxWidth = bounds.Width;
        }

        Boolean OldSlow;
        int Paused = 0;
        private void PauseSave()
        {
            Paused++;
            if (Paused > 1)
            {
                return;
            }
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
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
        private void Play()
        {
            if (ImageListReady == false)
            {
                return;
            }
            if (isMaximized)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                if (!MouseHidden)
                {
                    dispatcherMouseTimer.Stop();
                    dispatcherMouseTimer.Start();
                }
            }
            dispatcherImageTimer.Stop();            
            dispatcherImageTimer.Start();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
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
