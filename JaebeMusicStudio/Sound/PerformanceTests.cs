using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    internal class PerformanceTests
    {
        static async Task<TimeSpan> BigSum()
        {
            var sounds = GenerateSamples(64, 4 * 1024 * 1024);
            var start = DateTime.Now;
            var a = new SoundSample(4 * 1024 * 1024);
            foreach (var sound in sounds)
            {
                a.AddEqualLength(sound);
            }
            return DateTime.Now - start;
        }
        static async Task<TimeSpan> BigSumVolume()
        {
            var sounds = GenerateSamples(64, 4 * 1024 * 1024);
            var start = DateTime.Now;
            var a = new SoundSample(4 * 1024 * 1024);
            foreach (var sound in sounds)
            {
                a.AddEqualLength(sound, 0.5f);
            }
            return DateTime.Now - start;
        }
        static async Task<TimeSpan> SmallSums()
        {
            var sounds = GenerateSamples(2 * 1024 * 1024, 128);
            var start = DateTime.Now;
            var a = new SoundSample(128);
            foreach (var sound in sounds)
            {
                a.AddEqualLength(sound, 0.5f);
            }
            return DateTime.Now - start;
        }
        static async Task<TimeSpan> SmallSumsOffsets()
        {
            var sounds = GenerateSamples(2 * 1024 * 1024, 128);
            var start = DateTime.Now;
            var a = new SoundSample(256);
            foreach (var sound in sounds)
            {
                a.AddWithOffset(sound, (int)(sound[0, 1] * 64), 0.5f);
            }
            return DateTime.Now - start;
        }
        static async Task<TimeSpan> SmallSumsCache()
        {
            var sounds = GenerateSamples(16, 128);
            var start = DateTime.Now;
            var a = new SoundSample(128);
            for (var i = 0; i < 128 * 1024; i++)
            {
                foreach (var sound in sounds)
                {
                    a.AddEqualLength(sound, 0.5f);
                }
            }
            return DateTime.Now - start;
        }
        static List<SoundSample> GenerateSamples(int number, int length)
        {
            var rand = new Random();
            var sounds = new List<SoundSample>();
            for (int i = 0; i < number; i++)
            {
                var array = new float[2, length];
                for (int j = 0; j < length; j++)
                {
                    array[0, j] = (float)rand.NextDouble();
                    array[1, j] = (float)rand.NextDouble();
                }
                sounds.Add(new SoundSample(array));
            }
            return sounds;
        }
        internal static async Task<string> Run()
        {
            try
            {
                return $"BigSum: {await BigSum()}\r\n" +
                    $"BigSumVolume: {await BigSumVolume()}\r\n" +
                    $"SmallSums: {await SmallSums()}\r\n" +
                    $"SmallSumsOffsets: {await SmallSumsOffsets()}\r\n" +
                    $"SmallSumsCache: {await SmallSumsCache()}\r\n";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
