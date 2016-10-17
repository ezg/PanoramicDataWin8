using GeoAPI.Geometries;

namespace PanoramicDataWin8.model.data
{
    public interface OperationTypeModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void OperationTypeModelMoved(OperationTypeModel sender, OperationTypeModelEventArgs e, bool overElement);
        void OperationTypeModelDropped(OperationTypeModel sender, OperationTypeModelEventArgs e, bool overElement);
    }
}