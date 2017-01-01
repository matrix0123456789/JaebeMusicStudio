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
using JaebeMusicStudio.UI;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for EffectMini.xaml
    /// </summary>
    public partial class EffectMini : UserControl
    {
        public Effect effect;
        public EffectMini(Effect effect)
        {
            this.effect = effect;
            InitializeComponent();
            Title.Content = effect.ToString();
        }


        private void ButtonPretty_OnClick(object sender, RoutedEventArgs e)
        {
            if (effect is NonlinearDistortion)
            {
                PseudoWindow.OpenWindow(() => new NonlinearDistortionUI(effect as NonlinearDistortion));
            }
            else if (effect is Reverb)
            {
                PseudoWindow.OpenWindow(() => new ReverbUI(effect as Reverb));
            }
        }
    }
}
