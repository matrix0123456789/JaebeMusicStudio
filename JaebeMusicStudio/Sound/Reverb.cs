using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Reverb : Effect
    {
        //Queue<float[]> buffor = new Queue<float[]>();
        Queue<float> bufforLeft = new Queue<float>();
        Queue<float> bufforRight = new Queue<float>();
        float volume = 1, delay = .1f, feedback = .7f, pan = 0;
        public float Volume { get { return volume; } set { volume = value; } }
        public float Delay { get { return delay; } set { delay = value; } }
        public float Feedback { get { return feedback; } set { feedback = value; } }
        public float Pan { get { return pan; } set { pan = value; } }
        public Reverb(XmlElement x)
        {
            if (x.Attributes["volume"] != null)
                volume = float.Parse(x.Attributes["volume"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["delay"] != null)
                delay = float.Parse(x.Attributes["delay"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["feedback"] != null)
                feedback = float.Parse(x.Attributes["feedback"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["pan"] != null)
                pan = float.Parse(x.Attributes["pan"].Value, CultureInfo.InvariantCulture);
        }

        public Reverb()
        {
        }

        public void CleanMemory()
        {

        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Reverb");
            node2.SetAttribute("volume", Volume.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("delay", Delay.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("feedback", Feedback.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("pan", Pan.ToString(CultureInfo.InvariantCulture));
            node.AppendChild(node2);
        }

        public float[,] DoFilter(float[,] input)
        {
            long samplesToWait = (long)Project.current.CountSamples(delay);
            if (samplesToWait < 1)
                samplesToWait = 1;
            float[,] ret = new float[2, input.GetLength(1)];
            for (long i = 0; i < input.GetLength(1); i++)
            {
                var inp = input[0, i] + input[1, i];
                float fromLeft = 0, fromRight = 0;
                while (bufforLeft.Count >= samplesToWait)
                {
                    fromLeft = bufforLeft.Dequeue();
                    fromRight = bufforRight.Dequeue();
                }
                bufforLeft.Enqueue(fromRight * feedback + inp * volume * (1 - pan));
                bufforRight.Enqueue(fromLeft * feedback + inp * volume * pan);
                ret[0, i] = input[0, i] + fromRight;
                ret[1, i] = input[1, i] + fromLeft;
            }
            return ret;
        }
    }
}
