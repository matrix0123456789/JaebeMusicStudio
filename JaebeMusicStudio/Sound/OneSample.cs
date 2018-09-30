using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JaebeMusicStudio.Exceptions;

namespace JaebeMusicStudio.Sound
{
    public class OneSample : ISoundElement, ISoundElementDirectOutput
    {
        public SampledSound sample;
        float innerOffset, speed = 1;
        private float length, offset;
        public float Length { get { return length; } set { length = value; positionChanged?.Invoke(this); } }
        public float Offset { get { return offset; } set { offset = value; positionChanged?.Invoke(this); } }
        public SoundLine SoundLine { get; set; }
        private string title = "";
        public string Title { get { if (title != "") return title; else return Name; } set { title = value; } }

        public OneSample(XmlNode element)
        {
            if (element.Attributes["soundLine"] != null)
            {
                var number = uint.Parse(element.Attributes["soundLine"].Value);
                if (number >= Project.current.lines.Count)
                    throw new BadFileException();
                SoundLine = Project.current.lines[(int)number];
            }
            else
                SoundLine = Project.current.lines[0];


            length = float.Parse(element.Attributes["length"].Value, CultureInfo.InvariantCulture);
            offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            innerOffset = float.Parse(element.Attributes["innerOffset"].Value, CultureInfo.InvariantCulture);
            speed = float.Parse(element.Attributes["speed"].Value, CultureInfo.InvariantCulture);
            if (element.Attributes["name"] != null)
                Name = element.Attributes["name"].Value;
            sample = SampledSound.FindByUrl(element.Attributes["src"].Value);
        }

        public OneSample(SampledSound sample)
        {
            this.sample = sample;
            Length = this.sample.Length;
        }

        public float[,] GetSound(float start, float length, Rendering rendering)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var ret = new float[sample.channels, samples];//sound that will be returned
            var startOffset = ((innerOffset + start) / Project.current.tempo * 60f) * sample.sampleRate;//start of reading in sample
            var samplesRatio = sample.sampleRate / Project.current.sampleRate;

            if ((int)(startOffset + (float)samples * samplesRatio) >= sample.wave.GetLength(1))//end of sample
                samples = (long)((sample.wave.GetLength(1) - 1 - startOffset) / samplesRatio);

            if (sample.channels == 1)
                for (var i = 0; i < samples; i++)
                {
                    ret[0, i] = sample.wave[0, (int)(startOffset + (float)i * samplesRatio)];
                }
            else
                for (var i = 0; i < samples; i++)
                {
                    var positionInside = (int)(startOffset + (float)i * samplesRatio);
                    ret[0, i] = sample.wave[0, positionInside];
                    ret[1, i] = sample.wave[1, positionInside];
                }
            return ret;
        }
        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("OneSample");
            node2.SetAttribute("offset", Offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("innerOffset", innerOffset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("length", Length.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("speed", speed.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("name", Name);
            if (sample.path != null)
                node2.SetAttribute("src", sample.path);
            node.AppendChild(node2);
        }

        public ISoundElement Duplicate()
        {
            var newElem = MemberwiseClone() as OneSample;
            return newElem;
        }

        public event Action<ISoundElement> positionChanged;

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
    }
}
