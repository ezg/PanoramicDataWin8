using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.catalog;
using IDEA_common.operations;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.idea;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation.computational
{
    public class AttributeOperationModel : ComputationalOperationModel
    {
        public AttributeFuncModel.AttributeAssignedValueFuncModel FuncModel => GetAttributeModel().FuncModel as AttributeFuncModel.AttributeAssignedValueFuncModel;
        public AttributeOperationModel(SchemaModel schemaModel, string rawName, string displayName=null) : 
            base(schemaModel, DataType.Double, AttributeFuncModel.AttributeModelType.Assigned, "numeric", rawName, displayName ?? rawName)
        {
            IDEAAttributeModel.CodeDefinitionChangedEvent += TestForRefresh;
        }
        private void TestForRefresh(object sender)
        {
            var attributeChanged = sender as IDEAAttributeModel;
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<string> brushes;
            var aggregates = this.AttributeTransformationModelParameters.ToList();
            IDEAHelpers.GetBaseOperationParameters(this, out attributeCodeParameters, out brushes, new List<object>(), aggregates);

            foreach (var attr in attributeCodeParameters.OfType<AttributeCodeParameters>())
            {
                if (attr.RawName == attributeChanged.RawName || attr.Code.Contains(attributeChanged.RawName))
                {
                    FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    break;
                }
            }
        }
    }
}
