using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    public class LiveSoundLine: SoundLineAbstract
    {
        static Dictionary<int, LiveSoundLine> activeLines = new Dictionary<int, LiveSoundLine>();
        private WaveInCapabilities Capabilities;

        public static List<LiveSoundLine> getAvaibleInputs()
        {
            var ret = new List<LiveSoundLine>();
            for (var i = 0; i < WaveIn.DeviceCount; i++)
            {
                ret.Add(GetByDeviceID(i));
            }
            return ret;
        }

        private static LiveSoundLine GetByDeviceID(int deviceID)
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

        private LiveSoundLine(int DeviceID)
        {
            Capabilities = WaveIn.GetCapabilities(DeviceID);
          var wave=  new WaveIn(WaveCallbackInfo.FunctionCallback());
            wave.DataAvailable += Wave_DataAvailable;
        }

        private void Wave_DataAvailable(object sender, WaveInEventArgs e)
        {
            
        }
    }
}
