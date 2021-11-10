using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;

namespace JaebeMusicStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //   static List<Thread> windowsThreads=new List<Thread>();
        public MainWindow()
        {
            InitializeComponent();
            Sound.Project.current = new Sound.Project();
            Thread t2 = new Thread(delegate ()
            {
                while (true)
                {
                    Thread.Sleep(10);
                    Dispatcher.Invoke(() =>
                    {
                        var timePosition = Sound.Project.current.CountTime(Sound.Player.position);
                        var timeLength = Sound.Project.current.CountTime(Sound.Project.current.length);
                        Time.Content = timePosition.ToString() + '/' + timeLength.ToString();

                        if (Sound.Player.LastVolume[0] <= 1)
                            VolumeL.Width = Sound.Player.LastVolume[0] * 100;
                        else
                            VolumeL.Width = 100;

                        if (Sound.Player.LastVolume[1] <= 1)
                            VolumeR.Width = Sound.Player.LastVolume[1] * 100;
                        else
                            VolumeR.Width = 100;


                    });
                }
            });
            t2.Start();
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        public static void error(string message)
        {
            MessageBox.Show(message);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "pliki JMS|*.jms";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                Sound.Project.current.Serialize(dialog.FileName);
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "pliki JMS|*.jms";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                UI.PseudoWindow.closeAll();
                Sound.Project.current = new Sound.Project(dialog.FileName);
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Sound.Player.Play();
        }

        private void rewindButton_Click(object sender, RoutedEventArgs e)
        {

            Sound.Player.SetPosition(0);
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            Sound.Player.Pause();
        }

        private void openTimelineButton_Click(object sender, RoutedEventArgs e)
        {
            PseudoWindow.OpenWindow(() => new Widgets.Timeline());
        }

        private void openMixerButton_Click(object sender, RoutedEventArgs e)
        {
            PseudoWindow.OpenWindow(() => new Widgets.Mixer.Mixer());
        }

        private void openOscilloscopeButton_Click(object sender, RoutedEventArgs e)
        {
            PseudoWindow.OpenWindow(() => new Widgets.Oscilloscope());
        }

        private void OpenSynthsButton_OnClick(object sender, RoutedEventArgs e)
        {
            PseudoWindow.OpenWindow(() => new Widgets.Synths());
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            KeyboardInput.singleton1.KeyDown(e);
            KeyboardInput.singleton2.KeyDown(e);
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            KeyboardInput.singleton1.KeyUp(e);
            KeyboardInput.singleton2.KeyUp(e);
        }

        private void RenderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog() { Filter = "mp3|*.mp3|wave|*.wav" };
            dialog.ShowDialog();
            if (dialog.FileName != "")
                SaveSound.file = new FileInfo(dialog.FileName);
            SaveSound.SaveFileAsync();
        }

        private void openInputsButton_Click(object sender, RoutedEventArgs e)
        {
            PseudoWindow.OpenWindow(() => new Widgets.InputsUi());
        }

        private void TempoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Project.current != null)
            {
                Project.current.tempo = float.Parse(TempoTextBox.Text);
            }
        }
    }
}
