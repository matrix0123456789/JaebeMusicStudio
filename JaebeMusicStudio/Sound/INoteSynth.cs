﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    interface INoteSynth: INamedElement
    {
        SoundLine SoundLine { get; set; }
        float[,] GetSound(float start, float length, NotesCollection notes);
        void Serialize(XmlNode node);
    }
}