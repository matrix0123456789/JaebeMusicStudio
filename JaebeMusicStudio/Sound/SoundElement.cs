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
    public interface ISoundElement : INamedElement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        SoundSample GetSound(float start, float length, Rendering rendering);
        float Offset { get; set; }
        float Length { get; set; }
        SoundLine SoundLine { get; }
        string Title { get; }

        void Serialize(XmlNode node);
        event Action<ISoundElement> positionChanged;

        ISoundElement Duplicate();
    }

    public interface ISoundElementDirectOutput
    {

        SoundLine SoundLine { get; set; }
    }
}
