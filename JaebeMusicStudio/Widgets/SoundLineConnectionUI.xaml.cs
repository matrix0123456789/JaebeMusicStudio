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
    /// Logika interakcji dla klasy SoundLineConnectionUI.xaml
    /// </summary>
    public partial class SoundLineConnectionUI : UserControl
    {
        private SoundLineConnection connection;

        public SoundLineConnectionUI(SoundLineConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            name.Content = connection.input;
            volume.Value = connection.volume;

        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            connection.volume = (float)volume.Value;
        }
    }
}
