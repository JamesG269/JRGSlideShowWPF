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
        int MouseWheelCount = 0;
        int MouseOneIntCount = 0;

        private bool mRestoreForDragMove;

        private async void MouseWheel2(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MouseWheelCount++;
                if (OneInt == 1)
                {
                    MouseOneIntCount++;                    
                }
                await DisplayNextImage();                              
            }
            else if (e.Delta < 0)
            {
                await DisplayPrevImage();
            }
        }

        Boolean MouseLeftDown = false;
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseHidden == false)
            {
                dispatcherMouseTimer.Stop();
                dispatcherMouseTimer.Start();
            }
            if (e.ClickCount == 2)
            {
                if (isMaximized == true)
                {
                    var point = System.Windows.Forms.Cursor.Position;            // have to get cursor position before leavefullscreen() I think...
                    LeaveFullScreen(false);
                    WindowToCursor(point);
                }
                else
                {
                    GoFullScreen();
                }
            }
            else
            {
                mRestoreForDragMove = isMaximized;

                if (!isMaximized)
                {
                    MouseLeftDown = true;
                }
                StartMouseMove = System.Windows.Forms.Cursor.Position;
            }
        }
        System.Drawing.Point StartMouseMove;

        Point lastMovePosition;
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition((IInputElement)sender);

            if (mRestoreForDragMove)
            {
                var point = System.Windows.Forms.Cursor.Position;
                if (Math.Abs(point.X - StartMouseMove.X) >= SystemParameters.MinimumHorizontalDragDistance*3 ||
                    Math.Abs(point.Y - StartMouseMove.Y) >= SystemParameters.MinimumVerticalDragDistance*3)
                {
                    mRestoreForDragMove = false;
                    LeaveFullScreen(false);
                    WindowToCursor(point);
                    try
                    {
                        DragMove();
                    }
                    catch { }
                }
            }
            else if (MouseLeftDown == true)
            {
                var point = System.Windows.Forms.Cursor.Position;
                MouseLeftDown = false;
                LeaveFullScreen(false);
                WindowToCursor(point);
                try
                {
                    DragMove();
                }
                catch { }
            }
            if (currentPosition != lastMovePosition)
            {
                if (MouseHidden == true)
                {
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    MouseHidden = false;
                }
                lastMovePosition = currentPosition;
            }
            if (isMaximized && !MouseHidden)
            {
                dispatcherMouseTimer.Start();
            }
        }

        private void WindowToCursor(System.Drawing.Point point)
        {
            Matrix matrix;
            if (PSource != null)
            {
                matrix = PSource.CompositionTarget.TransformToDevice;                       // Have to check if this works when moving to a second monitor.
                Left = (int)((point.X / matrix.M11) - (RestoreBounds.Width / 2));
                Top = (int)((point.Y / matrix.M22) - (RestoreBounds.Height / 2));
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
            dispatcherMouseTimer.Stop();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
            MouseLeftDown = false;
        }
    }
}
