using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeGroupOperationViewModel : OperationViewModel
    {
        private void createBottomAttributeGroupMenu()
        {
            var attachmentViewModel = AttachementViewModels.First( avm => avm.AttachmentOrientation == AttachmentOrientation.Bottom);
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
                Label = "+",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            attr1.DroppedTriggered = attributeTransformationModel => {
                if (!AttributeGroupOperationModel.AttributeUsageTransformationModels.Contains(attributeTransformationModel))
                    AttributeGroupOperationModel.AttributeUsageTransformationModels.Add(attributeTransformationModel);
            };

            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            AttributeGroupOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;

                // remove old ones first
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                    {
                        var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            (((AttributeTransformationMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeTransformationViewModel != null) &&
                            (((AttributeTransformationMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel ==
                             oldAttributeTransformationModel));
                        if (found != null)
                            menuViewModel.MenuItemViewModels.Remove(found);
                    }

                menuViewModel.NrRows = (int)System.Math.Ceiling(coll.Count / 3.0) + 1;
                addMenuItem.Row = menuViewModel.NrRows - 1;

                // add new ones
                if (args.NewItems != null)
                    foreach (var newItem in args.NewItems)
                    {
                        var newAttributeTransformationModel = newItem as AttributeTransformationModel;
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
                            AttributeGroupOperationModel.AttributeUsageTransformationModels.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }

                var count = 0;
                foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                {
                    menuItemViewModel.Column = count % 3;
                    menuItemViewModel.Row = (int)System.Math.Floor(count / 3.0);
                    count++;
                }
                attachmentViewModel.ActiveStopwatch.Restart();
                menuViewModel.FireUpdate();
            };


            OperationViewModelTapped += (sender, args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

        }
        public AttributeGroupOperationViewModel(AttributeGroupOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
            addAttachmentViewModels();
            createBottomAttributeGroupMenu();
        }

        public AttributeGroupOperationModel AttributeGroupOperationModel => (AttributeGroupOperationModel)OperationModel;
    }
}