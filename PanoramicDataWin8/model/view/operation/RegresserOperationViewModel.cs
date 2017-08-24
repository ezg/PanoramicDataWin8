using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class RegresserOperationViewModel : AttributeUsageOperationViewModel
    {
        private void createBottomExampleMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Bottom);
            attachmentViewModel.ShowOnAttributeMove = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

            var addMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(25, 25),
                TargetSize = new Vec(25, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                Label = "x",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            attr1.DroppedTriggered = attributeTransformationModel =>
            {
                RegresserOperationModel.RegresserAttributeUsageTransformationModel = attributeTransformationModel;

                if (menuViewModel.MenuItemViewModels.Count > 1)
                    menuViewModel.MenuItemViewModels.RemoveAt(1);

                menuViewModel.NrRows = 2;
                addMenuItem.Row = menuViewModel.NrRows - 1;

                var newAttributeTransformationModel = RegresserOperationModel.RegresserAttributeUsageTransformationModel;
                var newMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Size = new Vec(50, 50),
                    TargetSize = new Vec(50, 50),
                    Position = addMenuItem.Position
                };
                var newAttr = new AttributeTransformationMenuItemViewModel
                {
                    Label = newAttributeTransformationModel.GetLabel(),
                    AttributeTransformationViewModel = new AttributeTransformationViewModel(this, newAttributeTransformationModel),
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    CanDrag = false,
                    CanDrop = false
                };
                newMenuItem.Deleted += (sender1, args1) =>
                {
                    var atm =
                        ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                    RegresserOperationModel.AttributeUsageTransformationModels.Remove(atm);
                };
                newMenuItem.MenuItemComponentViewModel = newAttr;
                menuViewModel.MenuItemViewModels.Add(newMenuItem);
            };


            OperationViewModelTapped += (sender, args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
        }

        public RegresserOperationViewModel(RegresserOperationModel regresserOperationModel) : base(regresserOperationModel)
        {
            addAttachmentViewModels();
            createBottomExampleMenu();
            createTopInputsExpandingMenu();
        }

        public RegresserOperationModel RegresserOperationModel => (RegresserOperationModel)OperationModel;
    }
}