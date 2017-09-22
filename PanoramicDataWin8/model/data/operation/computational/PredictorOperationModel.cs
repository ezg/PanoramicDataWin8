using System;
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
        string _rawName;

        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        public PredictorOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);

            _rawName = rawName;
            if (rawName != null && !IDEAAttributeModel.NameExists(rawName))
            {
                IDEAAttributeModel.AddBackendField(rawName, displayName == null ? rawName : displayName, null, DataType.Double, "numeric", new List<VisualizationHint>());
            }

            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }

        public AttributeTransformationModel TargetAttributeUsageTransformationModel { get; set; }
        public ObservableCollection<AttributeTransformationModel> IgnoredAttributeUsageTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();

        public void UpdateBackendOperatorId(string backendOperatorId)
        {
            IDEAAttributeModel attributeModel = GetAttributeModel();
            var funcModel = attributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeBackendFuncModel;
            funcModel.Id = backendOperatorId;
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
        public IDEAAttributeModel GetAttributeModel()
        {
            return IDEAAttributeModel.Function(_rawName);
        }

        public void SetRawName(string name)
        {
            var code = GetAttributeModel();
            code.DisplayName = code.RawName = _rawName = name;
        }

    }
}