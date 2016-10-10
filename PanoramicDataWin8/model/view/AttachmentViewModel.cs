using System.ComponentModel;
using System.Diagnostics;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttachmentViewModel : ExtendedBindableBase
    {
        private Stopwatch _activeStopwatch = new Stopwatch();

        private AttachmentOrientation _attachmentOrientation;

        private MenuViewModel _menuViewModel;

        private OperationViewModel _operationViewModel;

        public AttachmentViewModel()
        {
            ActiveStopwatch.Start();
        }

        public OperationViewModel OperationViewModel
        {
            get { return _operationViewModel; }
            set
            {
                if (_operationViewModel != null)
                    _operationViewModel.PropertyChanged -= OperationViewModelPropertyChanged;
                SetProperty(ref _operationViewModel, value);
                if (_operationViewModel != null)
                {
                    _operationViewModel.PropertyChanged += OperationViewModelPropertyChanged;
                    _operationViewModel.OperationModel.PropertyChanged += QueryModel_PropertyChanged;
                }
            }
        }

        public AttachmentOrientation AttachmentOrientation
        {
            get { return _attachmentOrientation; }
            set { SetProperty(ref _attachmentOrientation, value); }
        }

        public Stopwatch ActiveStopwatch
        {
            get { return _activeStopwatch; }
            set { SetProperty(ref _activeStopwatch, value); }
        }

        public MenuViewModel MenuViewModel
        {
            get { return _menuViewModel; }
            set { SetProperty(ref _menuViewModel, value); }
        }

        public MenuViewModel CreateMenuViewModel(AttachedTo attachedTo)
        {
            var menuViewModel = new MenuViewModel
            {
                AttachmentViewModel = this,
                AttachmentOrientation = AttachmentOrientation
            };

            /*// is value AttributeOperationModel
            if (attachedTo is AttachmentItemViewModel)
            {
                var attachmentItemViewModel = attachedTo as AttachmentItemViewModel;
                if (_operationViewModel.OperationModel.GetUsageInputOperationModel(AttributeUsage.Value).Contains(attachmentItemViewModel.AttributeTransformationModel))
                {
                    var aom = attachmentItemViewModel.AttributeTransformationModel;
                    if (((AttributeFieldModel) aom.AttributeModel).InputDataType == InputDataTypeConstants.INT ||
                        ((AttributeFieldModel) aom.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT)
                    {
                        menuViewModel.NrRows = 3;
                        menuViewModel.NrColumns = 4;

                        List<ToggleMenuItemComponentViewModel> toggles = new List<ToggleMenuItemComponentViewModel>();
                        List<MenuItemViewModel> items = new List<MenuItemViewModel>();

                        int count = 0;
                        foreach (var aggregationFunction in Enum.GetValues(typeof (AggregateFunction)).Cast<AggregateFunction>().Where(af => af != AggregateFunction.None))
                        {
                            var menuItem = new MenuItemViewModel()
                            {
                                MenuViewModel = menuViewModel,
                                Row = count <= 2 ? 0 : 1,
                                RowSpan = count <= 2 ? 1 : 2,
                                Column = count%3 + 1,
                                Size = new Vec(32, 50),
                                TargetSize = new Vec(32, 50)
                            };
                            menuItem.Position = attachmentItemViewModel.Position;
                            ToggleMenuItemComponentViewModel toggle = new ToggleMenuItemComponentViewModel()
                            {
                                Label = aggregationFunction.ToString(),
                                IsChecked = attachmentItemViewModel.AttributeTransformationModel.AggregateFunction == aggregationFunction
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
                                        attachmentItemViewModel.AttributeTransformationModel.AggregateFunction = aggregationFunction;
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
            }
            else if (attachedTo is AddAttachmentItemViewModel)
            {
                var addAttachmentItemViewModel = attachedTo as AddAttachmentItemViewModel;
                if (addAttachmentItemViewModel.Label == "minimum support")
                {
                    menuViewModel.NrRows = 1;
                    menuViewModel.NrColumns = 1;

                    var menuItem = new MenuItemViewModel()
                    {
                        MenuViewModel = menuViewModel,
                        Row = 0,
                        RowSpan = 1,
                        Column = 0,
                        Size = new Vec(100, 50),
                        TargetSize = new Vec(100, 50)
                    };
                    menuItem.Position = attachedTo.Position;
                    SliderMenuItemComponentViewModel slider = new SliderMenuItemComponentViewModel()
                    {
                        Label = "percentage",
                        Value = _operationViewModel.HistogramOperationModel.MinimumSupport * 100.0,
                        MaxValue = 100,
                        MinValue = 0
                    };
                    menuItem.MenuItemComponentViewModel = slider;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as SliderMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                        {
                            _operationViewModel.HistogramOperationModel.MinimumSupport = model.FinalValue / 100.0;
                        }
                    };
                    menuViewModel.MenuItemViewModels.Add(menuItem);
                }
            }
            */
            return menuViewModel;
        }

        private void OperationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }


        private void QueryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*if (e.PropertyName == _operationViewModel.HistogramOperationModel.GetPropertyName(() => _operationViewModel.HistogramOperationModel.OperationTypeModel) |
                e.PropertyName == _operationViewModel.HistogramOperationModel.GetPropertyName(() => _operationViewModel.HistogramOperationModel.VisualizationType))
            {
                initialize();
            }*/
        }

        private void initialize()
        {
            /*if (AttachmentHeaderViewModels.Count == 0)
            {
                AttachmentHeaderViewModels.Clear();
                if (_operationViewModel.HistogramOperationModel.OperationTypeModel == null && _operationViewModel.HistogramOperationModel.VisualizationType != VisualizationType.table)
                {
                    if (_attachmentOrientation == AttachmentOrientation.Bottom)
                    {
                        createDbBottom();
                    }
                }
                else if (_operationViewModel.HistogramOperationModel.OperationTypeModel != null)
                {
                    if (_operationViewModel.HistogramOperationModel.OperationTypeModel.Name != "frequent_itemsets")
                    {
                        if (_attachmentOrientation == AttachmentOrientation.Bottom)
                        {
                            createLogregBottom();
                        }
                    }
                    else
                    {
                        if (_attachmentOrientation == AttachmentOrientation.Bottom)
                        {
                            createFrequentItemsetBottom();
                        }
                    }
                    if (_attachmentOrientation == AttachmentOrientation.Left)
                    {
                        createLogregLeft();
                    }
                }

                if (_attachmentOrientation == AttachmentOrientation.Right)
                {
                    createLogregTop();
                }
            }*/
        }

        private void createFrequentItemsetBottom()
        {
            /*var header = new AttachmentHeaderViewModel
            {
                AttributeUsage = AttributeUsage.Label,
                AcceptsInputGroupModels = false,
                AcceptsInputModels = false
            };
            header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel
            {
                AttachmentHeaderViewModel = header,
                //Size = new Vec(25,25),
                //TargetSize = new Vec(25, 25),
                Label = "minimum support"
            };*/
            //AttachmentHeaderViewModels.Add(header);
        }

        private void createLogregTop()
        {
            if (MainViewController.Instance.MainModel.ShowCodeGen)
            {
                /*var header = new AttachmentHeaderViewModel
                {
                    AttributeUsage = AttributeUsage.Label,
                    AcceptsInputGroupModels = false,
                    AcceptsInputModels = false
                };
                header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel
                {
                    AttachmentHeaderViewModel = header,
                    //Size = new Vec(25,25),
                    //TargetSize = new Vec(25, 25),
                    Label = "codegen"
                };*/
                //AttachmentHeaderViewModels.Add(header);
            }
        }


        private void createLogregBottom()
        {
            /*AttachmentHeaderViewModel header = new AttachmentHeaderViewModel()
            {
                AttributeUsage = AttributeUsage.Feature,
                AcceptsInputGroupModels = true
            };
            // initialize items
            foreach (var item in _operationViewModel.HistogramOperationModel.GetUsageInputOperationModel(AttributeUsage.Feature))
            {
                header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                {
                    AttributeTransformationModel = item,
                    AttachmentHeaderViewModel = header
                });
            }

            // handle added
            header.AddedTriggered = (inputOperationModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (!histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Feature).Contains(inputOperationModel))
                {
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Feature, inputOperationModel);
                }
            };
            // handle removed
            header.RemovedTriggered = (attachmentItemViewModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Feature).Contains(attachmentItemViewModel.AttributeTransformationModel))
                {
                    histogramOperationModel.RemoveAttributeUsageTransformationModel(AttributeUsage.Feature, attachmentItemViewModel.AttributeTransformationModel);
                }
            };

            header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel()
            {
                AttachmentHeaderViewModel = header,
                //Size = new Vec(25,25),
                //TargetSize = new Vec(25, 25),
                Label = "feature"
            };

            // handle updates
            _operationViewModel.HistogramOperationModel.GetUsageInputOperationModel(AttributeUsage.Feature).CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (var item in args.OldItems)
                    {
                        if (header.AttachmentItemViewModels.Any(aiv => aiv.AttributeTransformationModel == item))
                        {
                            header.AttachmentItemViewModels.Remove(header.AttachmentItemViewModels.First(aiv => aiv.AttributeTransformationModel == item));
                        }
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems)
                    {
                        var iom = (item as AttributeTransformationModel);
                        header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                        {
                            AttributeTransformationModel = item as AttributeTransformationModel,
                            SubLabel = iom != null ? iom.AttributeModel.RawName.Replace("_", "") : " ",
                            MainLabel = "feature",
                            AttachmentHeaderViewModel = header
                        });
                    }
                }
            };
            AttachmentHeaderViewModels.Add(header);*/
        }

        private void createDbBottom()
        {
            // value
            var intensityHeader = createValueAttachmentHeader();
            //AttachmentHeaderViewModels.Add(intensityHeader);

            // grouping
            var groupHeader = createGroupingAttachmentHeader();
            //AttachmentHeaderViewModels.Add(groupHeader);
        }

        private AttachmentHeaderViewModel createValueAttachmentHeader()
        {
            var groupHeader = createInputFieldUsageAttachmentHeader(AttributeUsage.Value);
            /*
            // handle added
            groupHeader.AddedTriggered = (inputOperationModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (((AttributeFieldModel)inputOperationModel.AttributeModel).InputDataType == InputDataTypeConstants.INT ||
                    ((AttributeFieldModel)inputOperationModel.AttributeModel).InputDataType == InputDataTypeConstants.FLOAT)
                {
                    inputOperationModel.AggregateFunction = AggregateFunction.Avg;
                }
                else
                {
                    inputOperationModel.AggregateFunction = AggregateFunction.Count;
                }
                if (!histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Value).Contains(inputOperationModel))
                {
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Value, inputOperationModel);
                }
            };
            // handle removed
            groupHeader.RemovedTriggered = (attachmentItemViewModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Value).Contains(attachmentItemViewModel.AttributeTransformationModel))
                {
                    histogramOperationModel.RemoveAttributeUsageTransformationModel(AttributeUsage.Value, attachmentItemViewModel.AttributeTransformationModel);
                }
            };*/
            return groupHeader;
        }

        private AttachmentHeaderViewModel createGroupingAttachmentHeader()
        {
            var groupHeader = createInputFieldUsageAttachmentHeader(AttributeUsage.Group);

            /*// handle added
            groupHeader.AddedTriggered = (inputOperationModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (!histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Group).Contains(inputOperationModel))
                {
                    histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Group, inputOperationModel);
                }
            };
            // handle removed
            groupHeader.RemovedTriggered = (attachmentItemViewModel) =>
            {
                HistogramOperationModel histogramOperationModel = this.OperationViewModel.HistogramOperationModel;
                if (histogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.Group).Contains(attachmentItemViewModel.AttributeTransformationModel))
                {
                    histogramOperationModel.RemoveAttributeUsageTransformationModel(AttributeUsage.Group, attachmentItemViewModel.AttributeTransformationModel);
                }
            };*/
            return groupHeader;
        }

        private AttachmentHeaderViewModel createInputFieldUsageAttachmentHeader(AttributeUsage attributeUsage)
        {
            var header = new AttachmentHeaderViewModel
            {
                AttributeUsage = attributeUsage
            };
            /*// initialize items
            foreach (var item in _operationViewModel.HistogramOperationModel.GetUsageInputOperationModel(attributeUsage))
            {
                header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                {
                    AttributeTransformationModel = item,
                    AttachmentHeaderViewModel = header
                });
            }

            header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel()
            {
                AttachmentHeaderViewModel = header,
                Label = attributeUsage.ToString().ToLower()
            };

            // handle updates
            _operationViewModel.HistogramOperationModel.GetUsageInputOperationModel(attributeUsage).CollectionChanged += (sender, args) =>
            {
                if (args.OldItems != null)
                {
                    foreach (var item in args.OldItems)
                    {
                        if (header.AttachmentItemViewModels.Any(aiv => aiv.AttributeTransformationModel == item))
                        {
                            header.AttachmentItemViewModels.Remove(header.AttachmentItemViewModels.First(aiv => aiv.AttributeTransformationModel == item));
                        }
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems)
                    {
                        header.AttachmentItemViewModels.Add(new AttachmentItemViewModel()
                        {
                            AttributeTransformationModel = item as AttributeTransformationModel,
                            SubLabel = (item as AttributeTransformationModel).AttributeModel.RawName.Replace("_", " "),
                            MainLabel = attributeUsage.ToString().ToLower(),
                            AttachmentHeaderViewModel = header
                        });
                    }
                }
            };*/
            return header;
        }
    }

    public enum AttachmentOrientation
    {
        Left,
        Top,
        Bottom,
        Right
    }
}