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

        public static VisualizationViewModel CreateDefault(SchemaModel schemaModel, JobType jobType, InputOperationModel inputOperationModel)
        {
            VisualizationViewModel visualizationViewModel = CreateDefault(schemaModel, jobType, VisualizationType.table);

            if (jobType == JobType.DB)
            {
                if (schemaModel is TuppleWareSchemaModel)
                {
                    visualizationViewModel.QueryModel.VisualizationType = VisualizationType.table;
                    visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, inputOperationModel);
                }
                else if (inputOperationModel.InputModel.InputVisualizationType == InputVisualizationTypeConstants.ENUM)
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
                else if (inputOperationModel.InputModel.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
                {
                    visualizationViewModel.QueryModel.VisualizationType = VisualizationType.bar;

                    InputOperationModel x = new InputOperationModel(inputOperationModel.InputModel);
                    x.AggregateFunction = AggregateFunction.None;

                    InputOperationModel value = new InputOperationModel(inputOperationModel.InputModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    InputOperationModel y = new InputOperationModel(inputOperationModel.InputModel);
                   // y.AggregateFunction = AggregateFunction.Count;


                    visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, x);
                    visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.Y, y);
                    visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.DefaultValue, value);
                }
                else if (inputOperationModel.InputModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
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
                    visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, inputOperationModel);
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
