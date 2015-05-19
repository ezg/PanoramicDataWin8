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
            _idInputModel = new SimInputFieldModel("ID", InputDataTypeConstants.INT, InputVisualizationTypeConstants.NUMERIC);
            _idInputModel.OriginModel = this;
        }

        public void LoadInputFields()
        {
            _idInputModel.OriginModel = this;

            for (int i = 0; i < _datasetConfiguration.InputFieldNames.Count; i++)
            {
                InputFieldModel inputModel = new SimInputFieldModel(
                    _datasetConfiguration.InputFieldNames[i],
                    _datasetConfiguration.InputFieldDataTypes[i],
                    _datasetConfiguration.InputFieldVisualizationTypes[i]);
                inputModel.OriginModel = this;
                _inputModels.Add(inputModel);
            }

            _idInputModel.IsDisplayed = false;
            _inputModels.Add(_idInputModel);

            for (int i = 0; i < _inputModels.Count; i++)
            {
                if (_datasetConfiguration.InputFieldIsDisplayed.Count > i && !_datasetConfiguration.InputFieldIsDisplayed[i])
                {
                    _inputModels[i].IsDisplayed = false;
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

        private InputFieldModel _idInputModel = null;
        [JsonIgnore]
        public InputFieldModel IdInputModel
        {
            get
            {
                return _idInputModel;
            }
        }

        public override string Name
        {
            get
            {
                return _datasetConfiguration.Name;
            }
        }

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
