using System;
using System.Collections.ObjectModel;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class MenuViewModel : ExtendedBindableBase
    {
        private Pt _ankerPosition;

        private AttachmentOrientation _attachmentOrientation;

        private AttachmentViewModel _attachmentViewModel;

        private AttributeTransformationViewModel _attributeTransformationViewModel;

        private bool _isDisplayed;

        private bool _isToBeRemoved;

        private ObservableCollection<MenuItemViewModel> _menuItemViewModels = new ObservableCollection<MenuItemViewModel>();

        private int _nrColumns;

        private int _nrRows;

        public ObservableCollection<MenuItemViewModel> MenuItemViewModels
        {
            get { return _menuItemViewModels; }
            set { SetProperty(ref _menuItemViewModels, value); }
        }

        public Pt AnkerPosition
        {
            get { return _ankerPosition; }
            set { SetProperty(ref _ankerPosition, value); }
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }

        public int NrColumns
        {
            get { return _nrColumns; }
            set { SetProperty(ref _nrColumns, value); }
        }

        public int NrRows
        {
            get { return _nrRows; }
            set { SetProperty(ref _nrRows, value); }
        }

        public bool IsToBeRemoved
        {
            get { return _isToBeRemoved; }
            set { SetProperty(ref _isToBeRemoved, value); }
        }

        public AttachmentOrientation AttachmentOrientation
        {
            get { return _attachmentOrientation; }
            set { SetProperty(ref _attachmentOrientation, value); }
        }

        public event EventHandler Updated;

        public void FireUpdate()
        {
            Updated?.Invoke(this, new EventArgs());
        }
    }
}