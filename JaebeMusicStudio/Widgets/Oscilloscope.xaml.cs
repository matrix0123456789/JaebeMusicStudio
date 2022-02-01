using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JaebeMusicStudio.Sound;

namespace JaebeMusicStudio.Widgets
{
    /// <summary>
    /// Interaction logic for Oscilloscope.xaml
    /// </summary>
    public partial class Oscilloscope : Page
    {
        private Timer t;


        static float[,] aktualniePrzetwarzane;
        static int pozycja = 0;
        int typX = 2, typY = 1;
        private int bufforred = 0;
        public Queue<float[,]> dane = new Queue<float[,]>();
        float[,] currentFrame, previousFrame;
        bool synchronize = false;

        public Oscilloscope()
        {
            InitializeComponent();
            t = new Timer(Update, null, 0, 16);
            Player.SoundPlayed += Player_SoundPlayed;
        }

        private void Player_SoundPlayed(float[,] sound)
        {
            Task.Run(() =>
                {
                    lock (dane)
                    {
                        bufforred += sound.GetLength(1);

                        dane.Enqueue(sound);
                    }
                });
        }
        private int samplesPerFrame => (int)(48000 / 60.0);//todo tmp
        void UpdateFrame()
        {
            float[,] nextFrame;
            lock (dane)
            {
                while (bufforred > samplesPerFrame)
                {
                    var samplesPerFrame = this.samplesPerFrame;
                    nextFrame = new float[2, samplesPerFrame];

                    //while (bufforred > 2000)
                    //{
                    //    var skipped = dane.Dequeue();
                    //    bufforred -= skipped.GetLength(1);

                    //}
                    if (aktualniePrzetwarzane == null)
                    {
                        if (dane.Count == 0)
                        {
                            return;
                        }
                        aktualniePrzetwarzane = dane.Dequeue();
                        bufforred -= aktualniePrzetwarzane.GetLength(1);
                        pozycja = 0;
                    }

                    for (int i = 0; i < samplesPerFrame; i++)
                    {
                        while (pozycja >= aktualniePrzetwarzane.GetLength(1))
                        {
                            aktualniePrzetwarzane = dane.Dequeue();
                            bufforred -= aktualniePrzetwarzane.GetLength(1);
                            pozycja = 0;
                        }
                        nextFrame[0, i] = aktualniePrzetwarzane[0, pozycja];
                        nextFrame[1, i] = aktualniePrzetwarzane[1, pozycja];
                        pozycja++;
                    }
                    previousFrame = currentFrame;
                    currentFrame = nextFrame;
                }
            }
        }
        private void Update(object state)
        {
            UpdateFrame();
            float[,] previousFrame, currentFrame;
            var samplesPerFrame = this.samplesPerFrame;
            lock (dane)
            {
                previousFrame = this.previousFrame;
                currentFrame = this.currentFrame;
            }
            if (previousFrame == null) return;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)delegate ()
        {
            int goLeft = synchronize ? findTrigger(currentFrame) : 0;
            chart.Points.Clear();

            int przeskokint = (int)(samplesPerFrame / drawArea.ActualWidth);//optymalizacja
            if (przeskokint < 1)
                przeskokint = 1;
            for (int i = 0; i < goLeft; i += przeskokint)
            {
                double X, Y;
                X = drawArea.ActualWidth * i / samplesPerFrame;
                Y = (-previousFrame[0, previousFrame.GetLength(1) - goLeft + i] + 1f) / 2f * drawArea.ActualHeight;
                chart.Points.Add(new Point(X, Y));
            }
            for (int i = 0; i < samplesPerFrame; i += przeskokint)
            {
                double X, Y;
                X = drawArea.ActualWidth * (i + goLeft) / samplesPerFrame;
                Y = (-currentFrame[0, i] + 1f) / 2f * drawArea.ActualHeight;
                chart.Points.Add(new Point(X, Y));
            }

        });
        }
        int findTrigger(float[,] data)
        {
            for (var i = data.GetLength(1) / 2; i > 0; i--)
            {
                if (data[0, i - 1] > 0 && data[0, i] <= 0)
                {
                    return data.GetLength(1) / 2 - i;
                }
            }
            return 0;
        }
    }
}
