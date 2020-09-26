using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Principal;
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

namespace JaebeMusicStudio.Widgets.Mixer
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Mixer : Page, IDisposable
    {
        SoundLineUI selectedLine = null;
        SoundLineProperties selectedLineProperties;
        Thread refreshThread;
        public Mixer()
        {
            InitializeComponent();
            Sound.Project.current.lines.CollectionChanged += Lines_CollectionChanged;
            showContent();
            refreshThread = new Thread(delegate ()
            {
                while (true)
                {
                    Thread.Sleep(10);
                    Dispatcher?.Invoke(() =>
                    {
                        foreach (var x in SoundLinesList.Children)
                        {
                            (x as SoundLineUI)?.Refresh();
                        }
                    });
                }
            });
            refreshThread.Start();
        }

        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher?.InvokeAsync(() =>
            {
                foreach (var line in e.NewItems)
                {
                    var lineUI = new SoundLineUI(line as SoundLineAbstract);
                    lineUI.MouseDown += LineUI_MouseDown;
                    SoundLinesList.Children.Add(lineUI);
                }
            });
        }

        void showContent()
        {
            SoundLinesList.Children.Clear();

            foreach (var line in Sound.Project.current.lines)
            {
                var lineUI = new SoundLineUI(line);
                lineUI.MouseDown += LineUI_MouseDown;
                SoundLinesList.Children.Add(lineUI);
            }

            foreach (var line in Project.current.liveLines.getAvaibleInputs())
            {
                var lineUI = new SoundLineUI(line);
                lineUI.MouseDown += LineUI_MouseDown;
                SoundLinesList.Children.Add(lineUI);
            }
        }

        private void lineAdded(int index, SoundLineAbstract line)
        {
            var lineUI = new SoundLineUI(line);
            SoundLinesList.Children.Insert(index, lineUI);
            lineUI.MouseDown += LineUI_MouseDown;
        }

        private void LineUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedLine != null)
            {
                selectedLine.Background = Brushes.White;
            }
            selectedLinePropertiesWrapper.Children.Clear();
            if (sender is SoundLineUI)
            {
                selectedLine = sender as SoundLineUI;
                selectedLine.Background = new SolidColorBrush(Color.FromRgb(230, 230, 255));
                if (selectedLine.Line is SoundLine)
                {
                    selectedLineProperties = new SoundLineProperties(selectedLine.Line as SoundLine);
                    selectedLinePropertiesWrapper.Children.Add(selectedLineProperties);
                }
            }
        }

        private void AddLinButton_Click(object sender, RoutedEventArgs e)
        {
            Project.current.lines.Add(new SoundLine());
        }

        public void Dispose()
        {
            refreshThread.Abort();
        }
    }
}
