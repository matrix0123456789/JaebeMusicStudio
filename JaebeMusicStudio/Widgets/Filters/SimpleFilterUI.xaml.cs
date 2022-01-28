using JaebeMusicStudio.Sound;
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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Logika interakcji dla klasy SimpleFilterUI.xaml
    /// </summary>
    public partial class SimpleFilterUI : UserControl
    {
        private SimpleFilter simpleFilter;

        public SimpleFilterUI(SimpleFilter simpleFilter)
        {
            this.simpleFilter = simpleFilter;
            InitializeComponent();

            Frequency.Value = simpleFilter.Frequency;
            Resonation.Value = simpleFilter.Resonation;
            Volume.Value = simpleFilter.Volume;
            lowpass.IsChecked = simpleFilter.Type == SimpleFilter.FilterType.Lowpass;
            highpass.IsChecked = simpleFilter.Type == SimpleFilter.FilterType.Highpass;
        }

        private void Frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            simpleFilter.Frequency = (float)Frequency.Value;
        }

        private void Resonation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            simpleFilter.Resonation = (float)Resonation.Value;
        }
        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            simpleFilter.Volume = (float)Volume.Value;
        }


        private void lowpass_Checked(object sender, RoutedEventArgs e)
        {
            simpleFilter.Type = SimpleFilter.FilterType.Lowpass;
            highpass.IsChecked = false;
        }

        private void highpass_Checked(object sender, RoutedEventArgs e)
        {

            simpleFilter.Type = SimpleFilter.FilterType.Highpass;
            lowpass.IsChecked = false;
        }
    }
}
