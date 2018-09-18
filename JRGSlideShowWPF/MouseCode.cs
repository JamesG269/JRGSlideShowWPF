using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        Point mouseStartPoint = new Point(0, 0);

        int MouseWheenCount = 0;
        int MouseOneIntCount = 0;

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

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ResizeMode != ResizeMode.CanResize &&
                    ResizeMode != ResizeMode.CanResizeWithGrip)
                {
                    return;
                }

                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                mRestoreForDragMove = WindowState == WindowState.Maximized;
                DragMove();
            }
        }

        Point lastMovePosition;
        private void OnMouseMove(object sender, MouseEventArgs e)
        {            
            var l = e.GetPosition((IInputElement)sender);
            Boolean mouseStartTimer = false;
            if (l != lastMovePosition)
            {                
                if (MouseHidden == true)
                {
                    dispatcherTimerMouse.Stop();
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    MouseHidden = false;
                    mouseStartTimer = true;
                }
                lastMovePosition = l;                
            }
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;

                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y - (RestoreBounds.Height * 0.5);

                WindowState = WindowState.Normal;
                DragMove();
            }
            if (mouseStartTimer == true)
            {
                dispatcherTimerMouse.Start();
            }
        }

        Boolean MouseHidden = false;
        private void MouseHide(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                if (MouseHidden == true)
                {
                    return;
                }
                MouseHidden = true;
                this.Cursor = System.Windows.Input.Cursors.None;
            }
        }
    
        
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private bool mRestoreForDragMove;

    }
}
