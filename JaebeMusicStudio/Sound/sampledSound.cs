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
    class SampledSound
    {
        private FileStream stream;
        public ushort channels = 1;
        uint sampleRate;
        ushort bitrate;
        float[,] wave;
        public SampledSound(FileStream stream, soundFormat format)
        {
            this.stream = stream;
            BinaryReader streamRaw;
            if (format == soundFormat.mp3)
                streamRaw = startMP3();
            else streamRaw = startWave();
            read(streamRaw);
        }
        public enum soundFormat
        {
            mp3, wave
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
                long length = (data.BaseStream.Length - data.BaseStream.Position) / 4;
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadInt32() / (256f * 256f * 128f);
                }
            }
            else if (bitrate == 16)
            {
                long length = (data.BaseStream.Length - data.BaseStream.Position) / 2;
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadInt16() / (32768f);
                }
            }
            else if (bitrate == 8)
            {
                long length = (data.BaseStream.Length - data.BaseStream.Position);
                wave = new float[channels, length];
                for (long i = 0; i < length; i++)
                {
                    for (ushort c = 0; c < channels; c++)
                        wave[c, i] = data.ReadByte() / (128f);
                }
            }
        }
    }
}
