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
    public class DefinitionOperationViewModel : AttributeUsageOperationViewModel
    {
        public DefinitionOperationModel DefinitionOperationModel {  get { return OperationModel as DefinitionOperationModel;  } }
        public DefinitionOperationViewModel(DefinitionOperationModel histogramOperationModel, bool fromMouse=false) : base(histogramOperationModel)
        {
            addAttachmentViewModels();
            AttributeMenuItemViewModel attributeMenuItemViewModel;
            createLabelMenu(AttachmentOrientation.Bottom, DefinitionOperationModel.GetAttributeModel(), 
                 AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out attributeMenuItemViewModel);
        }
    }
}
