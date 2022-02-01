using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    public class Rendering
    {
        internal float renderingStart;
        internal float renderingLength;
        internal Project project;
        public bool canHarvest = false;//all tasks to render was created
        public RenderngType type;
        internal Dictionary<SoundLineAbstract, TaskCompletionSource<float[,]>> soundLinesRenderings;
        public readonly DateTime started = DateTime.Now;

        public int frequency { get; internal set; }
        public IEnumerable<ISoundElement> soundElements { get; internal set; }
        internal IEnumerable<ILiveInput> liveInputs { get; set; }


        public float CountSamples(float input)
        {
            return input / project.tempo * 60f * project.sampleRate;
        }
        public float SamplesToBeats(float input)
        {
            return input * project.tempo / 60f / project.sampleRate;
        }
    }
    public enum RenderngType { live, block };
}
