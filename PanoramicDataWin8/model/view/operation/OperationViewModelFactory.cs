using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.operation
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
                    null, operationViewModel.Position);
                HistogramOperationModel newOperationModel = (HistogramOperationModel)newHistogramOperationViewModel.OperationModel;
                newOperationModel.VisualizationType = VisualizationType.plot;

                foreach (var usage in oldOperationModel.AttributeUsageTransformationModels.Keys.ToArray())
                {
                    foreach (var atm in oldOperationModel.AttributeUsageTransformationModels[usage].ToArray())
                    {
                        newOperationModel.AddAttributeUsageTransformationModel(usage,
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


        private static void createAxisMenu(HistogramOperationViewModel histogramOperationViewModel, AttachmentOrientation attachmentOrientation, 
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel =
                histogramOperationViewModel.AttachementViewModels.First(
                    avm => avm.AttachmentOrientation == attachmentOrientation);

            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 3 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2  :3
            };

            var menuItem = new MenuItemViewModel()
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 3 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 3,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = histogramOperationViewModel.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            AttributeTransformationMenuItemViewModel attr1 = new AttributeTransformationMenuItemViewModel()
            {
                TextAngle = textAngle, 
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"))
            };
            histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                attr1.Label = coll.FirstOrDefault() == null ? "" : coll.FirstOrDefault().GetLabel();
                attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(histogramOperationViewModel, coll.FirstOrDefault());

                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                {
                    menuViewModel.MenuItemViewModels.Remove(mvm);
                }

                var aom = attr1.AttributeTransformationViewModel.AttributeTransformationModel;
                if (aom != null &&
                    (((AttributeFieldModel)aom.AttributeModel).InputDataType == InputDataTypeConstants.INT ||
                     ((AttributeFieldModel)aom.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT))
                {
                    List<ToggleMenuItemComponentViewModel> toggles = new List<ToggleMenuItemComponentViewModel>();
                    List<MenuItemViewModel> items = new List<MenuItemViewModel>();

                    int count = 0;
                    foreach (var aggregationFunction in new AggregateFunction[] { AggregateFunction.None, AggregateFunction.Avg, AggregateFunction.Count})
                    {
                        var toggleMenuItem = new MenuItemViewModel()
                        {
                            MenuViewModel = menuViewModel,
                            Row = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                            RowSpan = 0,
                            Position = histogramOperationViewModel.Position,
                            Column = attachmentOrientation == AttachmentOrientation.Bottom ? count : 0,
                            Size = new Vec(32, 50),
                            TargetSize = new Vec(32, 50)
                        };
                        //toggleMenuItem.Position = attachmentItemViewModel.Position;
                        ToggleMenuItemComponentViewModel toggle = new ToggleMenuItemComponentViewModel()
                        {
                            Label = aggregationFunction.ToString(),
                            IsChecked = aom.AggregateFunction == aggregationFunction
                        };
                        toggles.Add(toggle);
                        toggleMenuItem.MenuItemComponentViewModel = toggle;
                        toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                        {
                            var model = (sender2 as ToggleMenuItemComponentViewModel);
                            if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                            {
                                if (model.IsChecked)
                                {
                                    aom.AggregateFunction = aggregationFunction;
                                    foreach (var tg in model.OtherToggles)
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
            };
            attr1.TappedTriggered = () =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
            attr1.DroppedTriggered = (attributeTransformationModel) =>
            {
                if (histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).Any())
                {
                    histogramOperationViewModel.HistogramOperationModel.RemoveAttributeUsageTransformationModel(axis, 
                        histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(axis).First());
                }
                if (!histogramOperationViewModel.HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    AttributeTransformationModel value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                    value.AggregateFunction = AggregateFunction.Count;
                    histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                histogramOperationViewModel.HistogramOperationModel.AddAttributeUsageTransformationModel(axis, attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private static void addAttachmentViewModels(OperationViewModel operationViewModel)
        {
            foreach (var attachmentOrientation in Enum.GetValues(typeof(AttachmentOrientation)).Cast<AttachmentOrientation>())
            {
                operationViewModel.AttachementViewModels.Add(new AttachmentViewModel()
                {
                    AttachmentOrientation = attachmentOrientation,
                    OperationViewModel = operationViewModel,
                });
            }
        }

        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(SchemaModel schemaModel, AttributeModel attributeModel, Pt position)
        {
            HistogramOperationModel histogramOperationModel = new HistogramOperationModel(schemaModel);
            HistogramOperationViewModel histogramOperationViewModel = new HistogramOperationViewModel(histogramOperationModel);
            histogramOperationViewModel.Position = position;
            addAttachmentViewModels(histogramOperationViewModel);

            // axis attachment view models
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Left, AttributeUsage.Y, new Vec(50, 200), 270, false, true);


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

                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, y);
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
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

                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y, y);
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                else if (inputFieldModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
                {
                }
                else
                {
                    histogramOperationModel.VisualizationType = VisualizationType.table;
                    AttributeTransformationModel x = new AttributeTransformationModel(inputFieldModel);
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

            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

            var sliderItem = new MenuItemViewModel()
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

            SliderMenuItemComponentViewModel attr1 = new SliderMenuItemComponentViewModel()
            {
                Label = "dummy slider",
                Value = exampleOperationViewModel.ExampleOperationModel.DummyValue,
                MinValue = 1,
                MaxValue = 100
            };
            attr1.PropertyChanged += (sender, args) =>
            {
                var model = (sender as SliderMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                {
                    exampleOperationViewModel.ExampleOperationModel.DummyValue = model.FinalValue;
                }
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

            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

            var addMenuItem = new MenuItemViewModel()
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
            AttributeTransformationMenuItemViewModel attr1 = new AttributeTransformationMenuItemViewModel()
            {
                Label = "+",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            attr1.DroppedTriggered = (attributeTransformationModel) =>
            {
                exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.Add(attributeTransformationModel);
            };

            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;

                // remove old ones first
                if (args.OldItems != null)
                {
                    foreach (var oldItem in args.OldItems)
                    {
                        var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            ((AttributeTransformationMenuItemViewModel) mvm.MenuItemComponentViewModel).AttributeTransformationViewModel != null &&
                            ((AttributeTransformationMenuItemViewModel) mvm.MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel == oldAttributeTransformationModel);
                        if (found != null)
                        {
                            menuViewModel.MenuItemViewModels.Remove(found);
                        }
                    }
                }

                menuViewModel.NrRows = (int)Math.Ceiling(coll.Count/3.0) +1;
                addMenuItem.Row = menuViewModel.NrRows - 1;

                // add new ones
                if (args.NewItems != null)
                {
                    foreach (var newItem in args.NewItems)
                    {
                        var newAttributeTransformationModel = newItem as AttributeTransformationModel;
                        var newMenuItem = new MenuItemViewModel()
                        {
                            MenuViewModel = menuViewModel,
                            Size = new Vec(50, 50),
                            TargetSize = new Vec(50, 50),
                            Position = addMenuItem.Position
                        };
                        var newAttr = new AttributeTransformationMenuItemViewModel()
                        {
                            Label = newAttributeTransformationModel.GetLabel(),
                            AttributeTransformationViewModel = new AttributeTransformationViewModel(exampleOperationViewModel, newAttributeTransformationModel),
                            TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                            CanDrag = false,
                            CanDrop = false
                        };
                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm =  ((AttributeTransformationMenuItemViewModel) ((MenuItemViewModel) sender1).MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                            exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }
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

            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 3
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            List<ToggleMenuItemComponentViewModel> toggles = new List<ToggleMenuItemComponentViewModel>();
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();

            int count = 0;
            foreach (var exampleOperationType in new ExampleOperationType[] { ExampleOperationType.A, ExampleOperationType.B, ExampleOperationType.C })
            {
                var toggleMenuItem = new MenuItemViewModel()
                {
                    MenuViewModel = menuViewModel,
                    Row = count,
                    RowSpan = 0,
                    Column = 0,
                    Position = exampleOperationViewModel.Position,
                    Size = new Vec(50, 32),
                    TargetSize = new Vec(50, 32)
                };

                ToggleMenuItemComponentViewModel toggle = new ToggleMenuItemComponentViewModel()
                {
                    Label = exampleOperationType.ToString().ToLower(),
                    IsChecked = exampleOperationViewModel.ExampleOperationModel.ExampleOperationType == exampleOperationType
                };
                toggles.Add(toggle);
                toggleMenuItem.MenuItemComponentViewModel = toggle;
                toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                {
                    var model = (sender2 as ToggleMenuItemComponentViewModel);
                    if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                    {
                        if (model.IsChecked)
                        {
                            attachmentViewModel.ActiveStopwatch.Restart();
                            exampleOperationViewModel.ExampleOperationModel.ExampleOperationType = exampleOperationType;
                            foreach (var tg in model.OtherToggles)
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
        
        public static ExampleOperationViewModel CreateDefaultExampleOperationViewModel(SchemaModel schemaModel, Pt position)
        {
            ExampleOperationModel exampleOperationModel = new ExampleOperationModel(schemaModel);
            ExampleOperationViewModel exampleOperationViewModel = new ExampleOperationViewModel(exampleOperationModel);
            exampleOperationViewModel.Position = position;
            addAttachmentViewModels(exampleOperationViewModel);
            createBottomExampleMenu(exampleOperationViewModel);
            createRightExampleMenu(exampleOperationViewModel);
            createLeftExampleMenu(exampleOperationViewModel);
            return exampleOperationViewModel;
        }
    }
}
