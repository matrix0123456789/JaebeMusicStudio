using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class Note
    {
        private float length, offset, pitch;

        public float Length { get { return length; } set { length = value; } }
        public float Offset { get { return offset; } set { offset = value; } }
        public float Pitch { get { return pitch; } set { pitch = value; } }

        public Note()
        {
        }

        public Note(XmlElement element)
        {
            length = float.Parse(element.Attributes["length"].Value, CultureInfo.InvariantCulture);
            offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            pitch = float.Parse(element.Attributes["pitch"].Value, CultureInfo.InvariantCulture);
        }
        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Note");
            node2.SetAttribute("offset", Offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("length", Length.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("pitch", Pitch.ToString(CultureInfo.InvariantCulture));
            node.AppendChild(node2);
        }
    }
}
