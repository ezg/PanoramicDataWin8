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
            menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, RawDataOperationModel.AttributeTransformationModelParameters, "+", 8, out menuItemViewModel);
            RawDataOperationModel.AttributeTransformationModelParameters.CollectionChanged += AttributeUsageModels_CollectionChanged;

            var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
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
            var count = 0;
            var funcs = new List<string>(new string[] { "dummy", "group by", "avg", "sort", "count", "sum" });
            var attributeTransformationModel = attributeMenuItemViewModel.AttributeViewModel?.AttributeTransformationModel;
            if (attributeTransformationModel != null)
            {
                foreach (var aggregationFunction in funcs)
                {
                    menuViewModel.MenuItemViewModels.Add(createColumnOptionToggleMenuItem(count++, attributeTransformationModel, aggregationFunction, orientation));
                }

                var toggles = menuViewModel.MenuItemViewModels.Select(i => i.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
                foreach (var t in toggles.Where(t => t != null))
                    t.OtherToggles.AddRange(toggles.Where(ti => ti != null && ti != t));
            }
        }

        MenuItemViewModel createColumnOptionToggleMenuItem(int count, AttributeTransformationModel atm, string function, AttachmentOrientation orientation)
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
                    Label = function == null ? "Sort" : function.ToString(),
                    IsVisible = false,
                    IsChecked = false
                }
            };

            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                    if (model.IsChecked)
                    {
                        foreach (var tg in model.OtherToggles)
                            if (tg.IsChecked)
                                tg.IsChecked = false;

                        atm.AggregateFunction = function == "sum" ? AggregateFunction.Sum : function == "avg" ? AggregateFunction.Avg : function == "count" ? AggregateFunction.Count : AggregateFunction.None;
                        atm.GroupBy = function == "group by";
                        AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();

                        if (function == "sort")
                            RawDataOperationModel.Function = new RawDataOperationModel.FunctionApplied
                            {
                                Sorted = new Tuple<string, bool?>(SelectedColumn.AttributeViewModel.AttributeModel.RawName, null)
                            };

                        //else
                        //    RawDataOperationModel.Function = new RawDataOperationModel.FunctionApplied
                        //    {
                        //        Sorted = new Tuple<string, bool?>(SelectedColumn.AttributeViewModel.AttributeModel.RawName, RawDataOperationModel.Function?.Sorted?.Item2 != true)
                        //    };
                        RawDataOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    }
                    else
                        atm.AggregateFunction = AggregateFunction.None;
            };
            return toggleMenuItem;
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;

    }
}