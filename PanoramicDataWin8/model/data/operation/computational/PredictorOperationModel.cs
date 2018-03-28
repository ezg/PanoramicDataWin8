using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using IDEA_common.catalog;
using System.Collections.Generic;
using System.Linq;

namespace PanoramicDataWin8.model.data.operation
{
    public class PredictorOperationModel : ComputationalOperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        public PredictorOperationModel(OriginModel schemaModel, string rawName, string displayName = null) : base(schemaModel, DataType.Double, "numeric", rawName, displayName)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            
            AttributeTransformationModelParameters.CollectionChanged += _attributeUsageModels_CollectionChanged;
            IgnoredAttributeTransformationModels.CollectionChanged += _ignoredAttributeUsageModels_CollectionChanged;
        }

        public AttributeModel TargetAttributeUsageModel { get; set; }
        public ObservableCollection<AttributeTransformationModel> IgnoredAttributeTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();

        public void UpdateBackendOperatorId(string backendOperatorId)
        {
            IDEAAttributeModel attributeModel = GetAttributeModel();
            var funcModel = attributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeBackendFuncModel;
            funcModel.Id = backendOperatorId;

            attributeModel.DataType = TargetAttributeUsageModel.DataType;
            attributeModel.InputVisualizationType = TargetAttributeUsageModel.InputVisualizationType;
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

        private void _attributeUsageModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }

        private void _ignoredAttributeUsageModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}