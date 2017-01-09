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
    /// Interaction logic for BasicSynthOscUi.xaml
    /// </summary>
    public partial class BasicSynthOscUi : UserControl
    {
        private Oscillator osc;

        public BasicSynthOscUi()
        {
            InitializeComponent();
        }

        public BasicSynthOscUi(Oscillator osc)
        {
            this.osc = osc;
            InitializeComponent();
        }

        private void TypeOscSin_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type=OscillatorType.sine;
        }

        private void TypeOscTri_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type = OscillatorType.triangle;
        }

        private void TypeOscSq_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type = OscillatorType.square;
        }

        private void TypeOscSaw_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type = OscillatorType.saw;
        }

        private void TypeOscWhite_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type = OscillatorType.whiteNoise;
        }

        private void TypeOscPink_Checked(object sender, RoutedEventArgs e)
        {
            osc.Type = OscillatorType.pinkNoise;
        }
    }
}
