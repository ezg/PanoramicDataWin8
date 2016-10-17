using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttributeTransformationViewModel : BindableBase
    {
        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelMoved;
        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelDropped;

        public AttributeTransformationViewModel() { }

        public AttributeTransformationViewModel(OperationViewModel operationViewModel, AttributeTransformationModel attributeTransformationModel)
        {
            OperationViewModel = operationViewModel;
            AttributeTransformationModel = attributeTransformationModel;
        }

        public void FireMoved(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelMoved?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        public void FireDropped(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelDropped?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        private AttachmentOrientation _attachmentOrientation;
        public AttachmentOrientation AttachmentOrientation
        {
            get
            {
                return _attachmentOrientation;
            }
            set
            {
                this.SetProperty(ref _attachmentOrientation, value);
            }
        }


        private double _textAngle = 0;
        public double TextAngle
        {
            get
            {
                return _textAngle;
            }
            set
            {
                this.SetProperty(ref _textAngle, value);
            }
        }

        private bool _hideAggregationFunction = false;
        public bool HideAggregationFunction
        {
            get
            {
                return _hideAggregationFunction;
            }
            set
            {
                this.SetProperty(ref _hideAggregationFunction, value);
                updateLabels();
            }
        }


        private bool _isShadow = false;
        public bool IsShadow
        {
            get
            {
                return _isShadow;
            }
            set
            {
                this.SetProperty(ref _isShadow, value);
            }
        }

        private Thickness _borderThicknes = new Thickness();
        public Thickness BorderThicknes
        {
            get
            {
                return _borderThicknes;
            }
            set
            {
                this.SetProperty(ref _borderThicknes, value);
            }
        }

        private bool _isDraggable = true;
        public bool IsDraggable
        {
            get
            {
                return _isDraggable;
            }
            set
            {
                this.SetProperty(ref _isDraggable, value);
            }
        }
        
        private bool _isDraggableByPen = false;
        public bool IsDraggableByPen
        {
            get
            {
                return _isDraggableByPen;
            }
            set
            {
                this.SetProperty(ref _isDraggableByPen, value);
            }
        }

        private bool _isNoChrome = false;
        public bool IsNoChrome
        {
            get
            {
                return _isNoChrome;
            }
            set
            {
                this.SetProperty(ref _isNoChrome, value);
            }
        }

        private bool _isFiltered = false;
        public bool IsFiltered
        {
            get
            {
                return _isFiltered;
            }
            set
            {
                this.SetProperty(ref _isFiltered, value);
            }
        }

        private bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                this.SetProperty(ref _isHighlighted, value);
            }
        }

        private bool _isMenuEnabled = true;
        public bool IsMenuEnabled
        {
            get
            {
                return _isMenuEnabled;
            }
            set
            {
                this.SetProperty(ref _isMenuEnabled, value);
            }
        }

        private bool _isScaleFunctionEnabled = true;
        public bool IsScaleFunctionEnabled
        {
            get
            {
                return _isScaleFunctionEnabled;
            }
            set
            {
                this.SetProperty(ref _isScaleFunctionEnabled, value);
            }
        }

        private bool _isRemoveEnabled = false;
        public bool IsRemoveEnabled
        {
            get
            {
                return _isRemoveEnabled;
            }
            set
            {
                this.SetProperty(ref _isRemoveEnabled, value);
            }
        }

        private Vec _size = new Vec(50, 50);
        public Vec Size
        {
            get
            {
                return _size;
            }
            set
            {
                this.SetProperty(ref _size, value);
            }
        }

        private bool _isGestureEnabled = false;
        public bool IsGestureEnabled
        {
            get
            {
                return _isGestureEnabled;
            }
            set
            {
                this.SetProperty(ref _isGestureEnabled, value);
            }
        }

        private string _mainLabel = null;
        public string MainLabel
        {
            get
            {
                return _mainLabel;
            }
            set
            {
                this.SetProperty(ref _mainLabel, value);
            }
        }

        private string _sublabel = null;
        public string SubLabel
        {
            get
            {
                return _sublabel;
            }
            set
            {
                this.SetProperty(ref _sublabel, value);
            }
        }

        private OperationViewModel _operationViewModel = null;
        public OperationViewModel OperationViewModel
        {
            get
            {
                return _operationViewModel;
            }
            set
            {
                this.SetProperty(ref _operationViewModel, value);
            }
        }

        private AttributeTransformationModel _attributeTransformationModel = null;
        public AttributeTransformationModel AttributeTransformationModel
        {
            get
            {
                return _attributeTransformationModel;
            }
            set
            {
                if (_attributeTransformationModel != null)
                {
                    _attributeTransformationModel.PropertyChanged -= AttributeTransformationModelPropertyChanged;
                }
                this.SetProperty(ref _attributeTransformationModel, value);
                if (_attributeTransformationModel != null)
                {
                    _attributeTransformationModel.PropertyChanged += AttributeTransformationModelPropertyChanged;
                    updateLabels();
                }
            }
        }

        public MenuViewModel CreateMenuViewModel(Rct bounds)
        {
            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttributeTransformationViewModel = this,
                AttachmentOrientation = this.AttachmentOrientation
            };
            MenuItemViewModel menuItem = null;
            menuViewModel.NrRows = 3;
            menuViewModel.NrColumns = 4;

            // Sort None
            menuItem = new MenuItemViewModel()
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            ToggleMenuItemComponentViewModel toggle1 = new ToggleMenuItemComponentViewModel()
            {
                Label = "None",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.None
            };
            menuItem.MenuItemComponentViewModel = toggle1;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.None;
                        foreach (var tg in model.OtherToggles)
                        {
                            tg.IsChecked = false;
                        }
                    }
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // Sort Asc
            menuItem = new MenuItemViewModel()
            {
                MenuViewModel = menuViewModel,
                Row = 1,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            ToggleMenuItemComponentViewModel toggle2 = new ToggleMenuItemComponentViewModel()
            {
                Label = "asc",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.Asc
            };
            menuItem.MenuItemComponentViewModel = toggle2;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.Asc;
                        foreach (var tg in model.OtherToggles)
                        {
                            tg.IsChecked = false;
                        }
                    }
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // Sort Desc
            menuItem = new MenuItemViewModel()
            {
                MenuViewModel = menuViewModel,
                Row = 2,
                Column = 0,
                Size = new Vec(50, 32),
                TargetSize = new Vec(50, 32)
            };
            menuItem.Position = bounds.TopLeft;
            ToggleMenuItemComponentViewModel toggle3 = new ToggleMenuItemComponentViewModel()
            {
                Label = "desc",
                IsChecked = _attributeTransformationModel.SortMode == SortMode.Desc
            };
            menuItem.MenuItemComponentViewModel = toggle3;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _attributeTransformationModel.SortMode = SortMode.Desc;
                        foreach (var tg in model.OtherToggles)
                        {
                            tg.IsChecked = false;
                        }
                    }
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);

            // sort toogle groups
            toggle1.OtherToggles.AddRange(new ToggleMenuItemComponentViewModel[] { toggle2, toggle3 });
            toggle2.OtherToggles.AddRange(new ToggleMenuItemComponentViewModel[] { toggle1, toggle3 });
            toggle3.OtherToggles.AddRange(new ToggleMenuItemComponentViewModel[] { toggle1, toggle2 });

            if (((AttributeFieldModel)_attributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.INT ||
                ((AttributeFieldModel)_attributeTransformationModel.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT)
            {
                List<ToggleMenuItemComponentViewModel> toggles = new List<ToggleMenuItemComponentViewModel>();
                List<MenuItemViewModel> items = new List<MenuItemViewModel>();

                int count = 0;
                foreach (var aggregationFunction in Enum.GetValues(typeof(AggregateFunction)).Cast<AggregateFunction>())
                {
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = count <= 2 ? 0 : 1,
                        RowSpan = count <= 2 ? 1 : 2,
                        Column = count % 3 + 1,
                        Size = new Vec(32, 50),
                        TargetSize = new Vec(32, 50)
                    };
                    menuItem.Position = bounds.TopLeft;
                    ToggleMenuItemComponentViewModel toggle = new ToggleMenuItemComponentViewModel()
                    {
                        Label = aggregationFunction.ToString(),
                        IsChecked = _attributeTransformationModel.AggregateFunction == aggregationFunction
                    };
                    toggles.Add(toggle);
                    menuItem.MenuItemComponentViewModel = toggle;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                _attributeTransformationModel.AggregateFunction = aggregationFunction;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);
                    items.Add(menuItem);
                    count++;
                }

                foreach (var mi in items)
                {
                    (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                }
            }


            return menuViewModel;
        }

        void AttributeTransformationModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateLabels();
        }

        private void updateLabels()
        {
            if (_attributeTransformationModel != null)
            {
                MainLabel = _attributeTransformationModel.AttributeModel.RawName; //columnDescriptor.GetLabels(out mainLabel, out subLabel);

                string mainLabel = _attributeTransformationModel.AttributeModel.RawName;
                string subLabel = "";

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
            {
                name = "avg(" + name + ")";
            }
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Count)
            {
                name = "count";
            }
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Max)
            {
                name = "max(" + name + ")";
            }
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Min)
            {
                name = "min(" + name + ")";
            }
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Sum)
            {
                name = "sum(" + name + ")";
            }
            /*else if (AttributeTransformationViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (AttributeTransformationModel.ScaleFunction != ScaleFunction.None)
            {
                if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Log)
                {
                    name += " [Log]";
                }
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Normalize)
                {
                    name += " [Normalize]";
                }
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotal)
                {
                    name += " [RT]";
                }
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotalNormalized)
                {
                    name += " [RT Norm]";
                }
            }
            return name;
        }
    }



    public interface AttributeTransformationViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
        void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement);
    }


    public class AttributeTransformationViewModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public AttributeTransformationModel AttributeTransformationModel { get; set; }

        public  AttributeTransformationViewModelEventArgs(AttributeTransformationModel attributeTransformationModel, Rct bounds)
        {
            AttributeTransformationModel = attributeTransformationModel;
            Bounds = bounds;
        }
    }
}
