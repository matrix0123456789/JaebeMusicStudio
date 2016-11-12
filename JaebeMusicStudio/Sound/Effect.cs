using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    interface Effect
    {
        float[,] doFilter(float[,] input);
        void cleanMemory();
    }
}
