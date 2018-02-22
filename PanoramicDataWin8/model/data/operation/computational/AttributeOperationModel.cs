using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data.operation.computational
{
    public class AttributeOperationModel : ComputationalOperationModel
    {
        public AttributeOperationModel(SchemaModel schemaModel, string rawName, string displayName=null) : base(schemaModel, "0", DataType.Double, "numeric", rawName, displayName ?? rawName)
        {
            var attrModel = this.GetAttributeModel();
            if (attrModel != null)
            {
                attrModel.FuncModel.ModelType = attribute.AttributeModel.AttributeFuncModel.AttributeModelType.Assigned;
                (attrModel.FuncModel as attribute.AttributeModel.AttributeFuncModel.AttributeCodeFuncModel).Data = new Dictionary<List<object>, object>();
            }
        }
    }
}
