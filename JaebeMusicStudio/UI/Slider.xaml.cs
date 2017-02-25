using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        [Bindable(true)]
        public string Unit { get; set; }
        public Slider()
        {
            InitializeComponent();
        }

        private double lastSetValue = Double.NaN;
        [Bindable(true)]
        public double Value
        {
            get { return stepCalc(slider.Value); }
            set
            {
                value = stepCalc(value);lastSetValue = value; slider.Value = value;
                slider.ToolTip = getToolTip();
            }
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
            slider.ToolTip = getToolTip();
            if (e.NewValue != lastSetValue)
            {
                if (step > 0)
                    Value = stepCalc(e.NewValue);
                ValueChanged?.Invoke(this, e);
                lastSetValue = double.NaN;
            }
        }

        double stepCalc(double input)
        {

            if (step > 0)
                return Minimum + Math.Round((input - Minimum) / step) * step;
            else
            
                return input;
            
        }

        string getToolTip()
        {
            string ret = Value.ToString();
            if (ToolTip?.ToString() != "")
                ret = ToolTip + ": " + ret;
            if (Unit != null)
                ret += " " + Unit;
            return ret;
        }
    }
}
