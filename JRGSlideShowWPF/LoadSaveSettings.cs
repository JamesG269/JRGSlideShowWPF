using System;
using System.IO;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        string[] motd = new string[] {};
        public void LoadSettings()
        {            
            RandomizeImages = Properties.Settings.Default.Randomize;
            AllowMonitorSleepPaused = Properties.Settings.Default.AllowSleepPaused;
            AllowMonitorSleepPlaying = Properties.Settings.Default.AllowSleepPlay;
            AllowMonitorSleepFullScreenOnly = Properties.Settings.Default.AllowSleepFull;

            int i = Properties.Settings.Default.TimerSeconds;
            SlideShowDirectory = Properties.Settings.Default.SlideShowFolder;

            ContextMenuCheckBox.IsChecked = RandomizeImages;
            PrivateModeCheckBox.IsChecked = Properties.Settings.Default.PrivateMode;
            AllowSleepFullScreenXaml.IsChecked = AllowMonitorSleepFullScreenOnly;
            AllowSleepPausedXaml.IsChecked = AllowMonitorSleepPaused;
            AllowSleepPlayingXaml.IsChecked = AllowMonitorSleepPlaying;
            ShowMotd = Properties.Settings.Default.ShowMotd;
            MotdXaml.IsChecked = ShowMotd;            
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherPlaying.Interval = new TimeSpan(0, 0, 0, i, c);

            string[] args = Environment.GetCommandLineArgs();

            Boolean cmdlineGoFullScreen = false;
            if (args.Length > 1)
            {                
                cmdlineGoFullScreen = string.Compare(args[1], "/Fullscreen", true) == 0;                
            }
            if (Properties.Settings.Default.isMaximized || cmdlineGoFullScreen )
            {
                GoFullScreen();                
            }
        }
        public void SaveSettings()
        {
            Properties.Settings.Default.isMaximized = isMaximized;
            Properties.Settings.Default.Randomize = RandomizeImages;
            if (PrivateModeCheckBox.IsChecked == false)
            {
                Properties.Settings.Default.SlideShowFolder = SlideShowDirectory;
            }
            Properties.Settings.Default.PrivateMode = PrivateModeCheckBox.IsChecked;
            Properties.Settings.Default.TimerSeconds = dispatcherPlaying.Interval.Seconds;
            Properties.Settings.Default.AllowSleepPaused = AllowSleepPausedXaml.IsChecked;
            Properties.Settings.Default.AllowSleepPlay = AllowSleepPlayingXaml.IsChecked;
            Properties.Settings.Default.AllowSleepFull = AllowSleepFullScreenXaml.IsChecked;
            Properties.Settings.Default.ShowMotd = ShowMotd;
            Properties.Settings.Default.Save();            
        }
        
    }
}