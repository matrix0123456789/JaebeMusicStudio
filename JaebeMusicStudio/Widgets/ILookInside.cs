using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JaebeMusicStudio.Sound;

namespace JaebeMusicStudio.Widgets
{
    public interface ILookInside
    {
        ISoundElement Element { get; }
    }
}
