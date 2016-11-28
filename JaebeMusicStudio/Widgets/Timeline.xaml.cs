﻿using System;
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
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : Page
    {
        /// <summary>
        /// how many pixels represents one note
        /// </summary>
        double scaleX = 10;
        public Timeline()
        {
            InitializeComponent();
            Sound.Project.current.trackAdded += project_trackAdded;
            Sound.Player.positionChanged += Player_positionChanged;
            showContent();
            scrollHorizontal.ScrollChanged += showTimeLabels;
        }

        private void Player_positionChanged(float obj = 0)
        {
            Dispatcher.Invoke(() =>
            {
                playingPosition.Margin = new Thickness(Sound.Player.position * scaleX, 0, 0, 0);
            });
        }

        void showContent()
        {
            showTimeLabels();
            tracksStack.Children.RemoveRange(0, tracksStack.Children.Count - 2);
            tracksContentStack.Children.Clear();
            var index = 0;
            foreach (var track in Sound.Project.current.tracks)
            {
                project_trackAdded(index, track);
                index++;
            }
            Player_positionChanged();
        }
        void showTimeLabels(object a = null, object b = null)
        {
            TimeLabels.Children.Clear();
            double pixelOffset = -scrollHorizontal.HorizontalOffset + tracksStack.ActualWidth;
            var scale = 1 / scaleX * 50;
            var scale1 = Math.Pow(10, Math.Ceiling(Math.Log10(scale)));
            if (scale1 / 5 > scale)
                scale = scale1 / 5;
            else if (scale1 / 2 > scale)
                scale = scale1 / 2;
            else
                scale = scale1;
            for (double i = 0; pixelOffset + scale * i * scaleX < TimeLabels.ActualWidth; i++)
            {
                var text = new TextBlock();
                text.Margin = new Thickness(pixelOffset + scale * i * scaleX - 100, 0, 0, 0);
                text.HorizontalAlignment = HorizontalAlignment.Left;
                text.TextAlignment = TextAlignment.Center;
                text.Width = 200;
                text.Text = (i * scale).ToString();
                TimeLabels.Children.Add(text);

            }
        }
        private void project_trackAdded(int index, Sound.Track track)
        {
            var grid = new Grid();
            grid.Background = Brushes.Aqua;
            grid.Height = 40;
            tracksStack.Children.Insert(index, grid);

            var content = new Grid();
            var line = new Rectangle();
            line.HorizontalAlignment = HorizontalAlignment.Stretch;
            line.VerticalAlignment = VerticalAlignment.Bottom;
            line.Height = 1;
            line.Fill = Brushes.Black;
            content.Children.Add(line);
            content.Height = 40;
            tracksContentStack.Children.Insert(index, content);
            foreach (var element in track.Elements)
            {
                project_soundElementAdded(content, element);
            }
            track.SoundElementAdded += Track_SoundElementAdded;
        }

        private void Track_SoundElementAdded(Track arg1, ISoundElement arg2)
        {
            var index = Project.current.tracks.IndexOf(arg1);
            var trackContainer = (Grid)tracksContentStack.Children[index];
            project_soundElementAdded(trackContainer, arg2);
        }

        void project_soundElementAdded(Grid trackContainer, Sound.ISoundElement element)
        {
            var grid = new Grid();
            var rect = new Rectangle();
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;
            if (element.GetType() == typeof(Sound.OneSample))
                rect.Fill = Brushes.Red;
            else
                rect.Fill = Brushes.Blue;
            grid.Children.Add(rect);
            grid.Width = element.Length * scaleX;
            grid.Margin = new Thickness(element.Offset * scaleX, 0, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            trackContainer.Children.Add(grid);

        }
        private void addNewTrackButton_Click(object sender, RoutedEventArgs e)
        {
            Sound.Project.current.AddEmptyTrack();
        }

        private void openFileSampleButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "pliki dźwiękowe|*.wav,*.wave,*.mp3|Wszystkie Pliki|*.*";
            dialog.ShowDialog();
            try
            {
                if (dialog.FileName != "")
                {

                    var ss = new Sound.OneSample(SampledSound.FindByUrl(dialog.FileName));
                    Project.current.FindTrackWithSpace(ss.Offset, ss.Offset + ss.Length).AddElement(ss);
                }
            }
            catch
            {
                MainWindow.error("Błąd otwarcia pliku");
            }
        }

        private void Page_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                scaleX *= Math.Pow(2, e.Delta / 200f);
                showContent();
            }
        }

        private void Timeline_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileName"))
            {
                var files = (string[])e.Data.GetData("FileNameW");
                foreach (var file in files)
                {
                    try
                    {
                        var ss = new Sound.OneSample(SampledSound.FindByUrl(file));
                        Project.current.FindTrackWithSpace(ss.Offset, ss.Offset + ss.Length).AddElement(ss);

                    }
                    catch
                    {
                        MainWindow.error("Błąd otwarcia pliku");
                    }
                }
            }
        }
    }
}
