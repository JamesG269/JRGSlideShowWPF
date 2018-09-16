using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{    
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        Point mouseStartPoint = new Point(0, 0);
        Boolean IgnoreNext = false;
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IgnoreNext = true;
            maximize();

        }

        private void mouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MouseWheenCount++;
                if (OneInt == 1)
                {
                    MouseOneIntCount++;
                }
                displayNextImage();
            }
            else if (e.Delta < 0)
            {
                displayPrevImage();
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            if (WindowState == WindowState.Maximized && IgnoreNext == true)
            {
                IgnoreNext = false;
                return;
            }
            Vector mouseDiff = mouseStartPoint - e.GetPosition(null);
            if (Math.Abs(mouseDiff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(mouseDiff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    WindowToCenter();
                }
                //
                Dragging = true;
                this.DragMove();
                Dragging = false;
            }            
        }
        Boolean Dragging = false;
        
        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseStartPoint = e.GetPosition(null);
        }        
        private void WindowToCenter()
        {
            System.Drawing.Point point = Control.MousePosition;
            this.Left = point.X - (Width / 2);
            this.Top = point.Y - (Height / 2);
        }        
    }
}
