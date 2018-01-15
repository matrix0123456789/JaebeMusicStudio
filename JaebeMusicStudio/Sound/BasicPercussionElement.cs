using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class BasicPercussionElement
    {
        public float SoundLength {get;set;}
        public BasicPercussionElement(XmlNode ch)
        {
            SoundLength = .1f;
        }
        public float[,] GetSound(float start, float length, Note note)
        {
            long samples = (long)Project.current.CountSamples(length); //how many samples you need on output
            float samplesTotal = Project.current.CountSamples(SoundLength);
            var timeWaited = Project.current.CountSamples(start);
            var ret = new float[2, samples]; //sound that will be returned
            for(var i=0;i< samples; i++)
            {
                ret[0, i] = (i % 256) / 256;
            }
            return ret;
        }

        public void Serialize(XmlNode node, IEnumerable<int> pitches)
        {
            var node2 = node.OwnerDocument.CreateElement("BasicPercussionElement");
            node2.SetAttribute("pitches", string.Join(",", pitches.Select(x => x.ToString())));
            node.AppendChild(node2);
        }
    }
}
