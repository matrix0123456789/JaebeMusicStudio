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
    /// Interaction logic for BasicSynthUi.xaml
    /// </summary>
    public partial class BasicSynthUi : Page
    {
        private BasicSynth basic;
        public BasicSynthUi(BasicSynth basic)
        {
            this.basic = basic;
            InitializeComponent();
            showContent();
        }
        void showContent()
        {
            OscillatorsList.Children.RemoveRange(0, OscillatorsList.Children.Count - 2);
            int index = 0;
            foreach (var osc in basic.oscillators)
            {
                basicSynth_oscillatorAdded(index, osc);
                index++;
            }
        }

        private void basicSynth_oscillatorAdded(int index, Oscillator osc)
        {
            var oscUI=new BasicSynthOscUi(osc);
            OscillatorsList.Children.Insert(index, oscUI);
        }
    }
}
