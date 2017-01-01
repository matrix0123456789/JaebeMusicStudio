using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class NonlinearDistortion : Effect
    {
        NonlinearDistortionType effectType = NonlinearDistortionType.Power;
        float powerExponentiation = 2;
        public float PowerExponentiation { get { return powerExponentiation; } set { powerExponentiation = value; } }

        public NonlinearDistortion()
        {
        }

        public NonlinearDistortion(XmlElement x)
        {
            if (x.Attributes["exponentiation"] != null)
                powerExponentiation = float.Parse(x.Attributes["exponentiation"].Value, System.Globalization.CultureInfo.InvariantCulture);
        }
        public void CleanMemory()
        {
        }

        public float[,] DoFilter(float[,] input)
        {
            switch (effectType)
            {
                case NonlinearDistortionType.Power:
                    return DoPower(input);
            }
            throw new Exception();
        }

        private float[,] DoPower(float[,] input)
        {
            var newSound = new float[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    var old = input[i, j];
                    if (old > 0)
                        newSound[i, j] = (float)Math.Pow(old, PowerExponentiation);
                    else
                        newSound[i, j] = (float)-Math.Pow(-old, PowerExponentiation);
                }
            }
            return newSound;
        }
    }
    enum NonlinearDistortionType { Power }
}
