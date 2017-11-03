using IDEA_common.catalog;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using IDEA_common.aggregates;

namespace PanoramicDataWin8.model.view.operation
{
    public class RawDataOperationViewModel : AttributeUsageOperationViewModel
    {
        private void createAxisMenu(AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);

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
                Position = Position,
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
            RawDataOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();
                attr1.Label = attributeTransformationModel == null ? "" : attributeTransformationModel.GetLabel();
                attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, coll.FirstOrDefault());

                if (attributeTransformationModel != null)
                {
                    attributeTransformationModel.PropertyChanged += (sender2, args2) =>
                    {
                        attr1.Label = (sender2 as AttributeTransformationModel).GetLabel();
                    };
                    attributeTransformationModel.AttributeModel.PropertyChanged += (sender2, arg2) =>
                    {
                        attr1.Label = attributeTransformationModel.GetLabel();
                    };
                }

                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);

                var aom = attr1.AttributeTransformationViewModel.AttributeTransformationModel;
                var aggregateFunctions = new[] { AggregateFunction.None, AggregateFunction.Count }.ToList();
                if (aom != null)
                {
                    if (aom.AttributeModel.DataType == DataType.Float ||
                        aom.AttributeModel.DataType == DataType.Double ||
                        aom.AttributeModel.DataType == DataType.Int)
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
                            Position = Position,
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
                if (attributeTransformationModel.AttributeModel.DataType == DataType.Undefined)
                    return;
                var existingModel = RawDataOperationModel.GetAttributeUsageTransformationModel(axis).Any() ?
                      RawDataOperationModel.GetAttributeUsageTransformationModel(axis).First() : null;

                if (existingModel != null)
                    RawDataOperationModel.RemoveAttributeUsageTransformationModel(axis, existingModel);
                if (!RawDataOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    var value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                    value.AggregateFunction = AggregateFunction.None;
                    RawDataOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                RawDataOperationModel.AddAttributeUsageTransformationModel(axis, attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        public RawDataOperationViewModel(RawDataOperationModel rawDataOperationModel, AttributeModel attributeModel) : base(rawDataOperationModel)
        {
            addAttachmentViewModels();

            // axis attachment view models
            createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createTopRightFilterDragMenu();
            createTopInputsExpandingMenu(8);

            if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.ENUM ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.CATEGORY ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
            {
                var x = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.None };
                rawDataOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
            {
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.VECTOR)
            {
            }
            else
            {
            }
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;
        
    }
}