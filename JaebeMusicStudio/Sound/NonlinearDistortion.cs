using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class NonlinearDistortion : Effect
    {
        public bool IsActive { get; set; } = true;
        NonlinearDistortionType effectType = NonlinearDistortionType.Power;
        public NonlinearDistortionType EffectType { get { return effectType; } set { effectType = value; } }
        float powerExponentiation = 2;
        public float PowerExponentiation { get { return powerExponentiation; } set { powerExponentiation = value; } }

        public NonlinearDistortion()
        {
        }

        public NonlinearDistortion(XmlElement x)
        {
            if (x.Attributes["exponentiation"] != null)
                powerExponentiation = float.Parse(x.Attributes["exponentiation"].Value, System.Globalization.CultureInfo.InvariantCulture);
            if (x.Attributes["isActive"] != null)
                IsActive = bool.Parse(x.Attributes["isActive"].Value);
        }
        public void CleanMemory()
        {
        }

        public void Serialize(XmlNode node)
        {

            var node2 = node.OwnerDocument.CreateElement("NonlinearDistortion");
            node2.SetAttribute("exponentiation", powerExponentiation.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("isActive", IsActive.ToString(CultureInfo.InvariantCulture));
            node.AppendChild(node2);
        }

        public float[,] DoFilter(float[,] input, Rendering renderind)
        {
            switch (effectType)
            {
                case NonlinearDistortionType.Power:
                    return DoPower(input);
                case NonlinearDistortionType.ArcTan:
                    return DoArcTan(input);
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
        private float[,] DoArcTan(float[,] input)
        {
            var newSound = new float[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    var old = input[i, j];
                        newSound[i, j] = (float)(Math.Atan(old*Math.PI/2) / Math.PI * 2);
                }
            }
            return newSound;
        }
    }
    public enum NonlinearDistortionType { Power, ArcTan }
}
