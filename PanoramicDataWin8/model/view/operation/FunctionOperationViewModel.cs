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
    public class FunctionOperationViewModel : AttributeUsageOperationViewModel
    {
        private void createDummyParameterMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            attachmentViewModel.ShowOnAttributeTapped = true;

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
                attachmentViewModel.StartDisplayActivationStopwatch();
            };

            sliderItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(sliderItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        
        public FunctionOperationViewModel(FunctionOperationModel functionOperationModel, bool fromMouse = false) : base(functionOperationModel)
        {
            // fill-in UI specific to function's subtype
            if (FunctionOperationModel.FunctionSubtypeModel is MinMaxScaleFunctionSubtypeModel)
                ; // createDummyParameterMenu();

            createTopInputsExpandingMenu();
            createApplyAttributeMenu(FunctionOperationModel.GetAttributeModel(), AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(60, 50), 0, false, false);
        }

        public FunctionOperationModel FunctionOperationModel => (FunctionOperationModel)OperationModel;
    }
}
