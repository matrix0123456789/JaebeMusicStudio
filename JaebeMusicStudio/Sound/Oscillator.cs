using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    class Oscillator
    {
        public List<float> Pitchs = new List<float>() { 0 };
        public OscillatorType Type;
        public float A = 0, D = 0, S = 1, R = 0;

        internal float[,] GetSound(float start, float length, Note note)
        {
            long samples = (long)Project.current.CountSamples(length);//how many samples you need on output
            var timeWaited = Project.current.CountSamples(start);
            var ret = new float[2, samples];//sound that will be returned

            foreach (var p in Pitchs)
            {
                var waveTime = Project.current.waveTime(note.Pitch + p);
                switch (Type)
                {
                    case OscillatorType.sine:
                        createSine(ret, waveTime, timeWaited);
                        break;
                }
            }


            return ret;
        }

        void createSine(float[,] ret, double waveTime, double phase)
        {
            double divider = waveTime / Math.PI / 2;
            for (int i = 0; i < ret.LongLength / 2; i++)
            {
                var sin = (float)Math.Sin((i + phase) / divider);
                ret[0, i] += sin;
                ret[1, i] += sin;
            }
        }

        void createSaw(float[,] ret, double waveTime, double phase)
        {
            for (int i = 0; i < ret.LongLength / 2; i++)
            {
                var val = (float)(((i + phase) / waveTime) % 1 * 2 - 1);
                ret[0, i] += val;
                ret[1, i] += val;
            }
        }
    }

    enum OscillatorType
    {
        sine, triangle, saw, square, whiteNoise, pinkNoise
    }
}
