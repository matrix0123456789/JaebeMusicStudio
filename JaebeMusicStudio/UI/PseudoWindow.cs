using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JaebeMusicStudio.UI
{
    class PseudoWindow : Window
    {
        static List<PseudoWindow> allOpened = new List<PseudoWindow>();
        public PseudoWindow(object page)
        {
            allOpened.Add(this);
            Content = page;
            Show();
        }
        static public void closeAll()
        {
            foreach (var window in allOpened)
            {
                window.Dispatcher.Invoke(()=>{
                    window.Close();
                });
            }
            allOpened.Clear();
        }

        static public void OpenWindow(Func<object> method)
        {
            Thread viewerThread = new Thread(delegate ()
            {
                var okno = new UI.PseudoWindow(method());
                System.Windows.Threading.Dispatcher.Run();
            });
            viewerThread.Name = "PseudoWindow Thread";
            viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            viewerThread.Start();
        }

    }
}
