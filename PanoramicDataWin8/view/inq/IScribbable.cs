using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.view.inq
{
    public interface IScribbable
    {
        IGeometry Geometry { get; }
        List<IScribbable> Children { get; }

    } 
}
