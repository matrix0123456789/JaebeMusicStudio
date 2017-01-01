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
        float[,] DoFilter(float[,] input);
        void CleanMemory();
        void Serialize(XmlNode node);
    }
}
