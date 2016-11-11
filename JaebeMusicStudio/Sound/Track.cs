using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class Track
    {
        public List<SoundElement> elements = new List<SoundElement>();
        public Track()
        {
        }
        public Track(XmlNode xml)
        {
            foreach(XmlNode element in xml.ChildNodes)
            {
                SoundElement soundElement;
                switch (element.Name)
                {
                    case "OneSample":
                    soundElement = new OneSample(element);
                        break;
                    default: continue;
                }
                elements.Add(soundElement);
            }
        }
        internal void serialize(XmlDocument document)
        {
            var node = document.CreateElement("Track");
            foreach (var element in elements)
            {
                element.serialize(node);
            }
            document.DocumentElement.AppendChild(node);
        }
    }
}
