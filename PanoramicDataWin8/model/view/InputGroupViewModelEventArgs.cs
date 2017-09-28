using System;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputGroupViewModelEventArgs : EventArgs
    {
        public InputGroupViewModelEventArgs(Rct bounds, AttributeModel attributeGroupModel)
        {
            Bounds = bounds;
            AttributeGroupModel = attributeGroupModel;
        }

        public AttributeModel AttributeGroupModel { get; set; }
        public Rct Bounds { get; set; }
    }
}