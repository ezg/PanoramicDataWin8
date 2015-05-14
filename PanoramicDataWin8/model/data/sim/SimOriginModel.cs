using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.model.data.sim
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SimOriginModel : OriginModel
    {
        public SimOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;
            _idAttributeModel = new SimAttributeModel("ID", AttributeDataTypeConstants.INT, AttributeVisualizationTypeConstants.NUMERIC);
            _idAttributeModel.OriginModel = this;
        }

        public void LoadAttributes()
        {
            _idAttributeModel.OriginModel = this;

            for (int i = 0; i < _datasetConfiguration.AttributeNames.Count; i++)
            {
                AttributeModel attributeModel = new SimAttributeModel(
                    _datasetConfiguration.AttributeNames[i],
                    _datasetConfiguration.AttributeDataTypes[i],
                    _datasetConfiguration.AttributeVisualizationTypes[i]);
                attributeModel.OriginModel = this;
                _attributeModels.Add(attributeModel);
            }

            _idAttributeModel.IsDisplayed = false;
            _attributeModels.Add(_idAttributeModel);

            for (int i = 0; i < _attributeModels.Count; i++)
            {
                if (_datasetConfiguration.AttributeIsDisplayed.Count > i && !_datasetConfiguration.AttributeIsDisplayed[i])
                {
                    _attributeModels[i].IsDisplayed = false;
                }
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

        private AttributeModel _idAttributeModel = null;
        [JsonIgnore]
        public AttributeModel IdAttributeModel
        {
            get
            {
                return _idAttributeModel;
            }
        }

        public override string Name
        {
            get
            {
                return _datasetConfiguration.Name;
            }
        }

        private List<AttributeModel> _attributeModels = new List<AttributeModel>();
        public override List<AttributeModel> AttributeModels
        {
            get
            {
                return _attributeModels;
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
