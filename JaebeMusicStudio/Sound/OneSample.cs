using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    class OneSample : SoundElement
    {
        SampledSound sample;
        public OneSample(SampledSound sample)
        {
            this.sample = sample;
        }
        public float offset
        {
            get; set;
        }

        public float[,] getSound(float start, float length)
        {
            long samples = (long)Project.current.countSamples(length);
            var ret = new float[sample.channels, samples];
            return ret;
        }
    }
}
