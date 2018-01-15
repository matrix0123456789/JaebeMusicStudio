using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    public class Notes : ISoundElement
    {
        public INoteSynth Sound { get; set; }
        public NotesCollection Items { get; set; }

        private float length, offset;

        public Notes()
        {
            Items = new NotesCollection();
            Length = Items.CalcLength();
            Items.CollectionChanged += NotesChanged;
        }
        public Notes(XmlNode element)
        {
            Items = new NotesCollection();
            length = float.Parse(element.Attributes["length"].Value, CultureInfo.InvariantCulture);
            offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            if (element.Attributes["name"] != null)
                Name = element.Attributes["name"].Value;
            if (element.Attributes["sound"].Value != "")
            {
                Sound = Project.current[element.Attributes["sound"].Value] as INoteSynth;
            }
            foreach (XmlNode child in element.ChildNodes)
            {
                if ((child as XmlElement)?.Name == "Note")
                {
                    Items.Add(new Note(child as XmlElement));
                }
            }
            Items.CollectionChanged += NotesChanged;
        }

        private void NotesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Length = Math.Max(Length, Items.CalcLength());
        }

        public float Length { get { return length; } set { length = value; positionChanged?.Invoke(this); } }
        public float Offset { get { return offset; } set { offset = value; positionChanged?.Invoke(this); } }
        public SoundLine SoundLine => Sound?.SoundLine;

        public event Action<ISoundElement> positionChanged;

        public float[,] GetSound(float start, float length)
        {
            return Sound.GetSound(start, length, Items);
        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("Notes");
            node2.SetAttribute("offset", Offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("length", Length.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("name", Name);
            node2.SetAttribute("sound", Sound == null ? "" : Sound.Name);
            foreach (Note item in Items)
            {
                item.Serialize(node2);
            }
            node.AppendChild(node2);
        }

        public ISoundElement Duplicate()
        {
            var newElem = this.MemberwiseClone() as Notes;

            return newElem;
        }
        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    Project.current.generateNamedElement(this);
                return name;
            }
            set
            {
                Project.current[value] = this;
                name = value;
            }
        }
    }
}
