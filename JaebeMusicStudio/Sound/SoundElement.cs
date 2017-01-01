using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    /// <summary>
    /// 
    /// element on track
    /// </summary>
    interface ISoundElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        float[,] GetSound(float start, float length);
        float Offset { get; set; }
        float Length { get; set; }
        SoundLine SoundLine { get; set; }
        void Serialize(XmlNode node);
        event Action<ISoundElement> positionChanged;
    }
}
