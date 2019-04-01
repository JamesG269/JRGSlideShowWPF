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
        String[] ImageList;

        List<string> NewImageList = new List<string>();

        int[] ImageIdxList;

        Boolean ImageListReady = false;

        int ImageIdxListPtr = 0;
        int ImageIdxListDeletePtr = -1;       
        
        private void CreateIdxListCode()
        {
            if (ImageList != null && ImageList.Length > 0)
            {
                ImageIdxListDeletePtr = -1;
                ImageIdxList = null;
                ImageIdxList = new int[ImageList.Length];
                for (int i = 0; i < ImageList.Length; i++)
                {
                    ImageIdxList[i] = i;
                }
                ImageIdxListPtr = 0;
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
        private void GetFilesCode()
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
            
            GetFiles(SlideShowDirectory, "*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.webp");
            
            Application.Current.Dispatcher.Invoke(new Action(() => {
                TextBlockControl.Visibility = Visibility.Hidden;
            }));
        }
        
        public void GetFiles(string path, string searchPattern)
        {
            string[] patterns = searchPattern.Split(';');
            Stack<string> dirs = new Stack<string>();
            if (!Directory.Exists(path))
            {
                return;
            }
            dirs.Push(path);
            do
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
                        if (StartGetFiles_Cancel)
                        {
                            NewImageList = null;
                            return;
                        }
                        var fs = Directory.GetFiles(currentDir, filter);
                        NewImageList.AddRange(fs);
                        if (fs.Length > 0)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                TextBlockControl.Text = NewImageList.Count + " Images found...";
                            }));
                        }
                    }
                }
                catch { }
            } while (dirs.Count > 0);
        }
    }
}