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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for NotesEdit.xaml
    /// </summary>
    public partial class NotesEdit : Page
    {
        private Notes notes;
        /// <summary>
        /// how many pixels represents one note
        /// </summary>
        double scaleX = 20;
        double scaleY = 20;
        int offsetY = 80;

        public NotesEdit(Notes notes)
        {
            this.notes = notes;
            InitializeComponent();
            synthSelect.Selected = notes.Sound;
            synthSelect.Generate();

            Sound.Player.positionChanged += Player_positionChanged;
            showContent();
            scrollHorizontal.ScrollChanged += showTimeLabels;
        }

        private void SynthSelect_OnChanged(SynthSelect arg1, INoteSynth sound)
        {
            notes.Sound = sound;
        }
        void showContent()
        {
            showTimeLabels();
            tracksGrid.Children.Clear();
            tracksContentStackGrid.Children.Clear();
            foreach (var note in notes.Items)
            {
                noteAdded(note);
            }
            Player_positionChanged();
            showNotePitches();
        }
        void showTimeLabels(object a = null, object b = null)
        {
            TimeLabels.Children.Clear();
            double pixelOffset = -scrollHorizontal.HorizontalOffset + tracksGrid.ActualWidth;
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
        void showNotePitches(object a = null, object b = null)
        {
            tracksGrid.Children.Clear();

            for (int i = 0; i < offsetY; i++)
            {
                var text = new TextBlock();
                text.Margin = new Thickness(0, (offsetY - i) * scaleY, 0, 0);
                text.HorizontalAlignment = HorizontalAlignment.Left;
                text.VerticalAlignment = VerticalAlignment.Top;
                text.TextAlignment = TextAlignment.Left;
                text.Text = Note.GetName(i);
                tracksGrid.Children.Add(text);

            }
        }
        private void Player_positionChanged(float obj = 0)
        {
            Dispatcher.Invoke(() =>
            {
                playingPosition.Margin = new Thickness(Sound.Player.position * scaleX, 0, 0, 0);
            });
        }
        private void Page_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    scaleY *= Math.Pow(2, e.Delta / 200f);
                    WholeScrollable.ScrollToVerticalOffset((WholeScrollable.VerticalOffset) * Math.Pow(2, e.Delta / 200f));
                }
                else
                {
                    scaleX *= Math.Pow(2, e.Delta / 200f);
                    scrollHorizontal.ScrollToHorizontalOffset((scrollHorizontal.HorizontalOffset + scrollHorizontal.ActualWidth / 2) * Math.Pow(2, e.Delta / 200f) - scrollHorizontal.ActualWidth / 2);
                }
                showContent();
            }
        }


        private void Track_SoundElementRemoved(Track track, ISoundElement element)
        {

            Dispatcher.Invoke(() =>
            {
                foreach (var trackUI in tracksContentStackGrid.Children)
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



        void noteAdded(Note element)
        {
            var grid = new Grid();
            var rect = new Rectangle();
            grid.Children.Add(rect);
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;

            rect.Fill = Brushes.Green;

            grid.Width = element.Length * scaleX;
            grid.Height = scaleY;
            grid.Margin = new Thickness(element.Offset * scaleX, (offsetY - element.Pitch) * scaleY, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            tracksContentStackGrid.Children.Add(grid);
            grid.Tag = element;
            //grid.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            //element.positionChanged += Element_positionChanged;

            var menu = new ContextMenu();
            //if (element is Notes)
            //{
            //    var menuOpen = new MenuItem() { Header = "Otwórz" };
            //    menuOpen.Tag = new Object[] { element, trackContainer.Tag };
            //    menuOpen.Click += element_open_Click;
            //    menu.Items.Add(menuOpen);
            //}

            //var menuDuplicate = new MenuItem() { Header = "Duplikuj" };
            //menuDuplicate.Tag = new Object[] { element, trackContainer.Tag };
            //menuDuplicate.Click += element_duplicate_Click;
            //menu.Items.Add(menuDuplicate);

            //var menuClone = new MenuItem() { Header = "Klonuj" };
            //menuClone.Tag = new Object[] { element, trackContainer.Tag };
            //menuClone.Click += element_clone_Click;
            //menu.Items.Add(menuClone);

            //var menuDelete = new MenuItem() { Header = "Usun" };
            //menuDelete.Tag = new Object[] { element, trackContainer.Tag };
            //menuDelete.Click += element_delete_Click;
            //menu.Items.Add(menuDelete);


            grid.ContextMenu = menu;

        }

    }
}
