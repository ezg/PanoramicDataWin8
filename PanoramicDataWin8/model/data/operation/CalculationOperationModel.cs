using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace PanoramicDataWin8.model.data.operation
{
    public class CalculationOperationModel : OperationModel
    {
        public CalculationOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }
        public string Code { get; set; }
    }
}