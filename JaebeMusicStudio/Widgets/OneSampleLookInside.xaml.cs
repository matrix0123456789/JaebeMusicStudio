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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JaebeMusicStudio.Sound;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for OneSampleLookInside.xaml
    /// </summary>
    public partial class OneSampleLookInside : UserControl, ILookInside
    {
        public OneSampleLookInside(OneSample element)
        {
            InitializeComponent();
            this.element = element;
            SizeChanged += OneSampleLookInside_SizeChanged;
        }

        private void OneSampleLookInside_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private OneSample element;
        public ISoundElement Element { get { return element; } }

        void Draw()
        {
            grid.Children.Clear();
            var pixels = (long)ActualWidth;
            if (pixels > 10000)
                return;//todo wymyśl coś na to zwieszanie
            var samples = (long)(element.Length / Project.current.tempo * 60f * element.sample.sampleRate);//how many samples you need on output
            var levelOfDetails = (int)Math.Log(samples / ActualWidth, 64);
            if (levelOfDetails < 1)
                levelOfDetails = 1;
            long offset = (long)Project.current.CountSamples(element.Offset);

            var simpledSound = element.sample.simpled(levelOfDetails);
            double ratio = samples / (double)pixels / Math.Pow(64, levelOfDetails);
            long ssPosition = 0;
            long newSsPosition = 0;
            var line = new Polygon();
            line.Fill = Brushes.Black;
            var moveY = ActualHeight / 2;
            var maximum = pixels;
            if (maximum > simpledSound.GetLength(1) / ratio)
                maximum = (long)(simpledSound.GetLength(1) / ratio);
            for (long i = 0; i < maximum; i++)
            {
                newSsPosition = (long)(i*ratio);
                var max = simpledSound[0, ssPosition];
                while (ssPosition < newSsPosition)
                {
                    ssPosition++;
                    if (simpledSound[0, ssPosition] > max)
                        max = simpledSound[0, ssPosition];
                }
                line.Points.Add(new Point(i, ActualHeight * max / 2 + moveY));
            }
            moveY--;
            ssPosition = (long)(maximum*ratio);
            for (long i = maximum-1; i >= 0; i--)
            {
                newSsPosition = (long)(i * ratio);
                var min = simpledSound[1, ssPosition];
                while (ssPosition > newSsPosition)
                {
                    ssPosition--;
                    if (simpledSound[1, ssPosition] < min)
                        min = simpledSound[1, ssPosition];
                }
                line.Points.Add(new Point(i, ActualHeight * min / 2 + moveY));
            }
            grid.Children.Add(line);

        }
    }
}
