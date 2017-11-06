using System;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttributeViewModelEventArgs : EventArgs
    {
        public AttributeViewModelEventArgs(AttributeViewModel attributeViewModel, Rct bounds)
        {
            AttributeViewModel = attributeViewModel;
            Bounds = bounds;
        }

        public Rct Bounds { get; set; }
        public AttributeViewModel AttributeViewModel { get; set; }

        public AttributeModel AttributeModel
        {
            get
            {
                return AttributeViewModel.AttributeModel;
            }
        }
        public AttributeTransformationModel AttributeTransformationModel
        {
            get
            {
                return new AttributeTransformationModel(AttributeModel)
                {
                    AggregateFunction = AttributeViewModel.AttributeTransformationModel.AggregateFunction
                };
            }
        }
    }
}