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
            OperationViewModelTapped += (args) =>  attachmentViewModel.ActiveStopwatch.Restart();

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

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
            menuViewModel.MenuItemViewModels.Add(addMenuItem);

            var attr1 = new AttributeMenuItemViewModel
            {
                Label = "target",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            addMenuItem.MenuItemComponentViewModel = attr1;

            attr1.DroppedTriggered = attributeViewModel =>
            {
                PredictorOperationModel.TargetAttributeUsageModel = attributeViewModel.AttributeModel;
                PredictorNameMenuItemViewModel.Label = new Regex("\\(.*\\)", RegexOptions.None).Replace(PredictorNameMenuItemViewModel.Label, "(" + attributeViewModel.AttributeModel.DisplayName + ")");
                PredictorOperationModel.SetRawName(PredictorNameMenuItemViewModel.Label);
            
                if (menuViewModel.MenuItemViewModels.Count > 1)
                    menuViewModel.MenuItemViewModels.RemoveAt(1);

                menuViewModel.NrColumns = 2;
                addMenuItem.Column = menuViewModel.NrColumns - 1;

                var newAttributeModel = PredictorOperationModel.TargetAttributeUsageModel;
                var newMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Size = new Vec(50, 50),
                    TargetSize = new Vec(50, 50),
                    Position = addMenuItem.Position
                };
                var newAttr = new AttributeMenuItemViewModel
                {
                    Label = newAttributeModel.DisplayName,
                    AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    CanDrag = false,
                    CanDrop = false
                };
                newMenuItem.Deleted += (sender1, args1) =>
                {
                    var atm =
                        ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
                    PredictorOperationModel.AttributeUsageModels.Remove(atm);
                };
                newMenuItem.MenuItemComponentViewModel = newAttr;
                menuViewModel.MenuItemViewModels.Add(newMenuItem);

                PredictorOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            };
        }
        AttributeMenuItemViewModel PredictorNameMenuItemViewModel = null;
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
            PredictorNameMenuItemViewModel = new AttributeMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = PredictorOperationModel.GetAttributeModel().DisplayName,
                DisplayOnTap = true
            };
            PredictorOperationModel.SetRawName(PredictorNameMenuItemViewModel.Label);
            PredictorNameMenuItemViewModel.AttributeViewModel = new AttributeViewModel(this, PredictorOperationModel.GetAttributeModel());
            menuItem.MenuItemComponentViewModel = PredictorNameMenuItemViewModel;
            
            attachmentViewModel.MenuViewModel = menuViewModel;
            menuViewModel.MenuItemViewModels.Add(menuItem);

            PredictorOperationModel.PropertyChanged += (sender, args) =>
            {
                var model = PredictorOperationModel;
                if (args.PropertyName == model.GetPropertyName(() => model.Result))
                {
                    if (model.Result == null)
                    {
                        menuViewModel.MenuItemViewModels.Remove(menuItem);
                    }
                    else if (!menuViewModel.MenuItemViewModels.Contains(menuItem))
                    {
                        menuViewModel.MenuItemViewModels.Add(menuItem);
                    }
                }
            };
        }

        protected void createLeftInputsExpandingMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Left);
            attachmentViewModel.ShowOnAttributeMove = true;
            OperationViewModelTapped += (args) => attachmentViewModel.ActiveStopwatch.Restart();

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
            addAttachmentViewModels();

            createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X,  
                 new Vec(200, 50), 0, true, false);
            createRightTargetMenu();
            createTopInputsExpandingMenu();
            createLeftInputsExpandingMenu();
        }

        public PredictorOperationModel PredictorOperationModel => (PredictorOperationModel)OperationModel;
    }
}