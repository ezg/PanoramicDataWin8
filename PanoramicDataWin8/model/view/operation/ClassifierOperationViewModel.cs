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
    public class ClassifierOperationViewModel : AttributeUsageOperationViewModel
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
                Size = new Vec(50, 25),
                TargetSize = new Vec(50, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                Label = "label",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            attr1.DroppedTriggered = attributeTransformationModel =>
            {
                ClassifierOperationModel.ClassifierAttributeUsageTransformationModel = attributeTransformationModel;

                if (menuViewModel.MenuItemViewModels.Count > 1)
                    menuViewModel.MenuItemViewModels.RemoveAt(1);

                menuViewModel.NrRows = 2;
                addMenuItem.Row = menuViewModel.NrRows - 1;

                var newAttributeTransformationModel = ClassifierOperationModel.ClassifierAttributeUsageTransformationModel;
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
                    ClassifierOperationModel.AttributeUsageTransformationModels.Remove(atm);
                };
                newMenuItem.MenuItemComponentViewModel = newAttr;
                menuViewModel.MenuItemViewModels.Add(newMenuItem);

                ClassifierOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            };


            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
        }
        
        public ClassifierOperationViewModel(ClassifierOperationModel classifierOperationModel) : base(classifierOperationModel)
        {
            addAttachmentViewModels();
            createBottomExampleMenu();
            createTopInputsExpandingMenu();
        }

        public ClassifierOperationModel ClassifierOperationModel => (ClassifierOperationModel)OperationModel;
    }
}