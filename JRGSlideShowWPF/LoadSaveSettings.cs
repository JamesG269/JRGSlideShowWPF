using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public void LoadSettings()
        {           
            Randomize = Properties.Settings.Default.Randomize;
            int i = Properties.Settings.Default.TimerSeconds;
            SlideShowDirectory = Properties.Settings.Default.SlideShowFolder;

            ContextMenuCheckBox.IsChecked = Randomize;
            dispatcherTimerSlow.Interval = new TimeSpan(0, 0, 0, i, 0);

            if (SlideShowDirectory == null || !Directory.Exists(SlideShowDirectory))
            {
                OpenImageDirectory();
            }            
            if (Properties.Settings.Default.isMaximized)
            {
                GoFullScreen();                
            }
        }
        public void SaveSettings()
        {
            Properties.Settings.Default.isMaximized = isMaximized;
            Properties.Settings.Default.Randomize = Randomize;            
            Properties.Settings.Default.SlideShowFolder = SlideShowDirectory;
            Properties.Settings.Default.TimerSeconds = dispatcherTimerSlow.Interval.Seconds;
            Properties.Settings.Default.Save();            
        }
    }
}