using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view.operation
{
    public class CalculationOperationViewModel : AttributeUsageOperationViewModel
    {
        MenuItemViewModel CalculationNameMenuItemViewModel = null;
        public CalculationOperationModel CalculationOperationModel { get { return OperationModel as CalculationOperationModel;  } }      
        public CalculationOperationViewModel(CalculationOperationModel operationModel, bool fromMouse = false) : base(operationModel)
        {
            createAttributeLabelMenu(AttachmentOrientation.Bottom, CalculationOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out CalculationNameMenuItemViewModel);
        }
    }
}
