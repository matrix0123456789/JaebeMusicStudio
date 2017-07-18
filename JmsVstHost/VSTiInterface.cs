using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jacobi.Vst.Interop.Host;
using System.Diagnostics;
using System.Threading;
using Jacobi.Vst.Core;

namespace JmsVstHost
{
    class VSTiInterface
    {
        VstPluginContext ctx;
        public VSTiInterface(string pluginPath)
        {
            HostCommandStub hostCmdStub = new HostCommandStub();
            hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

            ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

            // add custom data to the context
            ctx.Set("PluginPath", pluginPath);
            ctx.Set("HostCmdStub", hostCmdStub);

            // actually open the plugin itself
            ctx.PluginCommandStub.Open();
        }

        internal void ReadCommand(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var command = (Commands)reader.ReadInt32();
            switch (command)
            {
                case Commands.ShowWindow:
                    ShowWindow();
                    break;
                case Commands.GetSoundNoteSynth:
                    GetSound(reader);
                    break;
            }
        }

        private void GetSound(BinaryReader reader)
        {
            var blockSize = reader.ReadInt32();
            var eventsCount = reader.ReadInt32();

            int inputCount = ctx.PluginInfo.AudioInputCount;
            int outputCount = ctx.PluginInfo.AudioOutputCount;

            VstAudioBufferManager inputMgr = new VstAudioBufferManager(inputCount, blockSize);
            VstAudioBufferManager outputMgr = new VstAudioBufferManager(outputCount, blockSize);

            VstAudioBuffer[] inputBuffers = inputMgr.ToArray();
            VstAudioBuffer[] outputBuffers = outputMgr.ToArray();

            ctx.PluginCommandStub.MainsChanged(true);

            for (var i = 0; i < eventsCount; i++)
            {
                SetEvent(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), new byte[] { reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte() }, reader.ReadInt16(), reader.ReadByte());
            }

            ctx.PluginCommandStub.StartProcess();
            ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);

            var stream = new BinaryWriter(Console.OpenStandardOutput());
            if (outputCount == 1)
            {
                for (var i = 0; i < blockSize; i++)
                {
                    stream.Write(outputBuffers[0][i]);
                    stream.Write(outputBuffers[0][i]);
                }
            }
            else
            {
                for (var i = 0; i < blockSize; i++)
                {
                    stream.Write(outputBuffers[0][i]);
                    stream.Write(outputBuffers[1][i]);
                }
            }
        }
        private void SetEvent(int deltaFrames, int noteLength, int noteOffset, byte[] midiData, short detune, byte noteOffVelocity)
        {// Reserved, unused

            VstMidiEvent vse = new VstMidiEvent(/*DeltaFrames*/ 	deltaFrames,
                /*NoteLength*/    noteLength,
                /*NoteOffset*/    noteOffset,
                                                midiData,
                /*Detune*/            detune,
                /*NoteOffVelocity*/ noteOffVelocity); // previously 0


            ctx.PluginCommandStub.ProcessEvents(new VstEvent[] { vse });
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

            // can be null when called from inside the plugin main entry point.
            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called:" + e.Message);
            }
        }
        static Dlg dlg = null;
        static object windowLock = new object();
        void ShowWindow()
        {

            var thread = new Thread(() =>
            {
                if (dlg == null)
                {
                    lock (windowLock)
                    {
                        dlg = new Dlg();
                        dlg.PluginCommandStub = ctx.PluginCommandStub;
                    }
                }

                lock (windowLock)
                {
                    ctx.PluginCommandStub.MainsChanged(true);
                    dlg.ShowDialog();
                    ctx.PluginCommandStub.MainsChanged(false);
                }
            });
            thread.Start();
        }
    }
}
