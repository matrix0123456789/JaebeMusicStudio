using System;
using System.Collections.Generic;
using System.Text;

namespace JaebeMusicStudio.Sound
{
    public class SoundSample
    {
        private float[,] raw;

        public SoundSample(long sampleCount)
        {
            this.raw = new float[2, sampleCount];
        }
        public SoundSample(float[,] raw)
        {
            this.raw = raw;
        }
        public static implicit operator SoundSample(float[,] raw) => new SoundSample(raw);
        public static implicit operator float[,](SoundSample x) => x.raw;
        public int SampleCount => raw.GetLength(1);

        public void AddEqualLength(SoundSample b)
        {
            for (int i = 0; i < b.SampleCount; i++)
            {
                this.raw[0, i] += b.raw[0, i];
                this.raw[1, i] += b.raw[1, i];
            }
            //fixed (float* t = target)
            //{
            //    fixed (float* s = source)
            //    {
            //        var vtarget = new Vector<float>(new Span<float>(t, target.Length));
            //        var vsource = new Vector<float>(new Span<float>(s, source.Length));
            //        var x=Vector.Add(vtarget, vsource);
            //    }
            //}
        }
        public void AddEqualLength(SoundSample b, float volume)
        {
            for (int i = 0; i < b.SampleCount; i++)
            {
                this.raw[0, i] += b.raw[0, i] * volume;
                this.raw[1, i] += b.raw[1, i] * volume;
            }
        }
        public void AddWithOffset(SoundSample b, long offset, float volume)
        {
            var aSample = SampleCount - offset;
            var bSample = b.SampleCount;
            var min = aSample > bSample ? bSample : aSample;
            for (long i = 0; i < min; i++)
            {
                this.raw[0, i + offset] += b.raw[0, i] * volume;
                this.raw[1, i + offset] += b.raw[1, i] * volume;
            }
        }
        public void Multiply(float volume)
        {
            if (volume != 0)
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    this.raw[0, i] *= volume;
                    this.raw[1, i] *= volume;
                }
            }
        }
        public float this[int channel, int sample] => this.raw[channel, sample];
    }
}