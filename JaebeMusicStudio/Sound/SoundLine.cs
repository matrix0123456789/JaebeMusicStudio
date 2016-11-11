using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class SoundLine
    {
        /// <summary>
        /// other lines, that are connected to this
        /// </summary>
        public List<SoundLineConnection> inputs = new List<SoundLineConnection>();


        public float volume;

        public SoundLine()
        {
        }
        public SoundLine(XmlNode xml)
        {
            if (xml.Attributes["volume"] != null)
                volume = float.Parse(xml.Attributes["volume"].Value, CultureInfo.InvariantCulture);
        }

        internal void serialize(XmlDocument document)
        {
            var node = document.CreateElement("SoundLine");
            node.SetAttribute("volume", volume.ToString(CultureInfo.InvariantCulture));
            foreach (var input in inputs)
            {
                var node2 = document.CreateElement("SoundLineInput");
                node2.SetAttribute("lineNumber", Project.current.lines.IndexOf(input.line).ToString());
                node2.SetAttribute("volume", input.volume.ToString(CultureInfo.InvariantCulture));
                node.AppendChild(node2);
            }
            document.DocumentElement.AppendChild(node);
        }
    }
    struct SoundLineConnection
    {
        public SoundLine line;
       public float volume;

        public SoundLineConnection(int lineNumber, float volume) : this()
        {
            this.line = Project.current.lines[lineNumber];
            this.volume = volume;
        }
    }
}
