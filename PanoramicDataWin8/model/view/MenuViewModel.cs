using System;
using System.Collections.ObjectModel;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class MenuViewModel : ExtendedBindableBase
    {
        private Pt _ankerPosition;

        private Pt _hidePosition;

        private AttachmentOrientation _attachmentOrientation;

        private bool _isDisplayed;

        private bool _moveOnHide = false;

        private bool _isRigid = false;

        private double _rigidSize = 54;

        private ObservableCollection<MenuItemViewModel> _menuItemViewModels = new ObservableCollection<MenuItemViewModel>();

        private int _nrColumns;

        private int _nrRows;

        private Vec _parentSize;

        public bool SynchronizeItemsDisplay { get; set; } = true;

        public ObservableCollection<MenuItemViewModel> MenuItemViewModels
        {
            get { return _menuItemViewModels; }
            set {
                foreach (var v in value)
                    v.MenuViewModel = this;
                SetProperty(ref _menuItemViewModels, value);
            }
        }
        

        public Pt HidePosition
        {
            get { return _hidePosition; }
            set { SetProperty(ref _hidePosition, value); }
        }

        public Vec ParentSize
        {
            get { return _parentSize; }
            set { SetProperty(ref _parentSize, value); }
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

        public bool MoveOnHide
        {
            get { return _moveOnHide; }
            set { SetProperty(ref _moveOnHide, value); }
        }


        public double RigidSize
        {
            get { return _rigidSize; }
            set { SetProperty(ref _rigidSize, value); }
        }

        public bool IsRigid
        {
            get { return _isRigid; }
            set { SetProperty(ref _isRigid, value); }
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