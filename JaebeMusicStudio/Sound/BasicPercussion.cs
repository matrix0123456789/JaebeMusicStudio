using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class BasicPercussion : INoteSynth
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
        public SoundLine SoundLine { get; set; }
        private Dictionary<int, BasicPercussionElement> pitchesToElement = new Dictionary<int, BasicPercussionElement>();

        public BasicPercussion(XmlNode element)
        {
            Name = element.Attributes["name"].Value;
            foreach (XmlNode ch in element.ChildNodes)
            {
                if (ch.Name == "BasicPercussionElement")
                {
                    var BPElem = new BasicPercussionElement(ch);
                    var pitches = ch.Attributes["pitches"].Value.Split(',').Select(x => int.Parse(x.Trim()));
                    foreach (var x in pitches)
                    {
                        pitchesToElement.Add(x, BPElem);
                    }
                }
            }
        }
        public float[,] GetSound(float start, float length, NotesCollection notes)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var ret = new float[2, samples];//sound that will be returned
            var notesCount = notes.Count;
            var tasks = new Task<float[,]>[notesCount];

            for (var i = 0; i < notesCount; i++)
            {
                var note = notes[i].Clone();
                if (!pitchesToElement.ContainsKey((int)note.Pitch))
                    continue;
                var BPElement = pitchesToElement[(int)note.Pitch];
                if (note.Offset < start + length && note.Offset + BPElement.SoundLength > start)
                {
                    tasks[i] = Task.Run(() =>
                    {
                        if (start > note.Offset)
                        {
                            var l1 = BPElement.SoundLength - (start - note.Offset);
                            if (length < l1)
                            {
                                l1 = length;
                            }
                            return pitchesToElement[(int)note.Pitch]?.GetSound(start - note.Offset, l1, note);
                        }
                        else
                        {
                            var l1 = length + start - note.Offset;
                            if (note.Length < l1)
                                l1 = BPElement.SoundLength;
                            return pitchesToElement[(int)note.Pitch]?.GetSound(0, l1, note);
                        }
                    });

                }
            }

            for (var i = 0; i < notesCount; i++)
            {
                var note = notes[i];
                if (start > note.Offset)
                {

                    if (tasks[i] != null)
                    {
                        try
                        {
                            var retTask = tasks[i].Result;
                            if (retTask == null)
                                continue;

                            for (long k = 0; k < retTask.LongLength / 2; k++)
                            {
                                ret[0, k] += retTask[0, k];
                                ret[1, k] += retTask[1, k];
                            }
                        }
                        catch (Exception e) { Console.Write(e); }
                    }

                }
                else
                {
                    var notSamplesOffset = (long)Project.current.CountSamples(note.Offset - start);

                    if (tasks[i] != null)
                    {
                        var retTask = tasks[i].Result;
                        if (retTask == null)
                            continue;
                        var lengthToCopy = retTask.GetLongLength(1);
                        if (ret.GetLongLength(1) - notSamplesOffset < lengthToCopy)
                        {
                            lengthToCopy = ret.GetLongLength(1) - notSamplesOffset;
                        }
                        for (long k = 0; k < lengthToCopy; k++)
                        {
                            ret[0, k + notSamplesOffset] += retTask[0, k];
                            ret[1, k + notSamplesOffset] += retTask[1, k];
                        }

                    }
                }
            }
            return ret;
        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("BasicPercussion");
            node2.SetAttribute("name", Name);
            node2.SetAttribute("soundLine", Project.current.lines.IndexOf(SoundLine).ToString());
            foreach (var BPElem in pitchesToElement.Values)
            {
                BPElem.Serialize(node2, pitchesToElement.Where(pair => pair.Value == BPElem).Select(pair => pair.Key));
            }
            node.AppendChild(node2);
        }
    }
}
