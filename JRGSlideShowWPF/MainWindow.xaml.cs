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
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Interop;
using System.Threading;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpCaster.Models;
using System.Collections.ObjectModel;
using SharpCaster.Services;
using SharpCaster.Controllers;
using SharpCaster.Models.ChromecastRequests;
using GoogleCast.Models.Media;
using GoogleCast.Channels;
using GoogleCast;

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

        const string version = "1.3";

        System.Windows.Threading.DispatcherTimer dispatcherPlaying = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherPureSense = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTextBoxMessage = new System.Windows.Threading.DispatcherTimer();

        string SlideShowDirectory;

        Boolean RandomizeImages = true;
        Boolean ImageReadyToDisplay = false;
        Boolean StartUp = true;

        int ImagesNotNull = 0;
        int OneInt = 0;
        int imagesDisplayed = 0;

        FolderBrowserDialog dialog = new FolderBrowserDialog();

        IntPtr thisHandle = IntPtr.Zero;

        PresentationSource PSource = null;

        Stack<bool> pauseStack = new Stack<bool>();
        Stack<string> TextBoxMessages = new Stack<string>();
        Stack<int> TextBoxMessagesTimes = new Stack<int>();
        TextBoxMessage MotdClass;
        TextBoxMessage TextBoxClass;

        public MainWindow()
        {
            InitializeComponent();
        }
        bool Starting = false;
        bool IsUserjgentile = false;
        public async void Window_ContentRendered(object sender, EventArgs e)
        {
            Starting = true;
            bool result = await Task.Run(() => InitAndClosePrevious());
            if (result == false)
            {
                Close();
            }
            MotdClass = new TextBoxMessage()
            {
                textBlock = MotdBlockControl,
                dispatchTimer = new System.Windows.Threading.DispatcherTimer()
            };
            TextBoxClass = new TextBoxMessage()
            {
                textBlock = TextBlockControl,
                dispatchTimer = new System.Windows.Threading.DispatcherTimer()
            };
            
            IsUserjgentile = String.Compare(Environment.UserName, "jgentile", true) == 0;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            PSource = PresentationSource.FromVisual(this);
            thisHandle = new WindowInteropHelper(this).Handle;

            progressBar.Visibility = Visibility.Hidden;
            TextBlockControl.Visibility = Visibility.Hidden;
            MotdBlockControl.Visibility = Visibility.Hidden;

            dispatcherPlaying.Tick += DisplayNextImageTimer;
            dispatcherPureSense.Tick += MouseHide;
            dispatcherPureSense.Interval = new TimeSpan(0, 0, 0, 5, 0);

            LoadSettings();
            NotifyStart();
            Starting = false;

            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                await OpenDirCheckCancel();
            }
            else
            {
                await Task.Run(() => StartGetFiles());
                await DisplayCurrentImage();
            }
            MenuPlay();
            dispatcherPureSense.Stop();
            dispatcherPureSense.Start();
            EnableMotdCode();

        }
        
        private bool InitAndClosePrevious()
        {
            InitRNGKeys();
            Process[] processlist = Process.GetProcessesByName("JRGSlideShowWPF");
            var currentProcess = Process.GetCurrentProcess();
            int c = 0;
            foreach (Process theprocess in processlist)
            {
                if (theprocess.Id == currentProcess.Id)
                {
                    continue;
                }
                theprocess.Kill();
                c++;
            }
            if (c > 0)
            {
                if (Environment.CommandLine.Contains("/close"))
                {
                    return false;
                }
            }
            return true;
        }
        private async Task DisplayGetNextImage(int i)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            if (Paused == false)
            {
                await DisplayGetNextImageWithoutCheck(i);
            }
            Interlocked.Exchange(ref OneInt, 0);
        }

        private async Task DisplayGetNextImageWithoutCheck(int i)
        {
            if (ImageListReady == true)
            {
                PauseSave();
                await Task.Run(() => LoadNextImage(i));
                await DisplayCurrentImage();
                FileInfo fileInfo = ImageList[ImageIdxList[ImageIdxListPtr]];
                PauseRestore();
            }
        }

        private void LoadNextImage(int i)
        {
            IteratePicList(i);
            imageTimeToDecode.Restart();
            ResizeImageCode();
        }

        private void IteratePicList(int i)
        {
            do
            {
                if (RandomizeImages == true)
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
        }
        bool ShowMotd = false;
        private async Task DisplayCurrentImage()
        {
            if (ImageReadyToDisplay == true)
            {
                if (ImageError == false)
                {
                    ImageIdxListDeletePtr = -1;
                    ImageControl.Source = displayPhoto;
                    imageTimeToDecode.Stop();
                    ImageIdxListDeletePtr = ImageIdxListPtr;
                    ImageReadyToDisplay = false;
                    imagesDisplayed++;
                    if (displayingInfo)
                    {
                        updateInfo();
                    }
                    PutMotd();
                }
                else
                {
                    if (IsUserjgentile)
                    {
                        await DeleteNoInterlock(true);
                    }
                    else
                    {
                        MessageBox.Show(ErrorMessage);
                    }
                    //copydeleteworker();
                }
            }

        }
        private void EnableMotdCode()
        {
            if (Starting)
            {
                return;
            }
            PutMotd();
        }
        private void PutMotd()
        {
            if (ShowMotd)
            {
                if (motd.Length == 0)
                {
                    getMotd();
                }
                if (motd.Length == 0)
                {
                    return;
                }
                Random r = new Random();
                int i = r.Next(0, motd.Length);
                MotdClass.messageDisplayStart(motd[i], -1, true, true);
            }
            else
            {
                MotdClass.messageDisplayEndUninterruptable(new Action(() => { MotdClass.textBlock.Visibility = Visibility.Hidden; }));
            }
        }
        public void getMotd()
        {
            if (!ShowMotd)
            {
                return;
            }
            string motdFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\motd.txt";
            if (File.Exists(motdFilePath))
            {
                motd = File.ReadAllLines(motdFilePath);
                TextBoxClass.messageDisplayStart(motd.Length.ToString() + " MOTDs loaded.", 5);
            }
            else
            {
                TextBoxClass.messageDisplayStart("no MOTD found at " + motdFilePath, 5, false, false);
            }
        }

        public class TextBoxMessage
        {
            public System.Windows.Threading.DispatcherTimer dispatchTimer;            
            public Stack<string> messages = new Stack<string>();
            public Stack<int> times = new Stack<int>();
            public Stack<bool> interruptable = new Stack<bool>();
            public System.Windows.Controls.TextBlock textBlock; 
            public static int InterlockInt = 0;
            public bool LastWasInterruptable = true;
            public void messageDisplayEndUninterruptable(Action action)
            {
                dispatchTimer.Stop();
                dispatchTimer.IsEnabled = false;
                LastWasInterruptable = true;
                DispatcherWait(action + new Action(() => { InterlockInt = 0; }));                
            }
            public void messageDisplayEnd(object sender, EventArgs e)
            {
                dispatchTimer.Stop();
                dispatchTimer.IsEnabled = false;
                if (messages.Count > 0)
                {
                    messageDisplayStart(messages.Pop(), times.Pop(), interruptable.Pop());
                }
                else
                {
                    DispatcherWait(new Action(() =>
                    {
                        textBlock.Visibility = Visibility.Hidden;
                        InterlockInt = 0;
                    }));
                }
            }
            public void DispatcherWait(Action action)
            {
                while (0 != Interlocked.Exchange(ref InterlockInt, 1))
                {
                    Thread.Sleep(10);
                }
                System.Windows.Application.Current.Dispatcher.Invoke(action);
                while (InterlockInt != 0)
                {
                    Thread.Sleep(10);
                }
            }
            public void messageDisplayStart(string displayText, int seconds, bool Interruptable = false, bool highPriority = false)
            {                
                if (dispatchTimer.IsEnabled && LastWasInterruptable == false && highPriority == false)
                {                    
                    messages.Push(displayText);
                    times.Push(seconds);
                    interruptable.Push(Interruptable);
                    InterlockInt = 0;
                    return;
                }                
                LastWasInterruptable = Interruptable;
                DispatcherWait(new Action(() => {
                    textBlock.Visibility = Visibility.Visible;
                    textBlock.Text = displayText;
                    InterlockInt = 0;
                }));   
                if (seconds == -1)
                {
                    return;
                }
                dispatchTimer.Stop();                                
                dispatchTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
                dispatchTimer.Tick -= messageDisplayEnd;
                dispatchTimer.Tick += messageDisplayEnd;
                dispatchTimer.Start();
                dispatchTimer.IsEnabled = true;
            }
        }       
        private async void DisplayNextImageTimer(object sender, EventArgs e)
        {            
            await DisplayGetNextImage(1);
        }

        private async void StartGetFiles()
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(1);
            }
            StartGetFilesNoInterlock();
            Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 0);
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void StartGetFilesNoInterlock()
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
        Boolean Paused = false;
        private void PauseSave()
        {
            pauseStack.Push(dispatcherPlaying.IsEnabled);
            Stop();
            Paused = true;
        }
        private void PauseRestore()
        {
            bool playState = true;
            if (pauseStack.Count > 0)
            {
                playState = pauseStack.Pop();
            }
            if (playState)
            {
                Play();
                Paused = false;
            }            
        }
        private void Stop()
        {
            dispatcherPlaying.Stop();
            SetDisplayMode();           
        }
        
        private void Play()
        {
            if (ImageListReady == false)
            {
                Stop();
                return;
            }
            dispatcherPlaying.Stop();
            dispatcherPlaying.Start();
            SetDisplayMode();            
        }
        private void SetDisplayMode()
        {
            if (!StopMonitorSleepFullScreenOnly || (StopMonitorSleepFullScreenOnly && isMaximized))
            {
                if (StopMonitorSleepPlaying && dispatcherPlaying.IsEnabled)
                {
                    //MessageBox.Show("Display sleep.");
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    return;
                }
                if (StopMonitorSleepPaused && !dispatcherPlaying.IsEnabled)
                {
                    //MessageBox.Show("Display sleep.");
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    return;
                }
            }
            //MessageBox.Show("display awake");
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);                            
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
            if (Starting == false)
            {
                SaveSettings();
            }                        
            base.OnClosing(e);
        }

        bool StopMonitorSleepPlaying = true;
        bool StopMonitorSleepPaused = false;
        bool StopMonitorSleepFullScreenOnly = true;
        private void AllowMonitorSleepPlaying_Checked(object sender, RoutedEventArgs e)
        {
            StopMonitorSleepPlaying = StopSleepPlayingXaml.IsChecked;            
        }

        private void AllowMonitorSleepPaused_Checked(object sender, RoutedEventArgs e)
        {
            StopMonitorSleepPaused = StopSleepPausedXaml.IsChecked;
        }

        private void AllowMonitorSleepFullScreenOnly_Checked(object sender, RoutedEventArgs e)
        {
            StopMonitorSleepFullScreenOnly = StopSleepFullScreenXaml.IsChecked;
        }
        private void EnableMotd(object sender, RoutedEventArgs e)
        {
            ShowMotd = MotdXaml.IsChecked;
            EnableMotdCode();
        }

        
    }
}
