﻿using System;
using System.Collections.Generic;
using System.Numerics;
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

        public unsafe void AddEqualLength(SoundSample b)
        {
            var count = b.SampleCount;
            var count2 = count * 2;
            //for (long i = 0; i < count; i++)
            //{
            //    this.raw[0, i] += b.raw[0, i];
            //    this.raw[1, i] += b.raw[1, i];
            //}
            fixed (float* pA = raw)
            {
                fixed (float* pB = b.raw)
                {
                    var sA = new Span<float>(pA, raw.Length);
                    var sB = new Span<float>(pB, b.raw.Length);
                    int i = 0;
                    for (; i + Vector<float>.Count < count2; i += Vector<float>.Count)
                    {
                        var vA = new Vector<float>(sA.Slice(i));
                        var vB = new Vector<float>(sB.Slice(i));

                        var x = Vector.Add(vA, vB);
                        x.CopyTo(sA.Slice(i));
                    }
                    for (; i < count2; i++)
                    {
                        sA[i] += sB[i];
                    }
                }
            }
        }
        public unsafe void AddEqualLength(SoundSample b, float volume)
        {
            var count = b.SampleCount;
            var count2 = count * 2;
            //for (long i = 0; i < count; i++)
            //{
            //    this.raw[0, i] += b.raw[0, i] * volume;
            //    this.raw[1, i] += b.raw[1, i] * volume;
            //}
            fixed (float* pA = raw)
            {
                fixed (float* pB = b.raw)
                {
                    var sA = new Span<float>(pA, raw.Length);
                    var sB = new Span<float>(pB, b.raw.Length);
                    int i = 0;
                    for (; i + Vector<float>.Count < count2; i += Vector<float>.Count)
                    {
                        var vA = new Vector<float>(sA.Slice(i));
                        var vB = new Vector<float>(sB.Slice(i));

                        var x = Vector.Multiply(Vector.Add(vA, vB), volume);
                        x.CopyTo(sA.Slice(i));
                    }
                    for (; i < count2; i++)
                    {
                        sA[i] = MathF.FusedMultiplyAdd(volume, sB[i], sA[i]);
                    }
                }
            }
        }
        public void AddWithOffset(SoundSample b, long offset, float volume)
        {
            var aSample = SampleCount - offset;
            var bSample = b.SampleCount;
            var min = aSample > bSample ? bSample : aSample;
            for (long i = 0; i < min; i++)
            {
                this.raw[0, i + offset] = MathF.FusedMultiplyAdd(b.raw[0, i], volume, this.raw[0, i + offset]);
                this.raw[1, i + offset] = MathF.FusedMultiplyAdd(b.raw[1, i], volume, this.raw[1, i + offset]);
            }
        }
        public void Multiply(float volume)
        {
            if (volume != 0)
            {
                var count = SampleCount;
                for (int i = 0; i < count; i++)
                {
                    this.raw[0, i] *= volume;
                    this.raw[1, i] *= volume;
                }
            }
        }
        public float this[int channel, int sample] => this.raw[channel, sample];
    }
}