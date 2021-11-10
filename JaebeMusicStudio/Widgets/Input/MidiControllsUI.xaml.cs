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

namespace JaebeMusicStudio.Widgets.Input
{
    /// <summary>
    /// Interaction logic for MidiControllsUI.xaml
    /// </summary>
    public partial class MidiControllsUI : UserControl
    {
        private ILiveInput input;

        public MidiControllsUI()
        {
            InitializeComponent();
        }
        internal void SetInput(ILiveInput input)
        {
            this.input = input;
            input.ControllsChanged += redraw;
            redraw();
        }

        private void redraw()
        {
            Dispatcher.InvokeAsync(() =>
            {
                panel.Children.Clear();
                lock (input) {
                    foreach (var (id, value) in input.Controlls.ToArray().OrderBy(x=>x.Key))
                    {
                        var row = new StackPanel();
                        row.Orientation = Orientation.Horizontal;
                        var idLabel = new Label();
                        idLabel.Content = id;
                        row.Children.Add(idLabel);
                        var valueSlider = new Slider();
                        valueSlider.Width = 200;
                        valueSlider.Maximum = 1;
                        valueSlider.IsEnabled = false;
                        valueSlider.Value = value / 127f;
                        row.Children.Add(valueSlider);
                        panel.Children.Add(row);
                    }
                }
            });
        }
    }
}
