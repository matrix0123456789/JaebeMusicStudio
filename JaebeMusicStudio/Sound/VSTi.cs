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
        private string filename;
        private string name;
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
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        public float[,] GetSound(float start, float length, NotesCollection notes)
        {
            var writer = new BinaryWriter(process.StandardInput.BaseStream);
            writer.Write((int)JmsVstHost.Commands.GetSoundNoteSynth);
            int samples = (int)Project.current.CountSamples(length);//how many samples you need on output
            writer.Write((int)samples);
            writer.Flush();
            var ret = new float[2, samples];//sound that will be returned

            var reader = new BinaryReader(process.StandardOutput.BaseStream);

            for(var i = 0;i< samples; i++)
            {
                ret[0, i] = reader.ReadSingle();
                ret[1, i] = reader.ReadSingle();
            }

            return ret;
        }

        public void Serialize(XmlNode node)
        {
            throw new NotImplementedException();
        }
        public void ShowWindow()
        {
            var writer = new BinaryWriter(process.StandardInput.BaseStream);
            writer.Write((int)JmsVstHost.Commands.ShowWindow);
            writer.Flush();
        }
    }
}
