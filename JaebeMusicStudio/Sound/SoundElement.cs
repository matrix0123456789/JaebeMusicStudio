using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    /// <summary>
    /// 
    /// element on track
    /// </summary>
    interface SoundElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        float[,] getSound(float start, float length);
        float offset { get; set; }
    }
}
