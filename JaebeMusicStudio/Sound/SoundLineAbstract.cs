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
        protected Dictionary<Rendering, SoundLineRendering> renderings = new Dictionary<Rendering, SoundLineRendering>();
        public void ConnectUI()
        {
            connectedUIs++;
        }
        public void DisconnectUI()
        {
            connectedUIs--;
        }
        protected int connectedUIs;

        public SoundLineRendering getByRendering(Rendering r)
        {
            lock (renderings)
            {
                if (!renderings.ContainsKey(r))
                {
                    renderings[r] = new SoundLineRendering();
                    prepareToRender(r);
                }
                return renderings[r];
            }
        }
        public void prepareToRender(Rendering rendering)
        {
            var slRend = getByRendering(rendering);
            slRend.data = new float[2, (int)rendering.project.CountSamples(rendering.renderingLength)];
        }
    }
}
