using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class SoundLine : SoundLineAbstract
    {
        /// <summary>
        /// other lines, that are connected to this
        /// </summary>
        public List<SoundLineConnection> inputs = new List<SoundLineConnection>();
        public List<Effect> effects = new List<Effect>();

        public string Title { get; set; } = "";
        public event Action<int, Effect> effectAdded;
        public event Action<int> effectRemoved;
        Dictionary<Rendering, SoundLineRendering> renderings = new Dictionary<Rendering, SoundLineRendering>();
        public SoundLine()
        {
        }
        public SoundLine(XmlElement xml)
        {
            if (xml.Attributes["volume"] != null)
                volume = float.Parse(xml.Attributes["volume"].Value, CultureInfo.InvariantCulture);
            if (xml.Attributes["title"] != null)
                Title = xml.Attributes["title"].Value;
            foreach (XmlElement x in xml.ChildNodes)
            {
                switch (x.Name)
                {
                    case "Flanger":
                        effects.Add(new Flanger(x));
                        break;
                    case "SimpleFilter":
                        effects.Add(new SimpleFilter(x));
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
            node.SetAttribute("title", Title);
            foreach (var input in inputs)
            {
                var node2 = document.CreateElement("SoundLineInput");
                if (input.input is SoundLine)
                {
                    node2.SetAttribute("lineNumber", Project.current.lines.IndexOf(input.input as SoundLine).ToString());
                }
                else
                {
                    node2.SetAttribute("liveLineNumber", (input.input as LiveSoundLine).DeviceID.ToString());
                }
                node2.SetAttribute("volume", input.volume.ToString(CultureInfo.InvariantCulture));
                node.AppendChild(node2);
            }
            foreach (var effect in effects)
            {
                effect.Serialize(node);
            }
            document.DocumentElement.AppendChild(node);
        }
        public SoundLineRendering getByRendering(Rendering r)
        {
            if (!renderings.ContainsKey(r))
            {
                renderings[r] = new SoundLineRendering();
                prepareToRender(r);
            }
            return renderings[r];
        }
        public void prepareToRender(Rendering rendering)
        {
            var slRend = getByRendering(rendering);
            slRend.currentToRender = inputs.Count;
            slRend.data = new float[2, (int)rendering.project.CountSamples(rendering.renderingLength)];
        }
        public void rendered(int offset, float[,] data, Rendering rendering, float volumeChange = 1)
        {
            float vol = volumeChange;
            var slRend = getByRendering(rendering);
            lock (this)
            {
                var lastRendered = slRend.data;
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
                        else
                        {
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
                        else
                        {
                            for (int i = 0; i < length; i++)
                            {
                                lastRendered[0, i + offset] += data[0, i] * vol;
                                lastRendered[1, i + offset] += data[1, i] * vol;
                            }
                        }
                    }
                }
                slRend.currentToRender--;
            }
            checkIfReady(rendering);

        }
        public void checkIfReady(Rendering rendering)
        {
            if (!rendering.canHarvest)
                return;
            lock (this)
            {
                var slRend = getByRendering(rendering);
                if (slRend.currentToRender == 0&&!slRend.completed)
                {
                    var sound = slRend.data;
                    if (volume != 0)
                    {
                        if (volume != 1)
                        {
                            var length = sound.GetLength(1);
                            for (int i = 0; i < length; i++)
                            {
                                sound[0, i] *= volume;
                                sound[1, i] *= volume;
                            }
                        }
                        foreach (var effect in effects)
                        {
                            if (effect.IsActive)
                                sound = effect.DoFilter(sound);
                        }
                    }
                    foreach (var output in outputs)
                    {
                        output.output.rendered(0, sound, rendering, output.volume);
                    }
                    slRend.data = sound;
                    slRend.Resolve();

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


        public void AddEffect(Effect e)
        {
            var index = effects.Count;
            effects.Add(e);
            effectAdded?.Invoke(index, e);
        }
        public void AddEffect(int index, Effect e)
        {
            effects.Insert(index, e);
            effectAdded?.Invoke(index, e);
        }

        public void RemoveEffect(Effect e)
        {
            var index = effects.IndexOf(e);
            effects.RemoveAt(index);
            effectRemoved?.Invoke(index);
        }

        public override string ToString()
        {
            string ret;
            var number = Project.current.lines.IndexOf(this);
            if (number == 0)
                ret = "Linia główna";
            else
                ret = "Linia " + number;
            if (Title != null || Title != "")
            {
                ret += " " + Title;
            }
            return ret;
        }
        public void clearAfterRender(Rendering rendering)
        {
            renderings.Remove(rendering);
        }
    }
    public class SoundLineConnection
    {
        public SoundLine output;
        public SoundLineAbstract input;
        public float volume;

        public SoundLineConnection(int lineNumberOutput, SoundLineAbstract input, float volume)
        {
            this.output = Project.current.lines[lineNumberOutput];
            this.input = input;
            this.volume = volume;
        }
    }
    public class SoundLineRendering
    {
        public int currentToRender;
        public float[,] data;
        public bool completed;
        List<Action> waitingOnCompletion = new List<Action>();
        public SoundLineRenderingAwaiter GetAwaiter()
        {
            return new SoundLineRenderingAwaiter(this);
        }
        public void Resolve()
        {
            lock (this)
            {
                this.completed = true;

                foreach (var x in waitingOnCompletion)
                {
                    try
                    {
                        x();
                    }
                    catch { }
                }
            }
        }
        public class SoundLineRenderingAwaiter : INotifyCompletion
        {
            private SoundLineRendering parent;

            public SoundLineRenderingAwaiter(SoundLineRendering parent)
            {
                this.parent = parent;
            }

            public bool IsCompleted
            {
                get
                {
                    return parent.completed;
                }
            }


            public float[,] GetResult()
            {
                return this.parent.data;
            }


            public void OnCompleted(Action continuation)
            {
                lock (parent)
                {
                    parent.waitingOnCompletion.Add(continuation);
                }
            }

            public void UnsafeOnCompleted(Action continuation) { OnCompleted(continuation); }
        }
    }
}
