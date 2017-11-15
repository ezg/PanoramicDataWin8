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
using System.ComponentModel;

namespace PanoramicDataWin8.model.view.operation
{
    public class BaseVisualizationOperationViewModel : AttributeUsageOperationViewModel
    {
        public BaseVisualizationOperationModel BaseVisualizationOperationModel => (BaseVisualizationOperationModel)OperationModel;
        public BaseVisualizationOperationViewModel(BaseVisualizationOperationModel baseVisualizationOperationModel) : base(baseVisualizationOperationModel)
        {

        }
        public class AttributeUsageMenu
        {
            BaseVisualizationOperationViewModel BaseVisualizationOperationViewModel;
            AttributeTransformationModel attributeTransformationModel;
            MenuItemViewModel menuItemViewModel;
            AttachmentViewModel attachmentViewModel;
            MenuViewModel menuViewModel;
            AttachmentOrientation AttachmentOrientation;
            AttributeUsage Axis;

            private void AttributeTransformationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                attributeMenuItemViewModel.Label = (sender as AttributeTransformationModel).GetLabel;
            }
            private void AttributeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                attributeMenuItemViewModel.Label = attributeTransformationModel.GetLabel;
            }
            public BaseVisualizationOperationModel BaseVisualizationOperationModel => (BaseVisualizationOperationModel)BaseVisualizationOperationViewModel.OperationModel;

            public bool CanSwapAxes { get; set; } = true;
            public bool CanTransformAxes { get; set; } = true;
            public AttributeUsageMenu(BaseVisualizationOperationViewModel baseVisualizationOperationViewModel, AttributeModel attributeModel, AttachmentOrientation attachmentOrientation, AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
            {
                Axis = axis;
                AttachmentOrientation = attachmentOrientation;
                BaseVisualizationOperationViewModel = baseVisualizationOperationViewModel;
                attachmentViewModel = BaseVisualizationOperationViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation);

                menuViewModel = BaseVisualizationOperationViewModel.createAttributeLabelMenu(AttachmentOrientation, attributeModel, Axis, size, textAngle,
                            isWidthBoundToParent, isHeightBoundToParent, droppedTriggered, out menuItemViewModel);
                menuViewModel.SynchronizeItemsDisplay = false;

                BaseVisualizationOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += AxisMenu_CollectionChanged;
            }

            void droppedTriggered(AttributeViewModel attributeViewModel)
            {
                var droppedTransformationModel = new AttributeTransformationModel(attributeViewModel.AttributeModel)
                { // copy ATM because drop target doesn't want to share parameters with source
                    AggregateFunction = CanTransformAxes ? (attributeViewModel.AttributeTransformationModel?.AggregateFunction ?? AggregateFunction.None) : AggregateFunction.None
                };
                if (droppedTransformationModel.AttributeModel.DataType != DataType.Undefined)
                {
                    var otherAxis = Axis == AttributeUsage.X ? AttributeUsage.Y : AttributeUsage.X;
                    var existingModel = BaseVisualizationOperationModel.GetAttributeUsageTransformationModel(Axis).FirstOrDefault();
                    var existingOtherModel = BaseVisualizationOperationModel.GetAttributeUsageTransformationModel(otherAxis).FirstOrDefault();
                    var swapAxes = CanSwapAxes && existingModel != null && existingOtherModel.AttributeModel == droppedTransformationModel.AttributeModel &&
                                                                      existingOtherModel.AggregateFunction == droppedTransformationModel.AggregateFunction;

                    AssignTransformationModel(droppedTransformationModel, existingModel, Axis);

                    if (swapAxes)
                    {
                        AssignTransformationModel(existingModel, existingOtherModel, otherAxis);
                    }
                }
            }

            void AssignTransformationModel(AttributeTransformationModel newModel, AttributeTransformationModel oldModel, AttributeUsage axis)
            {
                if (oldModel != null)
                {
                    BaseVisualizationOperationModel.RemoveAttributeUsageTransformationModel(axis, oldModel);
                    oldModel.PropertyChanged -= AttributeTransformationModel_PropertyChanged;
                    oldModel.AttributeModel.PropertyChanged -= AttributeModel_PropertyChanged;
                }
                if (attributeTransformationModel != null && !BaseVisualizationOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    BaseVisualizationOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue,
                        new AttributeTransformationModel(attributeTransformationModel.AttributeModel) { AggregateFunction = AggregateFunction.Count });
                }
                BaseVisualizationOperationModel.AddAttributeUsageTransformationModel(axis, newModel);
            }

            void AxisMenu_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
            {
                var oldAttributeTransformationModel = args.OldItems?.Count > 0 ? args.OldItems[0] as AttributeTransformationModel : null;
                if (oldAttributeTransformationModel != null)
                {
                    oldAttributeTransformationModel.PropertyChanged -= AttributeTransformationModel_PropertyChanged;
                    oldAttributeTransformationModel.AttributeModel.PropertyChanged -= AttributeModel_PropertyChanged;
                }

                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                attributeTransformationModel = coll.FirstOrDefault();
                attributeMenuItemViewModel.Label = attributeTransformationModel?.GetLabel;
                attributeMenuItemViewModel.AttributeViewModel = new AttributeViewModel(BaseVisualizationOperationViewModel, attributeTransformationModel);
                if (attributeTransformationModel != null)
                {
                    attributeTransformationModel.PropertyChanged += AttributeTransformationModel_PropertyChanged;
                    attributeTransformationModel.AttributeModel.PropertyChanged += AttributeModel_PropertyChanged;
                }

                if (CanTransformAxes)  // display menu options of the transformations that can be performed on the attribute
                {
                    addAttributeTransformationToggleMenuItems(attributeMenuItemViewModel);
                }
            }

            void addAttributeTransformationToggleMenuItems(AttributeMenuItemViewModel attributeMenuItemViewModel)
            {
                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);

                var atm = (attributeMenuItemViewModel.AttributeViewModel as AttributeViewModel)?.AttributeTransformationModel;
                if (atm != null)
                {
                    var count = 0;
                    foreach (var aggregationFunction in atm.AggregateFunctions)
                    {
                        menuViewModel.MenuItemViewModels.Add(createAggregateTransformationToggleMenuItem(atm, count++, aggregationFunction));
                    }

                    var toggles = menuViewModel.MenuItemViewModels.Select(i => i.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
                    foreach (var t in toggles.Where(t => t != null))
                        t.OtherToggles.AddRange(toggles.Where(ti => ti != null && ti != t));
                }
            }

            MenuItemViewModel createAggregateTransformationToggleMenuItem(AttributeTransformationModel atm, int count, AggregateFunction aggregationFunction)
            {
                var toggleMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Column = AttachmentOrientation == AttachmentOrientation.Bottom ? count : 0,
                    Row = AttachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                    RowSpan = 0,
                    Position = BaseVisualizationOperationViewModel.Position,
                    Size = new Vec(32, 32),
                    TargetSize = new Vec(32, 32),
                    MenuItemComponentViewModel = new ToggleMenuItemComponentViewModel
                    {
                        Label = aggregationFunction.ToString(),
                        IsChecked = atm.AggregateFunction == aggregationFunction
                    }
                };

                toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                {
                    var model = sender as ToggleMenuItemComponentViewModel;
                    if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        if (model.IsChecked)
                        {
                            atm.AggregateFunction = aggregationFunction;
                            foreach (var tg in model.OtherToggles)
                                tg.IsChecked = false;
                        }
                };
                return toggleMenuItem;
            }
        }
    }
}