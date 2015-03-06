﻿using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class MenuViewModel : ExtendedBindableBase
    {
        public MenuViewModel()
        {
        }

        private ObservableCollection<MenuItemViewModel> _menuItemViewModels = new ObservableCollection<MenuItemViewModel>();
        public ObservableCollection<MenuItemViewModel> MenuItemViewModels
        {
            get
            {
                return _menuItemViewModels;
            }
            set
            {
                this.SetProperty(ref _menuItemViewModels, value);
            }
        }

        private Pt _ankerPosition = new Pt();
        public Pt AnkerPosition
        {
            get
            {
                return _ankerPosition;
            }
            set
            {
                this.SetProperty(ref _ankerPosition, value);
            }
        }

        private bool _isDisplayed = false;
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

        private int _nrRows = 0;
        public int NrRows
        {
            get
            {
                return _nrRows;
            }
            set
            {
                this.SetProperty(ref _nrRows, value);
            }
        }

        private bool _isToBeRemoved = false;
        public bool IsToBeRemoved
        {
            get
            {
                return _isToBeRemoved;
            }
            set
            {
                this.SetProperty(ref _isToBeRemoved, value);
            }
        }

        private AttributeViewModel _attributeViewModel = null;
        public AttributeViewModel AttributeViewModel
        {
            get
            {
                return _attributeViewModel;
            }
            set
            {
                this.SetProperty(ref _attributeViewModel, value);
            }
        }

        private AttachmentItemViewModel _attachmentItemViewModel = null;
        public AttachmentItemViewModel AttachmentItemViewModel
        {
            get
            {
                return _attachmentItemViewModel;
            }
            set
            {
                this.SetProperty(ref _attachmentItemViewModel, value);
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
    }
}
