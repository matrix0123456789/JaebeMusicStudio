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
        private void Update(object state)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)delegate ()
            {
                lock (dane)
                {
                    var ilePróbek = (int)(Project.current.sampleRate*0.016);
                    if (bufforred < 0)
                        bufforred = 0;
                    //if (bufforred > 2000)
                    //{
                    //    bufforred -= aktualniePrzetwarzane.GetLength(1) + pozycja;
                    //    pozycja = 0;
                    //    aktualniePrzetwarzane = null;
                    //}
                    while (bufforred>2000)
                    {
                        var skipped = dane.Dequeue();
                        bufforred -= skipped.GetLength(1);

                    }
                    //while ((DateTime.Now - czas).TotalMilliseconds > 1000 / częstotliwość * 10)
                    //{
                    //    if (aktualniePrzetwarzane == null && dane.Count != 0)
                    //    {

                        //        aktualniePrzetwarzane = dane.Dequeue();
                        //        pozycja = 0;
                        //    }
                        //    int dododania = ilePróbek;
                        //    if (aktualniePrzetwarzane != null)
                        //        while (dododania > 0)
                        //        {
                        //            if (aktualniePrzetwarzane.Length / 2 > dododania + pozycja)
                        //            {
                        //                pozycja += dododania;
                        //                break;
                        //            }
                        //            else if (dane.Count > 0)
                        //            {
                        //                dododania -= aktualniePrzetwarzane.Length / 2 - pozycja;
                        //                pozycja = 0;
                        //                aktualniePrzetwarzane = dane.Dequeue();
                        //            }
                        //            else
                        //            {
                        //                break;
                        //            }
                        //        }
                        //}


                    chart.Points.Clear();
                    if (aktualniePrzetwarzane == null)
                    {
                        if (dane.Count == 0)
                        {

                            chart.Points.Add(new Point(0, 0.5 * drawArea.ActualHeight));
                            chart.Points.Add(new Point(drawArea.ActualWidth, 0.5 * drawArea.ActualHeight));
                            return;
                        }
                        aktualniePrzetwarzane = dane.Dequeue();
                        bufforred -= aktualniePrzetwarzane.GetLength(1);
                        pozycja = 0;
                    }
                    int przeskokint = (int)(ilePróbek / drawArea.ActualWidth);
                    if (przeskokint < 1)
                        przeskokint = 1;
                    for (int i = 0; i < ilePróbek; i += przeskokint)
                    {
                        pozycja += przeskokint;
                        if (pozycja >= aktualniePrzetwarzane.Length / 2)
                        {
                            if (dane.Count == 0)
                            {

                                //linia.Points.Add(new Point(wykres.ActualWidth * i / ilePróbek, 0.5 * wykres.ActualHeight));
                                //linia.Points.Add(new Point(wykres.ActualWidth, 0.5 * wykres.ActualHeight));
                                aktualniePrzetwarzane = null;
                                pozycja = 0;
                                //wykres.Children.Add(linia);
                                break;
                            }
                            aktualniePrzetwarzane = dane.Dequeue();
                            bufforred -= aktualniePrzetwarzane.GetLength(1);
                            pozycja = 0;
                        }
                        double X, Y;
                        if (typX == 0)
                        {
                            X = (-aktualniePrzetwarzane[0, pozycja] + 1f) / 2f * drawArea.ActualHeight;
                        }
                        else if (typX == 1)
                        {
                            X = (-aktualniePrzetwarzane[1, pozycja] + 1f) / 2f * drawArea.ActualHeight;
                        }
                        else
                        {
                            X = drawArea.ActualWidth * i / ilePróbek;
                        }
                        if (typY == 0)
                        {
                            Y = (-aktualniePrzetwarzane[0, pozycja] + 1f) / 2f * drawArea.ActualHeight;
                        }
                        else if (typY == 1)
                        {
                            Y = (-aktualniePrzetwarzane[1, pozycja] + 1f) / 2f * drawArea.ActualHeight;
                        }
                        else
                        {
                            Y = drawArea.ActualWidth * i / ilePróbek;
                        }
                        chart.Points.Add(new Point(X, Y));
                    }
                }
            });
        }
    }
}
