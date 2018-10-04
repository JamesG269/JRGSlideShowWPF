using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        List<string> ImageList = new List<string>();
        List<string> NewImageList = new List<string>();

        List<int> ImageIdxList = new List<int>();

        Boolean ImageListReady = false;

        int ImageIdxListPtr = 0;
        int ImageListDeletePtr = -1;       

        public void CreateIdxListBW_DoWork(object sender, DoWorkEventArgs e)
        {
            CreateIdxListCode();
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void CreateIdxListCode()
        {
            if (ImageList != null && ImageList.Count > 0)
            {
                ImageIdxList.Clear();                
                for (int i = 0; i < ImageList.Count; i++)
                {
                    ImageIdxList.Add(i);
                }
                ImageIdxListPtr = 0;
                ImageListDeletePtr = -1;
                if (Randomize == true)
                {
                    InitRNGKeys();
                    EncryptIdxListCode();
                    InitRNGKeys();
                }
                //FilesLoaded = imageFiles.Count;
            }
            else
            {
                MessageBox.Show("No images found in: " + SlideShowDirectory);
                //Focus();
                return;
            }
        }        
        private void GetFilesCode(BackgroundWorker bgw)
        {
            NewImageList.Clear();
            if (SlideShowDirectory == null || !Directory.Exists(SlideShowDirectory))
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(new Action(() => {
                TextBlockControl.Visibility = Visibility.Visible;
                TextBlockControl.Text = "Finding images...";
            }));
            
            GetFiles(SlideShowDirectory, bgw, "*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff");

            Application.Current.Dispatcher.Invoke(new Action(() => {
                TextBlockControl.Visibility = Visibility.Hidden;
            }));
        }
        
        public void GetFiles(string path, BackgroundWorker bgw, string searchPattern)
        {
            string[] patterns = searchPattern.Split(';');
            //List<string> files = new List<string>();
            Stack<string> dirs = new Stack<string>();
            if (!Directory.Exists(path))
            {
                throw new ArgumentException();
            }
            dirs.Push(path);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                try
                {
                    string[] subDirs = Directory.GetDirectories(currentDir);
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
                catch { }
                try
                {
                    foreach (string filter in patterns)
                    {
                        var fs = Directory.GetFiles(currentDir, filter);
                        NewImageList.AddRange(fs);
                        if (fs.Length > 0)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() => {
                                TextBlockControl.Text = NewImageList.Count + " Images found...";
                            }));                            
                        }
                        if (bgw != null && bgw.CancellationPending)
                        {
                            NewImageList.Clear();                            
                            return;
                        }
                    }
                }
                catch { }
            }           
        }
    }
}