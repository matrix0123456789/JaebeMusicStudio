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
    public partial class ReverbUI : Page
    {
        private Reverb Effect;
        public ReverbUI(Reverb effect)
        {
            Effect = effect;
            InitializeComponent();

            Volume.Value = effect.Volume;
            Delay.Value = effect.Delay;
            Feedback.Value = effect.Feedback;
            Pan.Value = effect.Pan;
        }

        private void Volume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Effect.Volume = (float)Volume.Value;
        }

        private void Delay_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Effect.Delay = (float)Delay.Value;
        }

        private void Feedback_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Effect.Feedback = (float)Feedback.Value;
        }

        private void Pan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Effect.Pan = (float)Pan.Value;
        }
    }
}
