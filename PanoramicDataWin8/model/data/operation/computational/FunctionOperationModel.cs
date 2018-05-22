using IDEA_common.catalog;
using IDEA_common.operations.ml.optimizer;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class FunctionOperationModel : ComputationalOperationModel
    {
        Dictionary<string, ObservableCollection<AttributeTransformationModel>> _attributeParameterGroups = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>();
        Dictionary<string, object>                                             _valueParameters        = new Dictionary<string, object>();
       
        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GetAttributeModel().SetCode(Code(), GetAttributeModel().DataType);
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }

        public FunctionOperationModel(OriginModel schemaModel, DataType outputType, string inputVisualizationType,
                    IEnumerable<string> attributeParameterGroups = null,
                    IEnumerable<Tuple<string, object>> valueParameters = null, string rawName = null, string displayName = null) :
        base(schemaModel, 
            outputType,
            AttributeFuncModel.AttributeModelType.Code,
            inputVisualizationType,
            rawName,
            displayName == null ? rawName : displayName)
        {
            foreach (var v in attributeParameterGroups ?? new string[] { })
            {
                var observableAttributeTransformationModels = new ObservableCollection<AttributeTransformationModel>();
                _attributeParameterGroups.Add(v, observableAttributeTransformationModels);
                observableAttributeTransformationModels.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
            }
            foreach (var v in valueParameters ?? new Tuple<string, object>[] { })
                _valueParameters.Add(v.Item1, v.Item2);
        }


        public IEnumerable<Tuple<string, ObservableCollection<AttributeTransformationModel>>> AttributeParameterGroups()
        {
            return _attributeParameterGroups.Select((am) => new Tuple<string, ObservableCollection<AttributeTransformationModel>>(am.Key, am.Value));
        }
        public IEnumerable<Tuple<string, object>> ValueParameterPairs() {
            return _valueParameters.Select((vm) => new Tuple<string,object>(vm.Key,vm.Value));
        }

        public void SetValue(string inputName, object value) { _valueParameters[inputName] = value; }
        public object GetValue(string inputName)             { return _valueParameters[inputName]; }

        public string Code()
        {
            var code = "";
            if (_attributeParameterGroups != null)
                foreach (var p in _attributeParameterGroups)
                    foreach (var v in p.Value)
                        code += (p.Key + ":" + v.AttributeModel.RawName) + ",";
            if (_valueParameters != null)
                foreach (var p in _valueParameters)
                    code += (p.Key + "=" + p.Value) + ",";
            return code;
        }
    }

    /// <summary>
    /// Example of a filled in function -- these should be created automatically from a pipeline script, etc
    /// </summary>
    public class MinMaxScaleFunctionModel : FunctionOperationModel
    {
        public MinMaxScaleFunctionModel(OriginModel schemaModel, string name="MinMaxScale") : base(
            schemaModel,
            DataType.Double,
            InputVisualizationTypeConstants.NUMERIC,
            new string[] { "P1", "P2" },
            new Tuple<string, object>[] { new Tuple<string, object>("Dummy", 0.0), new Tuple<string, object>("Yummy", 1.0) },
            name,
            name
            )
        {

        }
        // public double DummyValue { get { return (double)GetValue("Dummy"); } set { SetValue("Dummy", value); } }
    }

    /// <summary>
    /// Example of a filled in function -- these should be created automatically from a pipeline script, etc
    /// </summary>
    public class PipelineFunctionModel : FunctionOperationModel
    {
        public PipelineDescription PipelineDescription = null;
        public PipelineFunctionModel(OriginModel schemaModel, PipelineDescription pipelineDescription, string name="Pipeline") : base(
            schemaModel,
            DataType.Double,
            InputVisualizationTypeConstants.NUMERIC,
            new string[] { /*"P1", "P2" */},
            new Tuple<string, object>[] { new Tuple<string, object>("Dummy", 0.0), new Tuple<string, object>("Yummy", 1.0) },
            name,
            name
            )
        {
            PipelineDescription = pipelineDescription;
        }
        // public double DummyValue { get { return (double)GetValue("Dummy"); } set { SetValue("Dummy", value); } }
    }
}