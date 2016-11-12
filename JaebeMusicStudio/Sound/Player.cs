﻿using NAudio.CoreAudioApi;
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
        static System.Threading.Thread renderingThread;
        public enum Status { fileRendering, playing, paused }
        public static BufferedWaveProvider bufor = new BufferedWaveProvider(new WaveFormat((int)Sound.Project.current.sampleRate, 2));
        public static WasapiOut WasapiWyjście = new WasapiOut(AudioClientShareMode.Shared, false,10);
        static Player()
        {
            WasapiWyjście.Init(bufor);
            
        }
        public static void play()
        {
            if (status == Status.paused)
            {
                status = Status.playing;
                renderingThread = new System.Threading.Thread(render);
                renderingThread.Name = "renderingThread";
                renderingThread.Start();
                WasapiWyjście.Play();
            }
        }
        public static void pause()
        {
            if (status == Status.playing)
            {
                status = Status.paused;
                renderingThread.Abort();
                WasapiWyjście.Pause();
            }
        }
        public static void setPosition(float position)
        {
            Player.position = position;
        }
        static DateTime lastRendered;
        static void render(object a = null)
        {
            while (true)
            {
                lastRendered = DateTime.Now;
                if (!rendering && bufor.BufferedDuration.TotalMilliseconds < renderPeriod * 2)
                {
                    rendering = true;
                    var renderLength = (((float)renderPeriod * 2 - bufor.BufferedDuration.TotalMilliseconds) * Project.current.tempo / 60f) / 1000f;
                    Project.current.render(position, (float)renderLength);
                    position += (float)renderLength;
                }
                var sleepTime = renderPeriod - (int)(DateTime.Now - lastRendered).TotalMilliseconds;
                if (sleepTime > 0)
                    System.Threading.Thread.Sleep(sleepTime);
            }
        }
        static public void returnedSound(float[,] sound)
        {
            var data = new byte[sound.Length * 2];
            for (var i = 0; i < sound.GetLength(1); i++)
            {
                data[i * 4 + 1] = (byte)(sound[0, i] * 127);
                data[i * 4 + 3] = (byte)(sound[1, i] * 127);
            }
            bufor.AddSamples(data, 0, sound.Length * 2);
            rendering = false;
        }
    }
}
