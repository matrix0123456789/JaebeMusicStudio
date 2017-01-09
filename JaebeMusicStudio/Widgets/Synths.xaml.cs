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
            var grid=new Grid();
            var button=new Button();
            button.Content = "Domyślny";
            button.Tag = synth;
            button.Click += Default_Click;
            grid.Children.Add(button);
            SynthList.Children.Insert(index,grid);
        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            KeyboardInput.singleton.Synth = (sender as Button).Tag as INoteSynth;
        }

        private void AddStandard_Click(object sender, RoutedEventArgs e)
        {
            SynthListScroll.ContextMenu.IsOpen = true;
        }
    }
}
