using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
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
        public List<Track> tracks = new List<Track>();
        // Queue<SoundElement> renderingQueue = new Queue<SoundElement>();
        private float renderingStart;
        private float renderingLength;
        static Timer memoryCleaning;

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
        public void AddEmptyTrack()
        {
            var number = tracks.Count;
            var newTrack = new Track();
            tracks.Add(newTrack);
            if (trackAdded != null)
                trackAdded(number, newTrack);
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
                        var connection = new SoundLineConnection(otherLineNumber, this.lines[count], volume);
                        this.lines[otherLineNumber].inputs.Add(connection);
                        this.lines[count].outputs.Add(connection);
                    }
                    catch { }
                }
                count++;
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
