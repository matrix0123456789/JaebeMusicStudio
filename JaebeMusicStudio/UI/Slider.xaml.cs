using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace JaebeMusicStudio.UI
{
    /// <summary>
    /// Interaction logic for scroll.xaml
    /// </summary>
    public partial class Slider : UserControl
    {
        [Bindable(true)]
        public event RoutedPropertyChangedEventHandler<double> ValueChanged;
        public Slider()
        {
            InitializeComponent();
        }

        [Bindable(true)]
        public double Value
        {
            get { return slider.Value; }
            set { slider.Value = value; }
        }
        [Bindable(true)]
        public double Minimum
        {
            get { return slider.Minimum; }
            set { slider.Minimum = value; }
        }
        [Bindable(true)]
        public double Maximum
        {
            get { return slider.Maximum; }
            set { slider.Maximum = value; }
        }

        private double step = 0;
        [Bindable(true)]
        public double Step
        {
            get { return step; }
            set
            {
                step = value;
                if (step > 0)
                {
                    Value = Minimum + Math.Round((Value - Minimum)/step)*step;
                }
            }
        }

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(step>0)
                Value = Minimum + Math.Round((e.NewValue - Minimum) / step) * step;
            ValueChanged?.Invoke(this, e);
        }
    }
}
