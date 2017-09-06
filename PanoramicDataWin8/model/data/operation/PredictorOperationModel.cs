using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using IDEA_common.catalog;
using System.Collections.Generic;

namespace PanoramicDataWin8.model.data.operation
{
    public class PredictorOperationModel : AttributeUsageOperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;
        string _rawName;
        
        public PredictorOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeComputedFieldModel.NameExists(rawName))
            {
                IDEAAttributeComputedFieldModel.Add(rawName, displayName == null ? rawName : displayName, "0", DataType.String, "numeric",
                               new List<VisualizationHint>());
            }
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
        public void SetRawName(string name)
        {
            GetCode().RawName = name;
            _rawName = name;
            GetCode().DisplayName = name;
        }
        public IDEAAttributeComputedFieldModel GetCode()
        {
            return IDEAAttributeComputedFieldModel.Function(_rawName);
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}