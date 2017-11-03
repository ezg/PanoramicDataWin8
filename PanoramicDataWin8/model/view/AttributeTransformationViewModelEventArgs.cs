using System;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttributeViewModelEventArgs : EventArgs
    {
        public AttributeViewModelEventArgs(AttributeModel attributeModel, Rct bounds)
        {
            AttributeModel = attributeModel;
            Bounds = bounds;
        }
        public AttributeViewModelEventArgs(AttributeTransformationModel attributeTransformationModel, Rct bounds)
        {
            AttributeModel = attributeTransformationModel.AttributeModel;
            AttributeTransformationModel = attributeTransformationModel;
            Bounds = bounds;
        }

        public Rct Bounds { get; set; }
        public AttributeModel AttributeModel { get; set; }

        public AttributeTransformationModel AttributeTransformationModel { get; set; }
    }
}