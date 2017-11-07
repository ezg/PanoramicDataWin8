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
        private void createFunctionMenu(AttachmentOrientation attachmentOrientation,
              AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = Position,
                TargetSize = size,
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    TextAngle = textAngle,
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    Label = "Apply",
                    AttributeViewModel = new AttributeViewModel(this, AttributeGroupOperationModel.AttributeModel),
                    EditNameOnTap = true
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);
        }
        public AttributeGroupOperationViewModel(AttributeGroupOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
            createTopInputsExpandingMenu();
            createFunctionMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);

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