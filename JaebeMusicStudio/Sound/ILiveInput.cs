using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    interface ILiveInput
    {
        INoteSynth Synth { get; set; }
        SoundSample GetSound(float start, float length, Rendering rendering);
        event Action PressedNotesChanged;
        IEnumerable<Note> PressedNotes { get; }
        event Action ControllsChanged;
        Dictionary<int,int> Controlls { get; }
    }
}
