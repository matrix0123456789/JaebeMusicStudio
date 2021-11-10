using JaebeMusicStudio.Sound;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Linq;

namespace JaebeMusicStudio.Widgets.Input
{
    /// <summary>
    /// Interaction logic for CurrentNotesUI.xaml
    /// </summary>
    public partial class CurrentNotesUI : UserControl
    {
        private ILiveInput input;

        public CurrentNotesUI()
        {
            InitializeComponent();
        }
        internal void SetInput(ILiveInput input)
        {
            this.input = input;
            input.PressedNotesChanged += redraw;
            redraw();
        }

        private void redraw()
        {
            Dispatcher.InvokeAsync(() =>
            {
                panel.Children.Clear();
                foreach (var note in input.PressedNotes)
                {
                    var row = new StackPanel();
                    row.Orientation = Orientation.Horizontal;
                    var pitch = new Label();
                    pitch.Content = note.Pitch;
                    row.Children.Add(pitch);
                    var volume = new Label();
                    volume.Content = note.Volume;
                    row.Children.Add(volume);
                    panel.Children.Add(row);
                }
            });
        }
    }
}
