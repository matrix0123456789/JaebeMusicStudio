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
        private float volume = 1;
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                Changed?.Invoke(this);
            }
        }
        public float[] LastVolume = { 0, 0 };
        public event Action<SoundLineAbstract> Changed;
        public void ConnectUI()
        {
            connectedUIs++;
        }
        public void DisconnectUI()
        {
            connectedUIs--;
        }
        protected int connectedUIs;

        public abstract Task<float[,]> Render(Rendering rendering);
    }
}
