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
    public partial class TimeLabels : UserControl
    {
        public TimeLabels()
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
                    var text = new TextBlock();
                    text.Margin = new Thickness(PixelOffset + scale * i * ScaleX - 100, 0, 0, 0);
                    text.HorizontalAlignment = HorizontalAlignment.Left;
                    text.TextAlignment = TextAlignment.Center;
                    text.Width = 200;
                    text.Text = (i * scale).ToString();
                grid.Children.Add(text);
               
            }
        }
    }
}
