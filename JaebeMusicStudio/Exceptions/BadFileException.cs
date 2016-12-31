using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Exceptions
{
    /// <summary>
    /// file is corrupted.Zip file is good zip file, xml is good xml, but content of xml is bad
    /// </summary>
    class BadFileException:Exception
    {
    }
}
