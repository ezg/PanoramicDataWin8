using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PanoramicDataWin8.model.data;
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

                foreach (var usage in oldOperationModel.UsageAttributeTransformationModels.Keys.ToArray())
                {
                    foreach (var atm in oldOperationModel.UsageAttributeTransformationModels[usage].ToArray())
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


        private static void createAxisMenu(HistogramOperationViewModel histogramOperationViewModel, AttachmentOrientation attachmentOrientation, 
            InputUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
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
                TextAngle = textAngle
            };
            histogramOperationViewModel.HistogramOperationModel.GetUsageAttributeTransformationModel(axis).CollectionChanged += (sender, args) =>
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
            attr1.DroppedTriggered = (attributeTransformationModel) =>
            {
                if (histogramOperationViewModel.HistogramOperationModel.GetUsageAttributeTransformationModel(axis).Any())
                {
                    histogramOperationViewModel.HistogramOperationModel.RemoveUsageAttributeTransformationModel(axis, 
                        histogramOperationViewModel.HistogramOperationModel.GetUsageAttributeTransformationModel(axis).First());
                }
                histogramOperationViewModel.HistogramOperationModel.AddUsageAttributeTransformationModel(axis, attributeTransformationModel);
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
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

            // axis attachment view models
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Bottom, InputUsage.X, new Vec(200, 50), 0, true, false);
            createAxisMenu(histogramOperationViewModel, AttachmentOrientation.Left, InputUsage.Y, new Vec(50, 200), 270, false, true);
            
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
