using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public ContextMenu NotifyMenu;

        public NotifyIcon NIcon;

        private async void NotifyNextCode(object sender, EventArgs e)
        {
            await DisplayGetNextImage(1);
        }
        private async void NotifyPrevCode(object sender, EventArgs e)
        {
            await DisplayGetNextImage(1);
        }
        private async void NotifyDeleteCode(object sender, EventArgs e)
        {
            PauseSave();
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }            
            await DeleteNoInterlock();
            PauseRestore();
            Interlocked.Exchange(ref OneInt, 0);            
        }
        private async void NotifyCopyDeleteCode(object sender, EventArgs e)
        {            
            await CopyDeleteCode();            
        }
        private async void NotifyOpenCode(object sender, EventArgs e)
        {
            await OpenDirCheckCancel();           
        }
        private void NotifyTimerCode(object sender, EventArgs e)
        {
            ChangeTimerCode();
        }
        private void NotifyFullScreenCode(object sender, EventArgs e)
        {
            ToggleMaximize();
        }
        private void NotifyFullScreenPickCode(object sender, EventArgs e)
        {
        }
        private void NotifyNormalPickCode(object sender, EventArgs e)
        {
        }
        private async void NotifyRandomizeCode(object sender, EventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(1);
            }
            (sender as MenuItem).Checked = !(sender as MenuItem).Checked;
            var s = (sender as MenuItem).Checked;
            RandomizeImages = s;
            await Task.Run(() => RandomizeBW_DoWork());
            await DisplayCurrentImage();
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void NotifyWipeCode(object sender, EventArgs e)
        {
        }
        private void NotifyExitCode(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NotifyStart()
        {
            NIcon = new NotifyIcon();

            Uri cdico = new Uri(@"/cd.ico", UriKind.Relative);
            Stream IconStream = System.Windows.Application.GetResourceStream(cdico).Stream;
            NIcon.Icon = new Icon(IconStream);

            NIcon.Text = "JRG's SlideShow WPF";
            NIcon.Visible = true;
            NIcon.MouseClick += new MouseEventHandler(NotifyIconShowWindow);
            BuildNotify();
        }

        Boolean isMinimized = false;
        public void NotifyIconShowWindow(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (isMinimized)
            {
                Show();                
                isMinimized = false;                
                Activate();
                SetDisplayMode();
            }
            else
            {               
                Hide();
                isMinimized = true;
                SetDisplayMode();
            }
        }
        public void BuildNotify()
        {
            NotifyMenu = new ContextMenu();

            MenuItem NotifyPrev = new MenuItem();
            NotifyPrev.Index = 0;
            NotifyPrev.Text = "Previous";
            NotifyPrev.Click += new EventHandler(NotifyPrevCode);
            NotifyMenu.MenuItems.Add(NotifyPrev);

            MenuItem NotifyNext = new MenuItem();
            NotifyNext.Index = 1;
            NotifyNext.Text = "Next";
            NotifyNext.Click += new EventHandler(NotifyNextCode);
            NotifyMenu.MenuItems.Add(NotifyNext);

            MenuItem NotifyDele = new MenuItem();
            NotifyDele.Index = 2;
            NotifyDele.Text = "Delete";
            NotifyDele.Click += new EventHandler(NotifyDeleteCode);
            NotifyMenu.MenuItems.Add(NotifyDele);

            MenuItem NotifyDelc = new MenuItem();
            NotifyDelc.Index = 3;
            NotifyDelc.Text = "Delete and Copy";
            NotifyDelc.Click += new EventHandler(NotifyCopyDeleteCode);
            NotifyMenu.MenuItems.Add(NotifyDelc);

            MenuItem NotifyOpen = new MenuItem();
            NotifyOpen.Index = 4;
            NotifyOpen.Text = "Open Folder";
            NotifyOpen.Click += new EventHandler(NotifyOpenCode);
            NotifyMenu.MenuItems.Add(NotifyOpen);

            MenuItem NotifySett = new MenuItem();
            NotifySett.Index = 5;
            NotifySett.Text = "Timer Setting";
            NotifySett.Click += new EventHandler(NotifyTimerCode);
            NotifyMenu.MenuItems.Add(NotifySett);

            MenuItem NotifyFull = new MenuItem();
            NotifyFull.Index = 6;
            NotifyFull.Text = "Full Screen Toggle";
            NotifyFull.Click += new EventHandler(NotifyFullScreenCode);
            NotifyMenu.MenuItems.Add(NotifyFull);

            MenuItem NotifyFul2 = new MenuItem();
            NotifyFul2.Index = 7;
            NotifyFul2.Text = "Full Screen on Monitor";

            MenuItem NotifyNorm = new MenuItem();
            NotifyNorm.Index = 8;
            NotifyNorm.Text = "Normal Window on Monitor";

            foreach (var scrn in Screen.AllScreens)
            {
                MenuItem ts = new MenuItem(scrn.DeviceName);
                MenuItem nw = new MenuItem(scrn.DeviceName);
                ts.Name = scrn.DeviceName;
                nw.Name = scrn.DeviceName;
                ts.Click += new EventHandler(NotifyFullScreenPickCode);
                nw.Click += new EventHandler(NotifyNormalPickCode);
                NotifyFul2.MenuItems.Add(ts);
                NotifyNorm.MenuItems.Add(nw);
            }
            NotifyMenu.MenuItems.Add(NotifyFul2);
            NotifyMenu.MenuItems.Add(NotifyNorm);

            MenuItem NotifyRand = new MenuItem();
            NotifyRand.Checked = RandomizeImages;
            NotifyRand.Index = 9;
            NotifyRand.Text = "Randomize";
            NotifyRand.Click += new EventHandler(NotifyRandomizeCode);
            NotifyMenu.MenuItems.Add(NotifyRand);

            MenuItem NotifyWipe = new MenuItem();
            NotifyWipe.Index = 10;
            NotifyWipe.Text = "Wipe Settings and Restart";
            NotifyWipe.Click += new EventHandler(NotifyWipeCode);
            NotifyMenu.MenuItems.Add(NotifyWipe);

            MenuItem NotifyExit = new MenuItem();
            NotifyExit.Index = 11;
            NotifyExit.Text = "Exit";
            NotifyExit.Click += new EventHandler(NotifyExitCode);
            NotifyMenu.MenuItems.Add(NotifyExit);

            NIcon.ContextMenu = NotifyMenu;
        }        
    }
}