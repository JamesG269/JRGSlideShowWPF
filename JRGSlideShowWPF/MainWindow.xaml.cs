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
        TextBoxMessage topTextBoxClass;

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
            topTextBoxClass = new TextBoxMessage()
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
                await DisplayGetNextImageWithoutCheck(1);
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
            if (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                return;
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
                PauseSave(true);
                await Task.Run(() => LoadNextImage(i));
                await DisplayCurrentImage();                
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
                    topTextBoxClass.messageDisplayStart(ErrorMessage, 5, false, false);
                    if (IsUserjgentile)
                    {
                        await DeleteNoInterlock(true);
                    }                    
                }
            }

        }
        private void EnableMotdCode()
        {
            if (Starting)
            {
                return;
            }
            getMotd();
            PutMotd();
        }
        Random Rand = new Random();
        private void PutMotd()
        {
            if (ShowMotd)
            {                
                if (motd.Length == 0)
                {
                    return;
                }                
                int i = Rand.Next(0, motd.Length);
                MotdClass.messageDisplayStart(motd[i], -1, true, true);
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
                topTextBoxClass.messageDisplayStart(motd.Length.ToString() + " MOTDs loaded.", 5);
            }
            else
            {
                ShowMotd = false;
                topTextBoxClass.messageDisplayStart("no MOTD found at " + motdFilePath, 5, false, false);
            }
        }

        public class TextBoxStack
        {
            public string message = "test message";
            public int time = 0;
            public bool interruptable = true;
            public bool highPriority = false;
            public bool live = false;            
        }
        public class TextBoxMessage
        {            
            public System.Windows.Threading.DispatcherTimer dispatchTimer;            
            public System.Windows.Controls.TextBlock textBlock; 
            public static int InterlockInt = 0;
            public TextBoxStack textBoxStackCurrent = new TextBoxStack();
            public Stack<TextBoxStack> textBoxStackList = new Stack<TextBoxStack>();
            public void messageDisplayEndUninterruptable(Action action)
            {                
                dispatchTimer.Stop();
                dispatchTimer.IsEnabled = false;
                textBoxStackCurrent.live = false;                
                if (textBoxStackList.Count > 0)
                {
                    var textBoxStackSend = textBoxStackList.Pop();                    
                    messageDisplayStart(textBoxStackSend.message, textBoxStackSend.time, textBoxStackSend.interruptable, textBoxStackSend.highPriority);
                }
                else
                {
                    DispatcherWait(action + new Action(() => {
                        textBlock.Visibility = Visibility.Hidden;
                    }));
                }
            }
            public void messageDisplayEnd(object sender, EventArgs e)
            {
                messageDisplayEndUninterruptable(new Action(() => { }));
            }
            public void DispatcherWait(Action action)
            {
                while (0 != Interlocked.Exchange(ref InterlockInt, 1))
                {
                    Thread.Sleep(10);
                }
                System.Windows.Application.Current.Dispatcher.Invoke(action + new Action(() => { Interlocked.Exchange(ref InterlockInt, 0); }));
                while (InterlockInt != 0)
                {
                    Thread.Sleep(10);
                }
            }
            public void messageDisplayStart(string displayText, int seconds, bool Interruptable = false, bool highPriority = false)
            {                
                var textBoxStackNew = new TextBoxStack();
                textBoxStackNew.message = displayText;
                textBoxStackNew.time = seconds;
                textBoxStackNew.interruptable = Interruptable;
                textBoxStackNew.highPriority = highPriority;
                textBoxStackNew.live = true;                  
                if (textBoxStackNew.highPriority == false && (textBoxStackCurrent.live == true && dispatchTimer.IsEnabled == true && textBoxStackCurrent.interruptable == false))
                {                    
                    textBoxStackList.Push(textBoxStackNew);
                    InterlockInt = 0;
                    return;
                }
                textBoxStackCurrent = textBoxStackNew;
                DispatcherWait(new Action(() => {
                    textBlock.Visibility = Visibility.Visible;
                    textBlock.Text = displayText;
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
                return;
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
        private void PauseSave(bool temp = false)
        {
            pauseStack.Push(dispatcherPlaying.IsEnabled);
            Stop(temp);
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
        private void Stop(bool temp = false)
        {
            dispatcherPlaying.Stop();
            if (!temp)
            {
                SetDisplayMode();
            }            
            Paused = true;
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
        int LastDisplayMode = 0;

        private void SetDisplayMode()
        {
            SetDisplayModeCode();
            updateInfo();
        }
        private void SetDisplayModeCode()
        {
            if (AllowMonitorSleepFullScreenOnly == false)
            {
                SetDisplayModeCheckPlay();
                return;
            }
            if (isMaximized == true)
            {
                SetDisplayModeCheckPlay();
                return;
            }
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            LastDisplayMode = 1;
        }
        private void SetDisplayModeCheckPlay()
        {
            if (AllowMonitorSleepPlaying && dispatcherPlaying.IsEnabled)
            {
                //MessageBox.Show("Display sleep Playing.");
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                LastDisplayMode = 0;
                return;
            }
            else if (AllowMonitorSleepPaused && !dispatcherPlaying.IsEnabled)
            {
                //MessageBox.Show("Display sleep Paused.");
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                LastDisplayMode = 0;
                return;
            }
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            LastDisplayMode = 1;
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

        bool AllowMonitorSleepPlaying = false;
        bool AllowMonitorSleepPaused = true;
        bool AllowMonitorSleepFullScreenOnly = false;
        private void AllowMonitorSleepPlaying_Checked(object sender, RoutedEventArgs e)
        {
            AllowMonitorSleepPlaying = AllowSleepPlayingXaml.IsChecked;
            SetDisplayMode();
        }

        private void AllowMonitorSleepPaused_Checked(object sender, RoutedEventArgs e)
        {
            AllowMonitorSleepPaused = AllowSleepPausedXaml.IsChecked;
            SetDisplayMode();
        }

        private void AllowMonitorSleepFullScreenOnly_Checked(object sender, RoutedEventArgs e)
        {
            AllowMonitorSleepFullScreenOnly = AllowSleepFullScreenXaml.IsChecked;
            SetDisplayMode();
        }
        private void EnableMotd(object sender, RoutedEventArgs e)
        {
            ShowMotd = MotdXaml.IsChecked;
            if (ShowMotd == true)
            {
                EnableMotdCode();
            }
            else
            {
                MotdClass.messageDisplayEndUninterruptable(new Action(() => { MotdClass.textBlock.Visibility = Visibility.Hidden; }));
            }
        }

        private async void DisplayInfo_Checked(object sender, RoutedEventArgs e)
        {
            if (displayingInfo == false)
            {
                await DisplayFileInfo();
            }
        }

        private async void DisplayInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (displayingInfo == true)
            {
                await DisplayFileInfo();
            }
        }
    }
}
