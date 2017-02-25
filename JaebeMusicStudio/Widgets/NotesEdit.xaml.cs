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

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for NotesEdit.xaml
    /// </summary>
    public partial class NotesEdit : Page
    {
        private Notes notes;
        public NotesEdit(Notes notes)
        {
            this.notes = notes;
            InitializeComponent();
            synthSelect.Selected = notes.Sound;
            synthSelect.Generate();
        }

        private void SynthSelect_OnChanged(SynthSelect arg1, INoteSynth sound)
        {
            notes.Sound = sound;
        }
    }
}
