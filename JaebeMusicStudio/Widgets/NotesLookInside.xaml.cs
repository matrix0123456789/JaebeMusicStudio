using JaebeMusicStudio.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SoundElementLookInside.xaml
    /// </summary>
    public partial class NotesLookInside : UserControl
    {
        private Notes notes;

        public NotesLookInside(Sound.Notes notes)
        {
            InitializeComponent();
            this.notes = notes;
            SizeChanged += NotesLookInside_SizeChanged;
        }

        private void NotesLookInside_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }
        void Draw()
        {
            grid.Children.Clear();
            if (!notes.Items.Any()) return;
            var pixels = (long)ActualWidth;
            if (pixels > 10000)
                return;//todo wymyśl coś na to zwieszanie
            var scaleX = pixels / notes.Length;
            var minPith = notes.Items.Min(x => x.Pitch);
            var maxPith = notes.Items.Max(x => x.Pitch);
            var scaleY = ((long)ActualHeight - 10) / (maxPith + 1 - minPith);
            foreach (var note in notes.Items)
            {
                var rect = new Rectangle();
                rect.Fill = Brushes.White;
                rect.Width = note.Length * scaleX;
                rect.Height = scaleY;
                rect.Margin = new Thickness(note.Offset * scaleX, (maxPith - note.Pitch) * scaleY + 5, 0, 0);
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                grid.Children.Add(rect);
            }
        }
    }
}