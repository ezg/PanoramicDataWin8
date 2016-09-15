using System;
using System.Linq;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.model.view
{
    public class OperationViewModelFactory
    {
        public static OperationViewModel CopyOperationViewModel(OperationViewModel operationViewModel)
        {
            if (operationViewModel is HistogramOperationViewModel)
            {
                HistogramOperationViewModel oldOperationViewModel = (HistogramOperationViewModel) operationViewModel;
                HistogramOperationModel oldOperationModel = (HistogramOperationModel) oldOperationViewModel.OperationModel;

                HistogramOperationViewModel newHistogramOperationViewModel = CreateDefaultHistogramOperationViewModel(operationViewModel.OperationModel.SchemaModel, 
                    null);
                HistogramOperationModel newOperationModel = (HistogramOperationModel) oldOperationViewModel.OperationModel;

                
                foreach (var usage in oldOperationModel.UsageAttributeTransformationModels.Keys.ToArray())
                {
                    foreach (var atm in oldOperationModel.UsageAttributeTransformationModels[usage])
                    {
                        newOperationModel.AddUsageAttributeTransformationModel(usage,
                            new AttributeTransformationModel(atm.AttributeModel)
                            {
                                AggregateFunction = atm.AggregateFunction
                            });
                    }
                }
                newHistogramOperationViewModel.Size = operationViewModel.Size;
                return newHistogramOperationViewModel;
            }
            return null;
        }


        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(SchemaModel schemaModel, AttributeModel attributeModel)
        {
            HistogramOperationModel histogramOperationModel = new HistogramOperationModel(schemaModel);
            HistogramOperationViewModel histogramOperationViewModel = new HistogramOperationViewModel(histogramOperationModel);

            foreach (var attachmentOrientation in Enum.GetValues(typeof(AttachmentOrientation)).Cast<AttachmentOrientation>())
            {
                histogramOperationViewModel.AttachementViewModels.Add(new AttachmentViewModel()
                {
                    AttachmentOrientation = attachmentOrientation,
                    OperationViewModel = histogramOperationViewModel,
                });
            }
            //histogramOperationModel.VisualizationType = visualizationType;

            
                /*var county = schemaModel.OriginModels.First().InputModels.FirstOrDefault(im => im.RawName == "county");
                if (county != null)
                {
                    AttributeTransformationModel x = new AttributeTransformationModel(county);
                    x.AggregateFunction = AggregateFunction.Count;

                    AttributeTransformationModel y = new AttributeTransformationModel(county);
                    y.AggregateFunction = AggregateFunction.None;

                    AttributeTransformationModel value = new AttributeTransformationModel(county);
                    value.AggregateFunction = AggregateFunction.Count;

                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, x);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.Y, y);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.DefaultValue, value);
                }*/

            if (attributeModel != null && attributeModel is AttributeFieldModel)
            {
                var inputFieldModel = attributeModel as AttributeFieldModel;
                if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.ENUM)
                {
                    histogramOperationModel.VisualizationType = VisualizationType.plot;

                    AttributeTransformationModel x = new AttributeTransformationModel(inputFieldModel);
                    x.AggregateFunction = AggregateFunction.None;

                    AttributeTransformationModel value = new AttributeTransformationModel(inputFieldModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    AttributeTransformationModel y = new AttributeTransformationModel(inputFieldModel);
                    y.AggregateFunction = AggregateFunction.Count;

                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, x);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.Y, y);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.DefaultValue, value);
                }
                else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
                {
                    histogramOperationModel.VisualizationType = VisualizationType.plot;

                    AttributeTransformationModel x = new AttributeTransformationModel(inputFieldModel);
                    x.AggregateFunction = AggregateFunction.None;

                    AttributeTransformationModel value = new AttributeTransformationModel(inputFieldModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    AttributeTransformationModel y = new AttributeTransformationModel(inputFieldModel);
                    y.AggregateFunction = AggregateFunction.Count;

                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, x);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.Y, y);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.DefaultValue, value);
                }
                else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
                {
                }
                else
                {
                    histogramOperationModel.VisualizationType = VisualizationType.table;
                    AttributeTransformationModel x = new AttributeTransformationModel(inputFieldModel);
                    histogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, x);
                }
            }


            return histogramOperationViewModel;
        }
    }
}
