using GeoAPI.Geometries;

namespace PanoramicDataWin8.model.view
{
    public interface InputGroupViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void InputGroupViewModelMoved(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
        void InputGroupViewModelDropped(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
    }
}