using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading;
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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Synths : Page
    {
        public Synths()
        {
            InitializeComponent();
            showContent();
            Project.current.NoteSynths.CollectionChanged += NoteSynths_CollectionChanged;
        }

        private void NoteSynths_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                project_synthAdded(e.NewStartingIndex, e.NewItems[0] as INoteSynth);

            }
            else
            {
                showContent();
            }
        }

        void showContent()
        {
            SynthList.Children.RemoveRange(0, SynthList.Children.Count - 2);
            int index = 0;
            foreach (var synth in Sound.Project.current.NoteSynths)
            {
                project_synthAdded(index, synth);
                index++;
            }
        }

        private void project_synthAdded(int index, INoteSynth synth)
        {
            var oneSynth = new OneSynth(synth);
            SynthList.Children.Insert(index, oneSynth);
        }

        private void AddStandard_Click(object sender, RoutedEventArgs e)
        {
            Project.current.NoteSynths.Add(new BasicSynth());
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SynthListScroll.ContextMenu.IsOpen = true;
        }

        private void AddVSTi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "pliki VSTi|*.dll";
            dialog.ShowDialog();

            if (dialog.FileName != "")
            {
                Project.current.NoteSynths.Add(new VSTi(dialog.FileName));
            }
        }

        private void contextMenuPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xml = Clipboard.GetText();
                var xmlDocument=new XmlDocument();
                xmlDocument.LoadXml(xml);
                if (xmlDocument.DocumentElement.Name == "fragment")
                {
                    var BasicSynths = xmlDocument.GetElementsByTagName("BasicSynth");
                    foreach (XmlNode x in BasicSynths)
                    {
                        Project.current.NoteSynths.Add(new BasicSynth(x));
                    }
                }
            }
            catch { }
        }

        private void AddBasicPercussion_Click(object sender, RoutedEventArgs e)
        {
                        Project.current.NoteSynths.Add(new BasicPercussion());
        }
    }
}
