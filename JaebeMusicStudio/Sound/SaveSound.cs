using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JaebeMusicStudio.Sound.FileFormat;

namespace JaebeMusicStudio.Sound
{
    static class SaveSound
    {
        public static IFileFormat format = new Wave();
        public static FileInfo file;
        public static void SaveFile()
        {
            Player.rendering = true;
            Player.status = Player.Status.fileRendering;
            Project.current.Render(0, Project.current.length);


        }

        public static void SaveFileEnd(float[,] data)
        {

            var str = file.OpenWrite();
            if (file.Extension == "mp3")
            {
               // format = new Mp3();
            }
            else
            {
                format = new Wave();
            }
            format.Write(str, data);
            str.Close();
        }
    }
}
