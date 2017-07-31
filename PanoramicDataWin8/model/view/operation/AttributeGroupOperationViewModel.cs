using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeGroupOperationViewModel : OperationViewModel
    {
        public AttributeGroupOperationViewModel(AttributeGroupOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
        }

        public AttributeGroupOperationModel AttributeGroupOperationModel => (AttributeGroupOperationModel)OperationModel;
    }
}