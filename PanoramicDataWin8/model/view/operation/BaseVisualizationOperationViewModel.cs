using IDEA_common.catalog;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using IDEA_common.aggregates;
using System.ComponentModel;

namespace PanoramicDataWin8.model.view.operation
{
    public class BaseVisualizationOperationViewModel : AttributeUsageOperationViewModel
    {
        public BaseVisualizationOperationModel BaseVisualizationOperationModel => (BaseVisualizationOperationModel)OperationModel;
        public BaseVisualizationOperationViewModel(BaseVisualizationOperationModel baseVisualizationOperationModel) : base(baseVisualizationOperationModel)
        {

        }
    }
}