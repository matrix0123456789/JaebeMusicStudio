using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class SoundLine
    {
        /// <summary>
        /// other lines, that are connected to this
        /// </summary>
        public List<SoundLineConnection> inputs = new List<SoundLineConnection>();
        public List<SoundLineConnection> outputs = new List<SoundLineConnection>();
        public List<Effect> effects = new List<Effect>();
        public int currentToRender = 0;
        public float[,] lastRendered;
        public float volume = 1;
        private int connectedUIs;
        public float[] LastVolume = { 0, 0 };

        public event Action<int, Effect> effectAdded;
        public event Action<int> effectRemoved;

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
                    case "NonlinearDistortion":
                        effects.Add(new NonlinearDistortion(x));
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
                node2.SetAttribute("lineNumber", Project.current.lines.IndexOf(input.input).ToString());
                node2.SetAttribute("volume", input.volume.ToString(CultureInfo.InvariantCulture));
                node.AppendChild(node2);
            }
            foreach (var effect in effects)
            {
                effect.Serialize(node);
            }
            document.DocumentElement.AppendChild(node);
        }
        public void cleanToRender(int samples)
        {
            currentToRender = inputs.Count;
            lastRendered = new float[2, samples];
        }
        public void rendered(int offset, float[,] data, float volumeChange = 1)
        {
            float vol = volumeChange;
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
                                lastRendered[0, i] += data[0, i] * vol;
                                lastRendered[1, i] += data[0, i] * vol;
                            }
                        }
                        else {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i] += data[0, i] * vol;
                                lastRendered[1, i] += data[1, i] * vol;
                            }
                        }
                    }
                    else
                    {
                        if (data.GetLength(0) == 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i + offset] += data[0, i] * vol;
                                lastRendered[1, i + offset] += data[0, i] * vol;
                            }
                        }
                        else {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i + offset] += data[0, i] * vol;
                                lastRendered[1, i + offset] += data[1, i] * vol;
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
                        output.output.rendered(0, sound, output.volume);
                    }
                    if (this == Project.current.lines[0])
                    {
                        Project.current.ReturnedSound(sound);
                    }
                    if (connectedUIs != 0)
                    {
                        float minL = sound[0, 0];
                        float minR = sound[1, 0];
                        float maxL = sound[0, 0];
                        float maxR = sound[1, 0];
                        for (var i = 0; i < sound.GetLength(1); i++)
                        {
                            if (sound[0, i] < minL)
                                minL = sound[0, i];
                            else if (sound[0, i] > maxL)
                                maxL = sound[0, i];
                            if (sound[1, i] < minR)
                                minR = sound[1, i];
                            else if (sound[1, i] > maxR)
                                maxR = sound[1, i];
                        }
                        minL = Math.Abs(minL);
                        maxL = Math.Abs(maxL);
                        minR = Math.Abs(minR);
                        maxR = Math.Abs(maxR);
                        LastVolume[0] = minL > maxL ? minL : maxL;
                        LastVolume[1] = minR > maxR ? minR : maxR;
                    }
                }
            }
        }

        public void ConnectUI()
        {
            connectedUIs++;
        }
        public void DisconnectUI()
        {
            connectedUIs--;
        }

        public void AddEffect(Effect e)
        {
            var index = effects.Count;
            effects.Add(e);
            effectAdded?.Invoke(index, e);
        }
        public void AddEffect(int index, Effect e)
        {
            effects.Insert(index,e);
            effectAdded?.Invoke(index, e);
        }

        public void RemoveEffect(Effect e)
        {
            var index = effects.IndexOf(e);
            effects.RemoveAt(index);
            effectRemoved?.Invoke(index);
        }
    }
    public class SoundLineConnection
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
