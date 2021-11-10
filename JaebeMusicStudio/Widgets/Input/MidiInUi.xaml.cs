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
    /// Logika interakcji dla klasy keyboardUi.xaml
    /// </summary>
    public partial class MidiInUi : UserControl
    {
        Sound.MidiInput midi;
        public MidiInUi(Sound.MidiInput midi)
        {
            this.midi = midi;
            InitializeComponent();
            synthSelect1.Selected = midi.Synth;
            synthSelect1.Generate();
            currentNotes.SetInput(midi);
            controlls.SetInput(midi);
            Title.Content = "Midi input: " + midi.ToString();
        }

        private void SynthSelect1_OnChanged(SynthSelect arg1, Sound.INoteSynth arg2)
        {
            midi.Synth = arg2;
        }

        private void constantVolume_Checked(object sender, RoutedEventArgs e)
        {
            midi.ConstantVolume = constantVolume.IsChecked.Value;
        }
    }
}
