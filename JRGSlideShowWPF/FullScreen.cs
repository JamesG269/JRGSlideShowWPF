using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        WindowState WState = WindowState.Normal;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("called statechanged - WindowState = " + WindowState.ToString());
            WindowStateCode();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            //MessageBox.Show("called sizechanged - WindowState = " + WindowState.ToString());
            WindowStateCode();
        }
        private void WindowStateCode()
        {
            WState = WindowState;
            switch (WindowState)
            {
                case WindowState.Minimized:
                    Hide();
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
                    if (Dragging == true)
                    {                        
                        //WindowToCenter();
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