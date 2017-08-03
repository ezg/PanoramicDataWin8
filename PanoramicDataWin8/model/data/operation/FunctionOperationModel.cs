using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public interface FunctionSubtypeModel {
        DataType DataType { get; }
        string   Name { get; }
        string InputVisualizationType { get; }
        string Code(Dictionary<string, object> parameters);
    }

    public class MinMaxScaleFunctionSubtypeModel : FunctionSubtypeModel
    {
        public MinMaxScaleFunctionSubtypeModel()
        {
            Name = "MinMaxScale";
        }
        public DataType DataType {  get { return DataType.Double;  } }
        public string Name { get; }
        public string InputVisualizationType { get { return InputVisualizationTypeConstants.NUMERIC; } }

        public double DummyValue { get; set;  }

        public string Code(Dictionary<string,object> parameters)
        {
            var code = "MinMax(";
            if (parameters != null)
                foreach (var p in parameters)
                    code += (p.Key + ":" + (p.Value as AttributeTransformationModel).AttributeModel.RawName) + ",";

            code = code.TrimEnd(',') + ")";
            return "0";
            return code;
        }
    }

    public class FunctionOperationModel : AttributeUsageOperationModel
    {

        public FunctionSubtypeModel FunctionSubtypeModel;
        string       _rawName;
        public FunctionOperationModel(SchemaModel schemaModel, FunctionSubtypeModel functionSubtypeModel, string displayName = null) : base(schemaModel)
        {
            FunctionSubtypeModel = functionSubtypeModel;
            _rawName = FunctionSubtypeModel.Name + new Random().Next();
            DisplayName = DisplayName == null ? FunctionSubtypeModel.Name : displayName;
            if (_rawName != null && !IDEAAttributeComputedFieldModel.NameExists(_rawName))
            {
                IDEAAttributeComputedFieldModel.Add(_rawName, DisplayName, FunctionSubtypeModel.Code(null), FunctionSubtypeModel.DataType, FunctionSubtypeModel.InputVisualizationType,
                               new List<VisualizationHint>());
            }
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }

        public string DisplayName { get; set; }
        
        public void SetParameters(Dictionary<string,object> parameters)
        {
            GetCode().SetCode(FunctionSubtypeModel.Code(parameters), FunctionSubtypeModel.DataType);
        }
        public void SetRawName(string name)
        {
            GetCode().RawName = name;
            _rawName = name;
            GetCode().DisplayName = name;
        }
        public IDEAAttributeComputedFieldModel GetCode()
        {
            return IDEAAttributeComputedFieldModel.Function(_rawName);
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var parameters = new Dictionary<string, object>();
            foreach (var attributeUsageTransformationModel in AttributeUsageTransformationModels)
                parameters.Add(attributeUsageTransformationModel.AttributeModel.DisplayName, attributeUsageTransformationModel);
            SetParameters(parameters);
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}