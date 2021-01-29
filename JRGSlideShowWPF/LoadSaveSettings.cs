using System;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public void LoadSettings()
        {           
            RandomizeNotFinishedIHaveToLOL = Properties.Settings.Default.Randomize;
            int i = Properties.Settings.Default.TimerSeconds;
            SlideShowDirectory = Properties.Settings.Default.SlideShowFolder;

            ContextMenuCheckBox.IsChecked = RandomizeNotFinishedIHaveToLOL;
            PrivateModeCheckBox.IsChecked = Properties.Settings.Default.PrivateMode;
            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherE10E.Interval = new TimeSpan(0, 0, 0, i, c);

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
            Properties.Settings.Default.Randomize = RandomizeNotFinishedIHaveToLOL;
            if (PrivateModeCheckBox.IsChecked == false)
            {
                Properties.Settings.Default.SlideShowFolder = SlideShowDirectory;
            }
            Properties.Settings.Default.PrivateMode = PrivateModeCheckBox.IsChecked;
            Properties.Settings.Default.TimerSeconds = dispatcherE10E.Interval.Seconds;
            Properties.Settings.Default.Save();            
        }
    }
}