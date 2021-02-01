using System;
using System.IO;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        string[] motd;
        public void LoadSettings()
        {            
            RandomizeImages = Properties.Settings.Default.Randomize;
            StopMonitorSleepPaused = Properties.Settings.Default.AllowSleepPaused;
            StopMonitorSleepPlaying = Properties.Settings.Default.AllowSleepPlay;
            StopMonitorSleepFullScreenOnly = Properties.Settings.Default.AllowSleepFull;

            int i = Properties.Settings.Default.TimerSeconds;
            SlideShowDirectory = Properties.Settings.Default.SlideShowFolder;

            ContextMenuCheckBox.IsChecked = RandomizeImages;
            PrivateModeCheckBox.IsChecked = Properties.Settings.Default.PrivateMode;
            StopSleepFullScreenXaml.IsChecked = StopMonitorSleepFullScreenOnly;
            StopSleepPausedXaml.IsChecked = StopMonitorSleepPaused;
            StopSleepPlayingXaml.IsChecked = StopMonitorSleepPlaying;
            ShowMotd = Properties.Settings.Default.ShowMotd;
            MotdXaml.IsChecked = ShowMotd;
            getMotd();
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
            Properties.Settings.Default.AllowSleepPaused = StopSleepPausedXaml.IsChecked;
            Properties.Settings.Default.AllowSleepPlay = StopSleepPlayingXaml.IsChecked;
            Properties.Settings.Default.AllowSleepFull = StopSleepFullScreenXaml.IsChecked;
            Properties.Settings.Default.ShowMotd = ShowMotd;
            Properties.Settings.Default.Save();            
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
                StartTurnOffTextBoxDisplayTimer(motd.Length.ToString() + " MOTD's loaded.", 5);
            }
        }
    }
}