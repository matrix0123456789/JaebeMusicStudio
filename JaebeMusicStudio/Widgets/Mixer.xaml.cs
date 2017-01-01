using System;
using System.Collections.Generic;
using System.Linq;
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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Mixer : Page
    {
        SoundLineUI selectedLine = null;
        public Mixer()
        {
            InitializeComponent();
            showContent();
            Thread t2 = new Thread(delegate ()
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
            t2.Start();
        }

        void showContent()
        {
            SoundLinesList.Children.RemoveRange(0, SoundLinesList.Children.Count - 2);
            int index = 0;
            foreach (var line in Sound.Project.current.lines)
            {
                project_lineAdded(index, line);
                index++;
            }
        }

        private void project_lineAdded(int index, SoundLine line)
        {
            var lineUI = new SoundLineUI(line);
            SoundLinesList.Children.Insert(index, lineUI);
            lineUI.MouseDown += LineUI_MouseDown;
        }

        private void LineUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedLine != null)
                selectedLine.Background = Brushes.White;
            if (sender is SoundLineUI)
                selectedLine = sender as SoundLineUI;
            selectedLine.Background = new SolidColorBrush(Color.FromRgb(230, 230, 255));
            ShowEffect();
        }

        void ShowEffect()
        {
            EffectsList.Children.Clear();
            foreach (var effect in selectedLine.Line.effects)
            {
                var grid = new Grid();
                var label = new Label() { Content = effect.ToString() };
                grid.Children.Add(label);
                EffectsList.Children.Add(grid);
            }
        }
    }
}
