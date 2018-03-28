using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.operation
{
    public class GraphFilterOperationModel :  OperationModel
    {
        public GraphFilterOperationModel(OriginModel schemaModel) : base(schemaModel) { }

        public GraphOperationModel TargetGraphOperationModel;

        public override void Dispose()
        {
        }
    }
}
