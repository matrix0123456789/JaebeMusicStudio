using JaebeMusicStudio.Sound;
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
    /// Logika interakcji dla klasy BasicPercussionUI.xaml
    /// </summary>
    public partial class BasicPercussionUI : UserControl
    {
        private BasicPercussion basic;
        public BasicPercussionUI(BasicPercussion basic)
        {
            this.basic = basic;
            InitializeComponent();
            showContent();
            basic.elementAdded += basicSynth_elementAdded;
        }
        void showContent()
        {
            ElementsList.Children.RemoveRange(0, ElementsList.Children.Count - 2);
            int index = 0;
            foreach (var elem in basic.elements)
            {
                basicSynth_elementAdded(index, elem);
                index++;
            }
        }

        private void basicSynth_elementAdded(int index, BasicPercussionElement element)
        {
            var elementUI = new BasicPercussionElementUI(element);
            ElementsList.Children.Insert(index, elementUI);
        }

        private void AddOsc_OnClick(object sender, RoutedEventArgs e)
        {
            basic.elements.Add(new BasicPercussionElement());
        }
    }
}
