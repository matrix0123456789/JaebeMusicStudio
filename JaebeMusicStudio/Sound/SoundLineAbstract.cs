using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
   abstract public class SoundLineAbstract
    {
        public List<SoundLineConnection> outputs = new List<SoundLineConnection>();
        public float volume = 1;
        public float[] LastVolume = { 0, 0 };
        public void ConnectUI()
        {
            connectedUIs++;
        }
        public void DisconnectUI()
        {
            connectedUIs--;
        }
        protected int connectedUIs;
    }
}
