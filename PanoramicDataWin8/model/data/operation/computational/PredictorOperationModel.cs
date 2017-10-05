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

        public PredictorOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel, DataType.Double, "numeric", rawName, displayName)
        {
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
            IgnoredAttributeUsageTransformationModels.CollectionChanged += _ignoredAttributeUsageTransformationModels_CollectionChanged;
        }

        public AttributeTransformationModel TargetAttributeUsageTransformationModel { get; set; }
        public ObservableCollection<AttributeTransformationModel> IgnoredAttributeUsageTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();

        public void UpdateBackendOperatorId(string backendOperatorId)
        {
            IDEAAttributeModel attributeModel = GetAttributeModel();
            var funcModel = attributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeBackendFuncModel;
            funcModel.Id = backendOperatorId;

            attributeModel.DataType = TargetAttributeUsageTransformationModel.AttributeModel.DataType;
            attributeModel.InputVisualizationType = TargetAttributeUsageTransformationModel.AttributeModel.InputVisualizationType;
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

        private void _ignoredAttributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}