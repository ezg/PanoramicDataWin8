using GeoAPI.Geometries;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.view
{
    public interface AttributeTransformationViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
        void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
        AttributeTransformationModel CurrentAttributeTransformationModel { get; }
    }
}