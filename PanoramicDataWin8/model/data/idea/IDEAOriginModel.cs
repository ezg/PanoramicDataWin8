﻿using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAOriginModel : OriginModel
    {
        private DatasetConfiguration _datasetConfiguration;

        public IDEAOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;
        }

        public DatasetConfiguration DatasetConfiguration
        {
            get { return _datasetConfiguration; }
            set { SetProperty(ref _datasetConfiguration, value); }
        }

        public override string Name
        {
            get { return _datasetConfiguration.Schema.RawName; }
        }

        public long FileId { get; set; }

        public override List<AttributeModel> InputModels { get; } = new List<AttributeModel>();

        private void recursiveCreateAttributeModels(AttributeGroup attributeGroup, AttributeModel parentGroupModel)
        {
            var groupModel = IDEAAttributeModel.AddGroupField(attributeGroup.Name, attributeGroup.Name, this);
            // bcz: Group models no longer inherit from AttributeModels --- is this okay?
            if (parentGroupModel != null)
               (parentGroupModel.FuncModel as  AttributeModel.AttributeFuncModel.AttributeGroupFuncModel).InputModels.Add(groupModel);
            else
                InputModels.Add(groupModel);
            foreach (var childGroup in attributeGroup.AttributeGroups)
                recursiveCreateAttributeModels(childGroup, groupModel);
            foreach (var childAttribute in attributeGroup.Attributes)
                recursiveCreateAttributeModels(childAttribute, groupModel);
        }

        private void recursiveCreateAttributeModels(Attribute attribute, AttributeModel parentGroupModel)
        {
            var fieldAttributeModel = IDEAAttributeModel.AddColumnField(attribute.RawName, attribute.DisplayName,
                attribute.DataType,
                attribute.DataType == DataType.String ? "enum" : "numeric", attribute.VisualizationHints, this,
                attribute.IsTarget);

            if (parentGroupModel != null && parentGroupModel.FuncModel is AttributeModel.AttributeFuncModel.AttributeGroupFuncModel)
                (parentGroupModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeGroupFuncModel).InputModels.Add(fieldAttributeModel);
            else
                InputModels.Add(fieldAttributeModel);
        }

        public void LoadInputFields()
        {
            foreach (var attributeGroup in DatasetConfiguration.Schema.RootAttributeGroup.AttributeGroups)
                recursiveCreateAttributeModels(attributeGroup, null);
            foreach (var attribute in DatasetConfiguration.Schema.RootAttributeGroup.Attributes)
                recursiveCreateAttributeModels(attribute, null);
        }
    }
}