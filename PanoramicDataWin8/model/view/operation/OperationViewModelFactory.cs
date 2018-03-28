using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Media;
using IDEA_common.catalog;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Xaml;
using PanoramicDataWin8.model.data.operation.computational;
using PanoramicDataWin8.model.data.idea;

namespace PanoramicDataWin8.model.view.operation
{
    public class OperationViewModelFactory
    {
        public static OperationViewModel CopyOperationViewModel(OperationViewModel operationViewModel)
        {
            if (operationViewModel is HistogramOperationViewModel histogramOperationViewModel)
            {
                var oldOperationViewModel = histogramOperationViewModel;
                var oldOperationModel = histogramOperationViewModel.HistogramOperationModel;
                var attributeModel = histogramOperationViewModel.HistogramOperationModel.AttributeUsageTransformationModels[AttributeUsage.X].First().AttributeModel;
                var newHistogramOperationViewModel = CreateDefaultHistogramOperationViewModel(operationViewModel.OperationModel.OriginModel,
                    attributeModel, operationViewModel.Position);
                var newOperationModel = (HistogramOperationModel) newHistogramOperationViewModel.OperationModel;
                newOperationModel.VisualizationType = VisualizationType.plot;

                foreach (var usage in oldOperationModel.AttributeUsageTransformationModels.Keys.ToArray())
                    foreach (var atm in oldOperationModel.AttributeUsageTransformationModels[usage].ToArray())
                        newOperationModel.AddAttributeUsageTransformationModel(usage,
                            new AttributeTransformationModel(atm.AttributeModel)
                            {
                                AggregateFunction = atm.AggregateFunction
                            });
                newHistogramOperationViewModel.Size = operationViewModel.Size;
                return newHistogramOperationViewModel;
            }
            if (operationViewModel is RawDataOperationViewModel rawDataOperationViewModel)
            {
                var oldOperationViewModel        = rawDataOperationViewModel;
                var oldOperationModel            = rawDataOperationViewModel.RawDataOperationModel;
                var newRawDataOperationViewModel = CreateDefaultRawDataOperationViewModel(operationViewModel.OperationModel.OriginModel, operationViewModel.Position);
                var newOperationModel            = (RawDataOperationModel)newRawDataOperationViewModel.OperationModel;
                newOperationModel.VisualizationType = VisualizationType.plot;

                foreach (var atm in oldOperationModel.AttributeTransformationModelParameters.ToArray())
                        newOperationModel.AttributeTransformationModelParameters.Add(
                            new AttributeTransformationModel(atm.AttributeModel)
                            {
                                AggregateFunction = atm.AggregateFunction
                            });
                newRawDataOperationViewModel.Size = operationViewModel.Size;
                return newRawDataOperationViewModel;
            }
            if (operationViewModel is GraphOperationViewModel graphOperationViewModel)
            {
                var oldOperationViewModel = graphOperationViewModel;
                var oldOperationModel = graphOperationViewModel.GraphOperationModel;
                var newGraphOperationViewModel = CreateDefaultGraphOperationViewModel(operationViewModel.OperationModel.OriginModel, operationViewModel.Position);
                var newOperationModel = newGraphOperationViewModel.OperationModel;
                
                newGraphOperationViewModel.Size = operationViewModel.Size;
                return newGraphOperationViewModel;
            }
            if (operationViewModel is FilterOperationViewModel filterOperationViewModel)
            {
                var oldOperationViewModel = filterOperationViewModel;
                var oldOperationModel = oldOperationViewModel.FilterOperationModel;

                var newFilterOperationViewModel = CreateDefaultFilterOperationViewModel(operationViewModel.OperationModel.OriginModel,
                    operationViewModel.Position, controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse);
                var newOperationModel = (FilterOperationModel)newFilterOperationViewModel.OperationModel;
                foreach (var fm in oldOperationModel.FilterModels)
                    (newFilterOperationViewModel.OperationModel as FilterOperationModel).AddFilterModel(fm);

                return newFilterOperationViewModel;
            }
            return null;
        }

        public static GraphOperationViewModel CreateDefaultGraphOperationViewModel(OriginModel schemaModel, Pt position)
        {
            return new GraphOperationViewModel(new GraphOperationModel(schemaModel, null)) { Position = position };
        }

        public static RawDataOperationViewModel CreateDefaultRawDataOperationViewModel(OriginModel schemaModel, Pt position)
        {
            return new RawDataOperationViewModel(new RawDataOperationModel(schemaModel), null) { Position = position };
        }

        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(OriginModel schemaModel, AttributeModel attributeModel, Pt position)
        {
            return new HistogramOperationViewModel(new HistogramOperationModel(schemaModel), attributeModel) { Position = position };
        }

        public static ExampleOperationViewModel CreateDefaultExampleOperationViewModel(OriginModel schemaModel, Pt position)
        {
            return new ExampleOperationViewModel(new ExampleOperationModel(schemaModel)) { Position = position };
        }
        static int predictorCount = 0;
        public static PredictorOperationViewModel CreateDefaultPredictorOperationViewModel(OriginModel schemaModel, Pt position)
        {
            return new PredictorOperationViewModel(new PredictorOperationModel(schemaModel, "P"+(predictorCount++)+"()")) { Position = position };
        }
        static int groupCount = 0;
        public static AttributeGroupOperationViewModel CreateDefaultAttributeGroupOperationViewModel(OriginModel schemaModel, Pt position, AttributeModel groupModel=null)
        {
            return new AttributeGroupOperationViewModel(
                new AttributeGroupOperationModel(schemaModel, "G" + (groupCount++) + "()", groupModel), groupModel == null) { Position = position };
        }
        public static GraphOperationViewModel CreateDefaultGraphOperationViewModel(OriginModel schemaModel, Pt position, GraphOperationModel graphModel)
        {
            return new GraphOperationViewModel( graphModel) { Position = position };
        }
        static int attrCount = 0;
        public static AttributeOperationViewModel CreateDefaultAttributeOperationViewModel(OriginModel schemaModel, Pt position)
        {
            return new AttributeOperationViewModel(new AttributeOperationModel(schemaModel, "A" + (attrCount++))) { Position = position };
        }
        static int funcCount = 0;
        public static FunctionOperationViewModel CreateDefaultFunctionOperationViewModel(OriginModel schemaModel, Pt position, FunctionOperationModel genericFunctionModel, bool fromMouse = false)
        {
            return new FunctionOperationViewModel(new FunctionOperationModel(schemaModel,
                genericFunctionModel.GetAttributeModel().DataType, genericFunctionModel.GetAttributeModel().InputVisualizationType,
                genericFunctionModel.AttributeParameterGroups().Select((am) => am.Item1),
                genericFunctionModel.ValueParameterPairs(),
                genericFunctionModel.GetAttributeModel().RawName + (funcCount++) + "()",
                genericFunctionModel.GetAttributeModel().RawName), fromMouse) { Position = position }; ;
        }
        static int calcCount = 0;
        public static CalculationOperationViewModel CreateDefaultCalculationOperationViewModel(OriginModel schemaModel, Pt position, bool fromMouse = false)
        {
            return new CalculationOperationViewModel(new CalculationOperationModel(schemaModel, "Calc" + (calcCount++) + "()"), fromMouse) { Position = position };
        }

        static int defCount = 0;
        public static DefinitionOperationViewModel CreateDefaultDefinitionOperationViewModel(OriginModel schemaModel, Pt position, bool fromMouse=false)
        {
            return new DefinitionOperationViewModel(new DefinitionOperationModel(schemaModel, "Def" + (defCount++) + "()"), fromMouse) { Position = position };
        }
        public static FilterOperationViewModel CreateDefaultFilterOperationViewModel(OriginModel schemaModel, Pt position, bool fromMouse)
        {
            return new FilterOperationViewModel(new FilterOperationModel(schemaModel), fromMouse) { Position = position };
        }
        public static GraphFilterViewModel CreateDefaultGraphFilterOperationViewModel(OriginModel schemaModel, Pt position, bool fromMouse)
        {
            return new GraphFilterViewModel(new GraphFilterOperationModel(schemaModel)) { Position = position };
        }
    }
}