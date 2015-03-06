using Newtonsoft.Json;
using PanoramicData.controller.input;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PanoramicData.model.data.sim
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SimOriginModel : OriginModel
    {
        public SimOriginModel(DatasetConfiguration datasetConfiguration)
        {
            _datasetConfiguration = datasetConfiguration;
            _idAttributeModel = new SimAttributeModel("ID", AttributeDataTypeConstants.INT, AttributeVisualizationTypeConstants.NUMERIC);
            
        }

        public async void LoadData()
        {
            _idAttributeModel.OriginModel = this;
            var installedLoc = Package.Current.InstalledLocation;

            StorageFile file = await StorageFile.GetFileFromPathAsync(installedLoc.Path + "\\" + _datasetConfiguration.DataFile);
            StreamReader streamReader = new StreamReader(await file.OpenStreamForReadAsync());

            List<string> names = CSVParser.CSVLineSplit(await streamReader.ReadLineAsync());
            List<string> dataTypes = CSVParser.CSVLineSplit(await streamReader.ReadLineAsync());
            List<string> visualizationTypes = CSVParser.CSVLineSplit(await streamReader.ReadLineAsync());

            for (int i = 0; i < names.Count; i++)
            {
                AttributeModel attributeModel = new SimAttributeModel(names[i], dataTypes[i], visualizationTypes[i]);
                attributeModel.OriginModel = this;
                _attributeModels.Add(attributeModel);
            }
            string line = await streamReader.ReadLineAsync();
            int count = 0;
            while (line != null)
            {
                Dictionary<AttributeModel, object> items = new Dictionary<AttributeModel, object>();
                items[_idAttributeModel] = count;

                List<string> values = CSVParser.CSVLineSplit(line);
                for (int i = 0; i < values.Count; i++)
                {
                    object value = null;
                    if (_attributeModels[i].AttributeDataType == AttributeDataTypeConstants.NVARCHAR)
                    {
                        value = values[i].ToString();
                    }
                    else if (_attributeModels[i].AttributeDataType == AttributeDataTypeConstants.FLOAT)
                    {
                        double d = 0.0;
                        if (double.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    else if (_attributeModels[i].AttributeDataType == AttributeDataTypeConstants.INT)
                    {
                        int d = 0;
                        if (int.TryParse(values[i].ToString(), out d))
                        {
                            value = d;
                        }
                    }
                    if (value == null || value.ToString().Trim() == "")
                    {
                        value = null;
                    }
                    items[_attributeModels[i]] = value;
                }
                _data.Add(items);
                line = await streamReader.ReadLineAsync();
                count++;
            }

            _attributeModels.Add(_idAttributeModel);
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

        private List<Dictionary<AttributeModel, object>> _data = new List<Dictionary<AttributeModel, object>>();
        [JsonIgnore]
        public List<Dictionary<AttributeModel, object>> Data
        {
            get
            {
                return _data;
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
