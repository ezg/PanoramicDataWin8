using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using IDEA_common.catalog;
using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.operation
{
    public class PredictorOperationModel : ComputationalOperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;
        
        public PredictorOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel, "0", DataType.String, "numeric", rawName, displayName)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }
        public AttributeTransformationModel TargetAttributeUsageTransformationModel { get; set; }
        public ObservableCollection<AttributeTransformationModel> IgnoredAttributeUsageTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();


        public void UpdateCode()
        {
            //GetCode().VisualizationHints = new List<IDEA_common.catalog.VisualizationHint>(new IDEA_common.catalog.VisualizationHint[] { IDEA_common.catalog.VisualizationHint.TreatAsEnumeration });

            GetCode().SetCode("0", DataType.String);
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