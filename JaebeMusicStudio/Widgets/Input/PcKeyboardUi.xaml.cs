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
    /// Logika interakcji dla klasy PcKeyboardUi.xaml
    /// </summary>
    public partial class PcKeyboardUi : UserControl
    {
        public PcKeyboardUi(Sound.KeyboardInput singleton)
        {
            InitializeComponent();
            updateConfig();
            currentNotes.SetInput(singleton);
        }
        void updateConfig()
        {
            synthSelect1.Selected = KeyboardInput.singleton1.Synth;
            synthSelect2.Selected = KeyboardInput.singleton2.Synth;
            synthSelect1.Generate();
            synthSelect2.Generate();
        }

        private void SynthSelect1_OnChanged(SynthSelect arg1, Sound.INoteSynth arg2)
        {
            KeyboardInput.singleton1.Synth = arg2;
        }
        private void SynthSelect2_OnChanged(SynthSelect arg1, Sound.INoteSynth arg2)
        {
            KeyboardInput.singleton2.Synth = arg2;
        }


        private void keyboardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (keyboardType.SelectedIndex == 0)
            {
                KeyboardInput.singleton1.type = KeyboardInput.Type.lowerAndUpper;
                KeyboardInput.singleton2.type = KeyboardInput.Type.silent;
            }
            else if (keyboardType.SelectedIndex == 1)
            {
                KeyboardInput.singleton1.type = KeyboardInput.Type.lower;
                KeyboardInput.singleton2.type = KeyboardInput.Type.upper;
            }
            else
            {
                KeyboardInput.singleton1.type = KeyboardInput.Type.launchpad;
                KeyboardInput.singleton2.type = KeyboardInput.Type.silent;
            }
        }

        private void synthTranspose1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            KeyboardInput.singleton1.Pitch = (float)(synthTranspose1.Value * 12);
        }

        private void synthTranspose2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            KeyboardInput.singleton2.Pitch = (float)(synthTranspose1.Value * 12);
        }
    }
}
