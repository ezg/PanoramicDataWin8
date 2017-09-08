using IDEA_common.catalog;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class ComputationalOperationModel : AttributeUsageOperationModel
    {
        string _rawName;

        protected virtual void updateName()
        {
            var str = "(";
            var code = (GetAttributeModel().FuncModel as AttributeFuncModel.AttributeCodeFuncModel).Code;
            var terms = new Regex("\\b", RegexOptions.Compiled).Split(code);
            foreach (var n in terms)
                if (n != null && AttributeTransformationModel.MatchesExistingField(n, true) != null)
                    str += n + ",";
            str = str.TrimEnd(',') + ")";
            var newName = new Regex("\\(.*\\)", RegexOptions.Compiled).Replace(GetAttributeModel().RawName, str);
            //GetAttributeModel().DisplayName = newName;
            SetRawName(newName);
        }
        public ComputationalOperationModel(SchemaModel schemaModel, string code, DataType dataType, string visualizationType, string rawName, string displayName = null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeModel.NameExists(rawName))
            {
                IDEAAttributeModel.AddCodeField(rawName, displayName == null ? rawName : displayName, code, dataType, visualizationType, new List<VisualizationHint>());
            }
        }
        public void SetRawName(string name)
        {
            var code = GetAttributeModel();
            code.DisplayName = code.RawName = _rawName = name;
        }
        public IDEAAttributeModel GetAttributeModel()
        {
            return IDEAAttributeModel.Function(_rawName);
        }
        public void SetCode(string code, DataType dataType)
        {
            GetAttributeModel().SetCode(code, dataType);
            updateName();
        }
    }
}