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

namespace JaebeMusicStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<Thread> windowsThreads=new List<Thread>();
        public MainWindow()
        {
            InitializeComponent();
            Sound.Project.current = new Sound.Project();
            Thread viewerThread = new Thread(delegate ()
            {
                var okno = new UI.PseudoWindow(new Widgets.Timeline());
                System.Windows.Threading.Dispatcher.Run();
            });
            windowsThreads.Add(viewerThread);
            viewerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            viewerThread.Start();
        }
    }
}
