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
    public class ExampleOperationViewModel : OperationViewModel
    {
        private void createRightExampleMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var sliderItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Position = Position,
                Size = new Vec(100, 50),
                TargetSize = new Vec(100, 50),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                MenuItemComponentViewModel = new SliderMenuItemComponentViewModel
                {
                    Label = "dummy slider",
                    Value = ExampleOperationModel.DummyValue,
                    MinValue = 1,
                    MaxValue = 100
                }
            };

            (sliderItem.MenuItemComponentViewModel as SliderMenuItemComponentViewModel).PropertyChanged += (sender, args) =>
            {
                var model = sender as SliderMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                    ExampleOperationModel.DummyValue = model.FinalValue;
                attachmentViewModel.ActiveStopwatch.Restart();
            };
            menuViewModel.MenuItemViewModels.Add(sliderItem);
        }

        private void createBottomExampleMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Bottom);
            attachmentViewModel.ShowOnAttributeMove = true;
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
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
                    Label = "+",
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                    CanDrag = false,
                    CanDelete = false,
                    CanDrop = true,
                    DroppedTriggered = attributeViewModel => ExampleOperationModel.AttributeTransformationModelParameters.Add(attributeViewModel.AttributeTransformationModel)
                }
            };
            menuViewModel.MenuItemViewModels.Add(addMenuItem);

            ExampleOperationModel.AttributeTransformationModelParameters.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeModel>;

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

                menuViewModel.NrRows = (int)Math.Ceiling(coll.Count / 3.0) + 1;
                addMenuItem.Row = menuViewModel.NrRows - 1;

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
                        var newAttr = new AttributeMenuItemViewModel
                        {
                            Label = newAttributeModel.DisplayName,
                            AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                            TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                            CanDrag = false,
                            CanDelete = true,
                            CanDrop = false
                        };
                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm =
                                ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeTransformationModel;
                            ExampleOperationModel.AttributeTransformationModelParameters.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }

                var count = 0;
                foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                {
                    menuItemViewModel.Column = count % 3;
                    menuItemViewModel.Row = (int)Math.Floor(count / 3.0);
                    count++;
                }
                attachmentViewModel.ActiveStopwatch.Restart();
                menuViewModel.FireUpdate();
            };
        }

        private  void createLeftExampleMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Left);
            attachmentViewModel.ShowOnAttributeTapped = true;
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var toggles = new List<ToggleMenuItemComponentViewModel>();
            var items = new List<MenuItemViewModel>();

            var count = 0;
            foreach (var exampleOperationType in new[] { ExampleOperationType.A, ExampleOperationType.B, ExampleOperationType.C })
            {
                var toggleMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Row = count,
                    RowSpan = 0,
                    Column = 0,
                    Position = Position,
                    Size = new Vec(50, 32),
                    TargetSize = new Vec(50, 32)
                };

                var toggle = new ToggleMenuItemComponentViewModel
                {
                    Label = exampleOperationType.ToString().ToLower(),
                    IsChecked = ExampleOperationModel.ExampleOperationType == exampleOperationType
                };
                toggles.Add(toggle);
                toggleMenuItem.MenuItemComponentViewModel = toggle;
                toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                {
                    var model = sender2 as ToggleMenuItemComponentViewModel;
                    if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        if (model.IsChecked)
                        {
                            attachmentViewModel.ActiveStopwatch.Restart();
                            ExampleOperationModel.ExampleOperationType = exampleOperationType;
                            foreach (var tg in model.OtherToggles)
                                tg.IsChecked = false;
                        }
                };
                menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                items.Add(toggleMenuItem);
                count++;
            }

            foreach (var mi in items)
                (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
        }
        public ExampleOperationViewModel(ExampleOperationModel exampleOperationModel) : base(exampleOperationModel)
        {
            addAttachmentViewModels();
            createBottomExampleMenu();
            createRightExampleMenu();
            createLeftExampleMenu();
        }

        public ExampleOperationModel ExampleOperationModel => (ExampleOperationModel) OperationModel;
    }
}