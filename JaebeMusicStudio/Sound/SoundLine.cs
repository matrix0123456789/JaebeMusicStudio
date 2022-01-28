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
        public event Action<int, SoundLineConnection> inputAdded;
        public event Action<int> inputRemoved;
        public SoundLine()
        {
        }
        public SoundLine(XmlElement xml)
        {
            if (xml.Attributes["volume"] != null)
                Volume = float.Parse(xml.Attributes["volume"].Value, CultureInfo.InvariantCulture);
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
            node.SetAttribute("volume", Volume.ToString(CultureInfo.InvariantCulture));
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
        /*public void rendered(int offset, float[,] inputData, float[,] outputData, float volumeChange = 1)
        {
            float vol = volumeChange;

            if (Volume != 0)
            {
                var length = inputData.GetLength(1);
                if (length + offset > outputData.GetLength(1))
                    length = outputData.GetLength(1) - offset;
                if (offset == 0)
                {
                    if (inputData.GetLength(0) == 1)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[0, i] += inputData[0, i] * vol;
                            outputData[1, i] += inputData[0, i] * vol;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[0, i] += inputData[0, i] * vol;
                            outputData[1, i] += inputData[1, i] * vol;
                        }
                    }
                }
                else
                {
                    if (inputData.GetLength(0) == 1)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[0, i + offset] += inputData[0, i] * vol;
                            outputData[1, i + offset] += inputData[0, i] * vol;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[0, i + offset] += inputData[0, i] * vol;
                            outputData[1, i + offset] += inputData[1, i] * vol;
                        }
                    }
                }
            }

        
        }*/
        public override async Task<float[,]> Render(Rendering rendering)
        {
            //if (!rendering.canHarvest)
            //    return;

            //var slRend = getByRendering(rendering);


            //var sound = slRend.data;
            //Console.WriteLine("startAwaiting");
            //foreach (var oneTask in slRend.currentToRender)
            //{
            //    var result = await oneTask;
            //    Console.WriteLine("ElementAwaited");
            //    rendered(result.offset, result.data, sound, result.volumeChange);
            //}
            var sound = await RenderDirectSound(rendering);
            var length = sound.GetLength(1);
            foreach (var input in inputs)
            {
                var inputData = await rendering.soundLinesRenderings[input.input].Task;
                SoundOperations.AddEqualLengths(sound, inputData);
            }
            if (Volume != 0)
            {
                if (Volume != 1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        sound[0, i] *= Volume;
                        sound[1, i] *= Volume;
                    }
                }
                foreach (var effect in effects)
                {
                    if (effect.IsActive)
                        sound = effect.DoFilter(sound);
                }
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
            return sound;
        }

        public async Task<float[,]> RenderDirectSound(Rendering rendering)
        {

            var sound = new float[2, (int)rendering.project.CountSamples(rendering.renderingLength)];
            float position = rendering.renderingStart;
            float renderLength = rendering.renderingLength;

            var tasks = new List<Task<SoundElementRenderResult>>();
            foreach (var element in rendering.soundElements.Where(element => element.SoundLine == this))
            {
                var renderStart = element.Offset - position;
                var task = new Task<SoundElementRenderResult>(() =>
                {
                    if (renderStart >= 0) //you must wait to start playing
                    {
                        var rendered = element.GetSound(0, rendering.renderingLength - renderStart, rendering);
                        return new SoundElementRenderResult { data = rendered, offset = (int)rendering.project.CountSamples(renderStart) };
                    }
                    else
                    {
                        var rendered = element.GetSound(-renderStart, rendering.renderingLength, rendering);
                        return new SoundElementRenderResult { data = rendered, offset = 0 };
                    }
                });
                tasks.Add(task);
                task.Start();
            }
            foreach (var liveElement in rendering.liveInputs.Where(liveElement => liveElement.Synth.SoundLine == this))
            {
                var task = new Task<SoundElementRenderResult>(() =>
                {
                    var rendered = liveElement.GetSound(0, rendering.renderingLength, rendering);
                    return new SoundElementRenderResult { data = rendered, offset = 0 };
                });
                tasks.Add(task);
                task.Start();
            }
            foreach (var task in tasks)
            {
                var result = await task;
                float vol = Volume * result.volumeChange;

                if (Volume != 0)
                {
                    var length = result.data.GetLength(1);
                    if (length + result.offset > sound.GetLength(1))
                        length = sound.GetLength(1) - result.offset;
                    if (result.offset == 0)
                    {
                        if (result.data.GetLength(0) == 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                sound[0, i] += result.data[0, i] * vol;
                                sound[1, i] += result.data[0, i] * vol;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < length; i++)
                            {
                                sound[0, i] += result.data[0, i] * vol;
                                sound[1, i] += result.data[1, i] * vol;
                            }
                        }
                    }
                    else
                    {
                        if (result.data.GetLength(0) == 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                sound[0, i + result.offset] += result.data[0, i] * vol;
                                sound[1, i + result.offset] += result.data[0, i] * vol;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < length; i++)
                            {
                                sound[0, i + result.offset] += result.data[0, i] * vol;
                                sound[1, i + result.offset] += result.data[1, i] * vol;
                            }
                        }
                    }
                }
            }
            return sound;
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
        public void AddInput(SoundLineConnection c)
        {
            var index = inputs.Count;
            inputs.Add(c);
            inputAdded?.Invoke(index, c);
        }
        public void AddInput(int index, SoundLineConnection c)
        {
            inputs.Insert(index, c);
            inputAdded?.Invoke(index, c);
        }

        public void RemoveInput(SoundLineConnection c)
        {
            var index = inputs.IndexOf(c);
            inputs.RemoveAt(index);
            inputRemoved?.Invoke(index);
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
}
