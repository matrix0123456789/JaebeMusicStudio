using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public abstract class FilterIIR : Effect
    {
        public bool IsActive { get; set; } = true;
        protected float volume;
        protected float[] a;
        protected float[] b;
        protected int level;
        private float[,] inputHistory;
        private float[,] outputHistory;
        public void CleanMemory()
        {
            inputHistory = new float[2, level];
            outputHistory = new float[2, level];
        }

        public virtual float[,] DoFilter(float[,] input, Rendering rendering)
        {
            lock (this)
            {
                var antiVolume = 1 - volume;
                for (var i = 0; i < input.GetLength(1); i++)
                {
                    for (var j = level - 1; j >= 1; j--)
                    {
                        inputHistory[0, j] = inputHistory[0, j - 1];
                        inputHistory[1, j] = inputHistory[1, j - 1];
                    }
                    inputHistory[0, 0] = input[0, i];
                    inputHistory[1, 0] = input[1, i];
                    float newOutputLeft = 0, newOutputRight = 0;
                    for (var j = 0; j < level; j++)
                    {
                        newOutputLeft += inputHistory[0, j] * a[j];
                        newOutputLeft -= outputHistory[0, j] * b[j];
                        newOutputRight += inputHistory[1, j] * a[j];
                        newOutputRight -= outputHistory[1, j] * b[j];
                    }
                    if (newOutputRight > 1)
                        newOutputRight = 1;
                    if (newOutputRight < -1)
                        newOutputRight = -1;
                    input[0, i] = input[0, i] * volume + newOutputLeft * antiVolume;
                    input[1, i] = input[1, i] * volume + newOutputRight * antiVolume;
                    for (var j = level - 1; j >= 1; j--)
                    {
                        outputHistory[0, j] = outputHistory[0, j - 1];
                        outputHistory[1, j] = outputHistory[1, j - 1];
                    }
                    outputHistory[0, 0] = newOutputLeft;
                    outputHistory[1, 0] = newOutputRight;
                }
            }
            return input;
        }

        public abstract void Serialize(XmlNode node);
    }
}
