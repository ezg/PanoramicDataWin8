using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class GraphFilterViewModel : OperationViewModel
    {
        public GraphFilterViewModel(GraphFilterOperationModel model):base(model)
        {

        }

        public GraphFilterOperationModel GraphFilterOperationModel => (GraphFilterOperationModel)OperationModel;

        public override void Dispose()
        {

        }
    }
}
