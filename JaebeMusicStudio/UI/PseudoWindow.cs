using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JaebeMusicStudio.UI
{
    class PseudoWindow:Window
    {
        public PseudoWindow(object page)
        {
            Content = page;
            Show();
        }
    }
}
