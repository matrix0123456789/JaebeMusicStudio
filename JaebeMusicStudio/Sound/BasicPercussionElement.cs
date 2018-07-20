using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class BasicPercussionElement
    {
        Random RandomGenerator = new Random();
        public float SoundLength { get { return Math.Max(ToneHalfTime, NoiseHalfTime) * quality / 60 * Project.current.tempo; } }
        /*const*/
        int quality = 16;//where its silence
        float toneHalfTime = .4f;
        float toneModulationTime = .1f;
        float modulationAddFrequency = 150;
        float toneVolume = 1;
        float toneFrequency = 50;
        float noiseHalfTime = .1f;
        float noiseVolume = .4f;
        public BasicPercussionElement()
        {
        }
        public BasicPercussionElement(XmlNode ch)
        {
            foreach (XmlNode x in ch.ChildNodes)
            {
                if (x.Name == "Tone")
                {
                    ToneHalfTime = float.Parse(x.Attributes["halfTime"].Value, CultureInfo.InvariantCulture);
                    ToneFrequency = float.Parse(x.Attributes["frequency"].Value, CultureInfo.InvariantCulture);
                    ToneModulationTime = float.Parse(x.Attributes["modulationTime"].Value, CultureInfo.InvariantCulture);
                    ModulationAddFrequency = float.Parse(x.Attributes["modulationAddFrequency"].Value, CultureInfo.InvariantCulture);
                    ToneVolume = float.Parse(x.Attributes["volume"].Value, CultureInfo.InvariantCulture);
                }
                else if (x.Name == "Noise")
                {
                    NoiseHalfTime = float.Parse(x.Attributes["halfTime"].Value, CultureInfo.InvariantCulture);
                    NoiseVolume = float.Parse(x.Attributes["volume"].Value, CultureInfo.InvariantCulture);
                }
            }
        }
        public float[,] GetSound(float start, float length, Note note)
        {
            var Noise = GetNoise(start, length, note);
            var Tone = GetTone(start, length, note);
            var samples = Noise.GetLength(1);
            for (var i = 0; i < samples; i++)
            {
                Noise[0, i] += Tone[0, i];
                Noise[1, i] += Tone[1, i];
            }
            return Noise;
        }
        public float[,] GetNoise(float start, float length, Note note)
        {
            Random RandomGenerator = new Random();
            long samples = (long)Project.current.CountSamples(length); //how many samples you need on output
            var timeWaited = Project.current.CountSamples(start);
            var ret = new float[2, samples]; //sound that will be returned
            for (var i = 0; i < samples; i++)
            {
                var volume = (float)Math.Pow(.5, (i + timeWaited) / Project.current.sampleRate / NoiseHalfTime) * NoiseVolume;
                ret[0, i] = ((float)RandomGenerator.NextDouble() * 2f - 1f) * volume;
                ret[1, i] = ((float)RandomGenerator.NextDouble() * 2f - 1f) * volume;
            }
            return ret;
        }
        public float[,] GetTone(float start, float length, Note note)
        {
            long samples = (long)Project.current.CountSamples(length); //how many samples you need on output
            float samplesTotal = Project.current.CountSamples(SoundLength);
            var timeWaited = Project.current.CountSamples(start);
            var ret = new float[2, samples]; //sound that will be returned
            for (var i = 0; i < samples; i++)
            {
                var volume = Math.Pow(.5, (i + timeWaited) / Project.current.sampleRate / ToneHalfTime) * ToneVolume;
                var secondsOfNote = (i + timeWaited) / Project.current.sampleRate;
                double ModulatedX = 0;
                if (secondsOfNote < ToneModulationTime)
                    ModulatedX = ModulationFunction(secondsOfNote);
                ret[1, i] = ret[0, i] = (float)(SinusFunction(ModulatedX * ModulationAddFrequency + secondsOfNote * ToneFrequency) * volume);
            }
            return ret;
        }
        double ModulationFunction(double x)
        {
            var a = ToneModulationTime;
            return x - (x * x / (2 * a));
        }
        double SinusFunction(double x)
        {
            return Math.Sin(x * 2 * Math.PI);
        }
        public void Serialize(XmlNode node, IEnumerable<int> pitches)
        {
            var node2 = node.OwnerDocument.CreateElement("BasicPercussionElement");
            node2.SetAttribute("pitches", string.Join(",", pitches.Select(x => x.ToString())));
            node.AppendChild(node2);
        }

        public event Action<BasicPercussionElement> parametersChanged;
        public float ToneHalfTime
        {
            get { return toneHalfTime; }
            set
            {
                if (toneHalfTime == value) return; toneHalfTime = value; parametersChanged?.Invoke(this);
            }
        }



        public float ToneModulationTime
        {
            get { return toneModulationTime; }
            set
            {
                if (toneModulationTime == value) return; toneModulationTime = value; parametersChanged?.Invoke(this);
            }
        }

        public float ModulationAddFrequency
        {
            get { return modulationAddFrequency; }
            set
            {
                if (modulationAddFrequency == value) return; modulationAddFrequency = value; parametersChanged?.Invoke(this);
            }
        }

        public float ToneVolume
        {
            get { return toneVolume; }
            set
            {
                if (toneVolume == value) return; toneVolume = value; parametersChanged?.Invoke(this);
            }
        }

        public float ToneFrequency
        {
            get { return toneFrequency; }
            set
            {
                if (toneFrequency == value) return; toneFrequency = value; parametersChanged?.Invoke(this);
            }
        }
        public float NoiseVolume
        {
            get { return noiseVolume; }
            set
            {
                if (noiseVolume == value) return; noiseVolume = value; parametersChanged?.Invoke(this);
            }
        }
        public float NoiseHalfTime
        {
            get { return noiseHalfTime; }
            set
            {
                if (noiseHalfTime == value) return; noiseHalfTime = value; parametersChanged?.Invoke(this);
            }
        }
    }
}
