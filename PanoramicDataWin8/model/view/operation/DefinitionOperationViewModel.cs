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
    public class DefinitionOperationViewModel : OperationViewModel
    {
        public DefinitionOperationModel DefinitionOperationModel {  get { return OperationModel as DefinitionOperationModel;  } }
        private void createDefinitionMenu(AttachmentOrientation attachmentOrientation,
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
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = DefinitionOperationModel.GetCode().DisplayName
            };

            DefinitionOperationModel.SetRawName(attr1.Label);
            attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, new AttributeTransformationModel(DefinitionOperationModel.GetCode()));
            attr1.TappedTriggered = (() => attr1.Editing = Visibility.Visible);
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        public DefinitionOperationViewModel(DefinitionOperationModel histogramOperationModel, bool fromMouse=false) : base(histogramOperationModel)
        {
            addAttachmentViewModels();
            createDefinitionMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
        }
    }
}
