using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class ButtonPretty : UserControl
    {
        public ButtonPretty()
        {
            InitializeComponent();
            button.Click += Click_proxy;
        }

        private void Click_proxy(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }

        [Bindable(true)]
        public Object Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value?.ToString();
                refresh();
            }
        }

        private void refresh()
        {
            button.Content = (icon > 0 ? icon + " " : "") + text??"";
        }

        [Bindable(true)]
        public string Icon
        {
            get
            {
                return ((int)icon).ToString("X");
            }
            set
            {
                icon = ((char)int.Parse(value, NumberStyles.AllowHexSpecifier)); ;
                refresh();
            }
        }
        private char icon;
        private string text;
        [Bindable(true)]
        public event RoutedEventHandler Click;
    }
}
