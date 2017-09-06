using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;
using System;
using PanoramicDataWin8.model.data.idea;
using System.Collections.Generic;
using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data.operation
{
    public class AttributeGroupOperationModel : AttributeUsageOperationModel, IFilterConsumerOperationModel
    {
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;

        AttributeModel attributeGroupModel;

        public AttributeModel AttributeModel {  get { return attributeGroupModel;  } }

        public AttributeModel.AttributeFuncModel.AttributeGroupFuncModel GroupFuncModel
        {
            get
            {
                return AttributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeGroupFuncModel;
            }
        }

        public AttributeGroupOperationModel(SchemaModel schemaModel, string rawName) : base(schemaModel)
        {
            attributeGroupModel = IDEAAttributeModel.AddGroupField(rawName, rawName);
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }
        private string _name;

        public void SetName(string name)
        {
            attributeGroupModel.RawName = name;
            attributeGroupModel.DisplayName = name;
            SetProperty<string>(ref _name, name);
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
            foreach (var attributeUsageTransformationModel in AttributeUsageTransformationModels)
                GroupFuncModel.InputModels.Add(attributeUsageTransformationModel.AttributeModel);
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}