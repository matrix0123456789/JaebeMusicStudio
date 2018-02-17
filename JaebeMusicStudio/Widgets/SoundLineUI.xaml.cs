using JaebeMusicStudio.Sound;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SoundLineUI.xaml
    /// </summary>
    public partial class SoundLineUI : UserControl, IDisposable
    {
        private SoundLine line;
        public SoundLine Line { get { return line; } }
        public SoundLineUI(SoundLine line)
        {
            this.line = line;
            line.ConnectUI();
            InitializeComponent();
            volume.Value = line.volume;
        }

        public void Dispose()
        {
            line.DisconnectUI();
        }

        private void Volume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            line.volume = (float)volume.Value;
        }

        public void Refresh()
        {


            if (line.LastVolume[0] <= 1)
                VolumeL.Width = line.LastVolume[0] * 100;
            else
                VolumeL.Width = 100;

            if (line.LastVolume[1] <= 1)
                VolumeR.Width = line.LastVolume[1] * 100;
            else
                VolumeR.Width = 100;



        }

    }
}
