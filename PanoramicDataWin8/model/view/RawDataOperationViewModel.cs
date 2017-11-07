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

namespace PanoramicDataWin8.model.view.operation
{
    public class RawDataOperationViewModel : BaseVisualizationOperationViewModel
    {
        public RawDataOperationViewModel(RawDataOperationModel rawDataOperationModel, AttributeModel attributeModel) : base(rawDataOperationModel)
        {
            // axis attachment view models
            //createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            new AttributeUsageMenu(this, attributeModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false) { CanSwapAxes = false, CanTransformAxes = false };
            createTopRightFilterDragMenu();
            createTopInputsExpandingMenu(8);

            if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.ENUM ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.CATEGORY ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
            {
                var x = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.None };
                rawDataOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
            {
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.VECTOR)
            {
            }
            else
            {
            }
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;
        
    }
}