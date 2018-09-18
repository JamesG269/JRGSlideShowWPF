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

        PictureBox pictureBox = new PictureBox();

        FolderBrowserDialog dialog = new FolderBrowserDialog();
        
        public MainWindow()
        {
            InitializeComponent();            
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            GetMaxPicSize();
            LoadSettings();            
            NotifyStart();

            dispatcherTimerSlow.Tick += DisplayNextImageTimer;
            dispatcherTimerFast.Tick += DisplayImageTimer;
            dispatcherTimerFast.Interval = new TimeSpan(0, 0, 0, 0, 2);
            dispatcherTimerMouse.Tick += MouseHide;
            dispatcherTimerMouse.Interval = new TimeSpan(0, 0, 0, 5, 0);

            ChangeIdxPtrBW.DoWork += ChangeIdxPtrBW_DoWork;
            ChangeIdxPtrBW.RunWorkerCompleted += ChangeIdxPtr_RunWorkerCompleted;
            StartGetFilesBW.DoWork += StartGetFilesBW_DoWork;
            RandomizeBW.DoWork += RandomizeBW_DoWork;

            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            StartGetFilesBW.RunWorkerAsync();
        }

        private void displayPrevImage()
        {
            if (ImageWhenReady == true)
            {
                return;
            }
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }            
            Pause();
            ChangeIdxPtrDirection = -1;            
            ChangeIdxPtrBW.RunWorkerAsync();
        }
        private void displayNextImage()
        {
            if (ImageWhenReady == true)
            {
                return;
            }
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }            
            Pause();
            ChangeIdxPtrDirection = 1;                    
            ChangeIdxPtrBW.RunWorkerAsync();            
        }
        
        private void ChangeIdxPtrBW_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ImageListReady == false)
            {
                return;
            }
            ChangeIdxPtrCode();            
        }
        private void ChangeIdxPtrCode()
        {            
            if (ImagesNotNull > 0)
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

                    if (!File.Exists(ImageList[ImageIdxList[ImageIdxListPtr]]))
                    {
                        ImageList[ImageIdxList[ImageIdxListPtr]] = null;
                        ImagesNotNull--;
                    }
                } while (ImagesNotNull > 0 && ImageList[ImageIdxList[ImageIdxListPtr]] == null);
                
                if (ImagesNotNull > 0)
                {
                    ResizeImageCode();
                    ImageWhenReady = true;
                }
            }
            Unpause();                        
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
                Pause();
                if (ImageError == false)
                {
                    ImageControl.Source = null;
                    ImageControl.Source = bitmapImage;
                    ImageControl.InvalidateVisual();
                    ImageListDeletePtr = ImageIdxList[ImageIdxListPtr];
                   
                    GC.Collect();
                    if (DisplayPicInfoDPIx != DisplayPicInfoDPIy)
                    {
                        DisplayFileInfo(true);
                    }
                }                
                ImageWhenReady = false;
                Unpause();
            }            
            Interlocked.Exchange(ref OneInt, 0);            
        }
        private void DisplayNextImageTimer(object sender, EventArgs e)
        {            
            displayNextImage();            
        }
        
        private void StartGetFilesBW_DoWork(object sender, DoWorkEventArgs e)
        {
            StartGetFilesCode();
        }
        public List<string> NewImageList = new List<string>();

        private void StartGetFilesCode()
        {
            Pause();
            GetFilesCode();
            if (NewImageList != null && NewImageList.Count > 0)
            {
                ImageListReady = false;
                ImageWhenReady = false;
                ImageList.Clear();
                ImageList.AddRange(NewImageList);
                ImagesNotNull = ImageList.Count();
                CreateIdxListCode();
                ResizeImageCode();
                ImageListReady = true;
                ImageWhenReady = true;
            }
            StartUp = false;
            dispatcherTimerMouse.Start();
            Unpause();
            Interlocked.Exchange(ref OneInt, 0);            
        }          

        private void GetMaxPicSize()
        {
            var bounds = Screen.FromHandle(new WindowInteropHelper(this).Handle).Bounds;
            ResizeMaxHeight = bounds.Height;
            ResizeMaxWidth = bounds.Width;
        }
        private void Pause()
        {
            dispatcherTimerSlow.Stop();
            dispatcherTimerFast.Stop();
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
        private void Unpause()
        {
            dispatcherTimerSlow.Start();
            dispatcherTimerFast.Start();
            if (WState == WindowState.Maximized)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
        }        

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            dispatcherTimerFast.Stop();
            dispatcherTimerSlow.Stop();
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
