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
using JaebeMusicStudio.Sound;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for NonlinearDistortionUI.xaml
    /// </summary>
    public partial class FlangerUI : Page
    {
        private Flanger Effect;
        public FlangerUI(Flanger effect)
        {
            Effect = effect;
            InitializeComponent();
            Init();

        }

        void Init()
        {
            Grids.Children.Clear();
            for (int i = 0; i < this.Effect.Count; i++)
            {
                var x = this.Effect[i];
                var grid = new Grid();

                var amp = new JaebeMusicStudio.UI.Slider();
                amp.Maximum = 1;
                amp.Tag = i;
                amp.Value = x.Amplitude;
                amp.ValueChanged += Amp_ValueChanged;
                grid.Children.Add(amp);

                var fre = new JaebeMusicStudio.UI.Slider();
                fre.Maximum = 0.01;
                fre.Tag = i;
                fre.Value = x.Frequency;
                fre.ValueChanged += Fre_ValueChanged;
                fre.Margin=new Thickness(0,30,0,0);
                grid.Children.Add(fre);

                Grids.Children.Add(grid);
            }
        }

        private void Fre_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var x = Effect[(int) (sender as FrameworkElement).Tag];
            x.Frequency = (float)e.NewValue;
            Effect[(int) (sender as FrameworkElement).Tag] = x;
        }

        private void Amp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var x = Effect[(int)(sender as FrameworkElement).Tag];
            x.Amplitude = (float)e.NewValue;
            Effect[(int)(sender as FrameworkElement).Tag] = x;
        }
    }
}
