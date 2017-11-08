using System;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;
using Windows.UI.Xaml.Input;

namespace PanoramicDataWin8.model.view
{
    public class AttributeViewModelEventArgs : EventArgs
    {
        public AttributeViewModelEventArgs(AttributeViewModel attributeViewModel, Rct bounds, PointerManagerEvent e)
        {
            AttributeViewModel = attributeViewModel;
            Bounds = bounds;
            PointerArgs = e;
        }

        public PointerManagerEvent PointerArgs { get; set; }

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