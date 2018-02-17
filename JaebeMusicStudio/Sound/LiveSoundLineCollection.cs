using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    class LiveSoundLineCollection
    {

         Dictionary<int, LiveSoundLine> activeLines = new Dictionary<int, LiveSoundLine>();
        public  List<LiveSoundLine> getAvaibleInputs()
        {
            var ret = new List<LiveSoundLine>();
            for (var i = 0; i < WaveIn.DeviceCount; i++)
            {
                ret.Add(GetByDeviceID(i));
            }
            return ret;
        }

        public LiveSoundLine GetByDeviceID(int deviceID)
        {
            if (activeLines.ContainsKey(deviceID))
            {
                return activeLines[deviceID];
            }
            else
            {
                return activeLines[deviceID] = new LiveSoundLine(deviceID);
            }
        }

        internal void stop()
        {
            foreach(var line in activeLines.Values)
            {
                line.Stop();
            }
        }
    }
}
