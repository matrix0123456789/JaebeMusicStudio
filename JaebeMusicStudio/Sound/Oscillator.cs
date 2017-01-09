﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Oscillator
    {
        public List<float> Pitchs = new List<float>() { 0 };
        public OscillatorType Type;
        public float A = 0, D = 0, S = 1, R = 0;
        private float squareRatio = .5f;
        private float volume = 1;
        static Random rand = new Random();

        public Oscillator()
        {
        }

        public Oscillator(XmlNode x)
        {
            if (x.Attributes["a"] != null)
                A = float.Parse(x.Attributes["a"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["d"] != null)
                D = float.Parse(x.Attributes["d"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["s"] != null)
                S = float.Parse(x.Attributes["s"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["r"] != null)
                R = float.Parse(x.Attributes["r"].Value, CultureInfo.InvariantCulture);

            if (x.Attributes["squareRatio"] != null)
                squareRatio = float.Parse(x.Attributes["squareRatio"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["volume"] != null)
                volume = float.Parse(x.Attributes["volume"].Value, CultureInfo.InvariantCulture);
        }

        internal float[,] GetSound(float start, float length, Note note)
        {
            long samples = (long)Project.current.CountSamples(length); //how many samples you need on output
            var timeWaited = Project.current.CountSamples(start);
            var ret = new float[2, samples]; //sound that will be returned

            foreach (var p in Pitchs)
            {
                var waveTime = Project.current.waveTime(note.Pitch + p);
                switch (Type)
                {
                    case OscillatorType.sine:
                        createSine(ret, waveTime, timeWaited);
                        break;
                    case OscillatorType.saw:
                        createSaw(ret, waveTime, timeWaited);
                        break;
                    case OscillatorType.square:
                        createSquare(ret, waveTime, timeWaited);
                        break;
                    case OscillatorType.triangle:
                        createTriangle(ret, waveTime, timeWaited);
                        break;
                    case OscillatorType.whiteNoise:
                        createWhite(ret, waveTime, timeWaited);
                        break;
                }
            }

            for (int i = 0; i < samples; i++)
            {
                ret[0, i] *= volume;
                ret[1, i] *= volume;
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

        void createTriangle(float[,] ret, double waveTime, double phase)
        {
            for (int i = 0; i < ret.LongLength / 2; i++)
            {
                var val = (float)(((i + phase) / waveTime) % 1 * 4);
                if (val < 2)
                    val = val - 1;
                else
                {
                    val = 3 - val;
                }
                ret[0, i] += val;
                ret[1, i] += val;
            }
        }

        void createSquare(float[,] ret, double waveTime, double phase)
        {
            float val1, val2;
            if (squareRatio < .5)
            {
                val1 = 1;
                val2 = 1 - squareRatio - squareRatio;
            }
            else
            {

                val1 = -1 + squareRatio + squareRatio;
                val2 = -1;
            }
            for (int i = 0; i < ret.LongLength / 2; i++)
            {
                if (((i + phase) % waveTime) / waveTime < squareRatio)
                {
                    ret[0, i] += val1;
                    ret[1, i] += val1;
                }
                else
                {
                    ret[0, i] += val2;
                    ret[1, i] += val2;
                }
            }
        }

        void createWhite(float[,] ret, double waveTime, double phase)
        {
            for (int i = 0; i < ret.LongLength / 2; i++)
            {
                ret[0, i] += (float)(rand.NextDouble() * 2 - 1);
                ret[1, i] += (float)(rand.NextDouble() * 2 - 1);
            }

        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Oscillator");
            node2.SetAttribute("type", Type.ToString());
            node2.SetAttribute("squareRatio", squareRatio.ToString(CultureInfo.InvariantCulture));

            node2.SetAttribute("a", A.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("d", D.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("s", S.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("r", R.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("volume", volume.ToString(CultureInfo.InvariantCulture));
            foreach (var p in Pitchs)
            {
                var node3 = node.OwnerDocument.CreateElement("Pitch");
                node3.SetAttribute("value", p.ToString(CultureInfo.InvariantCulture));
                node2.AppendChild(node3);
            }
            node.AppendChild(node2);
        }
    }

    public enum OscillatorType
    {
        sine, triangle, saw, square, whiteNoise, pinkNoise
    }
}
