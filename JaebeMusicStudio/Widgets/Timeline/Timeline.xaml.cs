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
using JaebeMusicStudio.UI;

namespace JaebeMusicStudio.Widgets.Timeline
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
        double scaleY = 40;

        private ISoundElement editingElement;
        private FrameworkElement editingVisualElement;
        private Point editingStartposition;

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
            Dispatcher.InvokeAsync(() =>
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
            Dispatcher.Invoke(() =>
            {
                var grid = new Grid();
                grid.Background = Brushes.Aqua;
                grid.Height = scaleY;
                tracksStack.Children.Insert(index, grid);

                var content = new Grid();
                content.Tag = track;
                var line = new Rectangle();
                line.HorizontalAlignment = HorizontalAlignment.Stretch;
                line.VerticalAlignment = VerticalAlignment.Bottom;
                line.Height = 1;
                line.Fill = Brushes.Black;
                content.Children.Add(line);
                content.Height = scaleY;
                tracksContentStack.Children.Insert(index, content);
                foreach (var element in track.Elements)
                {
                    project_soundElementAdded(content, element);
                }
                track.SoundElementAdded += Track_SoundElementAdded;
                track.SoundElementRemoved += Track_SoundElementRemoved;
            });
        }

        private void Track_SoundElementRemoved(Track track, ISoundElement element)
        {

            Dispatcher.Invoke(() =>
            {
                foreach (var trackUI in tracksContentStack.Children)
                {
                    if ((trackUI as FrameworkElement)?.Tag == track)
                    {
                        foreach (var elem in (trackUI as Grid).Children)
                        {
                            if ((elem as Grid)?.Tag == element)
                            {
                                (trackUI as Grid).Children.Remove(elem as Grid);
                                break;
                            }
                        }
                        break;
                    }
                }
            });
        }

        private void Track_SoundElementAdded(Track arg1, ISoundElement arg2)
        {
            Dispatcher.Invoke(() =>
            {
                var index = Project.current.tracks.IndexOf(arg1);
                var trackContainer = (Grid)tracksContentStack.Children[index];
                project_soundElementAdded(trackContainer, arg2);
            });
        }

        void project_soundElementAdded(Grid trackContainer, Sound.ISoundElement element)
        {
            var grid = new Grid();
            var rect = new Rectangle();
            grid.Children.Add(rect);
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;
            if (element is Sound.OneSample)
            {
                rect.Fill = Brushes.Red;
                grid.Children.Add(new OneSampleLookInside(element as OneSample));
            }
            if (element is Sound.SoundElementClone clone)
            {
                rect.Fill = Brushes.Orange;
                if (clone.Oryginal is Sound.OneSample)
                {
                    grid.Children.Add(new OneSampleLookInside(clone.Oryginal as OneSample));
                }
                else if (clone.Oryginal is Sound.Notes)
                {
                    grid.Children.Add(new NotesLookInside(clone.Oryginal as Sound.Notes));

                }
            }
            else if (element is Sound.Notes)
            {
                rect.Fill = Brushes.Blue;

                grid.Children.Add(new NotesLookInside(element as Sound.Notes));
            }

            grid.ToolTip = element.Title;
            grid.Width = element.Length * scaleX;
            grid.MinWidth = 10;
            grid.Margin = new Thickness(element.Offset * scaleX, 0, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            foreach (var x in trackContainer.Children.Cast<FrameworkElement>().ToList().Where(x => x.Tag == element))
            {
                trackContainer.Children.Remove(x);
            }
            trackContainer.Children.Add(grid);
            grid.Tag = element;
            grid.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            element.positionChanged += Element_positionChanged;

            var menu = new ContextMenu();
            if (element is Notes)
            {
                var menuOpen = new MenuItem() { Header = "Otwórz" };
                menuOpen.Tag = new Object[] { element, trackContainer.Tag };
                menuOpen.Click += element_open_Click;
                menu.Items.Add(menuOpen);
            }

            var menuDuplicate = new MenuItem() { Header = "Duplikuj" };
            menuDuplicate.Tag = new Object[] { element, trackContainer.Tag };
            menuDuplicate.Click += element_duplicate_Click;
            menu.Items.Add(menuDuplicate);

            var menuClone = new MenuItem() { Header = "Klonuj" };
            menuClone.Tag = new Object[] { element, trackContainer.Tag };
            menuClone.Click += element_clone_Click;
            menu.Items.Add(menuClone);

            var menuDelete = new MenuItem() { Header = "Usun" };
            menuDelete.Tag = new Object[] { element, trackContainer.Tag };
            menuDelete.Click += element_delete_Click;
            menu.Items.Add(menuDelete);

            if (element is ISoundElementDirectOutput)
            {
                var menuOutput = new MenuItem() { Header = "Wyjście dźwięku" };
                menuOutput.Tag = new Object[] { element, trackContainer.Tag };
                menu.Items.Add(menuOutput);
                int i = 0;
                foreach (var line in Project.current.lines)
                {
                    var menuOutputLine = new MenuItem() { Header = "Linia " + (++i) };
                    menuOutputLine.Tag = new Object[] { element, line };
                    if (element.SoundLine == line)
                    {
                        menuOutputLine.IsChecked = true;
                    }
                    menuOutputLine.Click += element_setOutput_Click;
                    menuOutput.Items.Add(menuOutputLine);
                }
            }
            grid.ContextMenu = menu;

        }

        private void element_clone_Click(object sender, RoutedEventArgs e)
        {

            var data = (sender as FrameworkElement).Tag as Object[];
            var newElem = new SoundElementClone((data[0] as ISoundElement));
            Project.current.FindTrackWithSpace(newElem.Offset, newElem.Offset + newElem.Length).AddElement(newElem);
        }

        private void element_duplicate_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as FrameworkElement).Tag as Object[];
            var newElem = (data[0] as ISoundElement).Duplicate();
            Project.current.FindTrackWithSpace(newElem.Offset, newElem.Offset + newElem.Length).AddElement(newElem);
        }

        private void element_open_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((sender as FrameworkElement).Tag as Object[]);
            if (tag[0] is Notes)
                PseudoWindow.OpenWindow(() => new Widgets.NotesEdit(tag[0] as Notes));
        }

        private void element_setOutput_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((sender as FrameworkElement).Tag as Object[]);
            if (tag[0] is ISoundElementDirectOutput)
                (tag[0] as ISoundElementDirectOutput).SoundLine = tag[1] as SoundLine;
        }

        private void element_delete_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as FrameworkElement).Tag as Object[];
            (data[1] as Track).RemoveElement(data[0] as ISoundElement);
        }

        private void Element_positionChanged(ISoundElement obj)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var track in tracksContentStack.Children)
                {
                    if (track is Grid)
                    {
                        foreach (var elem in (track as Grid).Children)
                        {
                            if ((elem as Grid)?.Tag == obj)
                            {
                                (elem as Grid).Width = obj.Length * scaleX;
                                (elem as Grid).Margin = new Thickness(obj.Offset * scaleX, 0, 0, 0);
                            }
                        }
                    }
                }
            });
        }

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            editingStartposition = e.GetPosition(this);
            editingElement = (ISoundElement)(sender as Grid).Tag;
            editingVisualElement = (FrameworkElement)sender;
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
                scrollHorizontal.ScrollToHorizontalOffset((scrollHorizontal.HorizontalOffset + scrollHorizontal.ActualWidth / 2) * Math.Pow(2, e.Delta / 200f) - scrollHorizontal.ActualWidth / 2);
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

        float editCalcNewTime(MouseEventArgs e)
        {
            var timeDifference = (editingStartposition.X - e.GetPosition(this).X) / scaleX;
            var newTime = editingElement.Offset - timeDifference;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var scale = 1 / scaleX * 20;
                var scale1 = Math.Pow(10, Math.Ceiling(Math.Log10(scale)));
                if (scale1 / 5 > scale)
                    scale = scale1 / 5;
                else if (scale1 / 2 > scale)
                    scale = scale1 / 2;
                else
                    scale = scale1;
                newTime = Math.Round(newTime / scale) * scale;
            }
            return (float)newTime;
        }
        private int getTrackNumberByMouse(MouseEventArgs e)
        {
            var trackNumber = (int)Math.Round(e.GetPosition(this).Y / scaleY) - 2;
            if (trackNumber >= Project.current.tracks.Count)
                trackNumber = Project.current.tracks.Count - 1;
            if (trackNumber < 0)
                trackNumber = 0;
            return trackNumber;
        }
        private void Timeline_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && editingElement != null)
            {
                float newTime;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    newTime = editingElement.Offset;
                }
                else
                {
                    newTime = editCalcNewTime(e);
                }
                int trackNumber;
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    trackNumber = Project.current.tracks.FindIndex(track => track.Elements.Contains(editingElement));
                else
                    trackNumber = getTrackNumberByMouse(e);
                Panel newTrackVisual = tracksContentStack.Children[getTrackNumberByMouse(e)] as Panel;
                if (newTrackVisual != editingVisualElement.Parent)
                {
                    (editingVisualElement.Parent as Panel).Children.Remove(editingVisualElement);
                    newTrackVisual.Children.Add(editingVisualElement);
                }
                editingVisualElement.Margin = new Thickness(newTime * scaleX, 0, 0, 0);
            }

        }

        private void Timeline_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (editingElement != null)
                {
                    var newTime = editCalcNewTime(e);

                    if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        editingElement.Offset = newTime;

                    if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        var oldTrackNumber = Project.current.tracks.FindIndex(track => track.Elements.Contains(editingElement));
                        var trackNumber = getTrackNumberByMouse(e);
                        if (oldTrackNumber != trackNumber)
                        {
                            Project.current.tracks[oldTrackNumber].RemoveElement(editingElement);
                            Project.current.tracks[trackNumber].AddElement(editingElement);
                        }
                    }
                    editingVisualElement = null;
                    editingElement = null;
                }
            }
        }

        private void NewNotesButton_OnClick(object sender, RoutedEventArgs e)
        {
            var notes = new Notes();
            Project.current.FindTrackWithSpace(notes.Offset, notes.Offset + notes.Length).AddElement(notes);
        }

        private void TimeLabels_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var positionPixels = e.GetPosition(TimeLabels);
            double pixelOffset = -scrollHorizontal.HorizontalOffset + tracksStack.ActualWidth;
            var positionSound = (positionPixels.X - pixelOffset) / scaleX;
            Sound.Player.SetPosition((float)positionSound);
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
    }
}
