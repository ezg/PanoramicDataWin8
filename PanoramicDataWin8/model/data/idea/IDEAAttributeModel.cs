using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using IDEA_common.operations;
using System.Linq;

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
            return _allFieldAttributeModels.Where((fm) => fm.FuncModel is AttributeFuncModel.AttributeCodeFuncModel);
        }

        static public List<AttributeCodeParameters>  GetAllCode()
        {
            var attrList = new List<AttributeCodeParameters>();
            foreach (var func in GetAllCalculatedAttributeModels())
                attrList.Add(new AttributeCodeParameters()
                {
                    Code = (func.FuncModel as AttributeFuncModel.AttributeCodeFuncModel).Code,
                    RawName = func.RawName
                });
            return attrList;
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

    }
}