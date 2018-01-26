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
    public class FunctionGenericOperationModel : FunctionSubtypeModel
    {
        string _name;
        string _inputVisualizationType;
        DataType _outputDataType = DataType.Double;
        Dictionary<string, ObservableCollection<AttributeTransformationModel>> _datasetParameterGroups = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>();
        Dictionary<string, object> _valueParameters = new Dictionary<string, object>();
        public FunctionGenericOperationModel() { }
        public FunctionGenericOperationModel(
            DataType                   outputType = DataType.Object,
            string                     inputVisualizationType = null,
            string                     name = "<generic>",
            IEnumerable<string>        attributeParameterGroups = null,
            IEnumerable<Tuple<string,object>> valueParameters = null
            )
        {
            _name = name;
            _outputDataType = outputType;
            _inputVisualizationType = inputVisualizationType ?? InputVisualizationTypeConstants.NUMERIC;
            foreach (var v in attributeParameterGroups ?? new string[] { "P1", "P2" })
            {
                AttributeParameterGroups.Add(v, new ObservableCollection<AttributeTransformationModel>());
            }
            foreach (var v in valueParameters ?? new Tuple<string, object>[] { new Tuple<string, object>("dummy", 0.0) })
                ValueParameters.Add(v.Item1, v.Item2);
        }

        public string Name { get => _name; }
        public string InputVisualizationType { get => _inputVisualizationType; }
        public Dictionary<string, ObservableCollection<AttributeTransformationModel>> AttributeParameterGroups { get => _datasetParameterGroups;  }
        public Dictionary<string, object> ValueParameters { get => _valueParameters; }
        public DataType DataType { get => _outputDataType; }

        public void SetValue(string inputName, object value) { ValueParameters[inputName] = value; }
        public object GetValue(string inputName) { return ValueParameters[inputName]; }

        public string Code()
        {
            var code = Name+"(";
            if (AttributeParameterGroups != null)
                foreach (var p in AttributeParameterGroups)
                    foreach (var v in p.Value)
                        code += (p.Key + ":" + v.AttributeModel.RawName) + ",";
            if (ValueParameters != null)
                foreach (var p in ValueParameters)
                    code += (p.Key + "=" + p.Value) + ",";

            code = code.TrimEnd(',') + ")";
            return code;
        }
    }

    /// <summary>
    /// Example of a function subtype -- these should be created automatically from a pipeline script, etc
    /// </summary>
    public class MinMaxScaleFunctionSubtypeModel : FunctionGenericOperationModel
    {
        Dictionary<string, object> _valueParameters = new Dictionary<string, object>();
        public MinMaxScaleFunctionSubtypeModel() : base(
            DataType.Double,
            InputVisualizationTypeConstants.NUMERIC,
            "MinMaxScale",
            new string[] { "P1", "P2" },
            new Tuple<string, object>[] { new Tuple<string, object>("Dummy", 0.0) }
            )
        {

        }

        public double DummyValue { get { return (double)GetValue("Dummy"); } set { SetValue("Dummy", value); } }

    }
}
