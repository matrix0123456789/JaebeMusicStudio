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
