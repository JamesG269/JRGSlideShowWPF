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
        FileInfo[] ImageList;

        List<FileInfo> NewImageList = new List<FileInfo>();

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
                if (RandomizeNotFinishedIHaveToLOL == true)
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
            NewImageList = null;
            NewImageList = new List<FileInfo>();            
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(new Action(() => {
                TextBlockControl.Visibility = Visibility.Visible;
                TextBlockControl.Text = "Finding images...";
            }));            
            GetFiles(SlideShowDirectory, "*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.webp");
            StartTurnOffTextBoxDisplayTimer(NewImageList.Count() + " images found.", 5);            
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
                        DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
                        FileInfo[] fs = dirInfo.GetFiles(filter);
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