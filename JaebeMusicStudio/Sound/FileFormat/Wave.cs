using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound.FileFormat
{
    public class Wave : IFileFormat
    {
        public int BitsPerSample = 16;
        public void Write(FileStream str, float[,] data, Rendering rendering)
        {
            var writer = new BinaryWriter(str);
            var fileSize = 44 + data.Length * (BitsPerSample / 8);

            writer.Write("RIFF".ToCharArray());
            writer.Write(fileSize);
            writer.Write("WAVEfmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)2);
            writer.Write((int)rendering.sampleRate);
            writer.Write((int)rendering.sampleRate * BitsPerSample * 4);
            writer.Write((short)(BitsPerSample * 4));
            writer.Write((short)(BitsPerSample));
            writer.Write("data".ToCharArray());
            writer.Write(data.Length * (BitsPerSample / 8));
            for (var i = 0; i < data.GetLength(1); i++)
            {
                var left = (long)(data[0, i] * 0x8000);
                var right = (long)(data[1, i] * 0x8000);
                if (left > 0x7fff)
                    left = 0x7fff;
                if (left < -0x8000)
                    left = -0x8000;
                if (right > 0x7fff)
                    right = 0x7fff;
                if (right < -0x8000)
                    right = -0x8000;
                writer.Write((short)left);
                writer.Write((short)right);
            }
            writer.Flush();
        }
    }
}
