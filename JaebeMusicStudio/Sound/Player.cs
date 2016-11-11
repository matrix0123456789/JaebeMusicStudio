using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    static class Player
    {
        static public Status status = Status.paused;
        static public float position;
        static bool rendering = false;
        static int renderPeriod = 10;
        static System.Threading.Timer renderingTimer;
        public enum Status { fileRendering, playing, paused }
        public static BufferedWaveProvider bufor = new BufferedWaveProvider(new WaveFormat((int)Sound.Project.current.sampleRate, 2));
        public static WasapiOut WasapiWyjście = new WasapiOut(AudioClientShareMode.Shared, 10);
        static Player()
        {
            WasapiWyjście.Init(bufor);
            WasapiWyjście.Play();
        }
        public static void play()
        {
            if (status == Status.paused)
            {
                status = Status.playing;
                renderingTimer = new System.Threading.Timer(render, null, 0, renderPeriod);
            }
        }
        public static void pause()
        {
            if (status == Status.playing)
            {
                status = Status.paused;
                renderingTimer.Dispose();
            }
        }

        static void render(object a = null)
        {
            if (!rendering&&bufor.BufferedDuration.TotalMilliseconds < renderPeriod)
            {
                rendering = true;
                var renderLength = (((float)renderPeriod) * Project.current.tempo / 60f - bufor.BufferedDuration.TotalMilliseconds) / 1000f;
                Project.current.render(position, (float)renderLength);
            }
        }
        static public void returnedSound(float[,] sound)
        {
            var data = new byte[sound.Length * 2];
            for(var i=0;i<sound.GetLength(1);i++)
            {
                data[i * 4+1] = (byte)(sound[0, i] * 127);
                data[i * 4+3] = (byte)(sound[1, i] * 127);
            }
            bufor.AddSamples(data, 0, sound.Length * 2);
            rendering = false;
        }
    }
}
