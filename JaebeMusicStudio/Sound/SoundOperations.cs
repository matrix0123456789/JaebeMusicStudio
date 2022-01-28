using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace JaebeMusicStudio.Sound
{
    public static class SoundOperations
    {
        public static unsafe void AddEqualLengths(float[,] target, float[,] source)
        {
            for (int i = 0; i < target.Length/2; i++)
            {
                target[0, i] += source[0, i];
                target[1, i] += source[1, i];
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
    }
}
