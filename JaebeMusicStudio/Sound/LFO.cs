using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class LFO : Effect
    {
        public bool IsActive { get; set; } = true;
        public OscillatorType Type;
        float phase = 0;
        float frequency = 1;
        float squareRatio = .5f;
        Random rand = new Random();
        public IEffectModulable effect = null;//null means volume
        public void CleanMemory()
        {
            throw new NotImplementedException();
        }

        public float[,] DoFilter(float[,] input, Rendering rendering)
        {

            var time = input.GetLength(1) / rendering.sampleRate;
            var waveTime = rendering.sampleRate / frequency;
            var samples = input.GetLength(1);
            var phaseSamples = phase * waveTime;
            float[,] modulation;
            switch (Type)
            {
                case OscillatorType.sine:
                    modulation = createSine(waveTime, phaseSamples, samples);
                    break;
                case OscillatorType.saw:
                    modulation = createSaw(waveTime, phaseSamples, samples);
                    break;
                case OscillatorType.square:
                    modulation = createSquare(waveTime, phaseSamples, samples);
                    break;
                case OscillatorType.triangle:
                    modulation = createTriangle(waveTime, phaseSamples, samples);
                    break;
                case OscillatorType.whiteNoise:
                    modulation = createWhite(waveTime, phaseSamples, samples);
                    break;
                default: throw new NotImplementedException();
            }
            phase = (phase + time / waveTime) % 1;
            if (effect == null)
            {
                for (var i = 0; i < samples; i++)
                {
                    input[0, i] *= modulation[0, i];
                    input[1, i] *= modulation[1, i];
                }
                return input;
            }
            else throw new NotImplementedException();
        }
        float[,] createSine(double waveTime, double phaseSamples, int samples)
        {
            float[,] ret = new float[2, samples];
            double divider = waveTime / Math.PI / 2;
            for (int i = 0; i < samples; i++)
            {
                var sin = (float)Math.Sin((i + phaseSamples) / divider);
                ret[0, i] = sin;
                ret[1, i] = sin;
            }
            return ret;
        }
        float[,] createSaw(double waveTime, double phaseSamples, int samples)
        {
            float[,] ret = new float[2, samples];
            double divider = waveTime / Math.PI / 2;
            for (int i = 0; i < samples; i++)
            {
                var val = (float)(((i + phaseSamples) / waveTime) % 1 * 2 - 1);
                ret[0, i] = val;
                ret[1, i] = val;
            }
            return ret;
        }


        float[,] createTriangle(double waveTime, double phaseSamples, int samples)
        {
            float[,] ret = new float[2, samples];
            for (int i = 0; i < samples; i++)
            {
                var val = (float)(((i + phaseSamples) / waveTime) % 1 * 4);
                if (val < 2)
                    val = val - 1;
                else
                {
                    val = 3 - val;
                }
                ret[0, i] = val;
                ret[1, i] = val;
            }
            return ret;
        }

        float[,] createSquare(double waveTime, double phaseSamples, int samples)
        {
            float[,] ret = new float[2, samples];
            float val1, val2;

            val1 = -1;
            val2 = 1;

            for (int i = 0; i < samples; i++)
            {
                if (((i + phase) % waveTime) / waveTime < squareRatio)
                {
                    ret[0, i] = val1;
                    ret[1, i] = val1;
                }
                else
                {
                    ret[0, i] = val2;
                    ret[1, i] = val2;
                }
            }
            return ret;
        }

        float[,] createWhite(double waveTime, double phaseSamples, int samples)
        {
            float[,] ret = new float[2, samples];
            for (int i = 0; i < samples; i++)
            {
                ret[0, i] = (float)(rand.NextDouble() * 2 - 1);
                ret[1, i] = (float)(rand.NextDouble() * 2 - 1);
            }
            return ret;
        }
        public void Serialize(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
