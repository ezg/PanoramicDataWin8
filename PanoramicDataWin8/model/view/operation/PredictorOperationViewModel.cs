using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
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
    public class PredictorOperationViewModel : LabeledOperationViewModel
    {
        MenuViewModel     TargetMenuViewModel;
        MenuItemViewModel TargetMenuItemViewModel;
        MenuItemViewModel PredictorNameMenuItemViewModel = null;
        public PredictorOperationModel PredictorOperationModel => (PredictorOperationModel)OperationModel;
        void hideLabelUnlessPredictorHasResult(MenuViewModel menuViewModel)
        {
            menuViewModel.MenuItemViewModels.Remove(PredictorNameMenuItemViewModel);
            PredictorOperationModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PredictorOperationModel.GetPropertyName(() => PredictorOperationModel.Result))
                {
                    if (PredictorOperationModel.Result == null)
                    {
                        menuViewModel.MenuItemViewModels.Remove(PredictorNameMenuItemViewModel);
                    }
                    else if (!menuViewModel.MenuItemViewModels.Contains(PredictorNameMenuItemViewModel))
                    {
                        menuViewModel.MenuItemViewModels.Add(PredictorNameMenuItemViewModel);
                    }
                }
            };
        }
        void TargetDropped(AttributeViewModel attributeViewModel)
        {
            var attributeMenuItemViewModel = (PredictorNameMenuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel);
            PredictorOperationModel.TargetAttributeUsageModel = attributeViewModel.AttributeModel;
            attributeMenuItemViewModel.Label = new Regex("\\(.*\\)", RegexOptions.None).Replace(attributeMenuItemViewModel.Label, "(" + attributeViewModel.AttributeModel.DisplayName + ")");
            PredictorOperationModel.SetRawName(attributeMenuItemViewModel.Label);

            if (TargetMenuViewModel.MenuItemViewModels.Count > 1)
                TargetMenuViewModel.MenuItemViewModels.RemoveAt(1);

            TargetMenuViewModel.NrColumns = 2;
            TargetMenuItemViewModel.Column = TargetMenuViewModel.NrColumns - 1;

            var newAttributeModel = PredictorOperationModel.TargetAttributeUsageModel;
            var newMenuItem = new MenuItemViewModel
            {
                MenuViewModel = TargetMenuViewModel,
                Size = new Vec(50, 50),
                TargetSize = new Vec(50, 50),
                Position = TargetMenuItemViewModel.Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = newAttributeModel.DisplayName,
                    AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush,
                    CanDrag = false,
                    CanDelete = true,
                    CanDrop = false
                }
            };
            newMenuItem.Deleted += (sender1, args1) =>
            {
                var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeTransformationModel;
                PredictorOperationModel.AttributeTransformationModelParameters.Remove(atm);
            };
            TargetMenuViewModel.MenuItemViewModels.Add(newMenuItem);

            PredictorOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
        void createRightTargetMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            attachmentViewModel.ShowOnAttributeMove = true;
            attachmentViewModel.ShowOnAttributeTapped = true;

            TargetMenuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };
            attachmentViewModel.MenuViewModel = TargetMenuViewModel;

            TargetMenuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = TargetMenuViewModel,
                Row = 0,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                Size = new Vec(50, 25),
                TargetSize = new Vec(50, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = "target",
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush,
                    CanDrag = false,
                    CanDelete = true,
                    CanDrop = true,
                    DroppedTriggered = TargetDropped
                }
            };
            TargetMenuViewModel.MenuItemViewModels.Add(TargetMenuItemViewModel);
        }
        public PredictorOperationViewModel(PredictorOperationModel predictorOperationModel) : base(predictorOperationModel)
        {
            var menuViewModel = createAttributeLabelMenu(AttachmentOrientation.Bottom, predictorOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out PredictorNameMenuItemViewModel);
            createRightTargetMenu();
            MenuItemViewModel menuItemViewModel;
            createExpandingMenu(AttachmentOrientation.Top,  PredictorOperationModel.AttributeTransformationModelParameters, "+", 50, 3, false, false, true, out menuItemViewModel);
            createExpandingMenu(AttachmentOrientation.Left, PredictorOperationModel.IgnoredAttributeTransformationModels, "-", 50, 3, false, false, true, out menuItemViewModel);
            hideLabelUnlessPredictorHasResult(menuViewModel);
        }
    }
}