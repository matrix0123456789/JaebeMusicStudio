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
        public event Action loadEnd;
        public float tempo
        {
            get { return _tempo; }
            set { _tempo = value; }
        }

        public static Project current = null;
        public ObservableCollection<SoundLine> lines = new ObservableCollection<SoundLine>() { };
        public SoundLineAbstract OutputLine { get { return outputLine ?? lines.FirstOrDefault(); } set { outputLine = value; } }
        private SoundLineAbstract outputLine;
        public LiveSoundLineCollection liveLines = new LiveSoundLineCollection();

        /// <summary>
        /// for EXAMPLE KEYBOARD OR MIDI INPUT
        /// </summary>
        public static List<ILiveInput> live = new List<ILiveInput>();
        public string Path { get; private set; }


        public List<Track> tracks = new List<Track>();
        public ObservableCollection<INoteSynth> NoteSynths = new ObservableCollection<INoteSynth>();
        // Queue<SoundElement> renderingQueue = new Queue<SoundElement>();
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
            current?.stopOldProject();
            lines.Add(new SoundLine());
            outputLine = lines.First();
        }

        public Project(string path)
        {
            current?.stopOldProject();
            current = this;
            var read = new System.IO.FileStream(path, FileMode.Open);
            Path = path;
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
            Player.liveRenderingNow = false;
        }
        public void stopOldProject()
        {
            liveLines.stop();
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

        internal void Clear(Rendering rendering)
        {
            var liveLinesList = liveLines.getAvaibleInputs();
            foreach (var line in liveLinesList)
            {
                line.clearAfterRender(rendering);
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

        IEnumerable<ISoundElement> playTracks(Rendering rendering)
        {
            float position = rendering.renderingStart;
            float renderLength = rendering.renderingLength;
            foreach (var track in tracks)
            {
                foreach (var element in track.Elements)
                {
                    if (element.SoundLine == null) continue; //skip element without output
                    if (element.Offset < position + rendering.renderingLength && element.Offset + element.Length > position)
                    {
                        yield return element;
                    }
                }
            }
        }
        IEnumerable<ILiveInput> playLive(Rendering rendering)
        {
            foreach (var liveElement in live)
            {
                if (liveElement.Synth == null)
                {
                    if (NoteSynths.Count == 0)
                        continue;
                    continue;
                    liveElement.Synth = NoteSynths[0];
                }
                if (liveElement.Synth?.SoundLine == null) continue;
                yield return liveElement;
            }
        }
        public async Task<float[,]> Render(Rendering rendering)
        {
            //prepareToRender(rendering);
            IEnumerable<ISoundElement> soundElements = Array.Empty<ISoundElement>();
            IEnumerable<ILiveInput> liveInputs = Array.Empty<ILiveInput>();
            if (rendering.type == RenderngType.live)
            {
                if (Player.status != Player.Status.paused)
                {
                    soundElements = playTracks(rendering);
                }
                liveInputs = playLive(rendering);
            }
            else
            {
                soundElements = playTracks(rendering);
            }
            rendering.soundElements = soundElements;
            rendering.liveInputs = liveInputs;
            var lines = rendering.project.lines.Cast<SoundLineAbstract>().Concat(rendering.project.liveLines.getAvaibleInputs()).ToArray();
            rendering.soundLinesRenderings = lines.Select(x => new KeyValuePair<SoundLineAbstract, TaskCompletionSource<float[,]>>(x, new TaskCompletionSource<float[,]>())).ToDictionary(x => x.Key, x => x.Value);
            foreach (var r in rendering.soundLinesRenderings)
            {
                var task = Task.Run(async () =>
                {
                    var result = await r.Key.Render(rendering);
                    r.Value.SetResult(result);
                });
            }
            return await rendering.soundLinesRenderings[rendering.project.OutputLine].Task;
        }

        public void Serialize(string path)
        {
            var document = new XmlDocument();

            document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><project></project>");
            document.DocumentElement.SetAttribute("tempo", tempo.ToString(CultureInfo.InvariantCulture));
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
            if (document.DocumentElement.Attributes["tempo"] != null)
            {
                tempo = float.Parse(document.DocumentElement.Attributes["tempo"].Value, CultureInfo.InvariantCulture);
            }
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
                        if (input.Attributes["lineNumber"] != null)
                        {
                            var otherLineNumber = int.Parse(input.Attributes["lineNumber"].Value);
                            var connection = new SoundLineConnection(count, this.lines[otherLineNumber], volume);
                            this.lines[count].AddInput(connection);
                            this.lines[otherLineNumber].outputs.Add(connection);
                        }
                        else if (input.Attributes["liveLineNumber"] != null)
                        {
                            var liveLineNumber = int.Parse(input.Attributes["liveLineNumber"].Value);
                            var liveLine = liveLines.GetByDeviceID(liveLineNumber);
                            var connection = new SoundLineConnection(count, liveLine, volume);
                            this.lines[count].AddInput(connection);
                            liveLine.outputs.Add(connection);
                        }
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
            var basicPercussions = document.GetElementsByTagName("BasicPercussion");
            foreach (XmlNode basicPercussion in basicPercussions)
            {
                this.NoteSynths.Add(new BasicPercussion(basicPercussion));
            }
            var SamplePlays = document.GetElementsByTagName("SamplePlay");
            foreach (XmlNode SamplePlay in SamplePlays)
            {
                this.NoteSynths.Add(new SamplePlay(SamplePlay));
            }
            var VSTis = document.GetElementsByTagName("VSTi");
            foreach (XmlNode VSTi in VSTis)
            {
                this.NoteSynths.Add(new VSTi(VSTi));
            }


            var tracks = document.GetElementsByTagName("Track");
            foreach (XmlNode track in tracks)
            {
                this.tracks.Add(new Track(track));
            }
            loadEnd?.Invoke();
        }
       
        public TimeSpan CountTime(float input)
        {
            return new TimeSpan((long)(100 * input / tempo * 60f) * 100000);
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
