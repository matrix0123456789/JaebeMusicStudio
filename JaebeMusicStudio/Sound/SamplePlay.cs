using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class SamplePlay : INoteSynth
    {
        private List<SamplePlayItem> items = new List<SamplePlayItem>();
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

        public SamplePlay(XmlNode element)
        {
            foreach (XmlNode item in element.ChildNodes)
            {
                if (item.Name == "SamplePlayItem")
                {
                    items.Add(new SamplePlayItem(item));
                }
            }
        }
        SamplePlayItem findItem(Note note)
        {
            SamplePlayItem finded = items[0];
            foreach (var item in items)
            {
                if (Math.Abs(note.Pitch - item.Pitch) < Math.Abs(note.Pitch - finded.Pitch))
                {
                    finded = item;
                }
            }
            return finded;
        }
        public SoundSample GetSound(float start, float length, Rendering rendering, NotesCollection notes)
        {
            long samples = (long)rendering.CountSamples(length);//how many samples you need on output
            var ret = new float[2, samples];//sound that will be returned
            var notesCount = notes.Count;
            var tasks = new Task<float[,]>[notesCount];

            for (var i = 0; i < notesCount; i++)
            {
                var note = notes[i].Clone();
                var item = findItem(note);
                var currentNoteLength = item.CalcLengthByNote(note);
                if (note.Offset < start + length && note.Offset + currentNoteLength > start)
                {
                    tasks[i] = Task.Run(() =>
                    {
                        try
                        {
                            if (start > note.Offset)
                            {
                                var l1 = currentNoteLength - (start - note.Offset);
                                if (length < l1)
                                {
                                    l1 = length;
                                }
                                return item.GetSound(start - note.Offset, l1, note, rendering);
                            }
                            else
                            {
                                var l1 = length + start - note.Offset;
                                if (note.Length + item.R < l1)
                                    l1 = currentNoteLength;
                                return item.GetSound(0, l1, note, rendering);
                            }
                        }
                        catch
                        {
                            return null;
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
                    var notSamplesOffset = (long)rendering.CountSamples(note.Offset - start);

                    if (tasks[i] != null)
                    {
                        var retTask = tasks[i].Result;
                        var minLength = retTask.LongLength / 2;
                        if (ret.LongLength / 2 < minLength)
                            minLength = ret.LongLength / 2;
                        long k;
                        for (k = 0; k < minLength; k++)
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
            var node2 = node.OwnerDocument.CreateElement("SamplePlay");
            node2.SetAttribute("name", Name);
            node2.SetAttribute("soundLine", Project.current.lines.IndexOf(SoundLine).ToString());
            foreach (var item in items)
            {
                item.Serialize(node2);
            }
            node.AppendChild(node2);
        }
    }
    class SamplePlayItem
    {
        public SampledSound sample;
        public bool PlayToEnd { get; set; }
        public float R { get; set; } = 0;
        public float Pitch { get; set; }
        public float innerOffset { get; set; } = 0;
        public SamplePlayItem(XmlNode element)
        {
            Pitch = float.Parse(element.Attributes["pitch"].Value, CultureInfo.InvariantCulture);
            if (element.Attributes["playToEnd"] != null)
                PlayToEnd = bool.Parse(element.Attributes["playToEnd"].Value);
            if (element.Attributes["r"] != null)
                R = float.Parse(element.Attributes["r"].Value, CultureInfo.InvariantCulture);
            sample = SampledSound.FindByUrl(element.Attributes["src"].Value);
        }
        public float[,] GetSound(float start, float length, Note note, Rendering rendering)
        {
            long samples = (long)rendering.CountSamples(length);//how many samples you need on output
            var ret = new float[sample.channels, samples];//sound that will be returned
            var pitchRatio = Math.Pow(2, (note.Pitch - Pitch) / 12);
            var startOffset = ((innerOffset + start * pitchRatio) / Project.current.tempo * 60f) * sample.sampleRate;//start of reading in sample
            var filesRatio = sample.sampleRate / rendering.sampleRate;
            var samplesRatio = filesRatio * pitchRatio;

            if ((int)(startOffset + (float)samples * samplesRatio) >= sample.wave.GetLength(1))//end of sample
                samples = (long)((sample.wave.GetLength(1) - 1 - startOffset) / samplesRatio);

            if (sample.channels == 1)
                for (var i = 0; i < samples; i++)
                {
                    var offset = (startOffset + (float)i * samplesRatio);
                    var offsetInt = (int)offset;
                    var lerp = (offset - offsetInt);
                    if (offsetInt + 1 < sample.wave.GetLength(1))
                        ret[0, i] = (float)((sample.wave[0, offsetInt] * (1 - lerp) + sample.wave[0, offsetInt + 1] * lerp) * note.Volume);
                    else
                        ret[0, i] = (float)(sample.wave[0, offsetInt] * note.Volume);
                }
            else
                for (var i = 0; i < samples; i++)
                {
                    var offset = (startOffset + (float)i * samplesRatio);
                    var offsetInt = (int)offset;
                    var lerp = (offset - offsetInt);
                    if (offsetInt + 1 < sample.wave.GetLength(1))
                    {
                        ret[0, i] = (float)((sample.wave[0, offsetInt] * (1 - lerp) + sample.wave[0, offsetInt + 1] * lerp) * note.Volume);
                        ret[1, i] = (float)((sample.wave[1, offsetInt] * (1 - lerp) + sample.wave[1, offsetInt + 1] * lerp) * note.Volume);
                    }
                    else
                    {
                        ret[0, i] = (float)(sample.wave[0, offsetInt] * note.Volume);
                        ret[1, i] = (float)(sample.wave[1, offsetInt] * note.Volume);
                    }

                }
            return ret;
        }
        public float CalcLengthByNote(Note note)
        {
            var pitchRatio = Math.Pow(2, (note.Pitch - Pitch) / 12);
            var length = sample.Length / pitchRatio;
            if (!PlayToEnd)
                length = Math.Min(length, note.Length + R);
            return (float)length;
        }
        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("SamplePlayItem");
            node2.SetAttribute("playToEnd", PlayToEnd.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("r", R.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("pitch", Pitch.ToString());
            node2.SetAttribute("src", sample.path);
            node.AppendChild(node2);
        }
    }
}
