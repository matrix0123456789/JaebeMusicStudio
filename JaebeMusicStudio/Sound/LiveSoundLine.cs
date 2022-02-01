using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    public class LiveSoundLine : SoundLineAbstract
    {

        public int DeviceID { get; }
        public string Title => Capabilities.ProductName;
        private WaveInCapabilities Capabilities;
        private WaveIn input = null;
        private List<float[,]> buffer = new List<float[,]>();
        int bufferPosition = 0;
        int bufferAvalible = 0;


        internal LiveSoundLine(int DeviceID)
        {
            this.DeviceID = DeviceID;
            Capabilities = WaveIn.GetCapabilities(DeviceID);
            input = new WaveIn(WaveCallbackInfo.FunctionCallback());
            input.DataAvailable += Wave_DataAvailable;
            if (Capabilities.SupportsWaveFormat(SupportedWaveFormat.WAVE_FORMAT_48S16))
                input.WaveFormat = new WaveFormat(48000, 16, 2);
            input.StartRecording();
        }

        internal void Stop()
        {
            input.StopRecording();
        }

        private void Wave_DataAvailable(object sender, WaveInEventArgs e)
        {
            float[,] sound;
            lock (this)
            {
                var reader = new BinaryReader(new MemoryStream(e.Buffer));
                sound = read(reader, input.WaveFormat.BitsPerSample, input.WaveFormat.Channels);
                buffer.Add(sound);
                if (buffer.Count > 16)
                {
                    bufferAvalible -= buffer[0].GetLength(1);
                    buffer.RemoveAt(0);
                    bufferPosition = 0;
                }
                bufferAvalible += sound.GetLength(1);

                if (connectedUIs != 0)
                {
                    float minL = sound[0, 0];
                    float minR = sound[1, 0];
                    float maxL = sound[0, 0];
                    float maxR = sound[1, 0];
                    for (var i = 0; i < sound.GetLength(1); i++)
                    {
                        if (sound[0, i] < minL)
                            minL = sound[0, i];
                        else if (sound[0, i] > maxL)
                            maxL = sound[0, i];
                        if (sound[1, i] < minR)
                            minR = sound[1, i];
                        else if (sound[1, i] > maxR)
                            maxR = sound[1, i];
                    }
                    minL = Math.Abs(minL);
                    maxL = Math.Abs(maxL);
                    minR = Math.Abs(minR);
                    maxR = Math.Abs(maxR);
                    LastVolume[0] = minL > maxL ? minL : maxL;
                    LastVolume[1] = minR > maxR ? minR : maxR;
                }
            }
        }
        float[,] read(BinaryReader data, int bitrate, int channels)
        {
            float[,] wave;
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
            else
            {
                throw new NotImplementedException();
            }
            return wave;
        }

        internal void clearAfterRender(Rendering rendering)
        {

        }

        public override async Task<float[,]> Render(Rendering rendering)
        {

            var data = new float[2, (int)rendering.CountSamples(rendering.renderingLength)];
            var length = data.GetLength(1);

            if (buffer.Count > 0)
            {
                for (var i = 0; i < length; i++)
                {
                    data[0, i] = buffer[0][0, bufferPosition];
                    data[1, i] = buffer[0][1, bufferPosition];
                    bufferPosition++;
                    if (bufferPosition >= buffer[0].GetLength(1))
                    {
                        bufferAvalible -= buffer[0].GetLength(1);
                        buffer.RemoveAt(0);
                        bufferPosition = 0;
                        if (buffer.Count == 0)
                            break;
                    }
                }
            }
            return data;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
