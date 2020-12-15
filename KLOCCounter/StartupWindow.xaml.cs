using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KLOCCounter
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        DispatcherTimer timer = null;
        public StartupWindow()
        {
            InitializeComponent();
            StartTimer();
        }

        void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += new EventHandler(Timer_Elapsed);
            timer.Start();
        }

        void Timer_Elapsed(object sender, EventArgs e)
        {
            timer.Stop();
            new MainWindow().Show();
            Close();
        }
    }
}
