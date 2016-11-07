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
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : Page
    {
        public Timeline()
        {
            InitializeComponent();
            Sound.Project.current.trackAdded += Current_trackAdded;
        }

        private void Current_trackAdded(int index, Sound.Track track)
        {
            var grid = new Grid();
            grid.Background = Brushes.Aqua;
            grid.Height = 40;
            tracksStack.Children.Insert(index, grid);
        }

        private void addNewTrackButton_Click(object sender, RoutedEventArgs e)
        {
            Sound.Project.current.addEmptyTrack();
        }

        private void openFileSampleButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "pliki dźwiękowe|wav.*,wave.*,mp3.*|Wszystkie Pliki|*.*";
            dialog.ShowDialog();
            try
            {
                if (dialog.FileName != "")
                {

                    string[] explode = dialog.FileName.Split('.');
                    Sound.SampledSound.soundFormat format = Sound.SampledSound.soundFormat.wave;
                    if (explode.Last() == "mp3")
                        format = Sound.SampledSound.soundFormat.mp3;
                    var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Open);
                    var ss = new Sound.SampledSound(stream, format);
                }
            }
            catch
            {
                MainWindow.error("Błąd otwarcia pliku");
            }
        }
    }
}
