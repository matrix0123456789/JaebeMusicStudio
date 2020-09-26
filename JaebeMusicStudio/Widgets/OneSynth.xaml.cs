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
using System.Xml;
using JaebeMusicStudio.Sound;
using JaebeMusicStudio.UI;
using JaebeMusicStudio.addons;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for OneSynth.xaml
    /// </summary>
    public partial class OneSynth : UserControl
    {
        private INoteSynth synth;
        public OneSynth(INoteSynth synth)
        {
            this.synth=synth;
            InitializeComponent();
            slSelect.Selected = synth.SoundLine;
            slSelect.Generate();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (synth is BasicSynth)
            {
                PseudoWindow.OpenWindow(() => new Widgets.BasicSynthUi(synth as BasicSynth));
            }
            else if (synth is BasicPercussion)
            {
                PseudoWindow.OpenWindow(() => new Widgets.BasicPercussionUI(synth as BasicPercussion));
            }
            else if (synth is VSTi)
            {
                (synth as VSTi).ShowWindow();
            }
        }

        private void SlSelect_OnChanged(SoundLineSelect sender, SoundLineAbstract obj)
        {
            synth.SoundLine = obj as SoundLine;
        }

        private void Default_OnClick(object sender, RoutedEventArgs e)
        {
            KeyboardInput.singleton1.Synth = synth;
        }

        private void contextMenuCopy_Click(object sender, RoutedEventArgs e)
        {
            var xml = new XmlDocument();
            var xmlFragment = xml.CreateElement("fragment");
                synth.Serialize(xmlFragment);
            xml.AppendChild(xmlFragment);
            Clipboard.SetText(xml.AsString());
        }
    }
}
