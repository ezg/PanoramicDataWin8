using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class CalculationOperationModel : OperationModel
    {
        string _rawName;
        public CalculationOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeComputedFieldModel.NameExists(rawName))
            {
                IDEAAttributeComputedFieldModel.Add(rawName, displayName == null ? rawName : displayName, "0", IDEA_common.catalog.DataType.String, "numeric",
                               new List<IDEA_common.catalog.VisualizationHint>());
            }
        }

        public void SetCode(string code)
        {
            System.Diagnostics.Debug.WriteLine(_rawName + " = " + code);
            GetCode().SetCode(code);
        }
        public void SetRawName(string name)
        {
            GetCode().RawName = name;
            _rawName = name;
            GetCode().DisplayName = name;
        }
        public IDEAAttributeComputedFieldModel GetCode()
        {
            return IDEAAttributeComputedFieldModel.Function(_rawName);
        }
    }
}