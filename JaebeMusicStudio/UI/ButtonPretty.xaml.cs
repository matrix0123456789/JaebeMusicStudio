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
                return button.Content;
            }
            set
            {
                button.Content = value;
            }
        }
        [Bindable(true)]
        public string Icon
        {
            get
            {
                return ((int)(button.Content as string)[0]).ToString("X");
            }
            set
            {
                button.Content = ((char)int.Parse(value, NumberStyles.AllowHexSpecifier));
               var a= button.FontFamily.Source;
            }
        }
        [Bindable(true)]
        public event RoutedEventHandler Click;
    }
}
