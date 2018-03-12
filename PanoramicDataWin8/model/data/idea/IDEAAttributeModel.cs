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
using PanoramicDataWin8.controller.view;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeModel : AttributeModel
    {
        
        static List<IDEAAttributeModel>  _allFieldAttributeModels = new List<IDEAAttributeModel>();

        static public bool NameExists(string name, OriginModel model)
        {
            return Function(name, model) != null;
        }

        static public AttributeModel AddCodeField(string rawName, string displayName, AttributeFuncModel.AttributeModelType attrType,
            DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints, OriginModel originModel)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeAssignedValueFuncModel(),
                dataType, inputVisualizationType, visualizationHints, originModel, false);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddCodeField(string rawName, string displayName, string code,
            DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints, OriginModel originModel)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeCodeFuncModel(code), 
                dataType, inputVisualizationType, visualizationHints, originModel, false);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddBackendField(string rawName, string displayName, string backendOperatorId,
            DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints, OriginModel originModel)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeBackendFuncModel(backendOperatorId), 
                dataType, inputVisualizationType, visualizationHints, originModel, false);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddColumnField(string rawName, string displayName, DataType dataType, 
            string inputVisualizationType, List<VisualizationHint> visualizationHints, OriginModel originModel, bool isTarget)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeColumnFuncModel(),
                dataType, inputVisualizationType, visualizationHints, originModel, isTarget);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }
        static public AttributeModel AddGroupField(string rawName, string displayName, OriginModel originModel)
        {
            var fieldModel = new IDEAAttributeModel(rawName, displayName, new AttributeFuncModel.AttributeGroupFuncModel(), 
                DataType.Undefined, InputVisualizationTypeConstants.VECTOR, new List<VisualizationHint>(), originModel, false);
            _allFieldAttributeModels.Add(fieldModel);
            return fieldModel;
        }

        static public IEnumerable<IDEAAttributeModel> GetAllCalculatedAttributeModels(OriginModel model)
        {
            return _allFieldAttributeModels.Where((fm) =>
                (fm.FuncModel is AttributeFuncModel.AttributeCodeFuncModel ||
                fm.FuncModel is AttributeFuncModel.AttributeAssignedValueFuncModel ||
                fm.FuncModel is AttributeFuncModel.AttributeBackendFuncModel) &&
                model == fm.OriginModel);
        }
        static public void RefactorFunctionName(string oldName, string newName)
        {
            foreach (var am in _allFieldAttributeModels)
            {
                am.Refactor(oldName, newName);
            }

            foreach (var rend in (MainViewController.Instance.InkableScene.Elements).Select((e) => 
                                         (e as OperationContainerView)?.Children?.First() as Renderer))
            {
                rend?.Refactor(oldName, newName);
            }
        }

        public delegate void CodeDefinitionChangedHandler(object sender);
        static public event CodeDefinitionChangedHandler CodeDefinitionChangedEvent;

        static public IDEAAttributeModel Function(string name, OriginModel model)
        {
            foreach (var func in GetAllCalculatedAttributeModels(model))
                if (func.RawName == name)
                    return func;
            return null;
        }

        public IDEAAttributeModel()
        { }
        public IDEAAttributeModel(string rawName, string displayName, AttributeFuncModel funcModel, DataType dataType, 
                              string inputVisualizationType, List<VisualizationHint> visualizationHints, 
                              OriginModel originModel, bool isTarget):base(rawName, displayName, funcModel, dataType, inputVisualizationType, visualizationHints, 
                                  originModel, isTarget)
        {
        }

        public void Refactor(string oldName, string newName)
        {
            var cfm = FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
            if (cfm != null)
                SetCode(cfm.RefactorVariable(oldName, newName), DataType, true);
        }

        public void SetCode(string code, DataType dataType, bool refactoring=false)
        {
            var codeFuncModel = FuncModel as AttributeFuncModel.AttributeCodeFuncModel;
            if (codeFuncModel != null) {
                var oldCode = codeFuncModel.Code;
                if (oldCode.Trim(' ') != code.Trim(' '))
                {
                    codeFuncModel.Code = code;
                    this.DataType = dataType;
                     if (!refactoring && CodeDefinitionChangedEvent != null)
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