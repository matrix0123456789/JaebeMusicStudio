using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
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
        public float tempo { get { return tempo; } }
        public static Project current = null;
        List<Track> tracks = new List<Track>();
        /// <summary>
        /// Event: new track was added. First parameter is index of new track;
        /// </summary>
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

            var zip=ZipFile.Create(path);
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
        public void open(string path)
        {
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
        void loadXML(XmlDocument document)
        {

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
