using IDEA_common.catalog;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.view.vis.render;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class ComputationalOperationModel : AttributeUsageOperationModel
    {
        string _rawName;

        public void UpdateName()
        {
            var attrModel = GetAttributeModel();
            if (attrModel?.FuncModel is AttributeFuncModel.AttributeCodeFuncModel)
            {
                var str = "(";
                foreach (var n in (attrModel.FuncModel as AttributeFuncModel.AttributeCodeFuncModel).Terms)
                    if (n != null)
                        str += n + ",";
                str = str.TrimEnd(',') + ")";
                var newName = new Regex("\\(.*\\)", RegexOptions.Compiled).Replace(attrModel.RawName, str);
                if (attrModel.RawName != newName)
                {
                    RefactorFunctionName(newName);
                }
            }
        }
        public void RefactorFunctionName(string newName)
        {
            var attrModel = GetAttributeModel();
            IDEAAttributeModel.RefactorFunctionName(attrModel.RawName, newName);
            SetRawName(newName);

            var y = (controller.view.MainViewController.Instance.InkableScene.Elements).Where((e) => e is OperationContainerView && (e as OperationContainerView)?.Children?.First() is DefinitionRenderer);
            foreach (var fm in y)
            {
                var frend = (fm as OperationContainerView).Children.First() as DefinitionRenderer;
                frend.Refactor(null, newName);
            }
        }
        public ComputationalOperationModel(SchemaModel schemaModel, string code, DataType dataType, string visualizationType, string rawName, string displayName = null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeModel.NameExists(rawName, schemaModel.OriginModels.First()))
            {
                IDEAAttributeModel.AddCodeField(rawName, displayName == null ? rawName : displayName, code, dataType, visualizationType, new List<VisualizationHint>(), schemaModel.OriginModels.First());
            }
            CodeNameChangedEvent += (sender, oldname, newname) => UpdateName();
        }
        public ComputationalOperationModel(SchemaModel schemaModel, DataType dataType, string visualizationType, string rawName, string displayName = null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeModel.NameExists(rawName, SchemaModel.OriginModels.First()))
            {
                IDEAAttributeModel.AddBackendField(rawName, displayName == null ? rawName : displayName, null, DataType.Double, "numeric", new List<VisualizationHint>(), schemaModel.OriginModels.First());
            }
            CodeNameChangedEvent += (sender, oldName, newName) => UpdateName();
        }

        public delegate void CodeNameChangedHandler(object sender, string oldName, string newName);
        static public event CodeNameChangedHandler CodeNameChangedEvent;
        public void SetRawName(string name)
        {
            var code = GetAttributeModel();
            var oldName = code.RawName;
            code.DisplayName = code.RawName = _rawName = name;
            if (CodeNameChangedEvent != null)
                CodeNameChangedEvent(this, oldName, name);
        }
        public IDEAAttributeModel GetAttributeModel()
        {
            return IDEAAttributeModel.Function(_rawName, SchemaModel.OriginModels.First());
        }
        public void SetCode(string code, DataType dataType)
        {
            GetAttributeModel().SetCode(code, dataType);
            UpdateName();
        }
    }
}