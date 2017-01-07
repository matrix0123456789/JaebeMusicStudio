using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class BasicSynth:INoteSynth
    {
        private string name;
        public string Name
        {
            get
            {
                if(name==null)
                    Project.current.generateNamedElement(this);
                return name;
            }
            set
            {
                Project.current[value] = this;
                name = value;
            }
        }

        public SoundLine SoundLine { get; set; }

        public BasicSynth(XmlNode element)
        {
            Name = element.Attributes["name"].Value;
        }
        public float[,] GetSound(float start, float length, NotesCollection notes)
        {
            throw new NotImplementedException();
        }

        public void Serialize(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }
}
