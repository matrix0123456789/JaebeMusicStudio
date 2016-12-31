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
    public partial class SoundLineUI : UserControl
    {
        private SoundLine line;
        public SoundLineUI(SoundLine line)
        {
            this.line = line;
            InitializeComponent();
            volume.Value = line.volume;
        }

        private void Volume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            line.volume = (float)volume.Value;
        }
    }
}
