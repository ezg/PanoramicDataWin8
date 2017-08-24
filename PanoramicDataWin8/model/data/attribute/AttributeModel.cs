using System.Collections.Generic;
using IDEA_common.catalog;
using PanoramicDataWin8.utils;
using Microsoft.Practices.Prism.Mvvm;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeModel : ExtendedBindableBase
    {
        private bool _isDisplayed = true;

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }

        public abstract string RawName { get; set; }
        public abstract DataType DataType { get; set; }
        public abstract string DisplayName { get; set; }

        public class AttributeFuncModel : BindableBase {
        };

        public class AttributeCodeFuncModel : AttributeFuncModel {
            string _code;
            public AttributeCodeFuncModel(string code)
            {
                _code = code;
            }
            public string Code { get => _code; set => _code = value; }
        }
        public class AttributeColumnFuncModel : AttributeFuncModel
        {
        }

        public abstract AttributeFuncModel FuncModel { get; set;  }

        public abstract List<VisualizationHint> VisualizationHints { get; set; }
    }
}