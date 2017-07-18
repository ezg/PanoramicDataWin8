﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class CalculationOperationModel : OperationModel
    {
        private model.data.idea.IDEAAttributeComputedFieldModel _code;
        public CalculationOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _code = new idea.IDEAAttributeComputedFieldModel("", "", "", IDEA_common.catalog.DataType.String, "numeric",
                   new List<IDEA_common.catalog.VisualizationHint>());
        }

        public delegate void CodeDefinitionChangedHandler(object sender);
        static public event CodeDefinitionChangedHandler CodeDefinitionChangedEvent;

        public void SetCode(string code)
        {
            System.Diagnostics.Debug.WriteLine(_code.RawName + " = " + code);
            (_code.FuncModel as AttributeCodeFuncModel).Code = code;
            if (CodeDefinitionChangedEvent != null)
                CodeDefinitionChangedEvent(this);
        }
        public void SetRawName(string name)
        {
            _code.RawName = name;
            _code.DisplayName = name;
        }
        public model.data.idea.IDEAAttributeComputedFieldModel Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }
    }
}