using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class SimpleFilter : FilterIIR
    {
        private float frequency;
        private float resonation;
        private FilterType type;
        public float Frequency { get { return frequency; } set { frequency = value; recalcFilter(); } }
        public float Resonation { get { return resonation; } set { resonation = value; recalcFilter(); } }
        public FilterType Type { get { return type; } set { type = value; recalcFilter(); } }
        private void recalcFilter()
        {
            switch (type)
            {
                case FilterType.Lowpass:
                    {
                        var c = 1.0f / (float)Math.Tan(Math.PI * frequency / Project.current.sampleRate);
                        
                        level = 3;
                        a = new float[3];
                        b = new float[3];
                        a[0] = 1.0f / (1.0f + resonation * c + c * c);
                        a[1] = 2 * a[0];
                        a[2] = a[0];
                        b[0] = 2.0f * (1.0f - c * c) * a[0];
                        b[1] = (1.0f - resonation * c + c * c) * a[0];
                        break;
                    }
                case FilterType.Highpass:
                    {
                        var c = (float)Math.Tan(Math.PI * frequency / Project.current.sampleRate);
                        level = 3;
                        a = new float[3];
                        b = new float[3];
                        a[0] = 1.0f / (1.0f + resonation * c + c * c);
                        a[1] = -2 * a[0];
                        a[2] = a[0];
                        b[0] = 2.0f * (c * c - 1.0f) * a[0];
                        b[1] = (1.0f - resonation * c + c * c) * a[0];
                        break;
                    }
            }
            CleanMemory();
        }

        public SimpleFilter(XmlNode node)
        {
            frequency = float.Parse(node.Attributes["Frequency"].Value, CultureInfo.InvariantCulture);
            resonation = float.Parse(node.Attributes["Resonation"].Value,CultureInfo.InvariantCulture);
            type = (FilterType)Enum.Parse(typeof(FilterType), node.Attributes["Type"].Value);
            if (node.Attributes["isActive"] != null)
                IsActive = bool.Parse(node.Attributes["isActive"].Value);
            recalcFilter();
        }

        public SimpleFilter()
        {
            frequency = 200;
            resonation = (float)Math.Sqrt(2);
            type = FilterType.Lowpass;
            recalcFilter();
        }

        public override void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("SimpleFilter");
            node2.SetAttribute("Frequency", Frequency.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("Resonation", Resonation.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("Type", Type.ToString());
            node2.SetAttribute("isActive", IsActive.ToString(CultureInfo.InvariantCulture));
            node.AppendChild(node2);
        }
        public enum FilterType
        {
            Lowpass, Highpass
        }
    }
}
