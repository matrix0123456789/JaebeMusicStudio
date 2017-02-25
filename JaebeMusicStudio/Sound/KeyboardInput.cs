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
                if (pitch.HasValue)
                {
                    var newNote = new Note() {Offset = (float) curentPositon, Length = float.MaxValue, Pitch = pitch.Value};
                    pressedNotes.Add(e.Key, newNote);
                    Items.Add(newNote);
                }
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

        int? getPitchBykey(Key k)
        {
            switch (k)
            {
                case Key.Z:
                    return 60;

                case Key.S:
                    return 61;

                case Key.X:;
                    return 62;

                case Key.D:
                    return 63;

                case Key.C:
                    return 64;

                case Key.V:
                    return 65;

                case Key.G:
                    return 66;

                case Key.B:
                    return 67;

                case Key.H:
                    return 68;

                case Key.N:
                    return 69;

                case Key.J:
                    return 70;

                case Key.M:
                    return 71;
                case Key.OemComma:
                    return 72;
                case Key.L:
                    return 73;
                case Key.OemPeriod:
                    return 74;
                case Key.Oem1:
                    return 75;
                case Key.Oem2:
                    return 76;
                case Key.Oem7:
                    return 77;
                case Key.RightShift:
                    return 78;
                case Key.Enter:
                    return 79;
                case Key.Q:
                    return 72;
                case Key.D2:
                    return 73;
                case Key.W:
                    return 74;
                case Key.D3:
                    return 75;
                case Key.E:
                    return 76;
                case Key.R:
                    return 77;
                case Key.D5:
                    return 78;
                case Key.T:
                    return 79;
                case Key.D6:
                    return 80;
                case Key.Y:
                    return 81;
                case Key.D7:
                    return 82;
                case Key.U:
                    return 83;
                case Key.I:
                    return 84;
                case Key.D9:
                    return 85;
                case Key.O:
                    return 86;
                case Key.D0:
                    return 87;
                case Key.P:
                    return 88;
                case Key.Oem4:
                    return 89;
                case Key.OemPlus:
                    return 90;
                case Key.Oem6:
                    return 91;


            }
            return null;
        }
    }
}
