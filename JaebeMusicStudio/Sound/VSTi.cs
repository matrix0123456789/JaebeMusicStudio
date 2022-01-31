using JaebeMusicStudio.Exceptions;
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
        private List<Note> startedNotes = new List<Note>();
        private List<Note> endedNotes = new List<Note>();
        private float lastPlayedStart;

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
            Name = xml.Attributes["name"].Value;
            if (xml.Attributes["soundLine"] != null)
            {
                var number = uint.Parse(xml.Attributes["soundLine"].Value);
                if (number >= Project.current.lines.Count)
                    throw new BadFileException();
                SoundLine = Project.current.lines[(int)number];
            }
            else
                SoundLine = Project.current.lines[0];

            filename = xml.Attributes["filename"].Value;
            startProcess();
        }
        public VSTi(string filename)
        {
            this.filename = filename;
            startProcess();
        }
        private void startProcess()
        {
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

        public SoundSample GetSound(float start, float length,Rendering rendering, NotesCollection notes)
        {
            if (lastPlayedStart > start)
            {
                endedNotes.Clear();
            }
            lastPlayedStart = start;

            int samples = (int)Project.current.CountSamples(length);//how many samples you need on output

            Note[] startNotes, endNotes;
            List<float> timeBreaks;
            //lock (notes)
            // {
            //todo dokładna dłuość nuty
            startNotes = notes.Where(note => (note.Offset < start + length && !startedNotes.Contains(note) && !endedNotes.Contains(note))).ToArray();
            endNotes = notes.Where(note => (note.Offset + note.Length < start + length && !endedNotes.Contains(note))).ToArray();
            startNotes = notes.Where(note => (note.Offset < start + length)).ToArray();
            endNotes = notes.Where(note => (note.Offset + note.Length < start + length)).ToArray();
            timeBreaks = new List<float> { start, start + length };

            foreach (var x in startedNotes)
            {
                if (!timeBreaks.Contains(x.Offset))
                {
                    timeBreaks.Add(x.Offset);
                }
            }
            foreach (var x in endNotes)
            {
                if (!timeBreaks.Contains(x.Offset + x.Length))
                {
                    timeBreaks.Add(x.Offset + x.Length);
                }
            }
            //}
            timeBreaks.Sort();
            int sumSamples = 0;
            for (var i = 0; i < timeBreaks.Count - 1; i++)
            {
                //todo temporary
                //writer.Write((int)JmsVstHost.Commands.GetSoundNoteSynth);
                var nowSamples = (int)Project.current.CountSamples(timeBreaks[i + 1] - start);
                if (i == timeBreaks.Count - 2)
                    nowSamples = samples;//to provide errors of float incorrection
                if (nowSamples - sumSamples <= 0)
                    nowSamples = sumSamples + 1;//todo niebezpieczeńswo ze przekroczymy wartość samples
                writer.Write(nowSamples - sumSamples);
                sumSamples = nowSamples;
                //writer.Write(0);
                var startNotes2 = startNotes.Where(note => (note.Offset < timeBreaks[i + 1] && !startedNotes.Contains(note) && !endedNotes.Contains(note))).ToArray();
                var endNotes2 = endNotes.Where(note => (note.Offset + note.Length < timeBreaks[i + 1] && !endedNotes.Contains(note))).ToArray();
                writer.Write((int)startNotes2.Count() + (int)endNotes2.Count());
                foreach (var note in startNotes2)
                {
                    SendNote(note);
                    startedNotes.Add(note);
                }
                foreach (var note in endNotes2)
                {
                    SendEndNote(note);
                    startedNotes.Remove(note);
                    endedNotes.Add(note);
                }
            }


            writer.Flush();
            var ret = new float[2, samples];//sound that will be returned

            var reader = new BinaryReader(process.StandardOutput.BaseStream);
            Debug.WriteLine("startRching " + samples);
            for (var i = 0; i < samples; i++)
            {
                ret[0, i] = reader.ReadSingle();
                ret[1, i] = reader.ReadSingle();
            }
            Debug.WriteLine("endRching " + samples);

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
            var node2 = node.OwnerDocument.CreateElement("VSTi");
            node2.SetAttribute("name", name);
            node2.SetAttribute("soundLine", Project.current.lines.IndexOf(SoundLine).ToString());
            node2.SetAttribute("filename", filename);
            node.AppendChild(node2);
        }
        public void ShowWindow()
        {
            //todo temporary
        //    writer.Write((int)JmsVstHost.Commands.ShowWindow);
        //    writer.Flush();
        }
    }
}
