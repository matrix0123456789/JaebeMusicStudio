using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Note
    {
        private float length, offset, pitch, volume = 1;

        public float Length { get { return length; } set { if (length != value) { length = value; Changed?.Invoke(this); } } }
        public float Offset { get { return offset; } set { if (offset != value) { offset = value; Changed?.Invoke(this); } } }
        public float Pitch { get { return pitch; } set { if (pitch != value) { pitch = value; Changed?.Invoke(this); } } }
        public float Volume { get { return volume; } set { if (volume != value) { volume = value; Changed?.Invoke(this); } } }

        public Note()
        {
        }

        public Note(XmlElement element)
        {
            length = float.Parse(element.Attributes["length"].Value, CultureInfo.InvariantCulture);
            offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            pitch = float.Parse(element.Attributes["pitch"].Value, CultureInfo.InvariantCulture);
            try
            {
                volume = float.Parse(element.Attributes["volume"].Value, CultureInfo.InvariantCulture);
            }
            catch { }
        }
        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Note");
            node2.SetAttribute("offset", Offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("length", Length.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("pitch", Pitch.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("volume", Volume.ToString(CultureInfo.InvariantCulture));
            node.AppendChild(node2);
        }

        internal Note Clone()
        {
            return MemberwiseClone() as Note;
        }
        static string[] pitchesNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "H", "A", "A#", "B" };
        static bool[] pitchesBlack = new bool[] { false, true, false, true, false, false, true, false, true, false, true, false };
        public event Action<Note> Changed;

        public static string GetName(int pitch)
        {
            int octave = (pitch - 24) / 12;
            int note = pitch % 12;
            return pitchesNames[note] + octave;
        }

        internal static bool IsPitchBlack(int pitch)
        {
            int note = pitch % 12;
            return pitchesBlack[note];
        }
    }
}
