using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.operation.computational;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeOperationViewModel : LabeledOperationViewModel
    {
        MenuItemViewModel menuItemViewModel;
        public AttributeOperationViewModel(AttributeOperationModel attributeGroupOperationModel, bool editable) : base(attributeGroupOperationModel)
        {
            Editable = editable;
            var attributeParameterGroups = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>();
            attributeParameterGroups.Add("Hints", new ObservableCollection<AttributeTransformationModel>());
            attributeParameterGroups.Add("DType", new ObservableCollection<AttributeTransformationModel>());
            createExpandingMenu(AttachmentOrientation.TopStacked, attributeParameterGroups, 50, 100, !Editable, false, Editable, out menuItemViewModel);
            createApplyAttributeMenu(AttributeOperationModel.GetAttributeModel(), AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);
        }

        public bool Editable = true;

        public AttributeOperationModel AttributeOperationModel => (AttributeOperationModel)OperationModel;
    }
}
