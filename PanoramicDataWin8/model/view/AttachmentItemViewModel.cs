using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttachmentItemViewModel : ExtendedBindableBase
    {
        public AttachmentItemViewModel()
        {
            Random r = new Random();
            _dampingFactor = r.NextDouble() * 3.0 + 3;
        }

        private AttachmentHeaderViewModel _attachmentHeaderViewModel = null;
        public AttachmentHeaderViewModel AttachmentHeaderViewModel
        {
            get
            {
                return _attachmentHeaderViewModel;
            }
            set
            {
                this.SetProperty(ref _attachmentHeaderViewModel, value);
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
                this.SetProperty(ref _inputOperationModel, value);
            }
        }

        private double _dampingFactor = 0.0;
        public double DampingFactor
        {
            get
            {
                return _dampingFactor;
            }
            set
            {
                this.SetProperty(ref _dampingFactor, value);
            }
        }

        private bool _isDropTarget = false;
        public bool IsDropTarget
        {
            get
            {
                return _isDropTarget;
            }
            set
            {
                this.SetProperty(ref _isDropTarget, value);
            }
        }

        private Vec _preferedSize = new Vec(50, 50);
        public Vec PreferedSize
        {
            get
            {
                return _preferedSize;
            }
            set
            {
                this.SetProperty(ref _preferedSize, value);
            }
        }

        private Vec _targetSize = new Vec(50, 50);
        public Vec TargetSize
        {
            get
            {
                return _targetSize;
            }
            set
            {
                this.SetProperty(ref _targetSize, value);
            }
        }

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
        public Rct Bounds
        {
            get
            {
                return new Rct(Position, Size);
            }
        }

        private string _mainLabel = "";
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

        private string _subLabel = "";
        public string SubLabel
        {
            get
            {
                return _subLabel;
            }
            set
            {
                this.SetProperty(ref _subLabel, value);
            }
        }
    }
}
