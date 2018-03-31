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

        float[,] GetSound(float start, float length, Rendering rendering);
        
    }
}
