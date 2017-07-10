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

        private static void createTopHistogramMenu(HistogramOperationViewModel histogramOperationViewModel)
        {
            /*var attachmentViewModel = histogramOperationViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Top);
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 1
            };

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(25, 25),
                Position = histogramOperationViewModel.Position,
                TargetSize = new Vec(25, 25),
                IsAlwaysDisplayed = true
            };
            var attr1 = new CreateLinkMenuItemViewModel();
            attr1.CreateRecommendationEvent += (sender, bounds) =>
            {
                FilterLinkViewController.Instance.CreateFilterLinkViewModel(histogramOperationViewModel, bounds);
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);

            attachmentViewModel.MenuViewModel = menuViewModel;*/

            var attachmentViewModel = histogramOperationViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Top);
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 2,
                NrRows = 2
            };

            histogramOperationViewModel.HistogramOperationModel.PropertyChanged += (sender, args) =>
            {
                var model = histogramOperationViewModel.HistogramOperationModel;
                if (args.PropertyName == model.GetPropertyName(() => model.StatisticalComparisonOperationModel))
                {
                    if (model.StatisticalComparisonOperationModel == null)
                    {
                        foreach (var mvm in menuViewModel.MenuItemViewModels.ToArray())
                        {
                            menuViewModel.MenuItemViewModels.Remove(mvm);
                        }
                    }
                    else
                    {
                        var menuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Row = 0,
                            ColumnSpan = 1,
                            RowSpan = 2,
                            Column = 0,
                            Size = new Vec(54, 54),
                            Position = histogramOperationViewModel.Position,
                            TargetSize = new Vec(54, 54),
                            IsAlwaysDisplayed = true
                        };
                        var attr1 = new StatisticalComparisonMenuItemViewModel
                        {
                            StatisticalComparisonOperationModel = model.StatisticalComparisonOperationModel
                        };

                        menuItem.MenuItemComponentViewModel = attr1;
                        menuViewModel.MenuItemViewModels.Add(menuItem);

                        var toggles = new List<ToggleMenuItemComponentViewModel>();
                        var items = new List<MenuItemViewModel>();
                        TestType[] types = new TestType[] { TestType.chi2, TestType.ttest };
                        int count = 0;
                        foreach (var type in types)
                        {
                            var toggleMenuItem = new MenuItemViewModel
                            {
                                MenuViewModel = menuViewModel,
                                Row = count,
                                RowSpan = 0,
                                Position = histogramOperationViewModel.Position,
                                Column = 1,
                                Size = new Vec(54, 25),
                                TargetSize = new Vec(54, 25),
                                IsAlwaysDisplayed = true
                            };
                            //toggleMenuItem.Position = attachmentItemViewModel.Position;
                            var toggle = new ToggleMenuItemComponentViewModel
                            {
                                Label = type.ToString(),
                                IsChecked = model.StatisticalComparisonOperationModel.TestType == type
                            };
                            toggles.Add(toggle);
                            toggleMenuItem.MenuItemComponentViewModel = toggle;
                            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                            {
                                var toogleModel = sender2 as ToggleMenuItemComponentViewModel;
                                if (args2.PropertyName == model.GetPropertyName(() => toogleModel.IsChecked))
                                {
                                    if (toogleModel.IsChecked)
                                    {
                                        model.StatisticalComparisonOperationModel.TestType = type;
                                        model.StatisticalComparisonOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                                        foreach (var tg in toogleModel.OtherToggles)
                                        {
                                            tg.IsChecked = false;
                                        }
                                    }
                                }
                            };
                            menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                            items.Add(toggleMenuItem);
                            count++;
                        }
                        foreach (var mi in items)
                        {
                            (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                        }
                    }
                }
            };

            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private static void createRightHistogramMenu(HistogramOperationViewModel histogramOperationViewModel)
        {
            var rovm = RecommenderViewController.Instance.CreateRecommenderOperationViewModel(histogramOperationViewModel);

            var attachmentViewModel = histogramOperationViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 4,
                NrRows = 4,
                IsRigid = true,
                RigidSize = 54
            };

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(54, 54),
                Position = histogramOperationViewModel.Position,
                TargetSize = new Vec(54, 54),
                IsAlwaysDisplayed = false
            };
            var attr1 = new RecommenderMenuItemViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                RecommenderOperationViewModel = rovm
            };
            attr1.CreateRecommendationEvent += (sender, bounds, percentage) =>
            {
                histogramOperationViewModel.RecommenderOperationViewModel.RecommenderOperationModel.Page = 0;
                histogramOperationViewModel.RecommenderOperationViewModel.RecommenderOperationModel.Budget = 
                    (percentage / 100.0) * HypothesesViewController.Instance.HypothesesViewModel.StartWealth;
                histogramOperationViewModel.RecommenderOperationViewModel.RecommenderOperationModel.ModelId = HypothesesViewController.Instance.RiskOperationModel.ModelId;
                MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(
                    histogramOperationViewModel.RecommenderOperationViewModel.OperationModel, true);
                histogramOperationViewModel.RecommenderOperationViewModel.RecommenderOperationModel.Result = null;

                menuItem.IsAlwaysDisplayed = true;
                if (!menuViewModel.MenuItemViewModels.Any(
                    mi => mi.MenuItemComponentViewModel is RecommenderProgressMenuItemViewModel))
                {
                    var subMenuItem = new MenuItemViewModel
                    {
                        MenuViewModel = menuViewModel,
                        Row = 1,
                        ColumnSpan = 1,
                        RowSpan = 0,
                        Column = 0,
                        Size = new Vec(54, 54),
                        Position = histogramOperationViewModel.Position,
                        TargetSize = new Vec(54, 54),
                        IsAlwaysDisplayed = true
                    };
                    var attr2 = new RecommenderProgressMenuItemViewModel()
                    {
                        HistogramOperationViewModel = histogramOperationViewModel
                    };
                    subMenuItem.MenuItemComponentViewModel = attr2;
                    menuViewModel.MenuItemViewModels.Add(subMenuItem);
                }
            };
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            

            histogramOperationViewModel.OperationViewModelTapped += (sender, args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        
        private static void createAxisMenu(HistogramOperationViewModel histogramOperationViewModel, AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel =
                histogramOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = histogramOperationViewModel.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"))
            };
            histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();
                attr1.Label = attributeTransformationModel == null ? "" : attributeTransformationModel.GetLabel();
                attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(histogramOperationViewModel, coll.FirstOrDefault());

                if (attributeTransformationModel != null)
                {
                    attributeTransformationModel.PropertyChanged += (sender2, args2) =>
                    {
                        attr1.Label = (sender2 as AttributeTransformationModel).GetLabel();
                    };
                }

                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);

                var aom = attr1.AttributeTransformationViewModel.AttributeTransformationModel;
                var aggregateFunctions = new[] {AggregateFunction.None, AggregateFunction.Count}.ToList();
                if (aom != null)
                {
                    if (((AttributeFieldModel) aom.AttributeModel).InputDataType == InputDataTypeConstants.INT ||
                        (((AttributeFieldModel) aom.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT))
                    {
                        aggregateFunctions.Add(AggregateFunction.Avg);
                        aggregateFunctions.Add(AggregateFunction.Sum);
                        if (MainViewController.Instance.MainModel.IsUnknownUnknownEnabled)
                        {
                            aggregateFunctions.Add(AggregateFunction.SumE);
                        }
                    }
                    
                    var toggles = new List<ToggleMenuItemComponentViewModel>();
                    var items = new List<MenuItemViewModel>();

                    var count = 0;
                    foreach (var aggregationFunction in aggregateFunctions)
                    {
                        var toggleMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Row = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                            RowSpan = 0,
                            Position = histogramOperationViewModel.Position,
                            Column = attachmentOrientation == AttachmentOrientation.Bottom ? count : 0,
                            Size = new Vec(32, 32),
                            TargetSize = new Vec(32, 32)
                        };
                        //toggleMenuItem.Position = attachmentItemViewModel.Position;
                        var toggle = new ToggleMenuItemComponentViewModel
                        {
                            Label = aggregationFunction.ToString(),
                            IsChecked = aom.AggregateFunction == aggregationFunction
                        };
                        toggles.Add(toggle);
                        toggleMenuItem.MenuItemComponentViewModel = toggle;
                        toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                        {
                            var model = sender2 as ToggleMenuItemComponentViewModel;
                            if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                                if (model.IsChecked)
                                {
                                    aom.AggregateFunction = aggregationFunction;
                                    foreach (var tg in model.OtherToggles)
                                        tg.IsChecked = false;
                                }
                        };
                        menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                        items.Add(toggleMenuItem);
                        count++;
                    }

                    foreach (var mi in items)
                        (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                }

            };
            attr1.TappedTriggered = () => { attachmentViewModel.ActiveStopwatch.Restart(); };
            attr1.DroppedTriggered = attributeTransformationModel =>
            {
                var otherAxis = axis == AttributeUsage.X ? AttributeUsage.Y : AttributeUsage.X;
                var existingModel = histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).Any() ? 
                      histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).First() : null;
                var existingOtherModel = histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(otherAxis).Any() ?
                    histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(otherAxis).First() : null;
                var swapAxes = existingModel != null && existingOtherModel.AttributeModel == attributeTransformationModel.AttributeModel &&
                existingOtherModel.AggregateFunction == attributeTransformationModel.AggregateFunction;

                if (existingModel != null)
                    histogramOperationViewModel.HistogramOperationModel.RemoveAttributeUsageTransformationModel(axis, existingModel);
                if (!histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    var value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                    value.AggregateFunction = AggregateFunction.Count;
                    histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(axis, attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
                if (swapAxes)
                {
                    histogramOperationViewModel.HistogramOperationModel.RemoveAttributeUsageTransformationModel(otherAxis, existingOtherModel);
                    if (!histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                    {
                        var value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                        value.AggregateFunction = AggregateFunction.Count;
                        histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                    }
                    histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(otherAxis, existingModel);

                }
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private static void addAttachmentViewModels(OperationViewModel operationViewModel)
        {
            foreach (var attachmentOrientation in Enum.GetValues(typeof(AttachmentOrientation)).Cast<AttachmentOrientation>())
                operationViewModel.AttachementViewModels.Add(new AttachmentViewModel
                {
                    AttachmentOrientation = attachmentOrientation,
                    OperationViewModel = operationViewModel
                });
        }

        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(SchemaModel schemaModel, AttributeModel attributeModel, Pt position)
        {
            var histogramOperationModel = new HistogramOperationModel(schemaModel);
            var histogramOperationViewModel = new HistogramOperationViewModel(histogramOperationModel);
            histogramOperationViewModel.Position = position;
            addAttachmentViewModels(histogramOperationViewModel);

            // axis attachment view models
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Left, AttributeUsage.Y, new Vec(50, 200), 270, false, true);
            createRightHistogramMenu(histogramOperationViewModel);
            createTopHistogramMenu(histogramOperationViewModel);


            if ((attributeModel != null) && attributeModel is AttributeFieldModel)
            {
                var inputFieldModel = attributeModel as AttributeFieldModel;
                if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.ENUM)
                {
                    histogramOperationModel.VisualizationType = VisualizationType.plot;

                    var x = new AttributeTransformationModel(inputFieldModel);
                    x.AggregateFunction = AggregateFunction.None;

                    var value = new AttributeTransformationModel(inputFieldModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    var y = new AttributeTransformationModel(inputFieldModel);
                    y.AggregateFunction = AggregateFunction.Count;

                    if (attributeModel.VisualizationHints.Contains(VisualizationHint.DefaultFlipAxis))
                    {
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, y);
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, x);
                    }
                    else
                    {
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, y);
                    }
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
                {
                    histogramOperationModel.VisualizationType = VisualizationType.plot;

                    var x = new AttributeTransformationModel(inputFieldModel);
                    x.AggregateFunction = AggregateFunction.None;

                    var value = new AttributeTransformationModel(inputFieldModel);
                    value.AggregateFunction = AggregateFunction.Count;

                    var y = new AttributeTransformationModel(inputFieldModel);
                    y.AggregateFunction = AggregateFunction.Count;

                    if (attributeModel.VisualizationHints.Contains(VisualizationHint.DefaultFlipAxis))
                    {
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, y);
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, x);
                    }
                    else
                    {
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
                        histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, y);
                    }
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
                {
                }
                else
                {
                    histogramOperationModel.VisualizationType = VisualizationType.table;
                    var x = new AttributeTransformationModel(inputFieldModel);
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
                }
            }
            else
            {
                histogramOperationModel.VisualizationType = VisualizationType.plot;
            }

            return histogramOperationViewModel;
        }

        private static void createRightExampleMenu(ExampleOperationViewModel exampleOperationViewModel)
        {
            var attachmentViewModel =
                exampleOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == AttachmentOrientation.Right);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

            var sliderItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Position = exampleOperationViewModel.Position,
                Size = new Vec(100, 50),
                TargetSize = new Vec(100, 50),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false
            };

            var attr1 = new SliderMenuItemComponentViewModel
            {
                Label = "dummy slider",
                Value = exampleOperationViewModel.ExampleOperationModel.DummyValue,
                MinValue = 1,
                MaxValue = 100
            };
            attr1.PropertyChanged += (sender, args) =>
            {
                var model = sender as SliderMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                    exampleOperationViewModel.ExampleOperationModel.DummyValue = model.FinalValue;
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            sliderItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(sliderItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private static void createBottomExampleMenu(ExampleOperationViewModel exampleOperationViewModel)
        {
            var attachmentViewModel =
                exampleOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == AttachmentOrientation.Bottom);
            attachmentViewModel.ShowOnAttributeMove = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

            var addMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(25, 25),
                TargetSize = new Vec(25, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = exampleOperationViewModel.Position
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                Label = "+",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            attr1.DroppedTriggered = attributeTransformationModel => { exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.Add(attributeTransformationModel); };

            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;

                // remove old ones first
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                    {
                        var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            (((AttributeTransformationMenuItemViewModel) mvm.MenuItemComponentViewModel).AttributeTransformationViewModel != null) &&
                            (((AttributeTransformationMenuItemViewModel) mvm.MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel ==
                             oldAttributeTransformationModel));
                        if (found != null)
                            menuViewModel.MenuItemViewModels.Remove(found);
                    }

                menuViewModel.NrRows = (int) Math.Ceiling(coll.Count/3.0) + 1;
                addMenuItem.Row = menuViewModel.NrRows - 1;

                // add new ones
                if (args.NewItems != null)
                    foreach (var newItem in args.NewItems)
                    {
                        var newAttributeTransformationModel = newItem as AttributeTransformationModel;
                        var newMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Size = new Vec(50, 50),
                            TargetSize = new Vec(50, 50),
                            Position = addMenuItem.Position
                        };
                        var newAttr = new AttributeTransformationMenuItemViewModel
                        {
                            Label = newAttributeTransformationModel.GetLabel(),
                            AttributeTransformationViewModel = new AttributeTransformationViewModel(exampleOperationViewModel, newAttributeTransformationModel),
                            TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                            CanDrag = false,
                            CanDrop = false
                        };
                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm =
                                ((AttributeTransformationMenuItemViewModel) ((MenuItemViewModel) sender1).MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                            exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }

                var count = 0;
                foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                {
                    menuItemViewModel.Column = count%3;
                    menuItemViewModel.Row = (int) Math.Floor(count/3.0);
                    count++;
                }
                attachmentViewModel.ActiveStopwatch.Restart();
                menuViewModel.FireUpdate();
            };
        }

        private static void createLeftExampleMenu(ExampleOperationViewModel exampleOperationViewModel)
        {
            var attachmentViewModel =
                exampleOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == AttachmentOrientation.Left);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var toggles = new List<ToggleMenuItemComponentViewModel>();
            var items = new List<MenuItemViewModel>();

            var count = 0;
            foreach (var exampleOperationType in new[] {ExampleOperationType.A, ExampleOperationType.B, ExampleOperationType.C})
            {
                var toggleMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Row = count,
                    RowSpan = 0,
                    Column = 0,
                    Position = exampleOperationViewModel.Position,
                    Size = new Vec(50, 32),
                    TargetSize = new Vec(50, 32)
                };

                var toggle = new ToggleMenuItemComponentViewModel
                {
                    Label = exampleOperationType.ToString().ToLower(),
                    IsChecked = exampleOperationViewModel.ExampleOperationModel.ExampleOperationType == exampleOperationType
                };
                toggles.Add(toggle);
                toggleMenuItem.MenuItemComponentViewModel = toggle;
                toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                {
                    var model = sender2 as ToggleMenuItemComponentViewModel;
                    if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        if (model.IsChecked)
                        {
                            attachmentViewModel.ActiveStopwatch.Restart();
                            exampleOperationViewModel.ExampleOperationModel.ExampleOperationType = exampleOperationType;
                            foreach (var tg in model.OtherToggles)
                                tg.IsChecked = false;
                        }
                };
                menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                items.Add(toggleMenuItem);
                count++;
            }

            foreach (var mi in items)
                (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
        }

        public static ExampleOperationViewModel CreateDefaultExampleOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            var exampleOperationModel = new ExampleOperationModel(schemaModel);
            var exampleOperationViewModel = new ExampleOperationViewModel(exampleOperationModel);
            exampleOperationViewModel.Position = position;
            addAttachmentViewModels(exampleOperationViewModel);
            createBottomExampleMenu(exampleOperationViewModel);
            createRightExampleMenu(exampleOperationViewModel);
            createLeftExampleMenu(exampleOperationViewModel);
            return exampleOperationViewModel;
        }
        private static void createCalculationMenu(CalculationOperationViewModel calculationOperationViewModel, AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel =
                calculationOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = calculationOperationViewModel.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = "test"  // bcz: placeholder name for the newly defined field
            };

            // bcz: placeholder to get an Attribute{Transformation}Model for the newly defined field.
            var inputModels = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First()
                     .InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;
            AttributeTransformationModel attributeTransformationModel = null;
            attributeTransformationModel = new AttributeTransformationModel(inputModels.First() as AttributeFieldModel);
            attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(calculationOperationViewModel, attributeTransformationModel);
            attr1.TappedTriggered = (() => attr1.Editing = Visibility.Visible);
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        private static void createDefinitionMenu(DefinitionOperationViewModel defintionOperationViewModel, AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel =
                defintionOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = defintionOperationViewModel.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = "test"  // bcz: placeholder name for the newly defined field
            };

            // bcz: placeholder to get an Attribute{Transformation}Model for the newly defined field.
            var inputModels = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First()
                     .InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;
            AttributeTransformationModel attributeTransformationModel = null;
            attributeTransformationModel = new AttributeTransformationModel(inputModels.First() as AttributeFieldModel);
            attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(defintionOperationViewModel, attributeTransformationModel);
            attr1.TappedTriggered = (() => attr1.Editing = Visibility.Visible);
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }
        public static CalculationOperationViewModel CreateDefaultCalculationOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse = false)
        {
            var calculationOperationModel = new CalculationOperationModel(schemaModel);
            var calculationOperationViewModel = new CalculationOperationViewModel(calculationOperationModel, fromMouse);
            calculationOperationViewModel.Position = position;
            addAttachmentViewModels(calculationOperationViewModel);
            createCalculationMenu(calculationOperationViewModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);

            return calculationOperationViewModel;
        }
        
        public static DefinitionOperationViewModel CreateDefaultDefinitionOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse=false)
        {
            var definitionOperationModel = new DefinitionOperationModel(schemaModel);
            var definitionOperationViewModel = new DefinitionOperationViewModel(definitionOperationModel, fromMouse);
            definitionOperationViewModel.Position = position;
            addAttachmentViewModels(definitionOperationViewModel);
            createDefinitionMenu(definitionOperationViewModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);

            return definitionOperationViewModel;
        }
        public static FilterOperationViewModel CreateDefaultFilterOperationViewModel(SchemaModel schemaModel, Pt position, bool fromMouse)
        {
            var filterOperationModel = new FilterOperationModel(schemaModel);
            var filterOperationViewModel = new FilterOperationViewModel(filterOperationModel, fromMouse);
            filterOperationViewModel.Position = position;
            addAttachmentViewModels(filterOperationViewModel);
            //createBottomExampleMenu(exampleOperationViewModel);
            //createRightExampleMenu(exampleOperationViewModel);
            //createLeftExampleMenu(exampleOperationViewModel);
            return filterOperationViewModel;
        }
    }
}