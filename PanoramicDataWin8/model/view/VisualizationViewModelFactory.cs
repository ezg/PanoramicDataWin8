using System;
using System.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.model.view
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

                    AttributeOperationModel value = new AttributeOperationModel(attributeOperationModel.AttributeModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    AttributeOperationModel y = new AttributeOperationModel(attributeOperationModel.AttributeModel);
                   // y.AggregateFunction = AggregateFunction.Count;


                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.X, x);
                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.Y, y);
                    visualizationViewModel.QueryModel.AddFunctionAttributeOperationModel(AttributeFunction.DefaultValue, value);
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
