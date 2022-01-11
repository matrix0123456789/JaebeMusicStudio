using System;
using System.Collections.Generic;
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

namespace JaebeMusicStudio.Widgets.Timeline
{
    /// <summary>
    /// Interaction logic for TimeLabels.xaml
    /// </summary>
    public partial class TimeLines : UserControl
    {
        public TimeLines()
        {
            InitializeComponent();
        }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double PixelOffset { get; set; }
        public void Draw()
        {

            grid.Children.Clear();
            var scale = 1 / ScaleX * 50;
            var scale1 = Math.Pow(10, Math.Ceiling(Math.Log10(scale)));
            if (scale1 / 5 > scale)
                scale = scale1 / 5;
            else if (scale1 / 2 > scale)
                scale = scale1 / 2;
            else
                scale = scale1;


            for (double i = 0; PixelOffset + scale * i * ScaleX < grid.ActualWidth; i++)
            {
                var lineLeft = PixelOffset + scale * i * ScaleX;
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
                    grid.Children.Add(line);
                }

            }

        }
    }
}

