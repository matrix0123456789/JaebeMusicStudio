using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    /// <summary>
    /// Class contains whole project
    /// </summary>
    class Project
    {
        float _tempo = 120;
        public float tempo { get { return tempo; } }
        public static Project current = null;
        List<Track> tracks = new List<Track>();
        /// <summary>
        /// Event: new track was added. First parameter is index of new track;
        /// </summary>
        public event Action<int, Track> trackAdded;
        public void addEmptyTrack()
        {
            var number = tracks.Count;
            var newTrack = new Track();
            tracks.Add(newTrack);
            if (trackAdded != null)
                trackAdded(number, newTrack);
        }
    }
}
