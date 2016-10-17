using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttributeTransformationViewModel : BindableBase
    {
        private AttachmentOrientation _attachmentOrientation;

        private AttributeTransformationModel _attributeTransformationModel;

        private Thickness _borderThicknes;

        private bool _hideAggregationFunction;

        private bool _isDraggable = true;

        private bool _isDraggableByPen;

        private bool _isFiltered;

        private bool _isGestureEnabled;

        private bool _isHighlighted;

        private bool _isMenuEnabled = true;

        private bool _isNoChrome;

        private bool _isRemoveEnabled;

        private bool _isScaleFunctionEnabled = true;


        private bool _isShadow;

        private string _mainLabel;

        private OperationViewModel _operationViewModel;

        private Vec _size = new Vec(50, 50);

        private string _sublabel;


        private double _textAngle;

        public AttributeTransformationViewModel()
        {
        }

        public AttributeTransformationViewModel(OperationViewModel operationViewModel, AttributeTransformationModel attributeTransformationModel)
        {
            OperationViewModel = operationViewModel;
            AttributeTransformationModel = attributeTransformationModel;
        }

        public AttachmentOrientation AttachmentOrientation
        {
            get { return _attachmentOrientation; }
            set { SetProperty(ref _attachmentOrientation, value); }
        }

        public double TextAngle
        {
            get { return _textAngle; }
            set { SetProperty(ref _textAngle, value); }
        }

        public bool HideAggregationFunction
        {
            get { return _hideAggregationFunction; }
            set
            {
                SetProperty(ref _hideAggregationFunction, value);
                updateLabels();
            }
        }

        public bool IsShadow
        {
            get { return _isShadow; }
            set { SetProperty(ref _isShadow, value); }
        }

        public Thickness BorderThicknes
        {
            get { return _borderThicknes; }
            set { SetProperty(ref _borderThicknes, value); }
        }

        public bool IsDraggable
        {
            get { return _isDraggable; }
            set { SetProperty(ref _isDraggable, value); }
        }

        public bool IsDraggableByPen
        {
            get { return _isDraggableByPen; }
            set { SetProperty(ref _isDraggableByPen, value); }
        }

        public bool IsNoChrome
        {
            get { return _isNoChrome; }
            set { SetProperty(ref _isNoChrome, value); }
        }

        public bool IsFiltered
        {
            get { return _isFiltered; }
            set { SetProperty(ref _isFiltered, value); }
        }

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetProperty(ref _isHighlighted, value); }
        }

        public bool IsMenuEnabled
        {
            get { return _isMenuEnabled; }
            set { SetProperty(ref _isMenuEnabled, value); }
        }

        public bool IsScaleFunctionEnabled
        {
            get { return _isScaleFunctionEnabled; }
            set { SetProperty(ref _isScaleFunctionEnabled, value); }
        }

        public bool IsRemoveEnabled
        {
            get { return _isRemoveEnabled; }
            set { SetProperty(ref _isRemoveEnabled, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        public string MainLabel
        {
            get { return _mainLabel; }
            set { SetProperty(ref _mainLabel, value); }
        }

        public string SubLabel
        {
            get { return _sublabel; }
            set { SetProperty(ref _sublabel, value); }
        }

        public OperationViewModel OperationViewModel
        {
            get { return _operationViewModel; }
            set { SetProperty(ref _operationViewModel, value); }
        }

        public AttributeTransformationModel AttributeTransformationModel
        {
            get { return _attributeTransformationModel; }
            set
            {
                if (_attributeTransformationModel != null)
                    _attributeTransformationModel.PropertyChanged -= AttributeTransformationModelPropertyChanged;
                SetProperty(ref _attributeTransformationModel, value);
                if (_attributeTransformationModel != null)
                {
                    _attributeTransformationModel.PropertyChanged += AttributeTransformationModelPropertyChanged;
                    updateLabels();
                }
            }
        }

        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelMoved;
        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelDropped;

        public void FireMoved(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelMoved?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        public void FireDropped(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelDropped?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        public MenuViewModel CreateMenuViewModel(Rct bounds)
        {
            var menuViewModel = new MenuViewModel
            {
                AttributeTransformationViewModel = this,
                AttachmentOrientation = AttachmentOrientation
            };
            MenuItemViewModel menuItem = null;
            menuViewModel.NrRows = 3;
            menuViewModel.NrColumns = 4;

            // Sort None
            menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            var toggle1 = new ToggleMenuItemComponentViewModel
            {
                Label = "None",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.None
            };
            menuItem.MenuItemComponentViewModel = toggle1;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.None;
                        foreach (var tg in model.OtherToggles)
                            tg.IsChecked = false;
                    }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // Sort Asc
            menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 1,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            var toggle2 = new ToggleMenuItemComponentViewModel
            {
                Label = "asc",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.Asc
            };
            menuItem.MenuItemComponentViewModel = toggle2;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.Asc;
                        foreach (var tg in model.OtherToggles)
                            tg.IsChecked = false;
                    }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // Sort Desc
            menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 2,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            var toggle3 = new ToggleMenuItemComponentViewModel
            {
                Label = "desc",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.Desc
            };
            menuItem.MenuItemComponentViewModel = toggle3;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = sender as ToggleMenuItemComponentViewModel;
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.Desc;
                        foreach (var tg in model.OtherToggles)
                            tg.IsChecked = false;
                    }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // sort toogle groups
            toggle1.OtherToggles.AddRange(new[] {toggle2, toggle3});
            toggle2.OtherToggles.AddRange(new[] {toggle1, toggle3});
            toggle3.OtherToggles.AddRange(new[] {toggle1, toggle2});

            if ((((AttributeFieldModel) _attributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.INT) ||
                (((AttributeFieldModel) _attributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT))
            {
                var toggles = new List<ToggleMenuItemComponentViewModel>();
                var items = new List<MenuItemViewModel>();

                var count = 0;
                foreach (var aggregationFunction in Enum.GetValues(typeof(AggregateFunction)).Cast<AggregateFunction>())
                {
                    menuItem = new MenuItemViewModel
                    {
                        MenuViewModel = menuViewModel,
                        Row = count <= 2 ? 0 : 1,
                        RowSpan = count <= 2 ? 1 : 2,
                        Column = count%3 + 1,
                        Size = new Vec(32, 50),
                        TargetSize = new Vec(32, 50)
                    };
                    menuItem.Position = bounds.TopLeft;
                    var toggle = new ToggleMenuItemComponentViewModel
                    {
                        Label = aggregationFunction.ToString(),
                        IsChecked = _attributeTransformationModel.AggregateFunction == aggregationFunction
                    };
                    toggles.Add(toggle);
                    menuItem.MenuItemComponentViewModel = toggle;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = sender as ToggleMenuItemComponentViewModel;
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                            if (model.IsChecked)
                            {
                                _attributeTransformationModel.AggregateFunction = aggregationFunction;
                                foreach (var tg in model.OtherToggles)
                                    tg.IsChecked = false;
                            }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);
                    items.Add(menuItem);
                    count++;
                }

                foreach (var mi in items)
                    (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
            }


            return menuViewModel;
        }

        private void AttributeTransformationModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateLabels();
        }

        private void updateLabels()
        {
            if (_attributeTransformationModel != null)
            {
                MainLabel = _attributeTransformationModel.AttributeModel.RawName; //columnDescriptor.GetLabels(out mainLabel, out subLabel);

                var mainLabel = _attributeTransformationModel.AttributeModel.RawName;
                var subLabel = "";

                if (!HideAggregationFunction)
                {
                    mainLabel = addDetailToLabel(mainLabel);
                    MainLabel = mainLabel.Replace("_", " ");
                }
                else
                {
                    mainLabel = mainLabel;
                }
                SubLabel = subLabel;
            }
        }

        private string addDetailToLabel(string name)
        {
            if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Avg)
                name = "avg(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Count)
                name = "count";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Max)
                name = "max(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Min)
                name = "min(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Sum)
                name = "sum(" + name + ")";
            /*else if (AttributeTransformationViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (AttributeTransformationModel.ScaleFunction != ScaleFunction.None)
                if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Log)
                    name += " [Log]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Normalize)
                    name += " [Normalize]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotal)
                    name += " [RT]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotalNormalized)
                    name += " [RT Norm]";
            return name;
        }
    }
}