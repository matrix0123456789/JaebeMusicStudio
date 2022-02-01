using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class Compresor : Effect
    {
        public bool IsActive { get; set; } = true;
        float multiplerNow = 1;

        public void CleanMemory()
        {
        }

        public float[,] DoFilter(float[,] input, Rendering renderind)
        {
            var length = input.GetLength(1);
            var output = new float[2, length];
            for(var i=0;i< length; i++)
            {
                if(input[0, i]> multiplerNow|| input[0, i] < -multiplerNow)
                {
                    multiplerNow = Math.Abs(input[0, i]);
                }
                else
                {
                    multiplerNow *= 0.99f;
                    if (multiplerNow < 1)
                        multiplerNow = 1;
                }
                output[0, i] = input[0, i] * multiplerNow;
            }
            return output;
        }

        public void Serialize(XmlNode node)
        {
        }
    }
}
