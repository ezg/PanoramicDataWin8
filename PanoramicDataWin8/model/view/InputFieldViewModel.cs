using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputFieldViewModel : BindableBase
    {
        public static event EventHandler<InputFieldViewModelEventArgs> InputFieldViewModelMoved;
        public static event EventHandler<InputFieldViewModelEventArgs> InputFieldViewModelDropped;

        public InputFieldViewModel() { }

        public InputFieldViewModel(VisualizationViewModel visualizationViewModel, InputOperationModel inputOperationModel)
        {
            VisualizationViewModel = visualizationViewModel;
            InputOperationModel = inputOperationModel;
        }

        public void FireMoved(Rct bounds, InputOperationModel inputOperationModel, InputFieldViewModelEventArgType type)
        {
            if (InputFieldViewModelMoved != null)
            {
                InputFieldViewModelMoved(this, new InputFieldViewModelEventArgs(inputOperationModel, bounds, type));
            }
        }

        public void FireDropped(Rct bounds, InputFieldViewModelEventArgType type, InputOperationModel inputOperationModel)
        {
            if (InputFieldViewModelDropped != null)
            {
                InputFieldViewModelDropped(this, new InputFieldViewModelEventArgs(inputOperationModel, bounds, type));
            }
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

        private VisualizationViewModel _visualizationViewModel = null;
        public VisualizationViewModel VisualizationViewModel
        {
            get
            {
                return _visualizationViewModel;
            }
            set
            {
                this.SetProperty(ref _visualizationViewModel, value);
            }
        }

        private InputOperationModel _inputOperationModel = null;
        public InputOperationModel InputOperationModel
        {
            get
            {
                return _inputOperationModel;
            }
            set
            {
                if (_inputOperationModel != null)
                {
                    _inputOperationModel.PropertyChanged -= InputOperationModelPropertyChanged;
                }
                this.SetProperty(ref _inputOperationModel, value);
                if (_inputOperationModel != null)
                {
                    _inputOperationModel.PropertyChanged += InputOperationModelPropertyChanged;
                    updateLabels();
                }
            }
        }

        public MenuViewModel CreateMenuViewModel(Rct bounds)
        {
            MenuViewModel menuViewModel = new MenuViewModel()
            {
                InputFieldViewModel = this,
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
                IsChecked = _inputOperationModel.SortMode == SortMode.None
            };
            menuItem.MenuItemComponentViewModel = toggle1;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _inputOperationModel.SortMode = SortMode.None;
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
                IsChecked = _inputOperationModel.SortMode == SortMode.Asc
            };
            menuItem.MenuItemComponentViewModel = toggle2;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _inputOperationModel.SortMode = SortMode.Asc;
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
                IsChecked = _inputOperationModel.SortMode == SortMode.Desc
            };
            menuItem.MenuItemComponentViewModel = toggle3;
            menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
            {
                var model = (sender as ToggleMenuItemComponentViewModel);
                if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                {
                    if (model.IsChecked)
                    {
                        _inputOperationModel.SortMode = SortMode.Desc;
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

            if (_inputOperationModel.InputModel.InputDataType == InputDataTypeConstants.INT ||
                _inputOperationModel.InputModel.InputDataType == InputDataTypeConstants.FLOAT)
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
                        IsChecked = _inputOperationModel.AggregateFunction == aggregationFunction
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
                                _inputOperationModel.AggregateFunction = aggregationFunction;
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

        void InputOperationModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateLabels();
        }

        private void updateLabels()
        {
            MainLabel = _inputOperationModel.InputModel.Name; //columnDescriptor.GetLabels(out mainLabel, out subLabel);

            string mainLabel = _inputOperationModel.InputModel.Name;
            string subLabel = "";

            mainLabel = addDetailToLabel(mainLabel);
            MainLabel = mainLabel.Replace("_", "");
            SubLabel = subLabel;
        }

        private string addDetailToLabel(string name)
        {
            if (InputOperationModel.AggregateFunction == AggregateFunction.Avg)
            {
                name = "Avg(" + name + ")";
            }
            else if (InputOperationModel.AggregateFunction == AggregateFunction.Count)
            {
                name = "Count(" + name + ")";
            }
            else if (InputOperationModel.AggregateFunction == AggregateFunction.Max)
            {
                name = "Max(" + name + ")";
            }
            else if (InputOperationModel.AggregateFunction == AggregateFunction.Min)
            {
                name = "Min(" + name + ")";
            }
            else if (InputOperationModel.AggregateFunction == AggregateFunction.Sum)
            {
                name = "Sum(" + name + ")";
            }
            /*else if (InputFieldViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (InputOperationModel.ScaleFunction != ScaleFunction.None)
            {
                if (InputOperationModel.ScaleFunction == ScaleFunction.Log)
                {
                    name += " [Log]";
                }
                else if (InputOperationModel.ScaleFunction == ScaleFunction.Normalize)
                {
                    name += " [Normalize]";
                }
                else if (InputOperationModel.ScaleFunction == ScaleFunction.RunningTotal)
                {
                    name += " [RT]";
                }
                else if (InputOperationModel.ScaleFunction == ScaleFunction.RunningTotalNormalized)
                {
                    name += " [RT Norm]";
                }
            }
            return name;
        }
    }



    public interface InputFieldViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement);
        void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement);
    }


    public class InputFieldViewModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public InputOperationModel InputOperationModel { get; set; }
        public InputFieldViewModelEventArgType Type { get; set; }
        public bool UseDefaultSize { get; set; }
        public VisualizationViewModel CreateLinkFrom { get; set; }

        public  InputFieldViewModelEventArgs(InputOperationModel inputOperationModel, Rct bounds, InputFieldViewModelEventArgType type)
        {
            InputOperationModel = inputOperationModel;
            Bounds = bounds;
            Type = type;
            UseDefaultSize = true;
        }
    }

    public enum InputFieldViewModelEventArgType
    {
        Default,
        Copy,
        Snapshot
    }
}
