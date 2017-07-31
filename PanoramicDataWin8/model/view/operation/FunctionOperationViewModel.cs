using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class FunctionOperationViewModel : OperationViewModel
    {
        private void createDummyParameterMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

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
                IsHeightBoundToParent = false
            };

            var subtype = this.FunctionOperationModel.FunctionSubtypeModel as MinMaxScaleFunctionSubtypeModel;
            var attr1 = new SliderMenuItemComponentViewModel
            {
                Label = "dummy slider",
                Value = subtype.DummyValue,
                MinValue = 1,
                MaxValue = 100
            };
            attr1.PropertyChanged += (sender, args) =>
            {
                var model = sender as SliderMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                    subtype.DummyValue = model.FinalValue;
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            OperationViewModelTapped += (sender, args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            sliderItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(sliderItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private void createBottomInputsExpandingMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == AttachmentOrientation.Bottom);
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
                if (!FunctionOperationModel.AttributeUsageTransformationModels.Contains(attributeTransformationModel))
                    FunctionOperationModel.AttributeUsageTransformationModels.Add(attributeTransformationModel);
            };

            attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, new AttributeTransformationModel(FunctionOperationModel.GetCode()));
            attr1.TappedTriggered = (() =>
            attr1.Editing = Visibility.Visible);
            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            FunctionOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
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

                menuViewModel.NrRows = (int)Math.Ceiling(coll.Count / 3.0) + 1;
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
                            FunctionOperationModel.AttributeUsageTransformationModels.Remove(atm);
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


            OperationViewModelTapped += (sender, args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

        }

        public FunctionOperationViewModel(FunctionOperationModel functionOperationModel, bool fromMouse = false) : base(functionOperationModel)
        {
            addAttachmentViewModels();

            // fill-in UI specific to function's subtype
            if (FunctionOperationModel.FunctionSubtypeModel is MinMaxScaleFunctionSubtypeModel)
                createDummyParameterMenu();

            createBottomInputsExpandingMenu();
        }

        public FunctionOperationModel FunctionOperationModel => (FunctionOperationModel)OperationModel;
    }
}
