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

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };

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
                IsHeightBoundToParent = isHeightBoundToParent
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = "Apply"
            };

            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, new AttributeTransformationModel(AttributeGroupOperationModel.AttributeGroupModel));
            attr1.TappedTriggered = (() => attr1.Editing = Visibility.Visible);
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        public AttributeGroupOperationViewModel(AttributeGroupOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
            addAttachmentViewModels();
            createTopInputsExpandingMenu();
            createFunctionMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);

            TopInputAdded += (sender) =>
            {
                var str = "(";
                foreach (var g in AttributeGroupOperationModel.AttributeUsageTransformationModels)
                    str += g.AttributeModel.DisplayName + ",";
                str = str.TrimEnd(',') + ")";
                var newName = new Regex("\\(.*\\)", RegexOptions.Compiled).Replace(AttributeGroupOperationModel.AttributeGroupModel.DisplayName, str);
                AttributeGroupOperationModel.SetName(newName);
            };
        }

        public AttributeGroupOperationModel AttributeGroupOperationModel => (AttributeGroupOperationModel)OperationModel;
    }
}