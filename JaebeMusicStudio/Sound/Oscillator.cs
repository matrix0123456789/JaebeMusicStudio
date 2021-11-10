using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Oscillator
    {
        public ObservableCollection<float> Pitchs { get; private set; }
        public OscillatorType Type;
        public float a = 0, d = 0, s = 1, r = 0;
        private float squareRatio = .5f;
        private float volume = 1;
        public bool randomPhase = false;
        public float Volume { get { return volume; } set { volume = value; } }
        public float SquareRatio { get { return squareRatio; } set { squareRatio = value; } }
        static Random rand = new Random();
        public event Action<Oscillator> AdsrChanged;
        public event Action<Oscillator> TypeChanged;
        public float A
        {
            get { return a; }
            set
            {
                if (a == value) return; a = value; AdsrChanged?.Invoke(this);
            }
        }
        public float D
        {
            get { return d; }
            set { if (d == value) return; d = value; AdsrChanged?.Invoke(this); }
        }
        public float S
        {
            get { return s; }
            set { if (s == value) return; s = value; AdsrChanged?.Invoke(this); }
        }
        public float R
        {
            get { return r; }
            set { if (r == value) return; r = value; AdsrChanged?.Invoke(this); }
        }
        public Oscillator()
        {
            Pitchs = new ObservableCollection<float>() { 0 };
            A = 0;
            D = 0;
            S = 1; R = 0;
        }


        public Oscillator(XmlNode x) : this()
        {
            if (x.Attributes["a"] != null)
                A = float.Parse(x.Attributes["a"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["d"] != null)
                D = float.Parse(x.Attributes["d"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["s"] != null)
                S = float.Parse(x.Attributes["s"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["r"] != null)
                R = float.Parse(x.Attributes["r"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["randomPhase"] != null)
                randomPhase = bool.Parse(x.Attributes["randomPhase"].Value);
            if (x.Attributes["type"] != null)
                Type = (OscillatorType)Enum.Parse(typeof(OscillatorType), x.Attributes["type"].Value);

            if (x.Attributes["squareRatio"] != null)
                squareRatio = float.Parse(x.Attributes["squareRatio"].Value, CultureInfo.InvariantCulture);
            if (x.Attributes["volume"] != null)
                volume = float.Parse(x.Attributes["volume"].Value, CultureInfo.InvariantCulture);
            Pitchs.Clear();
            foreach (XmlElement pitch in x.ChildNodes)
            {
                Pitchs.Add(float.Parse(pitch.Attributes["value"].Value, CultureInfo.InvariantCulture));
            }
        }

        internal float[,] GetSound(float start, float length, Note note)
        {
            long samples = (long)Project.current.CountSamples(length); //how many samples you need on output
            float samplesTotal = Project.current.CountSamples(note.Length + R);
            var phaseTimeWaited = Project.current.CountSamples(randomPhase ? start + 1000 : start);//+1000 to taki trik
            var realTimeWaited = Project.current.CountSamples( start);//+1000 to taki trik
            var ret = new float[2, samples]; //sound that will be returned

            foreach (var p in Pitchs.ToArray())
            {
                var waveTime = Project.current.waveTime(note.Pitch + p);
                switch (Type)
                {
                    case OscillatorType.sine:
                        createSine(ret, waveTime, phaseTimeWaited);
                        break;
                    case OscillatorType.saw:
                        createSaw(ret, waveTime, phaseTimeWaited);
                        break;
                    case OscillatorType.square:
                        createSquare(ret, waveTime, phaseTimeWaited);
                        break;
                    case OscillatorType.triangle:
                        createTriangle(ret, waveTime, phaseTimeWaited);
                        break;
                    case OscillatorType.whiteNoise:
                        createWhite(ret, waveTime, phaseTimeWaited);
                        break;
                }
            }
            var aLen = Project.current.CountSamples(A);
            var dLen = Project.current.CountSamples(D);
            var rLen = Project.current.CountSamples(R);
            var noteVolume = volume * note.Volume;
            for (int i = 0; i < samples; i++)
            {
                float adsrVal;
                var sumTime = i + realTimeWaited;
                if (sumTime < dLen)
                    adsrVal = S + (1 - S) * (dLen - sumTime) / dLen;
                else
                    adsrVal = S;
                if (sumTime < aLen)
                    adsrVal *= sumTime / aLen;
                var toEnd = samplesTotal - sumTime;
                if (toEnd < rLen)
                    adsrVal *= toEnd / rLen;

                adsrVal *= noteVolume;
                ret[0, i] *= adsrVal;
                ret[1, i] *= adsrVal;
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

            val1 = squareRatio - 1;
            val2 = squareRatio;

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
            node2.SetAttribute("randomPhase", randomPhase.ToString());
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

    public struct Pitch
    {
        private float pitchNote, pitchHz;
    }
}
