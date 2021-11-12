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

namespace JaebeMusicStudio.Widgets.Mixer
{
    /// <summary>
    /// Interaction logic for SoundLineUI.xaml
    /// </summary>
    public partial class SoundLineUI : UserControl, IDisposable
    {
        public readonly SoundLineAbstract Line;
        public SoundLineUI(SoundLineAbstract line, bool isOutputLine)
        {
            Line = line;
            Line.ConnectUI();
            InitializeComponent();
            volume.Value = line.volume;

            title.Content = line.ToString();
            OutputRadioButton.IsChecked = isOutputLine;
        }

        public void Dispose()
        {
            Line.DisconnectUI();
        }

        private void Volume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Line.volume = (float)volume.Value;
        }

        public void Refresh()
        {


            if (Line.LastVolume[0] <= 1)
                VolumeL.Width = Line.LastVolume[0] * 100;
            else
                VolumeL.Width = 100;

            if (Line.LastVolume[1] <= 1)
                VolumeR.Width = Line.LastVolume[1] * 100;
            else
                VolumeR.Width = 100;



        }

        private void OutputRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Project.current.OutputLine = Line;
        }
    }
}
