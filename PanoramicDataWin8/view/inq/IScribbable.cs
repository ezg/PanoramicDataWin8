using System.Collections.Generic;
using GeoAPI.Geometries;

namespace PanoramicDataWin8.view.inq
{
    public interface IScribbable
    {
        bool IsDeletable { get; }
        IGeometry Geometry { get; }
        List<IScribbable> Children { get; }
        bool Consume(InkStroke inkStroke);
    }

    public class IScribbleHelpers
    {
        public static void GetScribbablesRecursive(List<IScribbable> allScribbable, List<IScribbable> currents)
        {
            foreach (var current in currents)
            {
                allScribbable.Add(current);
                if (current.Children.Count > 0)
                {
                   // allScribbable.AddRange(current.Children);
                    GetScribbablesRecursive(allScribbable, current.Children);
                }
            }
        }
    }
}
