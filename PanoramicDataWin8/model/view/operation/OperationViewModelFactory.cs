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

        public static RegresserOperationViewModel CreateDefaultRegresserOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new RegresserOperationViewModel(new RegresserOperationModel(schemaModel)) { Position = position };
        }
        public static ClassifierOperationViewModel CreateDefaultClassifierOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new ClassifierOperationViewModel(new ClassifierOperationModel(schemaModel)) { Position = position };
        }
        public static AttributeGroupOperationViewModel CreateDefaultAttributeGroupOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            return new AttributeGroupOperationViewModel(new AttributeGroupOperationModel(schemaModel)) { Position = position };
        }
        public static FunctionOperationViewModel CreateDefaultFunctionOperationViewModel(SchemaModel schemaModel, Pt position, FunctionSubtypeModel functionSubtypeModel, bool fromMouse = false)
        {
            return new FunctionOperationViewModel(new FunctionOperationModel(schemaModel, functionSubtypeModel), fromMouse) { Position = position }; ;
        }
        public static CalculationOperationViewModel CreateDefaultCalculationOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse = false)
        {
            return new CalculationOperationViewModel(new CalculationOperationModel(schemaModel, "Calc" + new Random().Next()), fromMouse) { Position = position };
        }
        
        public static DefinitionOperationViewModel CreateDefaultDefinitionOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse=false)
        {
            return new DefinitionOperationViewModel(new DefinitionOperationModel(schemaModel, "Def" + new Random().Next()), fromMouse) { Position = position };
        }
        public static FilterOperationViewModel CreateDefaultFilterOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse)
        {
            return new FilterOperationViewModel(new FilterOperationModel(schemaModel), fromMouse) { Position = position };
        }
    }
}