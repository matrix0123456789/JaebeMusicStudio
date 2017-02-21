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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for PitchUI.xaml
    /// </summary>
    public partial class PitchUI : UserControl
    {
        public PitchUI()
        {
            InitializeComponent();
        }

        public event Action<PitchUI> ValueChanged; 
        public float Value
        {
            get { return (float)(Octave.Value * 12 + Note.Value + SubNote.Value); }
            set
            {
                Octave.Value = Math.Floor(value / 12);
                Note.Value = Math.Floor(value) % 12;
                SubNote.Value = value % 1;
            }
        }
        
        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ValueChanged?.Invoke(this);
        }
    }
}
