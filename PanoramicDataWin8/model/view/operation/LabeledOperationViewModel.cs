using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.data.attribute;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using PanoramicDataWin8.model.data.idea;
using Windows.UI.Xaml;
using PanoramicDataWin8.controller.view;

namespace PanoramicDataWin8.model.view.operation
{
    public class LabeledOperationViewModel : OperationViewModel
    {
        public LabeledOperationViewModel(OperationModel operationModel) : base(operationModel)
        {
        }

        public delegate void DroppedTriggeredHandler(AttributeViewModel model);
       
        protected void createApplyAttributeMenu(AttributeModel attributeModel, AttachmentOrientation attachmentOrientation,
              AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent, bool clickToDismiss=false)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5,
                ClickToDismiss = clickToDismiss
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
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush,
                    Label = "Apply",
                    AttributeViewModel = new AttributeViewModel(this, attributeModel),
                    EditNameOnTap = true
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);
        }

        protected MenuViewModel createAttributeLabelMenu(AttachmentOrientation attachmentOrientation, AttributeModel code,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent, 
            DroppedTriggeredHandler droppedTriggered,
            out MenuItemViewModel menuItemViewModel)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            menuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = this.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    TextAngle = textAngle,
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush,
                    EditNameOnTap = !( code?.FuncModel is AttributeModel.AttributeFuncModel.AttributeColumnFuncModel),
                    Label = code?.RawName,
                    AttributeViewModel = new AttributeViewModel(this, code),
                    DroppedTriggered = droppedTriggered == null ? null : new Action<AttributeViewModel>(droppedTriggered)
                }
            };
            var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
            menuViewModel.MenuItemViewModels.Add(menuItemViewModel);
            if (code != null)
            {
                code.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "DisplayName")
                        attributeMenuItemViewModel.Label = code.DisplayName;
                };
            }
            attributeMenuItemViewModel.TappedTriggered += ((e) => // when label is tapped, display any attached menu options
                attachmentViewModel.StartDisplayActivationStopwatch());
            return menuViewModel;
        }
    }
}
