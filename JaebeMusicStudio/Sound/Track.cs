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
        public List<ISoundElement> Elements = new List<ISoundElement>();
        public event Action<Track, ISoundElement> SoundElementAdded; 
        public Track()
        {
        }
        public Track(XmlNode xml)
        {
            foreach(XmlNode element in xml.ChildNodes)
            {
                ISoundElement soundElement;
                switch (element.Name)
                {
                    case "OneSample":
                    soundElement = new OneSample(element);
                        break;
                    default: continue;
                }
                Elements.Add(soundElement);
            }
        }
        internal void Serialize(XmlDocument document)
        {
            var node = document.CreateElement("Track");
            foreach (var element in Elements)
            {
                element.Serialize(node);
            }
            document.DocumentElement.AppendChild(node);
        }

        public void AddElement(ISoundElement ss)
        {
            if (ss.SoundLine == null)
                ss.SoundLine = Project.current.lines[0];
            Elements.Add(ss);
            SoundElementAdded?.Invoke(this,ss);
        }
    }
}
