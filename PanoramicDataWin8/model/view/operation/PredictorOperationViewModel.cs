using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class PredictorOperationViewModel : AttributeUsageOperationViewModel
    {
        private void createRightTargetMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            attachmentViewModel.ShowOnAttributeMove = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };

            var addMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                Size = new Vec(50, 25),
                TargetSize = new Vec(50, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                Label = "target",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            attr1.DroppedTriggered = attributeTransformationModel =>
            {
                PredictorOperationModel.TargetAttributeUsageTransformationModel = attributeTransformationModel;
                PredictorNameMenuItemViewModel.Label = new Regex("\\(.*\\)", RegexOptions.None).Replace(PredictorNameMenuItemViewModel.Label, "(" + attributeTransformationModel.AttributeModel.DisplayName + ")");
                PredictorOperationModel.SetRawName(PredictorNameMenuItemViewModel.Label);
            
                if (menuViewModel.MenuItemViewModels.Count > 1)
                    menuViewModel.MenuItemViewModels.RemoveAt(1);

                menuViewModel.NrColumns = 2;
                addMenuItem.Column = menuViewModel.NrColumns - 1;

                var newAttributeTransformationModel = PredictorOperationModel.TargetAttributeUsageTransformationModel;
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
                    PredictorOperationModel.AttributeUsageTransformationModels.Remove(atm);
                };
                newMenuItem.MenuItemComponentViewModel = newAttr;
                menuViewModel.MenuItemViewModels.Add(newMenuItem);

                PredictorOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            };


            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
        }
        AttributeTransformationMenuItemViewModel PredictorNameMenuItemViewModel = null;
        private void createAxisMenu(AttachmentOrientation attachmentOrientation,
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
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            PredictorNameMenuItemViewModel = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = PredictorOperationModel.GetCode().DisplayName
            };
            PredictorOperationModel.SetRawName(PredictorNameMenuItemViewModel.Label);
            PredictorNameMenuItemViewModel.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, new AttributeTransformationModel(PredictorOperationModel.GetCode()));
            PredictorNameMenuItemViewModel.TappedTriggered = (() => PredictorNameMenuItemViewModel.Editing = Windows.UI.Xaml.Visibility.Visible);
            menuItem.MenuItemComponentViewModel = PredictorNameMenuItemViewModel;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        protected void createLeftInputsExpandingMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Left);
            attachmentViewModel.ShowOnAttributeMove = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrRows = 3,
                NrColumns = 1
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
                Label = "-",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            attr1.DroppedTriggered = attributeTransformationModel => {
                if (!PredictorOperationModel.IgnoredAttributeUsageTransformationModels.Contains(attributeTransformationModel) &&
                    !attributeTransformationModelContainsAttributeModel((AttributeUsageOperationModel as AttributeGroupOperationModel)?.AttributeModel, attributeTransformationModel))
                    PredictorOperationModel.IgnoredAttributeUsageTransformationModels.Add(attributeTransformationModel);
            };

            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            PredictorOperationModel.IgnoredAttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();

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

                menuViewModel.NrColumns = (int)Math.Ceiling(coll.Count / 3.0) + 1;

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

                        if (newAttributeTransformationModel != null)
                        {
                            newAttributeTransformationModel.PropertyChanged += (sender2, args2) =>
                            {
                                newAttr.Label = (sender2 as AttributeTransformationModel).GetLabel();
                            };
                            newAttributeTransformationModel.AttributeModel.PropertyChanged += (sender2, arg2) =>
                            {
                                newAttr.Label = (sender2 as AttributeModel).DisplayName;
                            };
                        }

                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm =
                                ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                            PredictorOperationModel.IgnoredAttributeUsageTransformationModels.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }

                var count = 0;
                foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                {
                    menuItemViewModel.Row = count % 3;
                    menuItemViewModel.Column = menuViewModel.NrColumns - 1 - (int)Math.Floor(count / 3.0);
                    count++;
                }
                attachmentViewModel.ActiveStopwatch.Restart();
                menuViewModel.FireUpdate();
            };


            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
        }
        public PredictorOperationViewModel(PredictorOperationModel predictorOperationModel) : base(predictorOperationModel)
        {
            addAttachmentViewModels();

            createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createRightTargetMenu();
            createTopInputsExpandingMenu();
            createLeftInputsExpandingMenu();
        }

        public PredictorOperationModel PredictorOperationModel => (PredictorOperationModel)OperationModel;
    }
}