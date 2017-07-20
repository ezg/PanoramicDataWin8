using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using IDEA_common.operations;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeComputedFieldModel : AttributeFieldModel
    {
        static List< IDEAAttributeComputedFieldModel> _calculatedFieldAttributeModels = new List<IDEAAttributeComputedFieldModel>();
        static public bool NameExists(string name)
        {
            return Function(name) != null;
        }
        static public void Add(string rawName, string displayName, string code, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            _calculatedFieldAttributeModels.Add(new IDEAAttributeComputedFieldModel(rawName, displayName, code, dataType, inputVisualizationType, visualizationHints));
        }

        static public List<AttributeCodeParameters>  GetAllCode()
        {
            var attrList = new List<AttributeCodeParameters>();
            foreach (var func in _calculatedFieldAttributeModels)
                attrList.Add(new AttributeCodeParameters()
                {
                    Code = func._codeModel.Code,
                    RawName = func.RawName
                });
            return attrList;
        }

        public delegate void CodeDefinitionChangedHandler(object sender);
        static public event CodeDefinitionChangedHandler CodeDefinitionChangedEvent;

        static public IDEAAttributeComputedFieldModel Function(string name)
        {
            foreach (var func in _calculatedFieldAttributeModels)
                if (func.RawName == name)
                    return func;
            return null;
        }

        private string _displayName = "";

        private AttributeCodeFuncModel _codeModel = null;

        private string _rawName = "";

        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();

        public IDEAAttributeComputedFieldModel()
        { }
        private IDEAAttributeComputedFieldModel(string rawName, string displayName, string code, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            _rawName = rawName;
            _displayName = displayName;
            DataType = dataType;
            InputVisualizationType = inputVisualizationType;
            _codeModel = new AttributeCodeFuncModel(code);
            _visualizationHints = visualizationHints;
        }

        public void SetCode(string code)
        {
            if (FuncModel is AttributeCodeFuncModel) {
                var oldCode = (FuncModel as AttributeCodeFuncModel).Code;
                if (oldCode != code)
                {
                    (FuncModel as AttributeCodeFuncModel).Code = code;
                    if (CodeDefinitionChangedEvent != null)
                        CodeDefinitionChangedEvent(this);
                }
            }
        }

        public override string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }


        public override List<VisualizationHint> VisualizationHints
        {
            get { return _visualizationHints; }
            set { SetProperty(ref _visualizationHints, value); }
        }

        public override AttributeFuncModel FuncModel {
            get { return _codeModel; }
            set { SetProperty(ref _codeModel, value as AttributeCodeFuncModel); }
        }

        public override string RawName
        {
            get { return _rawName; }
            set { SetProperty(ref _rawName, value); }
        }

        public override string InputVisualizationType { get; } = "";
        public override DataType DataType { get; set; } = DataType.Object;
    }
}