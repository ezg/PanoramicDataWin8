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
            if (_visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attachmentItemViewModel.AttributeOperationModel))
            {
                var aom = attachmentItemViewModel.AttributeOperationModel;
                if (aom.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT ||
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
                        IsChecked = aom.IsGrouped
                    };
                    menuItem.MenuItemComponentViewModel = toggle1;
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                aom.IsGrouped = true;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                            else
                            {
                                aom.IsGrouped = false;
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
                        IsChecked = aom.IsBinned
                    };
                    menuItem.MenuItemComponentViewModel = toggle2; 
                    menuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                    {
                        var model = (sender as ToggleMenuItemComponentViewModel);
                        if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                aom.IsBinned = true;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                            else
                            {
                                aom.IsBinned = false;
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
            }
            

            return menuViewModel;
        }

        void _visualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void initialize()
        {
            AttachmentHeaderViewModels.Clear();
            if (_attachmentOrientation == AttachmentOrientation.Bottom)
            {
                AttachmentHeaderViewModel header = new AttachmentHeaderViewModel()
                {
                    AttributeFunction = AttributeFunction.Group
                };
                // initialize items
                foreach (var item in _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group))
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
                    attributeOperationModel.IsGrouped = true;
                    QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                    if (!queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attributeOperationModel))
                    {
                        queryModel.AddFunctionAttributeOperationModel(AttributeFunction.Group, attributeOperationModel);
                    }
                };
                // handle removed
                header.RemovedTriggered = (attachmentItemViewModel) =>
                {
                    QueryModel queryModel = this.VisualizationViewModel.QueryModel;
                    if (queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).Contains(attachmentItemViewModel.AttributeOperationModel))
                    {
                        queryModel.RemoveFunctionAttributeOperationModel(AttributeFunction.Group, attachmentItemViewModel.AttributeOperationModel);
                    }
                };

                header.AddAttachmentItemViewModel = new AddAttachmentItemViewModel()
                {
                    AttachmentHeaderViewModel = header,
                    //Size = new Vec(25,25),
                    //TargetSize = new Vec(25, 25),
                    Label = "group"
                };

                // handle updates
                _visualizationViewModel.QueryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group).CollectionChanged += (sender, args) =>
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
                                MainLabel = "group",
                                AttachmentHeaderViewModel = header
                            });   
                        }
                    }
                };
                AttachmentHeaderViewModels.Add(header);
            }
        }
    }

    public enum AttachmentOrientation { Left, Top, Bottom, Right }
}
