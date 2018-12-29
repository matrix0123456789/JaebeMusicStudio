using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    static class Player
    {
        public static float[] LastVolume = { 0, 0 };
        public static Status status = Status.paused;
        public static float position;
        public static bool liveRenderingNow = false;
        static int renderPeriod = 15;
        static System.Threading.Thread renderingThread;
        public enum Status { fileRendering, playing, paused }
        public static BufferedWaveProvider bufor = new BufferedWaveProvider(new WaveFormat((int)Sound.Project.current.sampleRate, 2));
        public static WasapiOut WasapiWyjście = new WasapiOut(AudioClientShareMode.Shared, false, 10);
        public static event Action<float> positionChanged;
        public static event Action<float[,]> SoundPlayed;
        static Rendering liveRenderingObject = new Rendering();
        static Timer renderFallbackTimer;
        static Player()
        {
            WasapiWyjście.Init(bufor);
            renderingThread = new System.Threading.Thread(Render);
            renderingThread.Name = "renderingThread";
            renderingThread.Start();
            WasapiWyjście.Play();
            renderFallbackTimer = new Timer(Render, null, 0, 10);

        }
        public static void Play()
        {
            if (status == Status.paused)
            {
                status = Status.playing;
            }
        }
        public static void Pause()
        {
            if (status == Status.playing)
            {
                status = Status.paused;
            }
        }
        public static void SetPosition(float position)
        {
            Player.position = position;
            if (positionChanged != null)
                positionChanged(position);
        }
        static DateTime lastRendered;
        static async void Render(object a = null)
        {
            RenderOnce();
            //var sleepTime = renderPeriod - (int)(DateTime.Now - lastRendered).TotalMilliseconds;
            //if (sleepTime > 0)
            //    System.Threading.Thread.Sleep(sleepTime);
        }
        static async void RenderOnce()
        {
            try
            {
                lastRendered = DateTime.Now;
                Console.WriteLine("Buffered ms:" + bufor.BufferedDuration.TotalMilliseconds+" RP " + renderPeriod);
                if (bufor.BufferedDuration.TotalMilliseconds < renderPeriod * 2)
                {
                    var renderLength = (((float)renderPeriod * 2 - bufor.BufferedDuration.TotalMilliseconds) *
                                        Project.current.tempo / 60f) / 1000f;
                    if (renderLength < 0)
                        renderLength = 0;
                    liveRenderingNow = true;
                    var rendering = new Rendering() { renderingStart = position, renderingLength = (float)renderLength, project = Project.current, type = RenderngType.live };
                    var soundReady = rendering.project.lines[0].getByRendering(rendering);
                    rendering.project.Render(rendering);
                    var sound = await soundReady;
                    ReturnedSound(sound);
                    rendering.project.Clear(rendering);
                    if (status == Status.playing)
                    {
                        position += (float)renderLength;
                        positionChanged?.Invoke(position);
                    }
                    ThreadPool.QueueUserWorkItem(o => RenderOnce());
                }
                
            }
            catch
            {
                ThreadPool.QueueUserWorkItem(o => RenderOnce());

            }
        }
        static public void ReturnedSound(float[,] sound)
        {
            float minL = sound[0, 0];
            float minR = sound[1, 0];
            float maxL = sound[0, 0];
            float maxR = sound[1, 0];
            var data = new byte[sound.Length * 2];
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

                var left = (long)(sound[0, i] * 0x8000);
                var right = (long)(sound[1, i] * 0x8000);
                if (left > 0x7fff)
                    left = 0x7fff;
                if (left < -0x8000)
                    left = -0x8000;
                if (right > 0x7fff)
                    right = 0x7fff;
                if (right < -0x8000)
                    right = -0x8000;
                data[i * 4] = (byte)(left);
                data[i * 4 + 1] = (byte)((ushort)left >> 8);
                data[i * 4 + 2] = (byte)(right);
                data[i * 4 + 3] = (byte)((ushort)right >> 8);
            }
            bufor.AddSamples(data, 0, sound.Length * 2);
            liveRenderingNow = false;
            minL = Math.Abs(minL);
            maxL = Math.Abs(maxL);
            minR = Math.Abs(minR);
            maxR = Math.Abs(maxR);
            LastVolume[0] = minL > maxL ? minL : maxL;
            LastVolume[1] = minR > maxR ? minR : maxR;
            SoundPlayed?.Invoke(sound);
        }
    }
}
