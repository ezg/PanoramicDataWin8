using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputGroupViewModel : BindableBase
    {
        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelMoved;
        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelDropped;

        public InputGroupViewModel()
        {
        }

        public InputGroupViewModel(HistogramOperationViewModel histogramOperationViewModel, AttributeGroupModel attributeGroupModel)
        {
            HistogramOperationViewModel = histogramOperationViewModel;
            AttributeOperationModel = attributeGroupModel;
        }

        private string _mainLabel = null;
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

        private HistogramOperationViewModel _histogramOperationViewModel = null;
        public HistogramOperationViewModel HistogramOperationViewModel
        {
            get
            {
                return _histogramOperationViewModel;
            }
            set
            {
                this.SetProperty(ref _histogramOperationViewModel, value);
            }
        }

        private AttributeGroupModel _inpuGroupModel = null;
        public AttributeGroupModel AttributeOperationModel
        {
            get
            {
                return _inpuGroupModel;
            }
            set
            {
                this.SetProperty(ref _inpuGroupModel, value);
                if (_inpuGroupModel != null)
                {
                    updateLabels();
                }
            }
        }

        private bool _isShadow = false;
        public bool IsShadow
        {
            get
            {
                return _isShadow;
            }
            set
            {
                this.SetProperty(ref _isShadow, value);
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


        public void FireMoved(Rct bounds, AttributeGroupModel attributeGroupModel)
        {
            if (InputGroupViewModelMoved != null)
            {
                InputGroupViewModelMoved(this, new InputGroupViewModelEventArgs(bounds, attributeGroupModel));
            }
        }

        public void FireDropped(Rct bounds, AttributeGroupModel attributeGroupModel)
        {
            if (InputGroupViewModelDropped != null)
            {
                InputGroupViewModelDropped(this, new InputGroupViewModelEventArgs(bounds, attributeGroupModel));
            }
        }

        private void updateLabels()
        {
            MainLabel = _inpuGroupModel.RawName.Replace("_", "");
        }
    }



    public interface InputGroupViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void InputGroupViewModelMoved(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
        void InputGroupViewModelDropped(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
    }


    public class InputGroupViewModelEventArgs : EventArgs
    {
        public AttributeGroupModel AttributeGroupModel { get; set; }
        public Rct Bounds { get; set; }
        public InputGroupViewModelEventArgs(Rct bounds, AttributeGroupModel attributeGroupModel)
        {
            Bounds = bounds;
            AttributeGroupModel = attributeGroupModel;
        }
    }
}
