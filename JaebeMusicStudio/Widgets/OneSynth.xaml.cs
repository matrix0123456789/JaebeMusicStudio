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
using JaebeMusicStudio.UI;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for OneSynth.xaml
    /// </summary>
    public partial class OneSynth : UserControl
    {
        private INoteSynth synth;
        public OneSynth(INoteSynth synth)
        {
            this.synth=synth;
            InitializeComponent();
            slSelect.Generate();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (synth is BasicSynth)
            {
                PseudoWindow.OpenWindow(() => new Widgets.BasicSynthUi(synth as BasicSynth));
            }
        }

        private void SlSelect_OnChanged(SoundLineSelect sender, SoundLine obj)
        {
            synth.SoundLine = obj;
        }

        private void Default_OnClick(object sender, RoutedEventArgs e)
        {
            KeyboardInput.singleton.Synth = synth;
        }
    }
}
