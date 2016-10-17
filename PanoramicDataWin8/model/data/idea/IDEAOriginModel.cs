using System.Collections.Generic;
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

        public override List<OriginModel> OriginModels { get; } = new List<OriginModel>();

        private void recursiveCreateAttributeModels(AttributeGroup attributeGroup, IDEAAttributeGroupModel parentGroupModel)
        {
            var groupModel = new IDEAAttributeGroupModel(attributeGroup.Name, attributeGroup.Name);
            groupModel.OriginModel = this;
            if (parentGroupModel != null)
                parentGroupModel.InputModels.Add(groupModel);
            else
                InputModels.Add(groupModel);
            foreach (var childGroup in attributeGroup.AttributeGroups)
                recursiveCreateAttributeModels(childGroup, groupModel);
            foreach (var childAttribute in attributeGroup.Attributes)
                recursiveCreateAttributeModels(childAttribute, groupModel);
        }

        private void recursiveCreateAttributeModels(Attribute attribute, IDEAAttributeGroupModel parentGroupModel)
        {
            var fieldAttributeModel = new IDEAFieldAttributeModel(attribute.RawName, attribute.DisplayName, attribute.Index,
                InputDataTypeConstants.FromDataType(attribute.DataType),
                InputDataTypeConstants.FromDataType(attribute.DataType) == InputDataTypeConstants.NVARCHAR ? "enum" : "numeric");
            fieldAttributeModel.OriginModel = this;

            if (parentGroupModel != null)
                parentGroupModel.InputModels.Add(fieldAttributeModel);
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