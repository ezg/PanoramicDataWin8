﻿using System.ComponentModel;
using System.Diagnostics;
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
        private bool _showOnAttributeMove;

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

        public bool ShowOnAttributeMove
        {
            get { return _showOnAttributeMove; }
            set { SetProperty(ref _showOnAttributeMove, value); }
        }

        private void OperationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void QueryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}