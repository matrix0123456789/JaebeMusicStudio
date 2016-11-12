using System;
using System.Collections.Generic;
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

namespace JaebeMusicStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<Thread> windowsThreads=new List<Thread>();
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
                        var timePosition = Sound.Project.current.countTime(Sound.Player.position);
                        Time.Content = timePosition.ToString();
                    });
                }
            });
            t2.Start();
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
                Sound.Project.current.serialize(dialog.FileName);
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "pliki JMS|*.jms";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                Sound.Project.current=new Sound.Project(dialog.FileName);
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Sound.Player.play();
        }

        private void rewindButton_Click(object sender, RoutedEventArgs e)
        {

            Sound.Player.setPosition(0);
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void openTimelineButton_Click(object sender, RoutedEventArgs e)
        {
            Thread viewerThread = new Thread(delegate ()
            {
                var okno = new UI.PseudoWindow(new Widgets.Timeline());
                System.Windows.Threading.Dispatcher.Run();
            });
            windowsThreads.Add(viewerThread);
            viewerThread.Name = "PseudoWindow Thread";
            viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            viewerThread.Start();


        }
    }
}
