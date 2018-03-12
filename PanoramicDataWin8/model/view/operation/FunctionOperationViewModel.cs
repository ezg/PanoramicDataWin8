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
    public class FunctionOperationViewModel : LabeledOperationViewModel
    {
        private void createSliderParameterMenus(AttachmentViewModel attachmentViewModel, MenuViewModel parametersMenuViewModel, string name, double value, int ind)
        {
            var sliderItem = new MenuItemViewModel
            {
                MenuViewModel = parametersMenuViewModel,
                Row = ind,
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

            var attr1 = new SliderMenuItemComponentViewModel
            {
                Label = name,
                Value = value,
                MinValue = 1,
                MaxValue = 100
            };
            attr1.PropertyChanged += (sender, args) =>
            {
                var model = sender as SliderMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                    FunctionOperationModel.SetValue(name, model.FinalValue);
                attachmentViewModel.StartDisplayActivationStopwatch();
            };

            sliderItem.MenuItemComponentViewModel = attr1;
            parametersMenuViewModel.MenuItemViewModels.Add(sliderItem);
        }

        public ObservableCollection<AttributeTransformationModel> ExtraAttributeTransformationModelParameters { get; } = new ObservableCollection<AttributeTransformationModel>();

        public FunctionOperationViewModel(FunctionOperationModel functionOperationModel, bool fromMouse = false) : base(functionOperationModel)
        {
            var attachmentViewModel     = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            var parametersMenuViewModel = CreateParameterRightSideMenu(attachmentViewModel, FunctionOperationModel.ValueParameterPairs().Count());
            
            FunctionOperationModel.ValueParameterPairs().ForEach((p, ind) =>
                createSliderParameterMenus(attachmentViewModel, parametersMenuViewModel, p.Item1, (double)p.Item2, ind)
            );

            MenuItemViewModel menuItemViewModel;
            var dict = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>();
            foreach (var p in FunctionOperationModel.AttributeParameterGroups())
                dict.Add(p.Item1, p.Item2);
            var menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, dict, 30, 30, false, true, true, out menuItemViewModel);
            MenuItemViewModel labelViewModel;
            createAttributeLabelMenu(AttachmentOrientation.Bottom, FunctionOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(60, 50), 0, true, false, null, out labelViewModel);
         }

        private MenuViewModel CreateParameterRightSideMenu(AttachmentViewModel attachmentViewModel, int numParameters)
        {
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = numParameters,
                ClickToDismiss = true
            };
            attachmentViewModel.MenuViewModel = menuViewModel;
            return menuViewModel;
        }

        public FunctionOperationModel FunctionOperationModel => (FunctionOperationModel)OperationModel;
    }
}
