using System.Collections.Generic;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.model.data.tuppleware
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TuppleWareOriginModel : OriginModel
    {
        public TuppleWareOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;
            
        }
        
       /* private static void recursiveCreateAttributeModels(JToken token, TuppleWareInputGroupModel parentGroupModel, TuppleWareOriginModel tuppleWareOriginModel)
        {
            if (token is JArray)
            {
                if (token[0] is JValue)
                {
                    if (token[1] is JValue)
                    {
                        TuppleWareFieldInputModel fieldInputModel = new TuppleWareFieldInputModel(token[0].ToString(), "float", token[1].ToString().ToLower() == "true" ? "numeric" : "enum");
                        fieldInputModel.OriginModel = tuppleWareOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(fieldInputModel);
                        }
                        else
                        {
                            tuppleWareOriginModel.InputModels.Add(fieldInputModel);
                        }
                    }
                    else
                    {
                        TuppleWareInputGroupModel groupModel = new TuppleWareInputGroupModel(token[0].ToString());
                        groupModel.OriginModel = tuppleWareOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(groupModel);
                        }
                        else
                        {
                            tuppleWareOriginModel.InputModels.Add(groupModel);
                        }
                        foreach (var child in token[1])
                        {
                            recursiveCreateAttributeModels(child, groupModel, tuppleWareOriginModel);
                        }
                    }
                }
            }
        */
        public void LoadInputFields()
        {
           /* _idInputModel.OriginModel = this;

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
            }*/
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
                return _datasetConfiguration.Name;
            }
        }

        public long FileId{ get; set; }

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
