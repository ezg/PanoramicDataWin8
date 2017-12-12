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
using System;

namespace PanoramicDataWin8.model.view.operation
{
    public class RawDataOperationViewModel : OperationViewModel
    {
        MenuViewModel     menuViewModel;
        MenuItemViewModel menuItemViewModel;
        public RawDataOperationViewModel(RawDataOperationModel rawDataOperationModel, AttributeModel attributeModel) : base(rawDataOperationModel)
        {
            createTopRightFilterDragMenu();
            menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, RawDataOperationModel.AttributeTransformationModelParameters, "+", 50, 100, true, false, out menuItemViewModel);
            RawDataOperationModel.AttributeTransformationModelParameters.CollectionChanged += AttributeUsageModels_CollectionChanged;
            
            PropertyChanged += RawDataOperationViewModel_PropertyChanged;
        }

        private void RawDataOperationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Size")
            {
                menuViewModel.ParentSize = this.Size;
            }
        }

        public void ForceDrop(AttributeViewModel am)
        {
            (menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel).DroppedTriggered(am);
        }
        AttributeMenuItemViewModel SelectedColumn = null;
        private void AttributeUsageModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var mivm in menuViewModel.MenuItemViewModels.Where((mivm) => e.NewItems.Contains((mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel)))
                {
                    var amivm = (mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel);
                    var avm = amivm.AttributeViewModel;
                    RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    avm.AttributeTransformationModel.AggregateFunction = AggregateFunction.None; // strip off any aggregate function when dropping onto a RawData View

                    amivm.TappedTriggered = ((args) =>
                    {
                        if (!args.IsRightMouse)
                        {
                            if (SelectedColumn != amivm)
                            {
                                addColumnOptions(amivm, AttachmentOrientation.Top);
                                SelectedColumn = amivm;
                            }

                            AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();

                            foreach (var toggle in menuViewModel.MenuItemViewModels.Where((t) => t.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).Select((t) => t.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel))
                                toggle.IsVisible = !toggle.IsVisible;
                        }
                    });
                }
            }
            if (e.OldItems != null)
            {
                RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }
        void addColumnOptions(AttributeMenuItemViewModel attributeMenuItemViewModel, AttachmentOrientation orientation)
        {
            // remove old ones first
            foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                menuViewModel.MenuItemViewModels.Remove(mvm); 
            var attributeTransformationModel = attributeMenuItemViewModel.AttributeViewModel?.AttributeTransformationModel;
            if (attributeTransformationModel != null)
            {
                bool computeAsHistogram = RawDataOperationModel.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).Any();

                int count = 2 + (computeAsHistogram ? 0 : 1);
                if (computeAsHistogram && !attributeTransformationModel.GroupBy)
                    foreach (var aggregationFunction in attributeTransformationModel.AggregateFunctions)
                        if (aggregationFunction != AggregateFunction.None)
                        {
                            menuViewModel.MenuItemViewModels.Add(createColumnOptionToggleMenuItem(attributeMenuItemViewModel, count++, attributeTransformationModel, aggregationFunction.ToString(), 
                                                                 orientation, attributeTransformationModel.AggregateFunction == aggregationFunction));
                        }
                var toggles = menuViewModel.MenuItemViewModels.Select(i => i.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
                foreach (var t in toggles.Where(t => t != null))
                    t.OtherToggles.AddRange(toggles.Where(ti => ti != null && ti != t));
                
                menuViewModel.MenuItemViewModels.Insert(1, createColumnOptionToggleMenuItem(attributeMenuItemViewModel, 1, attributeTransformationModel, "group by", orientation, attributeTransformationModel.GroupBy));
                if (!computeAsHistogram)
                    menuViewModel.MenuItemViewModels.Insert(1, createColumnOptionToggleMenuItem(attributeMenuItemViewModel, 2, attributeTransformationModel, "sort", orientation, false));
            }
        }

        MenuItemViewModel createColumnOptionToggleMenuItem(AttributeMenuItemViewModel attributeMenuItemViewModel, int count, AttributeTransformationModel atm, string function, AttachmentOrientation orientation, bool isChecked)
        {
            var toggleMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Column = count,
                Row    = 0,
                RowSpan = 1,
                Position = this.Position,
                Size = new Vec(32, 32),
                TargetSize = new Vec(32, 32),
                MenuItemComponentViewModel = new ToggleMenuItemComponentViewModel
                {
                    Label = function,
                    IsVisible = false,
                    IsChecked = isChecked
                }
            };

            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                bool fire = true;
                bool tapTrigger = true;
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (function == "group by")
                    {
                        atm.GroupBy = model.IsChecked;
                        atm.AggregateFunction = AggregateFunction.None;
                    }
                    else if (function == "sort")
                    {
                        tapTrigger = false;
                        fire = false; // don't need to recompute anything, just need to order the display of the results
                        var curSort = RawDataOperationModel?.Function?.Sorted?.Item1 == SelectedColumn?.AttributeViewModel?.AttributeModel?.RawName ? RawDataOperationModel?.Function?.Sorted?.Item2 : null;
                        var sort = model.IsChecked ? (bool?)(curSort == null || curSort == false ? true : false) :
                                                            (curSort == true ? (bool?)false : null);
                        if (model.IsChecked)
                        {
                            if (SelectedColumn != null)
                                RawDataOperationModel.Function = new RawDataOperationModel.FunctionApplied
                                {
                                    Sorted = sort == null ? null : new Tuple<string, bool>(SelectedColumn.AttributeViewModel.AttributeModel.RawName, curSort == null || curSort == false ? true : false)
                                };
                        }
                        else
                        {
                            if (sort != null)
                            {
                                RawDataOperationModel.Function = new RawDataOperationModel.FunctionApplied
                                {
                                    Sorted = new Tuple<string, bool>(SelectedColumn.AttributeViewModel.AttributeModel.RawName, (bool)true)
                                };
                                model.IsChecked = true;
                            }
                            else
                            {
                                RawDataOperationModel.Function = new RawDataOperationModel.FunctionApplied { Sorted = null };
                                fire = true; // need to recompute everything since we don't keep track of the original unsorted results
                            }
                        }
                    }
                    else if (model.IsChecked)
                    {
                        var newAgg = function == AggregateFunction.Sum.ToString() ? AggregateFunction.Sum :
                                     function == AggregateFunction.Avg.ToString() ? AggregateFunction.Avg :
                                     function == AggregateFunction.Min.ToString() ? AggregateFunction.Min :
                                     function == AggregateFunction.Max.ToString() ? AggregateFunction.Max :
                                     function == AggregateFunction.Count.ToString() ? AggregateFunction.Count : atm.AggregateFunction;
                        if (atm.AggregateFunction == newAgg)
                            fire = false;
                        else
                        {
                            atm.AggregateFunction = newAgg;
                            foreach (var tg in model.OtherToggles)
                                if (tg.IsChecked)
                                    tg.IsChecked = false;
                        }
                        tapTrigger = false;
                    }
                    else if (model.Label == atm.AggregateFunction.ToString())
                    {
                        model.IsChecked = true;
                        fire = false;
                    }
                    else
                        fire = false;

                    if (fire)
                    {
                        AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();
                        RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                        SelectedColumn = null;
                        if (tapTrigger)
                            attributeMenuItemViewModel.TappedTriggered(new PointerManagerEvent());
                    }
                }
            };
            return toggleMenuItem;
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;

    }
}