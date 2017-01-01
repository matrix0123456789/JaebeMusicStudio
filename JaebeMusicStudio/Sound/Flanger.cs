using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class Flanger : Effect
    {
        List<float> frequency = new List<float>(), amplitude = new List<float>();
        long counter = 0;
        List<float[,]> history = new List<float[,]>();

        public Flanger()
        {
            frequency.Add(1);
            amplitude.Add(1);
        }
        public Flanger(XmlElement xml)
        {
            foreach (XmlElement x in xml.ChildNodes)
            {
                frequency.Add(float.Parse(x.Attributes["frequency"].Value, System.Globalization.CultureInfo.InvariantCulture));
                amplitude.Add(float.Parse(x.Attributes["amplitude"].Value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        public float[,] DoFilter(float[,] input)
        {
            lock (this)
            {
                history.Add(input);
                var len1 = input.GetLength(1);
                var ret = new float[input.GetLength(0), input.GetLength(1)];
                for (int n = 0; n < frequency.Count; n++)
                {
                    var amplitude_sample = amplitude[n] * Project.current.sampleRate;
                    var ileNaCykl = 1 / frequency[n] * Project.current.sampleRate / Math.PI / 2;
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
        public void CleanMemory()
        {
            lock (this)
            {
                var maxAmplitude = amplitude.Max();
                if (maxAmplitude < -amplitude.Min())
                    maxAmplitude = -amplitude.Min();
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
            for (int i = 0; i < frequency.Count; i++)
            {

                var node3 = node.OwnerDocument.CreateElement("FlangerElement");
                node3.SetAttribute("frequency", frequency[i].ToString(CultureInfo.InvariantCulture));
                node3.SetAttribute("amplitude", amplitude[i].ToString(CultureInfo.InvariantCulture));
                node2.AppendChild(node3);
            }
            node.AppendChild(node2);
        }
    }
}
