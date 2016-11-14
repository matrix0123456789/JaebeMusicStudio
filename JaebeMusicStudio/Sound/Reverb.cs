using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class Reverb : Effect
    {
        //Queue<float[]> buffor = new Queue<float[]>();
        Queue<float> bufforLeft = new Queue<float>();
        Queue<float> bufforRight = new Queue<float>();
        float volume = 1, delay = .1f, feedback = .7f, pan = 0;

        public Reverb(XmlElement x)
        {
        }

        public void cleanMemory()
        {

        }

        public float[,] doFilter(float[,] input)
        {
            long samplesToWait = (long)Project.current.countSamples(delay);
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
