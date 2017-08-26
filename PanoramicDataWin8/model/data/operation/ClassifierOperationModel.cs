using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.operation
{
    public class ClassifierOperationModel : AttributeUsageOperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        public ClassifierOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }
        public AttributeTransformationModel ClassifierAttributeUsageTransformationModel { get; set; }
        
        public FilteringOperation FilteringOperation
        {
            get { return _filterConsumerOperationModelImpl.FilteringOperation; }
            set { _filterConsumerOperationModelImpl.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> ConsumerLinkModels
        {
            get { return _filterConsumerOperationModelImpl.ConsumerLinkModels; }
            set { _filterConsumerOperationModelImpl.ConsumerLinkModels = value; }
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}