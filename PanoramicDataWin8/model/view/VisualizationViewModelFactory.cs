using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.view
{
    public class VisualizationViewModelFactory
    {
        public static VisualizationViewModel CreateDefault(SchemaModel schemaModel, JobType jobType, VisualizationType visualizationType)
        {
            VisualizationViewModel visualizationViewModel = new VisualizationViewModel(schemaModel);
            visualizationViewModel.QueryModel.JobType = jobType;

            foreach (var attachmentOrientation in Enum.GetValues(typeof(AttachmentOrientation)).Cast<AttachmentOrientation>())
            {
                visualizationViewModel.AttachementViewModels.Add(new AttachmentViewModel()
                {
                    AttachmentOrientation = attachmentOrientation,
                    VisualizationViewModel = visualizationViewModel,
                });
            }
            visualizationViewModel.QueryModel.VisualizationType = visualizationType;

            return visualizationViewModel;
        }

        public static VisualizationViewModel CreateDefault(SchemaModel schemaModel, JobType jobType, AttributeOperationModel attributeOperationModel)
        {
            VisualizationViewModel visualizationViewModel = CreateDefault(schemaModel, jobType, VisualizationType.table);

            if (jobType == JobType.DB)
            {
                if (schemaModel is TuppleWareSchemaModel)
                {
                    visualizationViewModel.QueryModel.VisualizationType = VisualizationType.table;
                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.X, attributeOperationModel);
                }
                else if (attributeOperationModel.AttributeModel.AttributeVisualizationType == AttributeVisualizationTypeConstants.ENUM)
                {
                    /*
                    PanoramicDataColumnDescriptor x = (PanoramicDataColumnDescriptor)columnDescriptor.Clone();
                    PanoramicDataColumnDescriptor g = (PanoramicDataColumnDescriptor)columnDescriptor.Clone();
                    g.IsGrouped = true;
                    PanoramicDataColumnDescriptor y = (PanoramicDataColumnDescriptor)columnDescriptor.Clone();
                    y.AggregateFunction = AggregateFunction.Count;

                    filterHolderViewModel.AddOptionColumnDescriptor(Option.X, x);
                    filterHolderViewModel.AddOptionColumnDescriptor(Option.ColorBy, g);
                    filterHolderViewModel.AddOptionColumnDescriptor(Option.Y, y);*/
                }
                else if (attributeOperationModel.AttributeModel.AttributeVisualizationType == AttributeVisualizationTypeConstants.NUMERIC)
                {
                    visualizationViewModel.QueryModel.VisualizationType = VisualizationType.bar;

                    AttributeOperationModel x = new AttributeOperationModel(attributeOperationModel.AttributeModel);
                    x.AggregateFunction = AggregateFunction.None;

                    AttributeOperationModel bin = new AttributeOperationModel(attributeOperationModel.AttributeModel);
                    bin.IsBinned = true;

                    AttributeOperationModel y = new AttributeOperationModel(attributeOperationModel.AttributeModel);
                   // y.AggregateFunction = AggregateFunction.Count;


                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.X, x);
                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.Y, y);
                    //visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.Group, bin);
                }
                else if (attributeOperationModel.AttributeModel.AttributeVisualizationType == AttributeVisualizationTypeConstants.GEOGRAPHY)
                {
                    /*visualizationViewModel.VisualizationType = VisualizationType.Map;

                    PanoramicDataColumnDescriptor x = (PanoramicDataColumnDescriptor)columnDescriptor.Clone();
                    x.AggregateFunction = AggregateFunction.Count;

                    PanoramicDataColumnDescriptor y = (PanoramicDataColumnDescriptor)columnDescriptor.Clone();
                    y.IsGrouped = true;

                    filterHolderViewModel.AddOptionColumnDescriptor(Option.Location, y);
                    filterHolderViewModel.AddOptionColumnDescriptor(Option.Label, x);
                    //filterHolderViewModel.AddOptionColumnDescriptor(Option.Y, y);
                    //filterHolderViewModel.AddOptionColumnDescriptor(Option.X, x);*/
                }
                else
                {
                    visualizationViewModel.QueryModel.VisualizationType = VisualizationType.table;
                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.X, attributeOperationModel);
                }
            }
            else if (jobType == JobType.Kmeans)
            {
                visualizationViewModel.QueryModel.KmeansClusters = 3;
                visualizationViewModel.QueryModel.KmeansNrSamples = 3;
            }

            return visualizationViewModel;
        }
    }
}
