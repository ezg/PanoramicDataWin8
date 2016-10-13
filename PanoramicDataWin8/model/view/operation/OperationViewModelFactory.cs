﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
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

        public static HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(SchemaModel schemaModel, AttributeModel attributeModel)
        {
            HistogramOperationModel histogramOperationModel = new HistogramOperationModel(schemaModel);
            HistogramOperationViewModel histogramOperationViewModel = new HistogramOperationViewModel(histogramOperationModel);
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
                NrColumns = 1,
                NrRows = 1
            };

            var menuItem = new MenuItemViewModel()
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
                IsHeightBoundToParent = false
            };
            AttributeTransformationMenuItemViewModel attr1 = new AttributeTransformationMenuItemViewModel()
            {
                Label = "+",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true

            };
            exampleOperationViewModel.ExampleOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                /*var coll = sender as ObservableCollection<AttributeTransformationModel>;
               attr1.Label = coll.FirstOrDefault() == null ? "" : coll.FirstOrDefault().GetLabel();
               attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(exampleOperationViewModel, coll.FirstOrDefault());

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
                   foreach (var aggregationFunction in new AggregateFunction[] { AggregateFunction.None, AggregateFunction.Avg, AggregateFunction.Count })
                   {
                       var toggleMenuItem = new MenuItemViewModel()
                       {
                           MenuViewModel = menuViewModel,
                           Row = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                           RowSpan = 0,
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
               }*/
            };
            attr1.TappedTriggered = () =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
            attr1.DroppedTriggered = (attributeTransformationModel) =>
            {
                var model = exampleOperationViewModel.ExampleOperationModel;
                model.AttributeUsageTransformationModels.Add(attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        public static ExampleOperationViewModel CreateDefaultExampleOperationViewModel(SchemaModel schemaModel)
        {
            ExampleOperationModel exampleOperationModel = new ExampleOperationModel(schemaModel);
            ExampleOperationViewModel exampleOperationViewModel = new ExampleOperationViewModel(exampleOperationModel);
            addAttachmentViewModels(exampleOperationViewModel);
            createBottomExampleMenu(exampleOperationViewModel);
            return exampleOperationViewModel;
        }
    }
}
