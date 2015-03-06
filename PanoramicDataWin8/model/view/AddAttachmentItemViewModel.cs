using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class AddAttachmentItemViewModel : ExtendedBindableBase
    {
        public AddAttachmentItemViewModel()
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

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                this.SetProperty(ref _isActive, value);
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

        private string _label = "";
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                this.SetProperty(ref _label, value);
            }
        }
    }
}
