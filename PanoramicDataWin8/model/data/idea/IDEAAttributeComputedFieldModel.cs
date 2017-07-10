using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeComputedFieldModel : AttributeFieldModel
    {
        private string _displayName = "";

        private AttributeCodeFuncModel _codeModel = null;

        private string _rawName = "";

        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();

        public IDEAAttributeComputedFieldModel(string rawName, string displayName, string code, string inputDataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            _rawName = rawName;
            _displayName = displayName;
            InputDataType = inputDataType;
            InputVisualizationType = inputVisualizationType;
            _codeModel = new AttributeCodeFuncModel(code);
            _visualizationHints = visualizationHints;
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

        public override string InputDataType { get; } = "";
    }
}