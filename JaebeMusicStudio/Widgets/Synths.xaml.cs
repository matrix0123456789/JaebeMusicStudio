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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Synths : Page
    {
        public Synths()
        {
            InitializeComponent();
            showContent();
        }

        void showContent()
        {
            SynthList.Children.RemoveRange(0, SynthList.Children.Count - 2);
            int index = 0;
            foreach (var synth in Sound.Project.current.NoteSynths)
            {
                project_synthAdded(index, synth);
                index++;
            }
        }

        private void project_synthAdded(int index, INoteSynth synth)
        {
            var grid = new Grid();
            var button = new Button();
            button.Content = "Domyślny";
            button.Tag = synth;
            button.VerticalAlignment=VerticalAlignment.Top;
            button.Height = 40;
            button.Click += Default_Click;
            grid.Children.Add(button);
            var button2 = new Button();
            button2.Content = "Otwórz";
            button2.Margin = new Thickness(0, 50, 0, 0);
            button2.Height = 40;
            button2.Tag = synth;
            button2.VerticalAlignment = VerticalAlignment.Top;
            button2.Click += Open_Click;

            grid.Children.Add(button2);


            var slSelect = new SoundLineSelect();
            slSelect.Margin = new Thickness(0, 10, 0, 0);
            slSelect.Tag = synth;
            slSelect.VerticalAlignment = VerticalAlignment.Top;
            slSelect.Height = 20;
            slSelect.Selected = synth.SoundLine;
            slSelect.Changed += SlSelect_Changed;


            grid.Children.Add(slSelect);
            SynthList.Children.Insert(index, grid);
        }

        private void SlSelect_Changed(SoundLineSelect sender, SoundLine obj)
        {
            (sender.Tag as INoteSynth).SoundLine = obj;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var synth = (sender as Button).Tag as INoteSynth;
            if (synth is BasicSynth)
            {
                PseudoWindow.OpenWindow(() => new Widgets.BasicSynthUi(synth as BasicSynth));
            }
        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            KeyboardInput.singleton.Synth = (sender as Button).Tag as INoteSynth;
        }

        private void AddStandard_Click(object sender, RoutedEventArgs e)
        {
           Project.current.NoteSynths.Add(new BasicSynth());
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SynthListScroll.ContextMenu.IsOpen = true;
        }
    }
}
