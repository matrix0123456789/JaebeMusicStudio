using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IEnumerable<Note> PressedNotes => pressedNotes.Values;
        private double curentPositon = 0;
        public NotesCollection Items { get; set; }

        public bool ConstantVolume = false;

        static Dictionary<int, MidiInput> singletons = new Dictionary<int, MidiInput>();
        private MidiIn midiIn;
        private MidiInCapabilities deviceInfo;
        public Dictionary<int, int> Controlls { get; } = new Dictionary<int, int>();

        public event Action PressedNotesChanged;
        public event Action ControllsChanged;

        private MidiInput(int deviceId)
        {
            Items = new NotesCollection();
            Project.live.Add(this);
            midiIn = new MidiIn(deviceId); // default device
            deviceInfo = MidiIn.DeviceInfo(deviceId);
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.Start();
        }

        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            lock (this)
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
                                var newNote = new Note() { Offset = (float)curentPositon, Length = float.MaxValue, Pitch = pitch, Volume = ConstantVolume ? 1 : ((float)eventObj.Velocity / 127) };
                                pressedNotes.Add(pitch, newNote);
                                Items.Add(newNote);
                                PressedNotesChanged?.Invoke();
                            }
                        }
                        else
                        {
                            if (pressedNotes.ContainsKey(pitch))
                            {
                                var endingNote = pressedNotes[pitch];
                                pressedNotes.Remove(pitch);
                                endingNote.Length = (float)(curentPositon - endingNote.Offset);
                                PressedNotesChanged?.Invoke();
                            }
                        }
                        break;
                    case MidiCommandCode.ControlChange:
                        Controlls[(int)(e.MidiEvent as ControlChangeEvent).Controller] = (e.MidiEvent as ControlChangeEvent).ControllerValue;
                        ControllsChanged?.Invoke();
                        break;
                    default:
                        Debug.WriteLine(e.MidiEvent.ToString());
                        break;
                }
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
        public override string ToString()
        {
            return deviceInfo.ProductName ?? "";
        }
    }
}
