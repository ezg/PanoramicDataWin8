using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.operation
{
    public class ExampleOperationModel : OperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        private double _dummyValue = 50;

        private ExampleOperationType _exampleOperationType = ExampleOperationType.A;

        public ExampleOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }
        
        public double DummyValue
        {
            get { return _dummyValue; }
            set
            {
                SetProperty(ref _dummyValue, value);
                FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }

        public ExampleOperationType ExampleOperationType
        {
            get { return _exampleOperationType; }
            set
            {
                SetProperty(ref _exampleOperationType, value);
                FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }

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