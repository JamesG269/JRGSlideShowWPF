using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            WindowStateCode();            
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            WindowStateCode();            
        }
        
        private void WindowStateCode()
        {                        
            switch (WindowState)
            {                
                case WindowState.Minimized:
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);                    
                    return;
                case WindowState.Maximized:                    
                    GoFullScreen();
                    
                    if (dispatcherTimerSlow.IsEnabled == true && dispatcherTimerFast.IsEnabled == true)
                    {
                        SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                    }                    
                    return;
                case WindowState.Normal:
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    LeaveFullScreen();                   
                    return;
            }                    
        }

        int oldHeight = 0;
        int oldWidth = 0;
        int oldTop = 0;
        int oldLeft = 0;
        Boolean isMaximized = false;
        private void GoFullScreen()
        {
            if (isMaximized == false)
            {
                WindowState = WindowState.Normal;
                oldHeight = (int)Height;
                oldWidth = (int)Width;
                oldTop = (int)Top;
                oldLeft = (int)Left;
                
                Height = ResizeMaxHeight;
                Width = ResizeMaxWidth;
                Top = Left = 0;                
                isMaximized = true;
                dispatcherTimerMouse.Start();
            }            
        }

        private void LeaveFullScreen()
        {
            if (isMaximized == true)
            {
                Height = oldHeight;
                Width = oldWidth;
                Top = oldTop;
                Left = oldLeft;
                isMaximized = false;
                dispatcherTimerMouse.Stop();
            }
        }
        private void ToggleMaximize()
        {
            if (isMaximized)
            {
                LeaveFullScreen();
            }
            else
            {
                GoFullScreen();
            }
        }
    }
}