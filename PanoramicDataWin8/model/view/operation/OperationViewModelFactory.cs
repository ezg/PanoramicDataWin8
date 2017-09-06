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

namespace PanoramicDataWin8.model.view.operation
{
    public class OperationViewModelFactory
    {
        public static OperationViewModel CopyOperationViewModel(OperationViewModel operationViewModel)
        {
            if (operationViewModel is HistogramOperationViewModel)
            {
                var oldOperationViewModel = (HistogramOperationViewModel) operationViewModel;
                var oldOperationModel = (HistogramOperationModel) oldOperationViewModel.OperationModel;

                var newHistogramOperationViewModel = CreateDefaultHistogramOperationViewModel(operationViewModel.OperationModel.SchemaModel,
                    null, operationViewModel.Position);
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
            if (operationViewModel is FilterOperationViewModel)
            {
                var oldOperationViewModel = (FilterOperationViewModel)operationViewModel;
                var oldOperationModel = (FilterOperationModel)oldOperationViewModel.OperationModel;

                var newFilterOperationViewModel = CreateDefaultFilterOperationViewModel(operationViewModel.OperationModel.SchemaModel,
                    operationViewModel.Position, controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse);
                var newOperationModel = (FilterOperationModel)newFilterOperationViewModel.OperationModel;
                foreach (var fm in oldOperationModel.FilterModels)
                    (newFilterOperationViewModel.OperationModel as FilterOperationModel).AddFilterModel(fm);

                return newFilterOperationViewModel;
            }
            return null;
        }

      
        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(SchemaModel schemaModel, AttributeModel attributeModel, Pt position)
        {
            return new HistogramOperationViewModel(new HistogramOperationModel(schemaModel), attributeModel) { Position = position };
        }

        public static ExampleOperationViewModel CreateDefaultExampleOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new ExampleOperationViewModel(new ExampleOperationModel(schemaModel)) { Position = position };
        }
        static int predictorCount = 0;
        public static PredictorOperationViewModel CreateDefaultPredictorOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new PredictorOperationViewModel(new PredictorOperationModel(schemaModel, "P"+(predictorCount++)+"()")) { Position = position };
        }
        static int groupCount = 0;
        public static AttributeGroupOperationViewModel CreateDefaultAttributeGroupOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new AttributeGroupOperationViewModel(new AttributeGroupOperationModel(schemaModel, "G" + (calcCount++) + "()")) { Position = position };
        }
        public static FunctionOperationViewModel CreateDefaultFunctionOperationViewModel(SchemaModel schemaModel, Pt position, FunctionSubtypeModel functionSubtypeModel, bool fromMouse = false)
        {
            return new FunctionOperationViewModel(new FunctionOperationModel(schemaModel, functionSubtypeModel), fromMouse) { Position = position }; ;
        }
        static int calcCount = 0;
        public static CalculationOperationViewModel CreateDefaultCalculationOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse = false)
        {
            return new CalculationOperationViewModel(new CalculationOperationModel(schemaModel, "Calc" + (calcCount++) + "()"), fromMouse) { Position = position };
        }

        static int defCount = 0;
        public static DefinitionOperationViewModel CreateDefaultDefinitionOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse=false)
        {
            return new DefinitionOperationViewModel(new DefinitionOperationModel(schemaModel, "Def" + (defCount++) + "()"), fromMouse) { Position = position };
        }
        public static FilterOperationViewModel CreateDefaultFilterOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse)
        {
            return new FilterOperationViewModel(new FilterOperationModel(schemaModel), fromMouse) { Position = position };
        }
    }
}