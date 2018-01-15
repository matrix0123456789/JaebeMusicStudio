using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaebeMusicStudio.Sound
{
    public class NotesCollection : ObservableCollection<Note>
    {
       public float CalcLength()
        {
            var max = this.Max(x=>x.Offset+x.Length);
            return max;
        }
    }
}
