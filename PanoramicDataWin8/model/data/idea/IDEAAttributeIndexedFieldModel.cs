﻿using System.Collections.Generic;
using IDEA_common.catalog;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.idea
{
    [JsonObject(MemberSerialization.OptOut)]
    public class IDEAAttributeIndexedFieldModel : AttributeFieldModel
    {
        private string _displayName = "";

        private AttributeIndexFuncModel _index = null;

        private string _rawName = "";

        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();

        public IDEAAttributeIndexedFieldModel(string rawName, string displayName, int index, DataType dataType, string inputVisualizationType, List<VisualizationHint> visualizationHints)
        {
            _rawName = rawName;
            _displayName = displayName;
            DataType = dataType;
            InputVisualizationType = inputVisualizationType;
            _index = new AttributeIndexFuncModel(index);
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
        public override AttributeFuncModel FuncModel
        {
            get { return _index; }
            set { SetProperty(ref _index, value as AttributeIndexFuncModel); }
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