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
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Mixer : Page
    {
        public Mixer()
        {
            InitializeComponent();
            showContent();
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
        }
    }
}
