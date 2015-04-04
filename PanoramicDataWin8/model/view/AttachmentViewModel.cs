using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class AttachmentViewModel : ExtendedBindableBase
    {
        private VisualizationViewModel _visualizationViewModel;
        public VisualizationViewModel VisualizationViewModel
        {
            get
            {
                return _visualizationViewModel;
            }
            set
            {
                if (_visualizationViewModel != null)
                {
                    _visualizationViewModel.PropertyChanged -= _visualizationViewModel_PropertyChanged;
                }
                this.SetProperty(ref _visualizationViewModel, value);
                if (_visualizationViewModel != null)
                {
                    _visualizationViewModel.PropertyChanged += _visualizationViewModel_PropertyChanged;
                    _visualizationViewModel.QueryModel.PropertyChanged += QueryModel_PropertyChanged;    
                    initialize();
                }
            }
        }


        private ObservableCollection<AttachmentHeaderViewModel> _attachmentHeaderViewModels = new ObservableCollection<AttachmentHeaderViewModel>();
        public ObservableCollection<AttachmentHeaderViewModel> AttachmentHeaderViewModels
        {
            get
            {
                return _attachmentHeaderViewModels;
            }
            set
            {
                this.SetProperty(ref _attachmentHeaderViewModels, value);
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

        private bool _isDisplayed;
        public bool IsDisplayed
        {
            get
            {
                return _isDisplayed;
            }
            set
            {
                this.SetProperty(ref _isDisplayed, value);
            }
        }

        private Stopwatch _activeStopwatch = new Stopwatch();
        public Stopwatch ActiveStopwatch
        {
            get
            {
                return _activeStopwatch;
            }
            set
            {
                this.SetProperty(ref _activeStopwatch, value);
            }
        }

        public MenuViewModel CreateMenuViewModel(AttachmentItemViewModel attachmentItemViewModel)
        {
            MenuViewModel menuViewModel = new MenuViewModel()
            {
                AttachmentViewModel = this,
                AttachmentItemViewModel = attachmentItemViewModel,
                AttachmentOrientation = this.AttachmentOrientation
            };

            // is grouping attributeOperationModel
            /*if (_visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attachmentItemViewModel.AttributeOperationModel))
            {
                var aom = attachmentItemViewModel.AttributeOperationModel;
                if (aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.DATE)
                {
                    menuViewModel.NrRows = 1;
                    // Year
                    MenuItemViewModel menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle1 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "year",
                        IsChecked = aom.GroupMode == GroupMode.Year
                    };
                    menuItem.MenuItemComponentViewModel = toggle1;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                                aom.GroupMode = GroupMode.Year;
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // month
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle2 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "month",
                        IsChecked = aom.GroupMode == GroupMode.MonthOfTheYear
                    };
                    menuItem.MenuItemComponentViewModel = toggle2;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                                aom.GroupMode = GroupMode.MonthOfTheYear;
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // day
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle3 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "day",
                        IsChecked = aom.GroupMode == GroupMode.DayOfTheMonth
                    };
                    menuItem.MenuItemComponentViewModel = toggle3;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                                aom.GroupMode = GroupMode.DayOfTheMonth;
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // week day
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle4 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "week day",
                        IsChecked = aom.GroupMode == GroupMode.DayOfTheWeek
                    };
                    menuItem.MenuItemComponentViewModel = toggle4;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                                aom.GroupMode = GroupMode.DayOfTheWeek;
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // set toogle groups
                    toggle1.OtherToggles.Add(toggle2);
                    toggle1.OtherToggles.Add(toggle3);
                    toggle1.OtherToggles.Add(toggle4);
                    
                    toggle2.OtherToggles.Add(toggle1);
                    toggle2.OtherToggles.Add(toggle3);
                    toggle2.OtherToggles.Add(toggle4);
                    
                    toggle3.OtherToggles.Add(toggle2);
                    toggle3.OtherToggles.Add(toggle1);
                    toggle3.OtherToggles.Add(toggle4);

                    toggle4.OtherToggles.Add(toggle2);
                    toggle4.OtherToggles.Add(toggle1);
                    toggle4.OtherToggles.Add(toggle3);

                }
                else if (aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT ||
                         aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                {
                    menuViewModel.NrRows = 2;

                    // Distinct
                    MenuItemViewModel menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle1 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "distinct",
                        IsChecked = aom.GroupMode == GroupMode.Distinct
                    };
                    menuItem.MenuItemComponentViewModel = toggle1;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                aom.GroupMode = GroupMode.Distinct;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // Binned
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    ToggleMenuItemComponentViewModel toggle2 = new ToggleMenuItemComponentViewModel()
                    {
                        Label = "binned",
                        IsChecked = aom.GroupMode == GroupMode.Binned
                    };
                    menuItem.MenuItemComponentViewModel = toggle2;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                aom.GroupMode = GroupMode.Binned;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                            else
                            {
                                aom.GroupMode = GroupMode.None;
                            }
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);

                    // set toogle groups
                    toggle1.OtherToggles.Add(toggle2);
                    toggle2.OtherToggles.Add(toggle1);

                    // Bin size slider
                    menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 1,
                        Size = new Vec(104, 50),
                        TargetSize = new Vec(104, 50)
                    };
                    menuItem.Position = attachmentItemViewModel.Position;
                    menuItem.MenuItemComponentViewModel = new SliderMenuItemComponentViewModel()
                    {
                        Label = "bin size",
                        Value = aom.BinSize,
                        MaxValue = aom.MaxBinSize,
                        MinValue = aom.MinBinSize
                    };
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as SliderMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.Value))
                        {
                            aom.BinSize = model.Value;
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);
                }
            }*/
            

            return menuViewModel;
        }

        void _visualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }


        void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _visualizationViewModel.QueryModel.GetPropertyName(() => _visualizationViewModel.QueryModel.JobType) |
                e.PropertyName == _visualizationViewModel.QueryModel.GetPropertyName(() => _visualizationViewModel.QueryModel.VisualizationType))
            {
                initialize();
            }
        }

        void initialize()
        {
            AttachmentHeaderViewModels.Clear();
            if (_visualizationViewModel.QueryModel.JobType == JobType.DB)
            {
                if (_attachmentOrientation == AttachmentOrientation.Bottom)
                {
                    createDbBottom();
                }
            }
            else if (_visualizationViewModel.QueryModel.JobType == JobType.Kmeans)
            {
                if (_attachmentOrientation == AttachmentOrientation.Bottom)
                {
                    createKMeansBottom();
                }
            }
        }

        void createKMeansBottom()
        {
            AttachmentHeaderViewModel header = new AttachmentHeaderViewModel()
            {
                AttributeFunction = AttributeFunction.JobInput
            };
            // initialize items
            foreach (var item in _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput))
            {
                header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                {
                    AttributeOperationModel = item,
                    AttachmentHeaderViewModel = header
                });
            }

            // handle added
            header.AddedTriggered = (attributeOperationModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (!queryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).Contains(attributeOperationModel))
                {
                    queryModel.AddFunctionAttributeOperationModel(AttributeFunction.JobInput, attributeOperationModel);
                }
            };
            // handle removed
            header.RemovedTriggered = (attachmentItemViewModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).Contains(attachmentItemViewModel.AttributeOperationModel))
                {
                    queryModel.RemoveFunctionAttributeOperationModel(AttributeFunction.JobInput, attachmentItemViewModel.AttributeOperationModel);
                }
            };

            header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel()
            {
                AttachmentHeaderViewModel = header,
                //Size = new Vec(25,25),
                //TargetSize = new Vec(25, 25),
                Label = "input"
            };

            // handle updates
            _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.JobInput).CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (var item in args.OldItems)
                    {
                        if (header.AttachmentItemViewModels.Any(aiv => aiv.AttributeOperationModel == item))
                        {
                            header.AttachmentItemViewModels.Remove(header.AttachmentItemViewModels.First(aiv => aiv.AttributeOperationModel == item));
                        }
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems)
                    {
                        header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                        {
                            AttributeOperationModel = item as AttributeOperationModel,
                            SubLabel = (item as AttributeOperationModel).AttributeModel.Name,
                            MainLabel = "input",
                            AttachmentHeaderViewModel = header
                        });
                    }
                }
            };
            AttachmentHeaderViewModels.Add(header);
        }

        void createDbBottom()
        {
            // intensity
            var intensityHeader = createIntensityAttachmentHeader();
            AttachmentHeaderViewModels.Add(intensityHeader);

            // grouping
            var groupHeader = createGroupingAttachmentHeader();
            AttachmentHeaderViewModels.Add(groupHeader);
        }

        AttachmentHeaderViewModel createIntensityAttachmentHeader()
        {
            var groupHeader = createAttributeFunctionAttachmentHeader(AttributeFunction.Intensity);

            // handle added
            groupHeader.AddedTriggered = (attributeOperationModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (!queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Intensity).Contains(attributeOperationModel))
                {
                    queryModel.AddFunctionAttributeOperationModel(AttributeFunction.Intensity, attributeOperationModel);
                }
            };
            // handle removed
            groupHeader.RemovedTriggered = (attachmentItemViewModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Intensity).Contains(attachmentItemViewModel.AttributeOperationModel))
                {
                    queryModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Intensity, attachmentItemViewModel.AttributeOperationModel);
                }
            };
            return groupHeader;
        }

        AttachmentHeaderViewModel createGroupingAttachmentHeader()
        {
             var groupHeader = createAttributeFunctionAttachmentHeader(AttributeFunction.Group);

            // handle added
            groupHeader.AddedTriggered = (attributeOperationModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (!queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attributeOperationModel))
                {
                    queryModel.AddFunctionAttributeOperationModel(AttributeFunction.Group, attributeOperationModel);
                }
            };
            // handle removed
            groupHeader.RemovedTriggered = (attachmentItemViewModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attachmentItemViewModel.AttributeOperationModel))
                {
                    queryModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Group, attachmentItemViewModel.AttributeOperationModel);
                }
            };
            return groupHeader;
        }

        AttachmentHeaderViewModel createAttributeFunctionAttachmentHeader(AttributeFunction attributeFunction)
        {
            AttachmentHeaderViewModel header = new AttachmentHeaderViewModel()
            {
                AttributeFunction = attributeFunction
            };
            // initialize items
            foreach (var item in _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(attributeFunction))
            {
                header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                {
                    AttributeOperationModel = item,
                    AttachmentHeaderViewModel = header
                });
            }

            header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel()
            {
                AttachmentHeaderViewModel = header,
                Label = attributeFunction.ToString().ToLower()
            };

            // handle updates
            _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(attributeFunction).CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (var item in args.OldItems)
                    {
                        if (header.AttachmentItemViewModels.Any(aiv => aiv.AttributeOperationModel == item))
                        {
                            header.AttachmentItemViewModels.Remove(header.AttachmentItemViewModels.First(aiv => aiv.AttributeOperationModel == item));
                        }
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems)
                    {
                        header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                        {
                            AttributeOperationModel = item as AttributeOperationModel,
                            SubLabel = (item as AttributeOperationModel).AttributeModel.Name,
                            MainLabel = attributeFunction.ToString().ToLower(),
                            AttachmentHeaderViewModel = header
                        });
                    }
                }
            };
            return header;
        }
    }

    public enum AttachmentOrientation { Left, Top, Bottom, Right }
}
