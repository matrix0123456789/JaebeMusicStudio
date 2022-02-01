using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public interface Effect
    {
        bool IsActive { get; set; }
        float[,] DoFilter(float[,] input, Rendering renderind);
        void CleanMemory();
        void Serialize(XmlNode node);
    }
}
