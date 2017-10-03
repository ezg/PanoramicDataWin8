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

        static public bool NameExists(string name, OriginModel model)
        {
            return Function(name, model) != null;
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
                fm.FuncModel is AttributeFuncModel.AttributeBackendFuncModel) &&
                model == fm.OriginModel);
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