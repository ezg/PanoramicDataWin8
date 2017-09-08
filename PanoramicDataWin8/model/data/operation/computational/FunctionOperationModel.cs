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

    public class FunctionOperationModel : ComputationalOperationModel
    {

        public FunctionSubtypeModel FunctionSubtypeModel;
        public FunctionOperationModel(SchemaModel schemaModel, FunctionSubtypeModel functionSubtypeModel, string displayName = null) : 
            base(schemaModel, "0", functionSubtypeModel.DataType, functionSubtypeModel.InputVisualizationType,
                 functionSubtypeModel.Name + new Random().Next(), displayName == null ? functionSubtypeModel.Name : displayName)

        {
            FunctionSubtypeModel = functionSubtypeModel;
            AttributeUsageTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }
        
        public void SetParameters(Dictionary<string,object> parameters)
        {
            GetAttributeModel().SetCode(FunctionSubtypeModel.Code(parameters), FunctionSubtypeModel.DataType);
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