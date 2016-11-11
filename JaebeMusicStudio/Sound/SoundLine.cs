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
        public int currentToRender = 0;
        public float[,] lastRendered;
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
        public void cleanToRender(int samples)
        {
            currentToRender = 0;
            lastRendered = new float[2, samples];
        }
        public void rendered(int offset, float[,] data)
        {//todo optymalizacja oofes=0 i 1 
            var length = data.GetLength(1);
            if (length + offset > lastRendered.GetLength(1))
                length = lastRendered.GetLength(1) - offset;
            for (int i=0;i< length; i++)
            {
                lastRendered[0, i + offset] = data[0, i];
                lastRendered[1, i + offset] = data[1, i];
            }
            currentToRender--;
            if (currentToRender == 0)
            {
                //todo dane międzyliniowe
                if (this == Project.current.lines[0])
                {
                    Project.current.returnedSound(lastRendered);
                }
            }
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
