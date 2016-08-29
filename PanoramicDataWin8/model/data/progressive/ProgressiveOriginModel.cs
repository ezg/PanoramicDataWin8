using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.catalog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;
using Attribute = IDEA_common.catalog.Attribute;

namespace PanoramicDataWin8.model.data.progressive
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressiveOriginModel : OriginModel
    {
        public ProgressiveOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;

        }

        private void recursiveCreateAttributeModels(AttributeGroup attributeGroup, ProgressiveInputGroupModel parentGroupModel)
        {
            ProgressiveInputGroupModel groupModel = new ProgressiveInputGroupModel(attributeGroup.Name.ToString(), attributeGroup.Name.ToString());
            groupModel.OriginModel = this;
            if (parentGroupModel != null)
            {
                parentGroupModel.InputModels.Add(groupModel);
            }
            else
            {
                this.InputModels.Add(groupModel);
            }
            foreach (var childGroup in attributeGroup.AttributeGroups)
            {
                recursiveCreateAttributeModels(childGroup, groupModel);
            }
            foreach (var childAttribute in attributeGroup.Attributes)
            {
                recursiveCreateAttributeModels(childAttribute, groupModel);
            }
        }

        private void recursiveCreateAttributeModels(IDEA_common.catalog.Attribute attribute, ProgressiveInputGroupModel parentGroupModel)
        {
            ProgressiveFieldInputModel fieldInputModel = new ProgressiveFieldInputModel(attribute.RawName, attribute.DisplayName,
                                InputDataTypeConstants.FromType(attribute.DataType),
                                InputDataTypeConstants.FromType(attribute.DataType) == InputDataTypeConstants.NVARCHAR ? "enum" : "numeric");
            fieldInputModel.OriginModel = this;

            if (parentGroupModel != null)
            {
                parentGroupModel.InputModels.Add(fieldInputModel);
            }
            else
            {
                this.InputModels.Add(fieldInputModel);
            }
        }

        public void LoadInputFields()
        {
            foreach (AttributeGroup attributeGroup in DatasetConfiguration.Schema.RootAttributeGroup.AttributeGroups)
            {
                recursiveCreateAttributeModels(attributeGroup, null);
            }
            foreach (Attribute attribute in DatasetConfiguration.Schema.RootAttributeGroup.Attributes)
            {
                recursiveCreateAttributeModels(attribute, null);
            }
        }

        private DatasetConfiguration _datasetConfiguration = null;
        public DatasetConfiguration DatasetConfiguration
        {
            get
            {
                return _datasetConfiguration;
            }
            set
            {
                this.SetProperty(ref _datasetConfiguration, value);
            }
        }

        public override string Name
        {
            get
            {
                return _datasetConfiguration.Schema.RawName;
            }
        }

        public long FileId { get; set; }

        private List<InputModel> _inputModels = new List<InputModel>();
        public override List<InputModel> InputModels
        {
            get
            {
                return _inputModels;
            }
        }

        private List<OriginModel> _originModels = new List<OriginModel>();
        public override List<OriginModel> OriginModels
        {
            get
            {
                return _originModels;
            }
        }
    }
}
