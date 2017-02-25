using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace JaebeMusicStudio.Sound
{
    /// <summary>
    /// Class contains whole project
    /// </summary>
    class Project
    {
        float _tempo = 120;
        uint _sampleRate = 48000;
        public float tempo { get { return _tempo; } }
        public float sampleRate { get { return _sampleRate; } }
        public static Project current = null;
        public List<SoundLine> lines = new List<SoundLine>() { };

        /// <summary>
        /// for EXAMPLE KEYBOARD OR MINI INPUT
        /// </summary>
        public static List<ILiveInput> live = new List<ILiveInput>();



        public List<Track> tracks = new List<Track>();
        public ObservableCollection<INoteSynth> NoteSynths = new ObservableCollection<INoteSynth>();
        // Queue<SoundElement> renderingQueue = new Queue<SoundElement>();
        private float renderingStart;
        private float renderingLength;
        static Timer memoryCleaning;

        Dictionary<string, INamedElement> NamedElements = new Dictionary<string, INamedElement>();

        static Project()
        {
            memoryCleaning = new Timer((o) =>
            {
                if (current != null)
                {
                    foreach (var x in current.lines)
                    {
                        foreach (var y in x.effects)
                        {
                            y.CleanMemory();
                        }
                    }
                }
            }, null, 10000, 10000);
        }

        public Project()
        {
            lines.Add(new SoundLine());
        }
        public Project(string path)
        {
            current = this;
            var read = new System.IO.FileStream(path, FileMode.Open);

            read.Position = 0;
            var zis = new ZipInputStream(read);
            ZipEntry ent;
            do
            {
                ent = zis.GetNextEntry();
                if (ent.Name == "project.xml")
                {
                    var document = new XmlDocument();
                    document.Load(zis);
                    LoadXml(document);
                    break;
                }


            } while (ent != null);
            zis.Close();
        }

        public float length
        {
            get
            {
                var ret = 0f;
                foreach (var track in tracks)
                {
                    foreach (var x in track.Elements)
                    {
                        if (x.Offset + x.Length > ret)
                        {
                            ret = x.Offset + x.Length;
                        }
                    }
                }
                return ret;
            }
        }


        /// <summary>
        /// Event: new track was added. First parameter is index of new track;
        /// </summary>
        /// 

        public event Action<int, Track> trackAdded;
        public Track AddEmptyTrack()
        {
            var number = tracks.Count;
            var newTrack = new Track();
            tracks.Add(newTrack);
            if (trackAdded != null)
                trackAdded(number, newTrack);
            return newTrack;
        }

        internal void Render(float position, float renderLength)
        {
            this.renderingStart = position;
            this.renderingLength = renderLength;
            foreach (var line in lines)
            {
                line.cleanToRender((int)CountSamples(renderingLength));
            }
            foreach (var track in tracks)
            {
                foreach (var element in track.Elements)
                {
                    if (element.SoundLine == null) continue;//skup element without output
                    if (element.Offset < position + renderingLength && element.Offset + element.Length > position)
                    {
                        lock (element.SoundLine)
                        {
                            element.SoundLine.currentToRender++;
                            System.Threading.ThreadPool.QueueUserWorkItem((el) =>
                            {
                                var renderStart = (el as ISoundElement).Offset - position;
                                if (renderStart >= 0)//you must wait to start playing
                                {
                                    var rendered = (el as ISoundElement).GetSound(0, renderingLength - renderStart);
                                    (el as ISoundElement).SoundLine.rendered((int)CountSamples(renderingStart), rendered);
                                }
                                else
                                {
                                    var rendered = (el as ISoundElement).GetSound(-renderStart, renderingLength);
                                    (el as ISoundElement).SoundLine.rendered(0, rendered);
                                }
                            }, element);
                        }
                    }
                }
            }
            foreach (var liveElement in live)
            {
                liveElement.Synth = NoteSynths[0];
                if (liveElement.Synth?.SoundLine == null) continue;
                lock (liveElement.Synth.SoundLine)
                {
                    liveElement.Synth.SoundLine.currentToRender++;
                    System.Threading.ThreadPool.QueueUserWorkItem((el) =>
                    {
                        var rendered = (el as ILiveInput).GetSound(0, renderingLength);
                        (el as ILiveInput).Synth.SoundLine.rendered(0, rendered);

                    }, liveElement);
                }
            }
            //if (lines[0].currentToRender == 0)
            //    returnedSound(lines[0].lastRendered);
            foreach (var line in lines)
            {
                line.checkIfReady();
            }
        }

        internal void RenderIdle(float renderLength)
        {
            this.renderingLength = renderLength;
            foreach (var line in lines)
            {
                line.cleanToRender((int)CountSamples(renderingLength));
            }
            foreach (var liveElement in live)
            {
                liveElement.Synth = NoteSynths[0];
                if (liveElement.Synth?.SoundLine == null) continue;
                lock (liveElement.Synth.SoundLine)
                {
                    liveElement.Synth.SoundLine.currentToRender++;
                    System.Threading.ThreadPool.QueueUserWorkItem((el) =>
                    {
                        var rendered = (el as ILiveInput).GetSound(0, renderingLength);
                        (el as ILiveInput).Synth.SoundLine.rendered(0, rendered);

                    }, liveElement);
                }
            }
            //if (lines[0].currentToRender == 0)
            //    returnedSound(lines[0].lastRendered);
            foreach (var line in lines)
            {
                line.checkIfReady();
            }
        }

        public void Serialize(string path)
        {
            var document = new XmlDocument();

            document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><project></project>");

            foreach (var x in lines)
            {
                x.Serialize(document);
            }
            foreach (var x in tracks)
            {
                x.Serialize(document);
            }

            foreach (var x in NoteSynths)
            {
                x.Serialize(document.DocumentElement);
            }


            var zip = ZipFile.Create(path);
            CustomStaticDataSource sds = new CustomStaticDataSource();
            var str = new MemoryStream();
            var writer = new StreamWriter(str);
            writer.Write(document.OuterXml);
            writer.Flush();
            str.Position = 0;
            sds.SetStream(str);
            zip.BeginUpdate();
            // If an entry of the same name already exists, it will be overwritten; otherwise added.
            zip.Add(sds, "project.xml");
            zip.CommitUpdate();
            zip.Close();
        }

        void LoadXml(XmlDocument document)
        {
            var lines = document.GetElementsByTagName("SoundLine");
            foreach (XmlElement line in lines)
            {
                this.lines.Add(new SoundLine(line));
            }
            int count = 0;
            foreach (XmlElement line in lines)
            {
                foreach (XmlElement input in line.ChildNodes)
                {
                    try
                    {
                        var volume = 1f;
                        if (input.Attributes["volume"] != null)
                            volume = float.Parse(input.Attributes["volume"].Value, CultureInfo.InvariantCulture);
                        var otherLineNumber = int.Parse(input.Attributes["lineNumber"].Value);
                        var connection = new SoundLineConnection(count, this.lines[otherLineNumber], volume);
                        this.lines[count].inputs.Add(connection);
                        this.lines[otherLineNumber].outputs.Add(connection);
                    }
                    catch { }
                }
                count++;
            }
            var basicSynths = document.GetElementsByTagName("BasicSynth");
            foreach (XmlNode basicSynth in basicSynths)
            {
                this.NoteSynths.Add(new BasicSynth(basicSynth));
            }


            var tracks = document.GetElementsByTagName("Track");
            foreach (XmlNode track in tracks)
            {
                this.tracks.Add(new Track(track));
            }
        }
        public void ReturnedSound(float[,] data)
        {
            if (Player.status == Player.Status.playing || Player.status == Player.Status.paused)
            {
                Player.ReturnedSound(data);


            }
        }
        public float CountSamples(float input)
        {
            return input / tempo * 60f * _sampleRate;
        }
        public TimeSpan CountTime(float input)
        {
            return new TimeSpan((long)(100 * input / tempo * 60f) * 100000);
        }
        public float SamplesToBeats(float input)
        {
            return input * tempo / 60f / _sampleRate;
        }
        public Track FindTrackWithSpace(float from, float to)
        {
            foreach (var track in tracks)
            {
                var canBe = true;
                foreach (var element in track.Elements)
                {
                    if (element.Offset < to && element.Offset + element.Length > from)
                    {
                        canBe = false;
                        break;
                    }
                }
                if (canBe)
                    return track;
            }
            return AddEmptyTrack();
        }
        public double waveTime(float pitch)
        {
            return sampleRate / (Math.Pow(2, (pitch - 69) / 12) * 440f);
        }
        public bool checkNamedElement(string name) => NamedElements.ContainsKey(name);
        long generatedNamedElementNumber = 0;
        public void generateNamedElement(INamedElement el)
        {
            do
            {
                generatedNamedElementNumber++;
            } while (NamedElements.ContainsKey("element_" + generatedNamedElementNumber));
            el.Name = "element_" + generatedNamedElementNumber;
        }
        public INamedElement this[string name]
        {
            get { return NamedElements[name]; }
            set
            {
                if (NamedElements.ContainsKey(name))
                {
                    throw new DuplicateNameException();
                }
                if (NamedElements.ContainsValue(value))
                {
                    NamedElements.Remove(value.Name);
                }
                NamedElements[name] = value;
            }
        }

        class CustomStaticDataSource : IStaticDataSource
        {
            private Stream _stream;
            // Implement method from IStaticDataSource
            public Stream GetSource()
            {
                return _stream;
            }

            // Call this to provide the memorystream
            public void SetStream(Stream inputStream)
            {
                _stream = inputStream;
                _stream.Position = 0;
            }
        }
    }
}
