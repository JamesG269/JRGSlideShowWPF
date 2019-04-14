using System;
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

            int c = 0;
            if (i == 0)
            {
                c++;
            }
            dispatcherImageTimer.Interval = new TimeSpan(0, 0, 0, i, c);

            string[] args = Environment.GetCommandLineArgs();

            Boolean cmdlineGoFullScreen = false;
            if (args.Length > 1)
            {                
                if (args[1] == "/Fullscreen")
                {
                    cmdlineGoFullScreen = true;
                }
            }


            if (Properties.Settings.Default.isMaximized || cmdlineGoFullScreen )
            {
                GoFullScreen();                
            }
        }
        public void SaveSettings()
        {
            Properties.Settings.Default.isMaximized = isMaximized;
            Properties.Settings.Default.Randomize = Randomize;            
            Properties.Settings.Default.SlideShowFolder = SlideShowDirectory;
            Properties.Settings.Default.TimerSeconds = dispatcherImageTimer.Interval.Seconds;
            Properties.Settings.Default.Save();            
        }
    }
}