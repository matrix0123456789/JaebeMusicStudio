using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JaebeMusicStudio.Sound
{
    public class SampledSound
    {
        private FileStream stream;
        public string path = null;
        public ushort channels = 1;
        public uint sampleRate;
        ushort bitrate;
        public float[,] wave;
        /// <summary>
        /// used to show wave
        /// </summary>
        private List<float[,]> simpledWave = new List<float[,]>();
        static Dictionary<string, SampledSound> filesByPaths = new Dictionary<string, SampledSound>();
        public SampledSound(FileStream stream, soundFormat format)
        {
            this.stream = stream;
            BinaryReader streamRaw;
            if (format == soundFormat.mp3)
                streamRaw = startMP3();
            else streamRaw = startWave();
            read(streamRaw);
        }
        public SampledSound(string path)
        {
            this.path = path;
            var explode = path.Split('.');
            stream = new System.IO.FileStream(path, System.IO.FileMode.Open);
            BinaryReader streamRaw;
            if (explode.Last() == "mp3")
                streamRaw = startMP3();
            else streamRaw = startWave();
            read(streamRaw);
        }
        BinaryReader startMP3()
        {
            var read = new Mp3FileReader(stream);
            WaveStream convertedStream = WaveFormatConversionStream.CreatePcmStream(read);
            channels = (ushort)convertedStream.WaveFormat.Channels;
            sampleRate = (uint)convertedStream.WaveFormat.SampleRate;
            bitrate = (ushort)convertedStream.WaveFormat.BitsPerSample;
            return new BinaryReader(convertedStream);
        }

        internal static SampledSound FindByUrl(string url)
        {
            if (filesByPaths.ContainsKey(url))
                return filesByPaths[url];
            else
            {
                var file = new SampledSound(url);
                filesByPaths[url] = file;
                return file;
            }
        }

        BinaryReader startWave()
        {
            var reader = new BinaryReader(stream);
            reader.BaseStream.Position = 22;
            channels = reader.ReadUInt16();
            sampleRate = reader.ReadUInt32();
            if (channels == 0 || sampleRate == 0)
            {
                MessageBox.Show("Nieprawidłowy plik", "Bląd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            bitrate = (ushort)(8 * reader.ReadInt32() / sampleRate / channels);

            reader.BaseStream.Position = 44;
            return reader;
        }
        void read(BinaryReader data)
        {
            if (bitrate == 32)
            {
                long length = (data.BaseStream.Length - data.BaseStream.Position) / 4 / channels;
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadInt32() / (256f * 256f * 128f);
                }
            }
            else if (bitrate == 16)
            {
                long length = (data.BaseStream.Length - data.BaseStream.Position) / 2 / channels;
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadInt16() / (32768f);
                }
            }
            else if (bitrate == 8)
            {
                long length = (data.BaseStream.Length - data.BaseStream.Position) / channels;
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadByte() / (128f);
                }
            }
        }

        public float Length
        {
            get { return Project.current.SamplesToBeats(wave.Length); }
        }

        public float[,] simpled(int levelOfDetail)
        {
            lock (simpledWave)
            {
                if (simpledWave.Count>=levelOfDetail)
                {
                    return simpledWave[levelOfDetail - 1];
                }
                else
                {
                    if (levelOfDetail == 1)
                    {
                        var newSamples = wave.GetLength(1) / 64;
                        var sound = new float[2, newSamples];
                        for (int i = 0; i < newSamples; i++)
                        {
                            float min, max = min = wave[0, i * 64];
                            for (int j = 1; j < 64; j++)
                            {
                                if (wave[0, i * 64 + j] > max)
                                    max = wave[0, i * 64 + j];
                                else if (wave[0, i * 64 + j] < min)
                                    min = wave[0, i * 64 + j];
                            }
                            sound[0, i] = max;
                            sound[1, i] = min;
                        }
                        simpledWave.Add(sound);
                        return sound;
                    }
                    else
                    {
                        var previous = simpled(levelOfDetail - 1);
                        var newSamples = previous.GetLength(1) / 64;
                        var sound = new float[2, newSamples];
                        for (int i = 0; i < newSamples; i++)
                        {
                            float min = previous[1, i * 64], max = previous[0, i * 64];
                            for (int j = 1; j < 64; j++)
                            {
                                if (previous[0, i * 64 + j] > max)
                                    max = previous[0, i * 64 + j];
                                else if (previous[1, i * 64 + j] < min)
                                    min = previous[1, i * 64 + j];
                            }
                            sound[0, i] = max;
                            sound[1, i] = min;
                        }
                        simpledWave.Add(sound);
                        return sound;
                    }
                }
            }
        }
    }
    public enum soundFormat
    {
        mp3, wave
    }
}
