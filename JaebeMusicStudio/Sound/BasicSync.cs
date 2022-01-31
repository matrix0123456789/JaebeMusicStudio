﻿using System;
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
                    oscillators.Add(new Oscillator(ch));
                }
            }
            oscillators.CollectionChanged += Oscillators_CollectionChanged;
        }
        public SoundSample GetSound(float start, float length, Rendering rendering, NotesCollection notes)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var ret = new float[2, samples];//sound that will be returned
            var notesCount = notes.Count;
            var oscillatorsCount = oscillators.Count;

            for (var i = 0; i < notesCount; i++)
            {
                var note = notes[i].Clone();
                var notSamplesOffset = (long)Project.current.CountSamples(note.Offset - start);
                for (var j = 0; j < oscillatorsCount; j++)
                {
                    if (note.Offset < start + length && note.Offset + note.Length + oscillators[j].R > start)
                    {
                        var j_copy = j;
                        float[,] returnedSound;
                        if (start > note.Offset)
                        {
                            var l1 = note.Length + oscillators[j_copy].R - (start - note.Offset);
                            if (length < l1)
                            {
                                l1 = length;
                            }
                            returnedSound = oscillators[j_copy].GetSound(start - note.Offset, l1, note);
                            for (long k = 0; k < returnedSound.LongLength / 2; k++)
                            {
                                ret[0, k] += returnedSound[0, k];
                                ret[1, k] += returnedSound[1, k];
                            }
                        }
                        else
                        {
                            var l1 = length + start - note.Offset;
                            if (note.Length + oscillators[j_copy].R < l1)
                                l1 = note.Length + oscillators[j_copy].R;
                            returnedSound = oscillators[j_copy].GetSound(0, l1, note);
                            var minLength = returnedSound.LongLength / 2;
                            if (ret.LongLength / 2 < minLength)
                                minLength = ret.LongLength / 2;
                            long k;
                            for (k = 0; k < minLength; k++)
                            {
                                ret[0, k + notSamplesOffset] += returnedSound[0, k];
                                ret[1, k + notSamplesOffset] += returnedSound[1, k];
                            }
                        }
                    }
                }
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
