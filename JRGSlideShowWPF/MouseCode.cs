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

        int MouseWheelCount = 0;
        int MouseOneIntCount = 0;

        private void mouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MouseWheelCount++;
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

        Boolean MouseLeftDown = false;
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseHidden == false)
            {
                dispatcherTimerMouse.Stop();
                dispatcherTimerMouse.Start();
            }
            if (e.ClickCount == 2)
            {
                if (isMaximized == true)
                {
                    LeaveFullScreen();                                       
                }
                else
                {
                    GoFullScreen();
                }                
            }
            else
            {
                mRestoreForDragMove = isMaximized;// = WindowState == WindowState.Maximized;
                
                if (isMaximized)
                {
                    //LeaveFullScreen();
                    //DragMove();
                }
                else
                {
                    MouseLeftDown = true;
                }
                
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

                //oldLeft = (int)(point.X - (oldLeft * .5)); // RestoreBounds.Width * 0.5);
                // oldTop = (int)(point.Y - (oldTop * .5));//  RestoreBounds.Height * 0.5);
                //MessageBox.Show("FUCK");
                LeaveFullScreen();

                WindowState = WindowState.Normal;
                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y - (RestoreBounds.Height * 0.5);
                DragMove();
            }
            else if (MouseLeftDown == true)
            {
                MouseLeftDown = false;
                var point = PointToScreen(e.MouseDevice.GetPosition(this));
                /*
                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y - (RestoreBounds.Height * 0.5);

                oldTop = (int)Top;
                oldLeft = (int)Left;
                */
                LeaveFullScreen();
                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y - (RestoreBounds.Height * 0.5);
                DragMove();
                
            }
            if (mouseStartTimer && isMaximized)
            {
                dispatcherTimerMouse.Start();
            }
        }

        
        Boolean MouseHidden = false;
        private void MouseHide(object sender, EventArgs e)
        {
            if (isMaximized)
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
            MouseLeftDown = false;
        }

        private bool mRestoreForDragMove;

    }
}
