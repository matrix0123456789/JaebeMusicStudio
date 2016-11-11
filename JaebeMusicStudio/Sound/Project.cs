using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        public float tempo { get { return tempo; } }
        public static Project current = null;
        public List<SoundLine> lines = new List<SoundLine>() { };
        List<Track> tracks = new List<Track>();
        /// <summary>
        /// Event: new track was added. First parameter is index of new track;
        /// </summary>
        /// 
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
                    loadXML(document);
                    break;
                }


            } while (ent != null);

        }
        public event Action<int, Track> trackAdded;
        public void addEmptyTrack()
        {
            var number = tracks.Count;
            var newTrack = new Track();
            tracks.Add(newTrack);
            if (trackAdded != null)
                trackAdded(number, newTrack);
        }
        public void serialize(string path)
        {
            var document = new XmlDocument();

            document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><project></project>");

            foreach (var x in lines)
            {
                x.serialize(document);
            }
            foreach (var x in tracks)
            {
                x.serialize(document);
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

        void loadXML(XmlDocument document)
        {
            var lines = document.GetElementsByTagName("SoundLine");
            foreach (XmlNode line in lines)
            {
                this.lines.Add(new SoundLine(line));
            }
            int count = 0;
            foreach (XmlNode line in lines)
            {
                foreach (XmlNode input in line.ChildNodes)
                {
                    try
                    {
                        var volume = 1f;
                        if (input.Attributes["volume"] != null)
                            volume = float.Parse(input.Attributes["volume"].Value, CultureInfo.InvariantCulture);
                        this.lines[count].inputs.Add(new SoundLineConnection(int.Parse(input.Attributes["lineNumber"].Value), volume));//todo zczytywać dane
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
        public float countSamples(float input)
        {
            return input * tempo / 60f * _sampleRate;
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
