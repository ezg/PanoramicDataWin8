using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.model.data.progressive
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressiveOriginModel : OriginModel
    {
        public ProgressiveOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;

        }

        private void recursiveCreateAttributeModels(JToken token, ProgressiveInputGroupModel parentGroupModel, ProgressiveOriginModel progressiveOriginModel)
        {
            if (token is JArray)
            {
                if (token[0] is JValue)
                {
                    if (token[1] is JValue)
                    {
                        var datatype = InputDataTypeConstants.NVARCHAR;
                        if (token[1].ToString().ToLower() == "int64")
                        {
                            datatype = InputDataTypeConstants.INT;
                        }
                        else if (token[1].ToString().ToLower() == "float64")
                        {
                            datatype = InputDataTypeConstants.FLOAT;
                        }
                        if (!token[1].ToString().ToLower().StartsWith("unnamed"))
                        {
                            ProgressiveFieldInputModel fieldInputModel = new ProgressiveFieldInputModel(token[0].ToString(),
                                datatype,
                                token[1].ToString().ToLower() == "object" ? "enum" : "numeric");
                            fieldInputModel.OriginModel = progressiveOriginModel;

                            if (parentGroupModel != null)
                            {
                                parentGroupModel.InputModels.Add(fieldInputModel);
                            }
                            else
                            {
                                progressiveOriginModel.InputModels.Add(fieldInputModel);
                            }
                        }
                    }
                    else
                    {
                        ProgressiveInputGroupModel groupModel = new ProgressiveInputGroupModel(token[0].ToString());
                        groupModel.OriginModel = progressiveOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(groupModel);
                        }
                        else
                        {
                            progressiveOriginModel.InputModels.Add(groupModel);
                        }
                        foreach (var child in token[1])
                        {
                            recursiveCreateAttributeModels(child, groupModel, progressiveOriginModel);
                        }
                    }
                }
            }
        }

        public void LoadInputFields()
        {
            foreach (var child in DatasetConfiguration.SchemaJson["schema"])
            {
                recursiveCreateAttributeModels(child, null, this);
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
                return _datasetConfiguration.Name;
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
