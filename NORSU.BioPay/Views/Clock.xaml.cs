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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;

namespace NORSU.BioPay.Views
{
    /// <summary>
    /// Interaction logic for Clock.xaml
    /// </summary>
    public partial class Clock : UserControl
    {
        public Clock()
        {
            InitializeComponent();
            
            Tick();
            ToggleMinutes();
        }

        private async void ToggleMinutes()
        {
            while (true)
            {
                Time.DisplayMode = ClockDisplayMode.Hours;
                await TaskEx.Delay(7777);
                Time.DisplayMode = ClockDisplayMode.Minutes;
                await TaskEx.Delay(4444);
            }
        }

        private async void Tick()
        {
            while (true)
            {
                Time.Time = DateTime.Now;
                await TaskEx.Delay(111);
            }
        }
    }
}
