using System;
using System.Collections.Generic;
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

        public SoundLine(XmlNode xml)
        {
        }

        internal void serialize(XmlDocument document)
        {
            var node = document.CreateElement("SoundLine");
            foreach (var input in inputs)
            {
                var node2 = document.CreateElement("SoundLineInput");
                node2.SetAttribute("lineNumber", Project.current.lines.IndexOf(input.line).ToString());
                node.AppendChild(node2);
            }
            document.DocumentElement.AppendChild(node);
        }
    }
    struct SoundLineConnection
    {
        public SoundLine line;
       public float volume;
    }
}
