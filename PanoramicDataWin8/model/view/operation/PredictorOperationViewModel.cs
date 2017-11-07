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
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    CanDrag = false,
                    CanDrop = false
                }
            };
            newMenuItem.Deleted += (sender1, args1) =>
            {
                var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
                PredictorOperationModel.AttributeUsageModels.Remove(atm);
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
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                    CanDrag = false,
                    CanDrop = true,
                    DroppedTriggered = TargetDropped
                }
            };
            TargetMenuViewModel.MenuItemViewModels.Add(TargetMenuItemViewModel);
        }
        void createLeftInputsExpandingMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Left);
            attachmentViewModel.ShowOnAttributeMove = true;
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrRows = 3,
                NrColumns = 1
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

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
                Position = Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = "-",
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                    CanDrag = false,
                    CanDrop = true,
                    DroppedTriggered = attributeViewModel =>
                    {
                        var attributeModel = attributeViewModel.AttributeModel;
                        if (!PredictorOperationModel.IgnoredAttributeUsageModels.Contains(attributeModel))
                            PredictorOperationModel.IgnoredAttributeUsageModels.Add(attributeModel);
                    }
                }
            };
            menuViewModel.MenuItemViewModels.Add(addMenuItem);

            PredictorOperationModel.IgnoredAttributeUsageModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeModel>;
                var attributeModel = coll.FirstOrDefault();

                // remove old ones first
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                    {
                        var oldAttributeModel = oldItem as AttributeModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            (((AttributeMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeViewModel != null) &&
                            (((AttributeMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeViewModel.AttributeModel ==
                             oldAttributeModel));
                        if (found != null)
                            menuViewModel.MenuItemViewModels.Remove(found);
                    }

                menuViewModel.NrColumns = (int)Math.Ceiling(coll.Count / 3.0) + 1;

                // add new ones
                if (args.NewItems != null)
                    foreach (var newItem in args.NewItems)
                    {
                        var newAttributeModel = newItem as AttributeModel;
                        var newMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Size = new Vec(50, 50),
                            TargetSize = new Vec(50, 50),
                            Position = addMenuItem.Position
                        };
                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
                            PredictorOperationModel.IgnoredAttributeUsageModels.Remove(atm);
                        };
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);

                        var newAttr = new AttributeMenuItemViewModel
                        {
                            Label = newAttributeModel.DisplayName,
                            AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                            TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                            CanDrag = false,
                            CanDrop = false
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        newAttributeModel.PropertyChanged += (sender2, args2) => newAttr.Label = (sender2 as AttributeModel).DisplayName;

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
        }
        public PredictorOperationViewModel(PredictorOperationModel predictorOperationModel) : base(predictorOperationModel)
        {
            var menuViewModel = createLabelMenu(AttachmentOrientation.Bottom, predictorOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out PredictorNameMenuItemViewModel);
            createRightTargetMenu();
            createTopInputsExpandingMenu();
            createLeftInputsExpandingMenu();
            hideLabelUnlessPredictorHasResult(menuViewModel);
        }
    }
}