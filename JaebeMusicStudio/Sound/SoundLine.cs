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
        public List<SoundLineConnection> outputs = new List<SoundLineConnection>();
        public List<Effect> effects = new List<Effect>();
        public int currentToRender = 0;
        public float[,] lastRendered;
        public float volume;

        public SoundLine()
        {
        }
        public SoundLine(XmlElement xml)
        {
            if (xml.Attributes["volume"] != null)
                volume = float.Parse(xml.Attributes["volume"].Value, CultureInfo.InvariantCulture);
            foreach (XmlElement x in xml.ChildNodes)
            {
                switch (x.Name)
                {
                    case "Flanger":
                        effects.Add(new Flanger(x));
                        break;
                        
                    case "Reverb":
                        effects.Add(new Reverb(x));
                        break;
                }
            }
        }

        internal void Serialize(XmlDocument document)
        {
            var node = document.CreateElement("SoundLine");
            node.SetAttribute("volume", volume.ToString(CultureInfo.InvariantCulture));
            foreach (var input in inputs)
            {
                var node2 = document.CreateElement("SoundLineInput");
                node2.SetAttribute("lineNumber", Project.current.lines.IndexOf(input.output).ToString());
                node2.SetAttribute("volume", input.volume.ToString(CultureInfo.InvariantCulture));
                node.AppendChild(node2);
            }
            document.DocumentElement.AppendChild(node);
        }
        public void cleanToRender(int samples)
        {
            currentToRender = inputs.Count;
            lastRendered = new float[2, samples];
        }
        public void rendered(int offset, float[,] data)
        {
            lock (this)
            {
                if (volume != 0)
                {
                    var length = data.GetLength(1);
                    if (length + offset > lastRendered.GetLength(1))
                        length = lastRendered.GetLength(1) - offset;
                    if (offset == 0)
                    {
                        if (data.GetLength(0) == 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i] += data[0, i];
                                lastRendered[1, i] += data[0, i];
                            }
                        }
                        else {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i] += data[0, i];
                                lastRendered[1, i] += data[1, i];
                            }
                        }
                    }
                    else
                    {
                        if (data.GetLength(0) == 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i + offset] += data[0, i];
                                lastRendered[1, i + offset] += data[0, i];
                            }
                        }
                        else {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i + offset] += data[0, i];
                                lastRendered[1, i + offset] += data[1, i];
                            }
                        }
                    }
                }
                currentToRender--;
            }
            checkIfReady();

        }
        public void checkIfReady()
        {
            lock (this)
            {
                if (currentToRender == 0)
                {
                    //todo dane międzyliniowe
                    var sound = lastRendered;
                    if (volume != 0)
                    {
                        if (volume != 1)
                        {
                            var length = lastRendered.GetLength(1);
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i] *= volume;
                                lastRendered[1, i] *= volume;
                            }
                        }
                        foreach (var effect in effects)
                        {
                            sound = effect.DoFilter(sound);
                        }
                    }
                    foreach (var output in outputs)
                    {
                        output.output.rendered(0, sound);
                    }
                    if (this == Project.current.lines[0])
                    {
                        Project.current.ReturnedSound(sound);
                    }
                }
            }
        }
    }
    class SoundLineConnection
    {
        public SoundLine output;
        public SoundLine input;
        public float volume;

        public SoundLineConnection(int lineNumberOutput, SoundLine input, float volume)
        {
            this.output = Project.current.lines[lineNumberOutput];
            this.input = input;
            this.volume = volume;
        }
    }
}
