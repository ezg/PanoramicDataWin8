using System;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttributeTransformationViewModelEventArgs : EventArgs
    {
        public AttributeTransformationViewModelEventArgs(AttributeTransformationModel attributeTransformationModel, Rct bounds)
        {
            AttributeTransformationModel = attributeTransformationModel;
            Bounds = bounds;
        }

        public Rct Bounds { get; set; }
        public AttributeTransformationModel AttributeTransformationModel { get; set; }
    }
}