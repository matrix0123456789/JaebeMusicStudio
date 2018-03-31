using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public interface INoteSynth: INamedElement
    {
        SoundLine SoundLine { get; set; }
        float[,] GetSound(float start, float length,Rendering rendering, NotesCollection notes);
        void Serialize(XmlNode node);
    }
}
