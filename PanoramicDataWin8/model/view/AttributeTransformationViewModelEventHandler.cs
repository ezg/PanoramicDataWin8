using GeoAPI.Geometries;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.view
{
    public interface AttributeViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void AttributeViewModelMoved(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement);
        void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement);
        AttributeModel CurrentAttributeModel { get; }
    }
}