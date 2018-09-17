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

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //IgnoreNext = true;
            //maximize();
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
        
        Boolean Pressed = false;
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {/*
            if (e.LeftButton != MouseButtonState.Pressed)
            {                
                return;
            }
            
            if (WindowState == WindowState.Maximized && IgnoreNext == true)
            {
                //IgnoreNext = false;
                //return;
            }
            //Vector mouseDiff = mouseStartPoint - e.GetPosition(null);
            //if (Math.Abs(mouseDiff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(mouseDiff.Y) > SystemParameters.MinimumVerticalDragDistance)
           */ 

                                                        
        }
        
        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //mouseStartPoint = e.GetPosition(null);
            if (e.ClickCount == 2)
            {
                maximize();
            }            
        }
        Stack myStack = new Stack();
        private void WindowToCenter(int height, int width)
        {            
            var oldLeft = Left;
            var oldTop = Top;

            myStack.Push(width);
            myStack.Push(height);            

            System.Drawing.Point point = Control.MousePosition;
            this.Left = point.X - (width / 2);
            this.Top = point.Y - (height / 2);

            

           // MessageBox.Show("OldLeft = " + oldLeft + " OldTop = " + oldTop + " Point.X = " + point.X + " point.Y = " + point.Y + " Left = " + Left + " Top = " + Top);
        }

        private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            MessageBox.Show("Drag");

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

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;

                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y;

                WindowState = WindowState.Normal;
                DragMove();
            }            
        }
        
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private bool mRestoreForDragMove;

    }
}
