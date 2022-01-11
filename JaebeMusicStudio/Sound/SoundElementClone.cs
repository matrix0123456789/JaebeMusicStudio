﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class SoundElementClone : ISoundElement
    {
        private ISoundElement original;
        private string oryginalName;
        private float offset;
        public string Title
        {
            get
            {
                return original.Title + " - clone";
            }
        }
        public ISoundElement Oryginal => original;
        public SoundElementClone(ISoundElement soundElement)
        {
            if (soundElement is SoundElementClone)
            {
                this.original = (soundElement as SoundElementClone).original;
            }
            else
            {
                this.original = soundElement;
            }
        }
        public SoundElementClone(XmlNode element)
        {
            oryginalName = element.Attributes["original"].Value;
                        offset = float.Parse(element.Attributes["offset"].Value, CultureInfo.InvariantCulture);
            Project.current.loadEnd += Current_loadEnd;
        }

        private void Current_loadEnd()
        {
            this.original = Project.current[oryginalName] as ISoundElement;
            if (this?.original == null)
            {
                throw new Exception("Bad File");
            }
        }

        public float Length { get { return original.Length; } set { } }
        public float Offset { get { return offset; } set { offset = value; positionChanged?.Invoke(this); } }

        public SoundLine SoundLine
        {
            get
            {
                return original.SoundLine;
            }
        }

        public event Action<ISoundElement> positionChanged;

        public ISoundElement Duplicate()
        {
            var newElem = original.Duplicate();
            newElem.Offset = offset;
            return newElem;
        }

        public float[,] GetSound(float start, float length, Rendering rendering)
        {
            return original.GetSound(start, length, rendering);
        }

        public void Serialize(XmlNode node)
        {
            var node2 = node.OwnerDocument.CreateElement("SoundElementClone");
            node2.SetAttribute("offset", Offset.ToString(CultureInfo.InvariantCulture));
            node2.SetAttribute("original", original.Name);
            node.AppendChild(node2);
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
