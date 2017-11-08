using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeGroupOperationViewModel : AttributeUsageOperationViewModel
    {
        public AttributeGroupOperationViewModel(AttributeGroupOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
            createTopInputsExpandingMenu();
            createApplyAttributeMenu(AttributeGroupOperationModel.AttributeModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);

            TopInputAdded += (sender) =>
            {
                var str = "(";
                foreach (var g in AttributeGroupOperationModel.AttributeUsageModels)
                    str += g.DisplayName + ",";
                str = str.TrimEnd(',') + ")";
                var newName = new Regex("\\(.*\\)", RegexOptions.Compiled).Replace(AttributeGroupOperationModel.AttributeModel.DisplayName, str);
                AttributeGroupOperationModel.SetName(newName);
            };
        }

        public AttributeGroupOperationModel AttributeGroupOperationModel => (AttributeGroupOperationModel)OperationModel;
    }
}