using PanoramicDataWin8.model.data.operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view.operation
{
    public class CalculationOperationViewModel : OperationViewModel
    {
        public CalculationOperationViewModel(CalculationOperationModel histogramOperationModel, bool fromMouse = false) : base(histogramOperationModel)
        {
        }
    }
}
