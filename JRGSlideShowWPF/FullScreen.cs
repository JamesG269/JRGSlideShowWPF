using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private WindowState WState = WindowState.Normal;

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            WindowStateCode(Height, Width);
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            WindowStateCode(sizeInfo.NewSize.Height, sizeInfo.NewSize.Width);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("called statechanged - WindowState = " + WindowState.ToString());
            //WindowStateCode();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            //MessageBox.Show("called sizechanged - WindowState = " + WindowState.ToString());
            //WindowStateCode();
        }        
        private void WindowStateCode(double height, double width)
        {
            WState = WindowState;
            switch (WindowState)
            {                
                case WindowState.Minimized:
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    return;
                case WindowState.Maximized:
                    if (dispatcherTimerSlow.IsEnabled == true && dispatcherTimerFast.IsEnabled == true)
                    {
                        SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                    }
                    return;
                case WindowState.Normal:
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    //if (Dragging == true)
                    {                        
                        //WindowToCenter((int)height,(int)width);
                    }                    
                    return;
            }
            
        }
        private void maximize()
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
                
            }
        }
    }
}