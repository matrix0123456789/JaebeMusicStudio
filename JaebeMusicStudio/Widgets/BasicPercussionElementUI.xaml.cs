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
    /// Logika interakcji dla klasy BasicPercussionElementUI.xaml
    /// </summary>
    public partial class BasicPercussionElementUI : UserControl
    {
        private BasicPercussionElement element;

        public BasicPercussionElementUI(BasicPercussionElement element)
        {
            this.element = element;
            InitializeComponent();
        }


        private void ToneVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.ToneVolume = (float)ToneVolume.Value;
        }

        private void ToneHalfTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.ToneHalfTime = (float)ToneHalfTime.Value;

        }

        private void ToneFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.ToneFrequency = (float)ToneFrequency.Value;

        }

        private void ModulationAddFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.ModulationAddFrequency = (float)ModulationAddFrequency.Value;

        }

        private void ToneModulationTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.ToneModulationTime = (float)ToneModulationTime.Value;

        }

        private void NoiseVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            element.NoiseVolume = (float)NoiseVolume.Value;
        }

        private void NoiseHalfTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            element.NoiseHalfTime = (float)NoiseHalfTime.Value;

        }
    }
}
