using System;
using System.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.model.view
{
    public class VisualizationViewModelFactory
    {
        public static VisualizationViewModel CreateDefault(SchemaModel schemaModel, TaskModel taskModel, VisualizationType visualizationType)
        {
            VisualizationViewModel visualizationViewModel = new VisualizationViewModel(schemaModel);
            visualizationViewModel.QueryModel.TaskModel = taskModel;

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

        public static VisualizationViewModel CreateDefault(SchemaModel schemaModel, TaskModel taskModel, InputModel inputModel)
        {
            VisualizationViewModel visualizationViewModel = CreateDefault(schemaModel, taskModel, VisualizationType.table);

            if (taskModel == null)
            {
                if (inputModel is InputFieldModel)
                {
                    var inputFieldModel = inputModel as InputFieldModel;
                    if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.ENUM)
                    {
                        visualizationViewModel.QueryModel.VisualizationType = VisualizationType.plot;

                        InputOperationModel x = new InputOperationModel(inputFieldModel);
                        x.AggregateFunction = AggregateFunction.None;

                        InputOperationModel value = new InputOperationModel(inputFieldModel);
                        value.AggregateFunction = AggregateFunction.Count;

                        InputOperationModel y = new InputOperationModel(inputFieldModel);
                        y.AggregateFunction = AggregateFunction.Count;

                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, x);
                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.Y, y);
                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.DefaultValue, value);
                    }
                    else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
                    {
                        visualizationViewModel.QueryModel.VisualizationType = VisualizationType.plot;

                        InputOperationModel x = new InputOperationModel(inputFieldModel);
                        x.AggregateFunction = AggregateFunction.None;

                        InputOperationModel value = new InputOperationModel(inputFieldModel);
                        value.AggregateFunction = AggregateFunction.Count;

                        InputOperationModel y = new InputOperationModel(inputFieldModel);
                        y.AggregateFunction = AggregateFunction.Count;

                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, x);
                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.Y, y);
                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.DefaultValue, value);
                    }
                    else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
                    {
                    }
                    else
                    {
                        visualizationViewModel.QueryModel.VisualizationType = VisualizationType.table;
                        InputOperationModel x = new InputOperationModel(inputFieldModel);
                        visualizationViewModel.QueryModel.AddUsageInputOperationModel(InputUsage.X, x);
                    }
                }
            }
            else
            {
                visualizationViewModel.QueryModel.TaskModel = taskModel;
            }

            return visualizationViewModel;
        }
    }
}
