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

            // is value attributeOperationModel
            if (_visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).Contains(attachmentItemViewModel.AttributeOperationModel))
            {
                var aom = attachmentItemViewModel.AttributeOperationModel;
                if (aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT ||
                    aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                {
                    menuViewModel.NrRows = 3;
                    menuViewModel.NrColumns = 4;

                    List<ToggleMenuItemComponentViewModel> toggles = new List<ToggleMenuItemComponentViewModel>();
                    List<MenuItemViewModel> items = new List<MenuItemViewModel>();

                    int count = 0;
                    foreach (var aggregationFunction in Enum.GetValues(typeof(AggregateFunction)).Cast<AggregateFunction>().Where(af => af != AggregateFunction.None))
                    {
                        var menuItem = new MenuItemViewModel()
                        {
                            MenuViewModel = menuViewModel,
                            Row = count <= 2 ? 0 : 1,
                            RowSpan = count <= 2 ? 1 : 2,
                            Column = count % 3 + 1,
                            Size = new Vec(32, 50),
                            TargetSize = new Vec(32, 50)
                        };
                        menuItem.Position = attachmentItemViewModel.Position;
                        ToggleMenuItemComponentViewModel toggle = new ToggleMenuItemComponentViewModel()
                        {
                            Label = aggregationFunction.ToString(),
                            IsChecked = attachmentItemViewModel.AttributeOperationModel.AggregateFunction == aggregationFunction
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
                                    attachmentItemViewModel.AttributeOperationModel.AggregateFunction = aggregationFunction;
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
            }

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
            // value
            var intensityHeader = createValueAttachmentHeader();
            AttachmentHeaderViewModels.Add(intensityHeader);

            // grouping
            var groupHeader = createGroupingAttachmentHeader();
            AttachmentHeaderViewModels.Add(groupHeader);
        }

        AttachmentHeaderViewModel createValueAttachmentHeader()
        {
            var groupHeader = createAttributeFunctionAttachmentHeader(AttributeFunction.Value);

            // handle added
            groupHeader.AddedTriggered = (attributeOperationModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT ||
                    attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                {
                    attributeOperationModel.AggregateFunction = AggregateFunction.Avg;
                }
                else
                {
                    attributeOperationModel.AggregateFunction = AggregateFunction.Count;
                }
                if (!queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).Contains(attributeOperationModel))
                {
                    queryModel.AddFunctionAttributeOperationModel(AttributeFunction.Value, attributeOperationModel);
                }
            };
            // handle removed
            groupHeader.RemovedTriggered = (attachmentItemViewModel) =>
            {
                QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).Contains(attachmentItemViewModel.AttributeOperationModel))
                {
                    queryModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Value, attachmentItemViewModel.AttributeOperationModel);
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
