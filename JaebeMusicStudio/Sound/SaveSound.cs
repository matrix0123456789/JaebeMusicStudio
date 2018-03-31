using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JaebeMusicStudio.addons;
using JaebeMusicStudio.Sound.FileFormat;

namespace JaebeMusicStudio.Sound
{
    static class SaveSound
    {
        public static IFileFormat format = new Wave();
        public static FileInfo file;
        public static async Task SaveFileAsync()
        {
            Player.rendering = true;
            Player.status = Player.Status.fileRendering;
            var rendering = new Rendering() { renderingStart = 0, renderingLength = Project.current.length };
            Project.current.Render(rendering);

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
