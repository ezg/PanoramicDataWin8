using GeoAPI.Geometries;

namespace PanoramicDataWin8.model.view
{
    public interface AttributeTransformationViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
        void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
    }
}