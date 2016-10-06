using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

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

        private int _nrColumns = 0;
        public int NrColumns
        {
            get
            {
                return _nrColumns;
            }
            set
            {
                this.SetProperty(ref _nrColumns, value);
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

        private AttributeTransformationViewModel _attributeTransformationViewModel = null;
        public AttributeTransformationViewModel AttributeTransformationViewModel
        {
            get
            {
                return _attributeTransformationViewModel;
            }
            set
            {
                this.SetProperty(ref _attributeTransformationViewModel, value);
            }
        }

        private AttachmentViewModel _attachmentViewModel = null;
        public AttachmentViewModel AttachmentViewModel
        {
            get
            {
                return _attachmentViewModel;
            }
            set
            {
                this.SetProperty(ref _attachmentViewModel, value);
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

    public class AttachedTo :ExtendedBindableBase
    {

        private Pt _targetPosition = new Pt(0, 0);
        public Pt TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                this.SetProperty(ref _targetPosition, value);
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

        private Pt _position = new Pt(0, 0);
        public Pt Position
        {
            get
            {
                return _position;
            }
            set
            {
                this.SetProperty(ref _position, value);
            }
        }
    }
}
