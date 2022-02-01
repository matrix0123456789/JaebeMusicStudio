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
    public partial class NonlinearDistortionUI : Page
    {
        private NonlinearDistortion Effect;
        public NonlinearDistortionUI(NonlinearDistortion effect)
        {
            Effect = effect;
            InitializeComponent();

            PowerExponentiation.Value = effect.PowerExponentiation;
            PowerExponentiation.IsEnabled = Effect.EffectType == NonlinearDistortionType.Power;
            power.IsChecked = Effect.EffectType == NonlinearDistortionType.Power;
            arctan.IsChecked = Effect.EffectType == NonlinearDistortionType.ArcTan;

            drawChart();
        }

        private void PowerExponentiation_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Effect.PowerExponentiation = (float)PowerExponentiation.Value;
            drawChart();
        }

        private void LimiterThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void power_Checked(object sender, RoutedEventArgs e)
        {
            Effect.EffectType = NonlinearDistortionType.Power;
            PowerExponentiation.IsEnabled = true;
            drawChart();
        }

        private void arctan_Checked(object sender, RoutedEventArgs e)
        {
            Effect.EffectType = NonlinearDistortionType.ArcTan;
            PowerExponentiation.IsEnabled = false;
            drawChart();
        }
        private void drawChart()
        {
            var points = 1024;
            var input = new float[2, points];

            for (var i = 0; i < points; i++)
            {
                input[0, i] = ((float)i / (float)points * 2) - 1;
            }
            var output = Effect.DoFilter(input, new Rendering { sampleRate=48000});
            chart.Points.Clear();
            for (var i = 0; i < points; i++)
            {
                chart.Points.Add(new Point((float)i * 256f / points,256f*(output[0, i]+1)/2));
            }
        }
    }
}
