using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.view.inq
{
    public interface IGesture
    {
        bool Recognize(InkStroke inkStroke);
    }
}
