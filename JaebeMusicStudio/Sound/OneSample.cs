using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class OneSample : SoundElement
    {
        SampledSound sample;
        float innerOffset, speed;
        public float length { get; set; }
        public float offset { get; set; }
        public SoundLine soundLine { get; set; }
        public OneSample(XmlNode element)
        {
            //todo tymczasowo
            soundLine = Project.current.lines[0];


            length = float.Parse(element.Attributes["length"].Value, CultureInfo.InvariantCulture);
            offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            innerOffset = float.Parse(element.Attributes["innerOffset"].Value, CultureInfo.InvariantCulture);
            speed = float.Parse(element.Attributes["speed"].Value, CultureInfo.InvariantCulture);
            sample = SampledSound.FindByUrl(element.Attributes["src"].Value);
        }

        public OneSample(SampledSound sample)
        {
            this.sample = sample;
        }

        public float[,] getSound(float start, float length)
        {
            long samples = (long)Project.current.countSamples(length);
            var ret = new float[sample.channels, samples];
            for (var i = 0; i < samples; i++)//todo koniec sampla
            {//todo mono w samplu
                ret[0, i] = sample.wave[0, (int)(((innerOffset + start) * Project.current.tempo / 60f + (float)i / Project.current.sampleRate) * sample.sampleRate)];
                ret[1, i] = sample.wave[1, (int)(((innerOffset + start) * Project.current.tempo / 60f + (float)i / Project.current.sampleRate) * sample.sampleRate)];
            }
            return ret;
        }
        public void serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("OneSample");
            node2.SetAttribute("offset", offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("innerOffset", innerOffset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("length", length.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("speed", speed.ToString(CultureInfo.InvariantCulture));
            if (sample.path != null)
                node2.SetAttribute("src", sample.path);
            node.AppendChild(node2);
        }
    }
}
