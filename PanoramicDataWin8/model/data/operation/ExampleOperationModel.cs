using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PanoramicDataWin8.model.data.operation
{
    public class ExampleOperationModel : OperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        public ExampleOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }

        public ObservableCollection<AttributeTransformationModel> AttributeUsageTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();

        public FilteringOperation FilteringOperation
        {
            get { return _filterConsumerOperationModelImpl.FilteringOperation; }
            set { _filterConsumerOperationModelImpl.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _filterConsumerOperationModelImpl.LinkModels; }
            set { _filterConsumerOperationModelImpl.LinkModels = value; }
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}