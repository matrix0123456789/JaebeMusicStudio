using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JaebeMusicStudio.Exceptions;

namespace JaebeMusicStudio.Sound
{
    public class BasicSynth : INoteSynth
    {
        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    Project.current.generateNamedElement(this);
                return name;
            }
            set
            {
                Project.current[value] = this;
                name = value;
            }
        }
        public ObservableCollection<Oscillator> oscillators = new ObservableCollection<Oscillator>();
        public SoundLine SoundLine { get; set; }

        public BasicSynth()
        {
            lock (oscillators)
                oscillators.Add(new Oscillator());
            oscillators.CollectionChanged += Oscillators_CollectionChanged;
            try
            {
                SoundLine = Project.current.lines[0];
            }
            catch
            {
                SoundLine = null;
            }
        }

        public event Action<int, Oscillator> oscillatorAdded;
        private void Oscillators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                oscillatorAdded?.Invoke(e.NewStartingIndex, e.NewItems[0] as Oscillator);
            }
        }

        public BasicSynth(XmlNode element)
        {
            Name = element.Attributes["name"].Value;
            if (element.Attributes["soundLine"] != null)
            {
                var number = uint.Parse(element.Attributes["soundLine"].Value);
                if (number >= Project.current.lines.Count)
                    throw new BadFileException();
                SoundLine = Project.current.lines[(int)number];
            }
            else
                SoundLine = Project.current.lines[0];
            foreach (XmlNode ch in element.ChildNodes)
            {
                if (ch.Name == "Oscillator")
                {
                    lock (oscillators)
                        oscillators.Add(new Oscillator(ch));
                }
            }
            oscillators.CollectionChanged += Oscillators_CollectionChanged;
        }
        public SoundSample GetSound(float start, float length, Rendering rendering, NotesCollection notes)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var ret = new SoundSample(samples);//sound that will be returned
            //we could make parralelism here, but need to consider if it will make better performance, becouse each sound element is already paraller 
            Oscillator[] oscs;
            lock (oscillators)
            {
                oscs = oscillators.ToArray();
            }
            var arr = oscs.SelectMany(o => notes.Where(note => note.Offset < start + length && note.Offset + note.Length + o.R > start).Select(note => (o, note.Clone()))).Select(x =>
            {
                var (oscillator, note) = x;
                var notSamplesOffset = (long)Project.current.CountSamples(note.Offset - start);

                float[,] returnedSound;
                if (start > note.Offset)
                {
                    var l1 = note.Length + oscillator.R - (start - note.Offset);
                    if (length < l1)
                    {
                        l1 = length;
                    }
                    returnedSound = oscillator.GetSound(start - note.Offset, l1, note, rendering);
                    return (returnedSound, 0);
                }
                else
                {
                    var l1 = length + start - note.Offset;
                    if (note.Length + oscillator.R < l1)
                        l1 = note.Length + oscillator.R;
                    returnedSound = oscillator.GetSound(0, l1, note, rendering);

                    return (returnedSound, notSamplesOffset);
                }
            }).ToArray();

            foreach (var x in arr)
            {
                ret.AddWithOffset(x.returnedSound, x.notSamplesOffset, 1);
            }
            return ret;
        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("BasicSynth");
            node2.SetAttribute("name", Name);
            node2.SetAttribute("soundLine", Project.current.lines.IndexOf(SoundLine).ToString());
            foreach (var osc in oscillators)
            {
                osc.Serialize(node2);
            }
            node.AppendChild(node2);
        }
    }
}
