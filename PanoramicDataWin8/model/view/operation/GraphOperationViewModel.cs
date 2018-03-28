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
using System;

namespace PanoramicDataWin8.model.view.operation
{
    public class GraphOperationViewModel : OperationViewModel
    {
        public GraphOperationViewModel(GraphOperationModel graphOperationModel) : base(graphOperationModel)
        {
        }

        public override void Dispose()
        {
            GraphOperationModel.Dispose();
        }
        public GraphOperationModel GraphOperationModel => (GraphOperationModel)OperationModel;

    }
}