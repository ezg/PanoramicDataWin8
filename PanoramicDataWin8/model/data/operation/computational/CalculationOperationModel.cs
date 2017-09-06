using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class CalculationOperationModel : ComputationalOperationModel 
    {
        public CalculationOperationModel(SchemaModel schemaModel, string rawName, string displayName = null) : base(schemaModel, "0", DataType.Double, "numeric", rawName, displayName)
        {
        }
    }
}