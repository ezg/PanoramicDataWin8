using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using PanoramicDataWin8.utils;
using Microsoft.Practices.Prism.Mvvm;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeModel : ExtendedBindableBase
    {
        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();
        private string _displayName = "";
        private string _rawName = "";
        private bool _isTarget = false;
        private OriginModel _originModel = null;
        private bool _isDisplayed = true;
        private AttributeFuncModel _funcModel = null;

        public AttributeModel()
        {

        }
        public AttributeModel(string rawName, string displayName, AttributeFuncModel funcModel, DataType dataType, string inputVisualizationType,
            List<VisualizationHint> visualizationHints, OriginModel originModel, bool isTarget)
        {
            _rawName = rawName;
            _displayName = displayName;
            _funcModel = funcModel;
            _visualizationHints = visualizationHints;
            _originModel = originModel;
            InputVisualizationType = inputVisualizationType;
            _isTarget = isTarget;
            DataType = dataType;
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }
        public bool IsTarget
        {
            get { return _isTarget; }
            set { SetProperty(ref _isTarget, value); }
        }
        public OriginModel OriginModel
        {
            get { return _originModel; }
            set { SetProperty(ref _originModel, value); }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
        public string RawName
        {
            get { return _rawName; }
            set { SetProperty(ref _rawName, value); }
        }
        public DataType DataType { get; set; } = DataType.Object;

        public AttributeFuncModel FuncModel
        {
            get { return _funcModel; }
            set { SetProperty(ref _funcModel, value); }
        }
        public List<VisualizationHint> VisualizationHints
        {
            get { return _visualizationHints; }
            set { SetProperty(ref _visualizationHints, value); }
        }

        public string InputVisualizationType { get; set; } = "";

        public override bool Equals(object obj)
        {
            if (obj is AttributeModel)
            {
                var am = obj as AttributeModel;
                return
                    am.RawName.Equals(RawName) &&
                    am.InputVisualizationType.Equals(InputVisualizationType) &&
                    am.DataType.Equals(DataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= RawName.GetHashCode();
            code ^= InputVisualizationType.GetHashCode();
            code ^= DataType.GetHashCode();
            return code;
        }

        public class AttributeFuncModel : BindableBase
        {
            public class AttributeCodeFuncModel : AttributeFuncModel
            {
                string _code;
                public AttributeCodeFuncModel(string code)
                {
                    _code = code;
                }
                public string Code { get => _code; set => _code = value; }
            }
            public class AttributeBackendFuncModel : AttributeFuncModel
            {
                string _id;
                public AttributeBackendFuncModel(string id)
                {
                    _id = id;
                }
                public string Id { get => _id; set => _id = value; }
            }
            public class AttributeColumnFuncModel : AttributeFuncModel
            {
            }

            public class AttributeGroupFuncModel : AttributeFuncModel
            {
                private List<AttributeModel> _inputModels = new List<AttributeModel>();
                public List<AttributeModel> InputModels
                {
                    get { return _inputModels; }
                    set { SetProperty(ref _inputModels, value); }
                }
            }
        };
    }
}