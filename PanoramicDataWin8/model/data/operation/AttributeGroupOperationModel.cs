using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PanoramicDataWin8.model.data.attribute;
using System;
using PanoramicDataWin8.model.data.idea;
using System.Collections.Generic;
using System.Linq;
using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data.operation
{
    public class AttributeGroupOperationModel : OperationModel
    {

        AttributeModel attributeGroupModel;

        public AttributeModel AttributeModel {  get { return attributeGroupModel;  } }

        public AttributeModel.AttributeFuncModel.AttributeGroupFuncModel GroupFuncModel
        {
            get
            {
                return AttributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeGroupFuncModel;
            }
        }

        public AttributeGroupOperationModel(SchemaModel schemaModel, string rawName, AttributeModel groupModel) : base(schemaModel)
        {
            attributeGroupModel = groupModel ?? IDEAAttributeModel.AddGroupField(rawName, rawName, schemaModel.OriginModels.First());
            GroupFuncModel.InputModels.CollectionChanged += GroupFuncModel_CollectionChanged;

            foreach (var am in GroupFuncModel.InputModels)
                AttributeTransformationModelParameters.Add(new AttributeTransformationModel(am));
            AttributeTransformationModelParameters.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }

        public override void Dispose()
        {
        }

        private void GroupFuncModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AttributeTransformationModelParameters.Count != GroupFuncModel.InputModels.Count)
            {
                AttributeTransformationModelParameters.CollectionChanged -= _attributeUsageTransformationModels_CollectionChanged;
                AttributeTransformationModelParameters.Clear();
                foreach (var am in GroupFuncModel.InputModels)
                    AttributeTransformationModelParameters.Add(new AttributeTransformationModel(am));
                AttributeTransformationModelParameters.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
            }
        }

        private string _name;

        public void SetName(string name)
        {
            attributeGroupModel.RawName = name;
            attributeGroupModel.DisplayName = name;
            SetProperty<string>(ref _name, name);
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GroupFuncModel.InputModels.CollectionChanged -= GroupFuncModel_CollectionChanged;
            GroupFuncModel.InputModels.Clear();
            foreach (var attributeUsageModel in AttributeTransformationModelParameters)
                GroupFuncModel.InputModels.Add(attributeUsageModel.AttributeModel);
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            GroupFuncModel.InputModels.CollectionChanged += GroupFuncModel_CollectionChanged;
        }
    }
}