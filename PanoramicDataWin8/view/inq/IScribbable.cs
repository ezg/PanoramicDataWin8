using System.Collections.Generic;
using GeoAPI.Geometries;

namespace PanoramicDataWin8.view.inq
{
    public interface IScribbable
    {
        IGeometry Geometry { get; }
        List<IScribbable> Children { get; }

    } 
}
