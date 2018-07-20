using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JaebeMusicStudio.Sound
{
    public class MidiInput : ILiveInput
    {
        public INoteSynth Synth { get; set; }
        Dictionary<int, Note> pressedNotes = new Dictionary<int, Note>();
        private double curentPositon = 0;
        public NotesCollection Items { get; set; }
        static Dictionary<int, MidiInput> singletons = new Dictionary<int, MidiInput>();
        private MidiIn midiIn;

        private MidiInput(int deviceId)
        {
            Items = new NotesCollection();
            Project.live.Add(this);
            midiIn = new MidiIn(deviceId); // default device
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.Start();
        }

        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            switch (e.MidiEvent.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                case MidiCommandCode.NoteOff:
                    var eventObj = e.MidiEvent as NoteEvent;
                    var eventObjOn = e.MidiEvent as NoteEvent;
                    var pitch = eventObj.NoteNumber;
                    if (eventObjOn != null && eventObj.Velocity > 0)
                    {
                        if (!pressedNotes.ContainsKey(pitch))
                        {
                            var newNote = new Note() { Offset = (float)curentPositon, Length = float.MaxValue, Pitch = pitch, Volume = (float)eventObj.Velocity / 127 };
                            pressedNotes.Add(pitch, newNote);
                            Items.Add(newNote);
                        }
                    }
                    else
                    {
                        if (pressedNotes.ContainsKey(pitch))
                        {
                            var endingNote = pressedNotes[pitch];
                            pressedNotes.Remove(pitch);
                            endingNote.Length = (float)(curentPositon - endingNote.Offset);
                        }
                    }
                    break;

                default:
                    Console.WriteLine(e.MidiEvent.ToString());
                    break;
            }
        }

        static public List<MidiInput> getAllObject()
        {
            var ret = new List<MidiInput>();
            for (var i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                ret.Add(getObject(i));
            }
            return ret;
        }
        static public MidiInput getObject(int deviceId)
        {
            if (singletons.ContainsKey(deviceId))
            {
                return singletons[deviceId];
            }
            else
            {
                var obj = new MidiInput(deviceId);
                singletons[deviceId] = obj;
                return obj;
            }
        }
        public void KeyDown(KeyEventArgs e)
        {
            //if (!pressedNotes.ContainsKey(e.Key))
            //{
            //    var pitch = getPitchBykey(e.Key);
            //    if (pitch.HasValue)
            //    {
            //        var newNote = new Note() { Offset = (float)curentPositon, Length = float.MaxValue, Pitch = pitch.Value };
            //        pressedNotes.Add(e.Key, newNote);
            //        Items.Add(newNote);
            //    }
            //}
        }

        internal void KeyUp(KeyEventArgs e)
        {
            //if (pressedNotes.ContainsKey(e.Key))
            //{
            //    var endingNote = pressedNotes[e.Key];
            //    pressedNotes.Remove(e.Key);
            //    endingNote.Length = (float)(curentPositon - endingNote.Offset);
            //}


        }
        public float[,] GetSound(float start, float length, Rendering rendering)
        {
            var ret = Synth.GetSound((float)curentPositon, length, rendering, Items);
            curentPositon += length;
            return ret;
        }
    }
}
