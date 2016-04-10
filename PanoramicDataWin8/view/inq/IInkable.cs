using System.Collections.Generic;
using GeoAPI.Geometries;

namespace PanoramicDataWin8.view.inq
{
    public interface IInkable
    {

        bool Consume(InkStroke inkStroke);
    }
}
