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

        BackgroundWorker ChangeIdxPtrBW = new BackgroundWorker();
        BackgroundWorker StartGetFilesBW = new BackgroundWorker();
        BackgroundWorker RandomizeBW = new BackgroundWorker();

        System.Windows.Threading.DispatcherTimer dispatcherTimerSlow = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTimerFast = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTimerMouse = new System.Windows.Threading.DispatcherTimer();

        string SlideShowDirectory;

        Boolean Randomize = true;
        Boolean ImageWhenReady = false;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PSource = PresentationSource.FromVisual(this);
            thisHandle = new WindowInteropHelper(this).Handle;

            dispatcherTimerSlow.Tick += DisplayNextImageTimer;
            dispatcherTimerFast.Tick += DisplayImageTimer;
            dispatcherTimerFast.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimerMouse.Tick += MouseHide;
            dispatcherTimerMouse.Interval = new TimeSpan(0, 0, 0, 5, 0);

            LoadSettings();
            StartUp = false;
            NotifyStart();
            
            ChangeIdxPtrBW.DoWork += ChangeIdxPtrBW_DoWork;
            ChangeIdxPtrBW.RunWorkerCompleted += ChangeIdxPtr_RunWorkerCompleted;
            StartGetFilesBW.DoWork += StartGetFilesBW_DoWork;
            StartGetFilesBW.WorkerSupportsCancellation = true;
            StartGetFilesBW.RunWorkerCompleted += StartGetFilesBW_RunWorkerCompleted;
            RandomizeBW.DoWork += RandomizeBW_DoWork;
            RandomizeBW.RunWorkerCompleted += RandomizeBW_RunWorkerCompleted;

            if (SlideShowDirectory == null || !Directory.Exists(SlideShowDirectory))
            {
                OpenDir();
            }  
            else
            {
                if (0 != Interlocked.Exchange(ref OneInt, 1))
                {
                    return;
                }                
                StartGetFilesBW.RunWorkerAsync();
            }
        }
        
        private void displayPrevImage()
        {
            if (ImageWhenReady == true || ImageListReady == false)
            {
                return;
            }
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }            
            ChangeIdxPtrDirection = -1;
            ChangeIdxPtrBW.RunWorkerAsync();
        }
        private void displayNextImage()
        {
            if (ImageWhenReady == true || ImageListReady == false)
            {
                return;
            }
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }            
            ChangeIdxPtrDirection = 1;
            ChangeIdxPtrBW.RunWorkerAsync();
        }

        private void ChangeIdxPtrBW_DoWork(object sender, DoWorkEventArgs e)
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

            } while (ImageList[ImageIdxList[ImageIdxListPtr]] == null);

            ResizeImageCode();
        }

        private void ChangeIdxPtr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Interlocked.Exchange(ref OneInt, 0);            
        }

        private void DisplayImageTimer(object sender, EventArgs e)
        {
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            if (ImageWhenReady == true)
            {
                ImageWhenReady = false;
                if (ImageError == false)
                {
                    ImageControl.Source = bitmapImage;
                    ImageListDeletePtr = ImageIdxList[ImageIdxListPtr];                    
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
            Interlocked.Exchange(ref OneInt, 0);            
        }

        private void DisplayNextImageTimer(object sender, EventArgs e)
        {
            displayNextImage();
        }

        private void StartGetFilesBW_DoWork(object sender, DoWorkEventArgs e)
        {
            Stop();            
            GetFilesCode();
            if (NewImageList != null && NewImageList.Count > 0)
            {
                ImageListReady = false;
                ImageList.Clear();
                ImageList.AddRange(NewImageList);
                ImagesNotNull = ImageList.Count();
                CreateIdxListCode();
                ImageListReady = true;
                ResizeImageCode();                                
                dispatcherTimerFast.Start();
            }            
            if (StartGetFilesBW.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        private void StartGetFilesBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Interlocked.Exchange(ref OneInt, 0);            
            if (e.Cancelled)
            {
                OpenDir();
            }
            else
            {
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
