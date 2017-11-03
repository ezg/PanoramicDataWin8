using System;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.data.operation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace PanoramicDataWin8.model.view
{
    public class InputGroupViewModel : AttributeTransformationViewModel
    {
        public InputGroupViewModel()
        {
        }
        
        public InputGroupViewModel(OperationViewModel operationViewModel, AttributeModel attributeGroupModel):
            base(operationViewModel, new AttributeTransformationModel(attributeGroupModel))
        {
        }
        
        public override Brush HighlightBrush => Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush;

        public override Brush NormalBrush => Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush;
        
    }
}