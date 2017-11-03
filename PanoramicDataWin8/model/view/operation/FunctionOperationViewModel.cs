﻿using PanoramicDataWin8.model.data.attribute;
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
    public class FunctionOperationViewModel : AttributeUsageOperationViewModel
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

            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            sliderItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(sliderItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private void createFunctionMenu(AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);
            OperationViewModelTapped += (args) => attachmentViewModel.ActiveStopwatch.Restart();

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
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
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    Label = "Apply",
                    DisplayOnTap = true,
                    AttributeViewModel = new AttributeViewModel(this, FunctionOperationModel.GetAttributeModel())
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);
        }

        public FunctionOperationViewModel(FunctionOperationModel functionOperationModel, bool fromMouse = false) : base(functionOperationModel)
        {
            addAttachmentViewModels();

            // fill-in UI specific to function's subtype
            if (FunctionOperationModel.FunctionSubtypeModel is MinMaxScaleFunctionSubtypeModel)
                ; // createDummyParameterMenu();

            createTopInputsExpandingMenu();
            createFunctionMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);
        }

        public FunctionOperationModel FunctionOperationModel => (FunctionOperationModel)OperationModel;
    }
}
