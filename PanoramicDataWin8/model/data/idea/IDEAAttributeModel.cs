using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using IDEA_common.operations;
using System.Linq;
using PanoramicDataWin8.model.data.operation;
using System.Text.RegularExpressions;
using PanoramicDataWin8.model.view.operation;
using Windows.UI.Xaml.Controls;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.view.vis.render;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeModel : AttributeModel
    {
        
        static List<IDEAAttributeModel>  _allFieldAttributeModels = new List<IDEAAttributeModel>();

        static public bool NameExists(string name)
        {
            return Function(name) != null;
        }

        static public AttributeModel AddCodeField(string rawName, string displayName, string code, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeCodeFuncModel(code), dataType, inputVisualizationType, visualizationHints);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddBackendField(string rawName, string displayName, string backendOperatorId, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeBackendFuncModel(backendOperatorId), dataType, inputVisualizationType, visualizationHints);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddColumnField(string rawName, string displayName, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeColumnFuncModel(), dataType, inputVisualizationType, visualizationHints);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddGroupField(string rawName, string displayName)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeGroupFuncModel(), DataType.Undefined, InputVisualizationTypeConstants.VECTOR, new List<VisualizationHint>());
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }

        static public IEnumerable<IDEAAttributeModel> GetAllCalculatedAttributeModels()
        {
            return _allFieldAttributeModels.Where((fm) =>
                fm.FuncModel is AttributeFuncModel.AttributeCodeFuncModel ||
                fm.FuncModel is AttributeFuncModel.AttributeBackendFuncModel);
        }
        static public void RefactorFunctionName(string oldName, string newName)
        {
            foreach (var am in _allFieldAttributeModels.Where((fm) =>
                fm.FuncModel is AttributeFuncModel.AttributeCodeFuncModel))
            {
                var cfm = am.FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
                am.SetCode(cfm.RefactorVariable(oldName,newName), am.DataType);
            }
            var x = (controller.view.MainViewController.Instance.InkableScene.Elements).Where((e) => e is OperationContainerView && (e as OperationContainerView)?.Children?.First() is FilterRenderer);
            foreach (var fm in x)
            {
                var frend = (fm as OperationContainerView).Children.First() as FilterRenderer;
                var newFilterCode = AttributeFuncModel.AttributeCodeFuncModel.TransformCode(frend.ExpressionTextBox.Text, oldName, newName).Item1;
                frend.ExpressionTextBox.Text = newFilterCode;
            }
        }

        public delegate void CodeDefinitionChangedHandler(object sender);
        static public event CodeDefinitionChangedHandler CodeDefinitionChangedEvent;

        static public IDEAAttributeModel Function(string name)
        {
            foreach (var func in GetAllCalculatedAttributeModels())
                if (func.RawName == name)
                    return func;
            return null;
        }

        public IDEAAttributeModel()
        { }
        public IDEAAttributeModel(string rawName, string displayName, AttributeFuncModel funcModel, DataType dataType, 
                              string inputVisualizationType, List<VisualizationHint> visualizationHints):base(rawName, displayName, funcModel, dataType, inputVisualizationType, visualizationHints)
        {
        }

        public void SetCode(string code, DataType dataType)
        {
            var codeFuncModel = FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
            if (codeFuncModel != null) {
                var oldCode = codeFuncModel.Code;
                if (oldCode != code)
                {
                    codeFuncModel.Code = code;
                    this.DataType = dataType;
                     if (CodeDefinitionChangedEvent != null)
                        CodeDefinitionChangedEvent(this);
                }
            }
        }

        public string GetCode()
        {
            var codeFuncModel = FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
            if (codeFuncModel != null)
            {
                return codeFuncModel.Code;
            }
            return "";
        }
    }
}