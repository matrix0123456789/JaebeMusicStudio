using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JaebeMusicStudio.Sound
{
    class KeyboardInput : ILiveInput
    {
        public INoteSynth Synth { get; set; }
        public static KeyboardInput singleton = new KeyboardInput();
        public NotesCollection Items { get; set; }
        Dictionary<Key, Note> pressedNotes = new Dictionary<Key, Note>();
        private double curentPositon = 0;

        public KeyboardInput()
        {
            Items = new NotesCollection();
            Project.live.Add(this);
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (!pressedNotes.ContainsKey(e.Key))
            {
                var pitch = getPitchBykey(e.Key);
                var newNote = new Note() {Offset = (float) curentPositon, Length = float.MaxValue, Pitch = pitch};
                pressedNotes.Add(e.Key, newNote);
                Items.Add(newNote);
            }
        }

        internal void KeyUp(KeyEventArgs e)
        {
            if (pressedNotes.ContainsKey(e.Key))
            {
                var endingNote = pressedNotes[e.Key];
                pressedNotes.Remove(e.Key);
                endingNote.Length = (float) (curentPositon - endingNote.Offset);
            }


        }

        public float[,] GetSound(float start, float length)
        {
            var ret= Synth.GetSound((float)curentPositon, length, Items);
            curentPositon += length;
            return ret;
        }

        int getPitchBykey(Key k)
        {
            switch (k)
            {
                case Key.Z:
                    return 60;

                case Key.S:
                    return 61;

                case Key.X:
                    return 62;

                case Key.D:
                    return 63;

                case Key.C:
                    return 64;

                case Key.V:
                    return 65;

                case Key.G:
                    return 66;

            }
            return -1;
        }
    }
}
