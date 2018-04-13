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
        const string GroupBy = "group by";
        const string Sort = "sort";
        MenuViewModel     menuViewModel;
        MenuItemViewModel menuItemViewModel;
        public RawDataOperationViewModel(RawDataOperationModel rawDataOperationModel, AttributeModel attributeModel) : base(rawDataOperationModel)
        {
            createTopRightFilterDragMenu();
            menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, RawDataOperationModel.AttributeTransformationModelParameters, "+", 50, 100, true, false, true, out menuItemViewModel);
            RawDataOperationModel.AttributeTransformationModelParameters.CollectionChanged += AttributeUsageModels_CollectionChanged;
        }

        public override void Dispose()
        {
            RawDataOperationModel.Dispose();
        }

        public void ForceDrop(AttributeViewModel am)
        {
            (menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel).DroppedTriggered(am);
        }
        AttributeMenuItemViewModel SelectedDataAttribute = null;
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
                            if (SelectedDataAttribute != amivm)
                            {
                                addColumnOptions(amivm, AttachmentOrientation.Top);
                                SelectedDataAttribute = amivm;
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
            {
                if (mvm.MenuItemComponentViewModel is AttributeToggleMenuItemComponentViewModel atToggleMenu)
                    atToggleMenu.Dispose();
                menuViewModel.MenuItemViewModels.Remove(mvm);
            }
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
                
                menuViewModel.MenuItemViewModels.Insert(1, createColumnOptionToggleMenuItem(attributeMenuItemViewModel, 1, attributeTransformationModel, GroupBy, orientation, attributeTransformationModel.GroupBy));
                menuViewModel.MenuItemViewModels.Insert(1, createColumnOptionToggleMenuItem(attributeMenuItemViewModel, 2, attributeTransformationModel, Sort,     orientation, attributeTransformationModel.OrderingFunction != OrderingFunction.None));
            }
        }

        public class AttributeToggleMenuItemComponentViewModel : ToggleMenuItemComponentViewModel
        {
            private AttributeTransformationModel _atm;
            // when the AggregateFunction or GroupBy parameters of a AttributeTransformationModel have changed,
            // we need to make sure that the appropriate menu items are toggled on/off.
            void Atm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(AttributeTransformationModel.AggregateFunction))
                {
                    if (this.Label == _atm.AggregateFunction.ToString())
                    {
                        if (this.IsChecked == false)
                            this.IsChecked = true;
                    }
                    else if (this.Label != GroupBy)
                        if (this.IsChecked == true)
                            this.IsChecked = false;
                }
                if (e.PropertyName == nameof(OrderingFunction))
                {
                    if (this.Label == Sort)
                    {
                        if (_atm.OrderingFunction != OrderingFunction.None)
                        {
                            if (this.IsChecked == false)
                                this.IsChecked = true;
                        }
                        else if (this.IsChecked == true)
                            this.IsChecked = false;
                    }
                }
                if (e.PropertyName == nameof(AttributeTransformationModel.GroupBy))
                {
                    if (this.Label == GroupBy)
                        if (this.IsChecked != _atm.GroupBy)
                            this.IsChecked = _atm.GroupBy;
                }
            }
            public void Dispose()
            {
                _atm.PropertyChanged -= Atm_PropertyChanged;
            }
            public AttributeToggleMenuItemComponentViewModel(AttributeTransformationModel atm): base()
            {
                _atm = atm;
                _atm.PropertyChanged += Atm_PropertyChanged;
            } 
        }

        MenuItemViewModel createColumnOptionToggleMenuItem(AttributeMenuItemViewModel attributeMenuItemViewModel, int count, AttributeTransformationModel atm, string function, AttachmentOrientation orientation, bool isChecked)
        {
            var toggleMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Column = count,
                Row = 0,
                RowSpan = 1,
                Position = this.Position,
                Size = new Vec(32, 32),
                TargetSize = new Vec(32, 32),
                MenuItemComponentViewModel = new AttributeToggleMenuItemComponentViewModel(atm)
                {
                    CanToggleOff = function == Sort || function == GroupBy,
                    TriState = function == Sort,
                    Label = function,
                    IsVisible = false,
                    IsChecked = isChecked
                }
            };

            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                bool fire = true;
                if (args.PropertyName == model.GetPropertyName(() => model.TriStateCount))
                {
                    if (function == Sort)
                    {
                        RawDataOperationModel.SetOrderingFunction(atm,
                            model.TriStateCount == 0 ? OrderingFunction.None : model.TriStateCount == 1 ? OrderingFunction.SortDown : OrderingFunction.SortUp);
                        AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();
                        this.RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs() { });
                    }
                }
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (function == GroupBy)
                    {
                        atm.AggregateFunction = AggregateFunction.None;
                        atm.GroupBy = model.IsChecked;
                        if (!model.IsChecked)
                            SelectedDataAttribute = null;
                    }
                    else if (model.IsChecked)
                    {
                        var newAgg = function == AggregateFunction.Sum.ToString() ? AggregateFunction.Sum :
                                     function == AggregateFunction.Avg.ToString() ? AggregateFunction.Avg :
                                     function == AggregateFunction.Min.ToString() ? AggregateFunction.Min :
                                     function == AggregateFunction.Max.ToString() ? AggregateFunction.Max :
                                     function == AggregateFunction.Count.ToString() ? AggregateFunction.Count : atm.AggregateFunction;
                        fire = RawDataOperationModel.SetAggregationForModel(atm, newAgg);
                    }
                    else
                        fire = false;

                    if (fire)
                    {
                        AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();
                        this.RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs() { });
                    }
                }
            };
            return toggleMenuItem;
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;

    }
}