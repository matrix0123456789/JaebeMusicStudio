using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    class VSTi : INoteSynth
    {
        Process process;
        BinaryWriter writer;
        private string filename;
        private string name;
        private List<Note> startedNotes = new List<Note>(0);
        public string Name
        {
            get
            {
                if (name == null)
                    Project.current.generateNamedElement(this);
                return name;
            }
            set
            {
                Project.current[value] = this;
                name = value;
            }
        }
        public SoundLine SoundLine { get; set; }

        public VSTi(XmlNode xml)
        {
            filename = xml.Attributes["filename"].Value;
            var startInfo = new ProcessStartInfo("JmsVstHost.exe", "JmsVstHost.exe \"" + filename + "\"");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            process = Process.Start(startInfo);
            process.OutputDataReceived += Process_OutputDataReceived;
            writer = new BinaryWriter(process.StandardInput.BaseStream);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        public float[,] GetSound(float start, float length, NotesCollection notes)
        {


            writer.Write((int)JmsVstHost.Commands.GetSoundNoteSynth);
            int samples = (int)Project.current.CountSamples(length);//how many samples you need on output
            writer.Write((int)samples);

            //todo dokładna dłuość nuty
            var startNotes = notes.Where(x => x.Offset >= start && x.Offset < start + length);
            var endNotes = startedNotes.Where(x => x.Offset + x.Length < start || !notes.Contains(x));
            writer.Write((int)startNotes.Count() + (int)endNotes.Count());
            foreach (var note in startNotes)
            {
                SendNote(note);
                startedNotes.Add(note);
            }
            foreach (var note in endNotes)
            {
                SendEndNote(note);
            }

            writer.Flush();
            var ret = new float[2, samples];//sound that will be returned

            var reader = new BinaryReader(process.StandardOutput.BaseStream);

            for (var i = 0; i < samples; i++)
            {
                ret[0, i] = reader.ReadSingle();
                ret[1, i] = reader.ReadSingle();
            }

            return ret;
        }

        private void SendNote(Note note)
        {
            SendEvent(0, 0, 0, new byte[] { 0x90, (byte)note.Pitch, 127, 0 }, 0, 127);
        }
        private void SendEndNote(Note note)
        {
            SendEvent(0, 0, 0, new byte[] { 0x80, (byte)note.Pitch, 0, 0 }, 0, 127);
        }
        private void SendEvent(int deltaFrames, int noteLength, int noteOffset, byte[] midiData, short detune, byte noteOffVelocity)
        {
            writer.Write(deltaFrames);
            writer.Write(noteLength);
            writer.Write(noteOffset);
            writer.Write(midiData[0]);
            writer.Write(midiData[1]);
            writer.Write(midiData[2]);
            writer.Write(midiData[3]);
            writer.Write(detune);
            writer.Write(noteOffVelocity);
        }

        public void Serialize(XmlNode node)
        {
            throw new NotImplementedException();
        }
        public void ShowWindow()
        {
            writer.Write((int)JmsVstHost.Commands.ShowWindow);
            writer.Flush();
        }
    }
}
