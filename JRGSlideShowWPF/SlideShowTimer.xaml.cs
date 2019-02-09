using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JRGSlideShowWPF
{
    /// <summary>
    /// Interaction logic for SlideShowTimer.xaml
    /// </summary>
    public partial class SlideShowTimer : Window
    {
        public SlideShowTimer()
        {
            InitializeComponent();
        }

        private void TimerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TimerTextBox.Text.Length < 1)
                {
                    e.Handled = true;
                    return;
                }                
                this.Close();
            }
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                e.Handled = true;
            }                
        }
    }
}
