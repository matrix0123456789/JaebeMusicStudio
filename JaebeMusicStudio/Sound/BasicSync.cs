using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Oscillator> oscillators = new List<Oscillator>();
        public SoundLine SoundLine { get; set; }

        public BasicSynth()
        {
            oscillators.Add(new Oscillator());
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
                if (ch.Name== "Oscillator")
                {
                    oscillators.Add(new Oscillator(ch));
                }
            }
        }
        public float[,] GetSound(float start, float length, NotesCollection notes)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var ret = new float[2, samples];//sound that will be returned

            var tasks = new Task<float[,]>[notes.Count, oscillators.Count];

            for (var i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                for (var j = 0; j < oscillators.Count; j++)
                {
                    if (note.Offset < start + length && note.Offset + note.Length + oscillators[j].R > start)
                    {
                        var j_copy = j;
                        tasks[i, j] = Task.Run(() =>
                        {
                            if (start > note.Offset)
                                return oscillators[j_copy].GetSound(start - note.Offset, length, note);
                            else
                                return oscillators[j_copy].GetSound(0, length + start - note.Offset, note);
                        });
                    }
                }
            }

            for (var i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                if (start > note.Offset)
                {
                    for (var j = 0; j < oscillators.Count; j++)
                    {
                        if (tasks[i, j] != null)
                        {
                            var retTask = tasks[i, j].Result;

                            for (long k = 0; k < retTask.LongLength / 2; k++)
                            {
                                ret[0, k] += retTask[0, k];
                                ret[1, k] += retTask[1, k];
                            }
                        }
                    }
                }
                else
                {
                    var notSamplesOffset = (long)Project.current.CountSamples(note.Offset - start);
                    for (var j = 0; j < oscillators.Count; j++)
                    {
                        if (tasks[i, j] != null)
                        {
                            var retTask = tasks[i, j].Result;

                            for (long k = 0; k < retTask.LongLength / 2; k++)
                            {
                                ret[0, k + notSamplesOffset] += retTask[0, k];
                                ret[1, k + notSamplesOffset] += retTask[1, k];
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
            node2.SetAttribute("name", name);
            node2.SetAttribute("soundLine", Project.current.lines.IndexOf(SoundLine).ToString());
            foreach (var osc in oscillators)
            {
                osc.Serialize(node2);
            }
            node.AppendChild(node2);
        }
    }
}
