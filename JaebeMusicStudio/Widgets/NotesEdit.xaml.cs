using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private List<Note> editingElements = new List<Note>();
        private List<FrameworkElement> editingVisualElements = new List<FrameworkElement>();
        private Point editingStartposition;
        private DateTime editingStarTime;
        private EditingTypes editingType;
        List<Line> timeLines = new List<Line>();
        enum EditingTypes { None, Move, Rezise }
        private float selectStart = 0;
        private float selectEnd = 0;
        public NotesEdit(Notes notes)
        {
            this.notes = notes;
            InitializeComponent();
            synthSelect.Selected = notes.Sound;
            synthSelect.Generate();

            Sound.Player.positionChanged += Player_positionChanged;
            showContent();
            scrollHorizontal.ScrollChanged += showTimeLabels;
            notes.Items.CollectionChanged += NotesChanged;
        }
        Dictionary<Notes, Grid> generatedNotes = new Dictionary<Notes, Grid>();

        private void NotesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var note in e.NewItems)
                        {
                            noteAdded(note as Note);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var note in e.OldItems)
                        {
                            noteRemoved(note as Note);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (var note in e.NewItems)
                        {
                            noteAdded(note as Note);
                        }
                        foreach (var note in e.OldItems)
                        {
                            noteRemoved(note as Note);
                        }
                        break;
                }
            });
        }


        private void SynthSelect_OnChanged(SynthSelect arg1, INoteSynth sound)
        {
            notes.Sound = sound;
        }
        void showContent()
        {
            showTimeLabels();
            tracksGrid.Children.Clear();
            var tracksContentStackGridArray = new UIElement[tracksContentStackGrid.Children.Count];
            for (var i = 0; i < tracksContentStackGridArray.Length; i++)
            {
                tracksContentStackGridArray[i] = tracksContentStackGrid.Children[i];
            }
            foreach (var x in tracksContentStackGridArray)
            {
                if (x is Grid)
                    tracksContentStackGrid.Children.Remove(x as Grid);
            }
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
            timeLines.ForEach(x => WholeScrollableGrid.Children.Remove(x));

            double pixelOffset = -scrollHorizontal.HorizontalOffset + scrollHorizontal.Margin.Left;
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

                var lineLeft = pixelOffset + scale * i * scaleX;
                if (lineLeft > 100)
                {
                    var line = new Line();
                    line.VerticalAlignment = VerticalAlignment.Stretch;
                    line.HorizontalAlignment = HorizontalAlignment.Left;
                    line.X1 = 0;
                    line.Y1 = 0;
                    line.X2 = 0;
                    line.Y2 = 20;
                    line.Margin = new Thickness(lineLeft, 0, 0, 0);
                    line.Stretch = Stretch.Fill;
                    line.Stroke = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    line.StrokeThickness = 1;
                    timeLines.Add(line);
                    WholeScrollableGrid.Children.Add(line);
                }

            }
        }
        void showNotePitches(object a = null, object b = null)
        {
            tracksGrid.Children.Clear();

            for (int i = 0; i < offsetY; i++)
            {
                var text = new TextBlock();
                text.Margin = new Thickness(0, (offsetY - i) * scaleY, 0, 0);
                text.HorizontalAlignment = HorizontalAlignment.Stretch;
                text.VerticalAlignment = VerticalAlignment.Top;
                text.TextAlignment = TextAlignment.Left;
                text.Height = scaleY;
                text.Text = Note.GetName(i);
                text.Tag = i;
                if (Note.IsPitchBlack(i))
                {
                    text.Background = new SolidColorBrush(Color.FromArgb(56, 0, 0, 0));
                }
                else
                {
                    text.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                }
                tracksGrid.Children.Add(text);
                text.MouseDown += pitchMouseDown; text.MouseUp += pitchMouseUp;

            }
        }

        private void pitchMouseDown(object sender, MouseButtonEventArgs e)
        {
            var block = (TextBlock)sender;
            var pitch = (int)block.Tag;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (lastClick != null && (DateTime.Now - lastClickTime).TotalSeconds < 1 && (e.GetPosition(this) - lastClick.Value).Length < 10)
                {
                    var newNote = new Note();
                    newNote.Offset = selectStart;
                    newNote.Pitch = pitch;
                    newNote.Length = 1;
                    notes.Items.Add(newNote);
                    lastClick = null;
                }
                else
                {
                    lastClick = e.GetPosition(this);
                    lastClickTime = DateTime.Now;

                }
                e.Handled = true;
            }
        }
        private void pitchMouseUp(object sender, MouseButtonEventArgs e)
        {

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
            Dispatcher.Invoke(() =>
            {
                var grid = new Grid();
                var rect = new Rectangle();
                grid.Children.Add(rect);
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 1;
                rect.HorizontalAlignment = HorizontalAlignment.Left;

                rect.Fill = Brushes.Green;

                setNoteUiParams(grid, element);

                grid.Tag = element;
                grid.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                grid.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                element.Changed += Note_Changed;

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

                var menuDelete = new MenuItem() { Header = "Usun" };
                menuDelete.Tag = element;
                menuDelete.Click += element_delete_Click;
                menu.Items.Add(menuDelete);


                grid.ContextMenu = menu;

                tracksContentStackGrid.Children.Add(grid);


                var resizer = new Grid();
                resizer.Cursor = Cursors.SizeWE;
                resizer.Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
                resizer.Width = 10;
                resizer.VerticalAlignment = VerticalAlignment.Stretch;
                resizer.HorizontalAlignment = HorizontalAlignment.Right;
                resizer.MouseLeftButtonDown += ElementResizer_MouseLeftButtonDown;
                grid.Children.Add(resizer);
            });
        }

        private void Note_Changed(Note note)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var noteElement = findNoteUi(note);
                if (noteElement != null)
                    setNoteUiParams(noteElement as Grid, note);
            }));
        }
        void setNoteUiParams(Grid grid, Note note)
        {
            var rect = grid.Children[0] as Rectangle;
            grid.Width = note.Length * scaleX;
            if (grid.Width < 15)//wąska nuta, zeby dało się myszą chwycić
                grid.Width += 10;
            rect.Width = note.Length * scaleX;
            grid.Height = scaleY;
            grid.Margin = new Thickness(note.Offset * scaleX, (offsetY - note.Pitch) * scaleY, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
        }
        private void element_delete_Click(object sender, RoutedEventArgs e)
        {
            notes.Items.Remove((sender as FrameworkElement).Tag as Note);
        }

        private void noteRemoved(Note note)
        {
            var noteElement = findNoteUi(note);
            if (noteElement != null)
                tracksContentStackGrid.Children.Remove(noteElement);
        }
        FrameworkElement findNoteUi(Note note)
        {
            foreach (var grid in tracksContentStackGrid.Children)
            {
                if ((grid as FrameworkElement)?.Tag == note)
                    return grid as FrameworkElement;
            }
            return null;
        }
        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            editingStartposition = e.GetPosition(this);
            editingStarTime = DateTime.Now;
            var visualElement = (Grid)sender;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (editingVisualElements.Contains(visualElement))
                    deselectNote(visualElement);
                else
                    selectNote(visualElement);
            }
            else
            {
                clearAllNotesSelection();
                selectNote(visualElement);
            }

            editingType = EditingTypes.Move;
        }
        void clearAllNotesSelection()
        {
            foreach (Grid visualElement in editingVisualElements)
            {
                (visualElement.Children[0] as Rectangle).Stroke = Brushes.Black;
            }
            editingElements.Clear();
            editingVisualElements.Clear();
        }
        void selectNote(Grid visualElement)
        {
            var note = (Note)visualElement.Tag;
            editingElements.Add(note);
            editingVisualElements.Add(visualElement);
            (visualElement.Children[0] as Rectangle).Stroke = Brushes.Yellow;
        }
        void deselectNote(Grid visualElement)
        {
            var note = (Note)visualElement.Tag;
            editingElements.Remove(note);
            editingVisualElements.Remove(visualElement);
            (visualElement.Children[0] as Rectangle).Stroke = Brushes.Black;
        }
        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void ElementResizer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            editingStartposition = e.GetPosition(this);
            var resizer = (sender as Grid);
            var parent = resizer.Parent as Grid;
            var note = (Note)(parent).Tag;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                editingElements.Add(note);
                editingVisualElements.Add(parent);
            }
            else
            {
                editingElements = new List<Note> { note };
                editingVisualElements = new List<FrameworkElement> { parent };
            }

            editingType = EditingTypes.Rezise;
            e.Handled = true;
        }


        float editCalcNewTime(MouseEventArgs e)
        {
            return -(float)((editingStartposition.X - e.GetPosition(this).X) / scaleX);
        }
        float roundTime(float input)
        {
            var scale = 1 / scaleX * 20;
            var scaleRounded = Math.Pow(2, Math.Ceiling(Math.Log(scale) / Math.Log(2)));

            return (float)(Math.Round(input / scaleRounded) * scaleRounded);

        }

        int editCalcNewPitch(MouseEventArgs e)
        {
            var deltaPitch = (editingStartposition.Y - e.GetPosition(this).Y) / scaleY;
            return (int)Math.Round(deltaPitch);
        }
        float AddCalcNewTime(MouseEventArgs e)
        {
            var timeDifference = e.GetPosition(tracksContentStackGrid).X / scaleX;
            var newTime = timeDifference;
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
        int AddCalcNewPitch(MouseEventArgs e)
        {
            var timeDifference = e.GetPosition(tracksContentStackGrid).Y / scaleY;
            var newPitch = offsetY - timeDifference;
            return (int)newPitch;
        }


        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && editingType != EditingTypes.None)
            {
                moveByMouseMovement(e, false);

            }
        }
        void moveByMouseMovement(MouseEventArgs e, bool save)
        {
            var deltaTime = editCalcNewTime(e);
            var deltaPitch = editCalcNewPitch(e);
            if (editingElements.Count != 1)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    deltaTime = roundTime(deltaTime);
                }
            }
            for (var i = 0; i < editingElements.Count; i++)
            {
                var element = editingElements[i];
                var visualElement = editingVisualElements[i];

                var offset = element.Offset;
                var length = element.Length;
                var pitch = element.Pitch;


                if (editingType == EditingTypes.Move)
                {
                    offset += deltaTime;
                    if (editingElements.Count == 1)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            offset = roundTime(offset);
                        }
                    }
                    pitch += deltaPitch;
                }
                else if (editingType == EditingTypes.Rezise)
                {
                    length += deltaTime;
                    if (editingElements.Count == 1)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            length = roundTime(length);
                        }
                    }
                }

                if (length < 0)
                    length = 0;
                if (offset < 0)
                    offset = 0;

                visualElement.Margin = new Thickness(offset * scaleX, (offsetY - pitch) * scaleY, 0, 0);
                visualElement.Width = length * scaleX + 1;
                if (save)
                {
                    element.Offset = offset;
                    element.Length = length;
                    element.Pitch = pitch;
                }
            }



        }
        private void Page_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (editingType != EditingTypes.None)
                {
                    moveByMouseMovement(e, true);
                    editingType = EditingTypes.None;
                }
            }
        }
        static Point? lastClick;
        static DateTime lastClickTime;
        private void tracksContentStackGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (lastClick != null && (DateTime.Now - lastClickTime).TotalSeconds < 1 && (e.GetPosition(this) - lastClick.Value).Length < 10)
                {
                    var newNote = new Note();
                    newNote.Offset = AddCalcNewTime(e);
                    newNote.Pitch = AddCalcNewPitch(e);
                    newNote.Length = 1;
                    notes.Items.Add(newNote);
                    lastClick = null;
                }
                else
                {
                    lastClick = e.GetPosition(this);
                    lastClickTime = DateTime.Now;
                    selectEnd = selectStart = AddCalcNewTime(e);
                    updateSelect();
                }
            }
        }

        private void updateSelect()
        {
            if (selectStart > selectEnd)
            {
                float tmp = selectStart;
                selectStart = selectEnd;
                selectEnd = tmp;
            }
            selection.Margin = new Thickness(selectStart * scaleX - 1, 0, 0, 0);
            selection.Width = (selectEnd - selectStart) * scaleX + 2;
        }
    }
}
