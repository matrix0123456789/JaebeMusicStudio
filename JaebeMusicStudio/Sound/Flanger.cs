using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Flanger : Effect
    {
        public bool IsActive { get; set; } = true;
        private List<FlangerItem> items = new List<FlangerItem>();
        long counter = 0;
        List<float[,]> history = new List<float[,]>();
        public Flanger()
        {
            items.Add(new FlangerItem(1, 0.001f));
        }
        public Flanger(XmlElement xml)
        {
            if (xml.Attributes["isActive"] != null)
                IsActive = bool.Parse(xml.Attributes["isActive"].Value);
            foreach (XmlElement x in xml.ChildNodes)
            {
                items.Add(new FlangerItem(float.Parse(x.Attributes["frequency"].Value, System.Globalization.CultureInfo.InvariantCulture),float.Parse(x.Attributes["amplitude"].Value, System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        public float[,] DoFilter(float[,] input, Rendering rendering)
        {
            lock (this)
            {
                history.Add(input);
                var len1 = input.GetLength(1);
                var ret = new float[input.GetLength(0), input.GetLength(1)];
                for (int n = 0; n < items.Count; n++)
                {
                    var amplitude_sample = items[n].Amplitude * rendering.sampleRate;
                    var ileNaCykl = 1 / items[n].Frequency * rendering.sampleRate / Math.PI / 2;
                    for (int i = 0; i < len1; i++)
                    {

                        var z = amplitude_sample * (-1 - Math.Sin((i + counter) / ileNaCykl)) + i - 1;
                        var x = (int)z;
                        var proporcje = z - x;
                        if (x >= 0)
                        {
                            ret[0, i] += ((float)(input[0, x] * (1 - proporcje) + input[0, x + 1] * proporcje) / 2);
                            ret[1, i] += ((float)(input[1, x] * (1 - proporcje) + input[1, x + 1] * proporcje) / 2);
                        }
                        else
                        {
                            float input_x00, input_x01, input_x10, input_x11;
                            var historyPosition = history.Count - 1;
                            do
                            {
                                historyPosition--;
                                if (historyPosition < 0)
                                    break;
                                x += history[historyPosition].GetLength(1);
                            } while (x < 0);
                            if (historyPosition < 0)
                                continue;
                            input_x00 = history[historyPosition][0, x];
                            input_x10 = history[historyPosition][1, x];
                            if (x + 1 == history[historyPosition].GetLength(1))
                            {
                                input_x01 = history[historyPosition + 1][0, 0];
                                input_x11 = history[historyPosition + 1][1, 0];
                            }
                            else
                            {
                                input_x01 = history[historyPosition][0, x + 1];
                                input_x11 = history[historyPosition][1, x + 1];
                            }
                            ret[0, i] += ((float)(input_x00 * (1 - proporcje) + input_x01 * proporcje) / 2);
                            ret[1, i] += ((float)(input_x10 * (1 - proporcje) + input_x11 * proporcje) / 2);
                        }
                    }
                }
                counter += input.GetLength(1);
                return ret;
            }
        }

        internal void Add(float Frequency = 1,float Amplitude=0.001f)
        {
            items.Add(new FlangerItem(Frequency,Amplitude));
        }

        public void CleanMemory()
        {
            lock (this)
            {
                var maxAmplitude = items.Select(x=>x.Amplitude).Max();
                if (maxAmplitude < -items.Select(x => x.Amplitude).Min())
                    maxAmplitude = -items.Select(x => x.Amplitude).Min();
                maxAmplitude *= 2;
                var historyPosition = history.Count - 1;
                do
                {
                    historyPosition--;
                    if (historyPosition < 0)
                        break;
                    maxAmplitude -= history[historyPosition].GetLength(1);
                } while (maxAmplitude >= 0);

                if (historyPosition - 1 > 0)
                    history.RemoveRange(0, historyPosition - 1);

            }
        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Flanger");
            node2.SetAttribute("isActive", IsActive.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < items.Count; i++)
            {

                var node3 = node.OwnerDocument.CreateElement("FlangerElement");
                node3.SetAttribute("frequency", items[i].Frequency.ToString(CultureInfo.InvariantCulture));
                node3.SetAttribute("amplitude", items[i].Amplitude.ToString(CultureInfo.InvariantCulture));
                node2.AppendChild(node3);
            }
            node.AppendChild(node2);
        }

        public FlangerItem this[int i]
        {
            get { return items[i]; }
            set { items[i] = value; }

        }

        public int Count { get { return items.Count; } }
    }

    public struct FlangerItem
    {
        public float Frequency, Amplitude;

        public FlangerItem(float Frequency, float Amplitude)
        {
            this.Frequency = Frequency;
            this.Amplitude = Amplitude;
        }
    }
}
