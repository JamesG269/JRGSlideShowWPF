using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Interop;
using System.Threading;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;
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

        System.Windows.Threading.DispatcherTimer dispatcherTimerSlow = new System.Windows.Threading.DispatcherTimer();        
        System.Windows.Threading.DispatcherTimer dispatcherTimerMouse = new System.Windows.Threading.DispatcherTimer();

        string SlideShowDirectory;

        Boolean Randomize = true;
        Boolean ImageReady = false;
        Boolean StartUp = true;

        int ChangeIdxPtrDirection = 1;
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

            dispatcherTimerSlow.Tick += DisplayNextImageTimer;            
            dispatcherTimerMouse.Tick += MouseHide;
            dispatcherTimerMouse.Interval = new TimeSpan(0, 0, 0, 5, 0);

            LoadSettings();
            StartUp = false;
            NotifyStart();
            
            if (SlideShowDirectory == null || !Directory.Exists(SlideShowDirectory))
            {
                await OpenDirCheckCancel();
            }  
            else
            {           
                if (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    return;
                }
                await Task.Run(() => StartGetFiles());
                DisplayCurrentImage();
                Interlocked.Exchange(ref OneInt, 0);
            }
        }
        int Outstanding = 0;
        private async Task<Boolean> displayPrevImage()
        {
            await displayGetNextImage(-1);
            return true;
        }
        private async Task<Boolean> displayNextImage()
        {
            await displayGetNextImage(1);
            return true;
        }
        private async Task<Boolean> displayGetNextImage(int i)
        { 
            if (ImageListReady == false)
            {
                return false;
            }
            if (ImageReady == true)
            {
                Outstanding += i;
                return false;
            }
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                Outstanding += i;
                return false;
            }            
            ChangeIdxPtrDirection = i;
            await Task.Run(() => LoadNextImage());
            DisplayCurrentImage();
            Interlocked.Exchange(ref OneInt, 0);
            return true;
        }

        private void LoadNextImage()
        {
            do
            {
                if (Randomize == true)
                {
                    if (ImageIdxListPtr == 0 && ChangeIdxPtrDirection == -1)
                    {
                        DecryptIdxListCode();
                    }
                    else if (ImageIdxListPtr == (ImageIdxList.Count - 1) && ChangeIdxPtrDirection == 1)
                    {
                        EncryptIdxListCode();
                    }
                }
                ImageIdxListPtr += ChangeIdxPtrDirection;
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
                if (dispatcherTimerSlow.IsEnabled)
                {
                    dispatcherTimerSlow.Stop();
                    dispatcherTimerSlow.Start();
                }
            }                
        }

        private async void DisplayNextImageTimer(object sender, EventArgs e)
        {
            await displayNextImage();
        }
        
        private void StartGetFiles()
        {
            StartGetFilesBW_Cancel = false;                                  
            GetFilesCode();
            if (StartGetFilesBW_Cancel == false && NewImageList != null && NewImageList.Count > 0)
            {
                ImageListReady = false;
                ImageList.Clear();
                ImageList.AddRange(NewImageList);
                ImagesNotNull = ImageList.Count();
                CreateIdxListCode();                                
                ImageListReady = true;
                ResizeImageCode();
                Play();                                                                              
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
            OldSlow = dispatcherTimerSlow.IsEnabled;
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
            dispatcherTimerSlow.Stop();
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
                    dispatcherTimerMouse.Stop();
                    dispatcherTimerMouse.Start();
                }
            }            
            dispatcherTimerSlow.Stop();
            Outstanding = 0;
            dispatcherTimerSlow.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Stop();
            if (NIcon != null)
            {
                NIcon.Dispose();
                NIcon = null;
            }
            if (0 == Interlocked.Exchange(ref OneInt, 1))
            {
                if (StartUp == false)
                {
                    SaveSettings();
                }
            }
            Interlocked.Exchange(ref OneInt, 0);
            base.OnClosing(e);
        }
    }
}
