using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound.FileFormat
{
    public interface IFileFormat
    {
        void Write(FileStream str, float[,] data, Rendering renderind);
    }
}
