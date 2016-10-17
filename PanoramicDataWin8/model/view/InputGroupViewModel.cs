using System;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputGroupViewModel : BindableBase
    {
        private HistogramOperationViewModel _histogramOperationViewModel;

        private AttributeGroupModel _inpuGroupModel;

        private bool _isShadow;

        private string _mainLabel;

        private Vec _size = new Vec(50, 50);

        public InputGroupViewModel()
        {
        }

        public InputGroupViewModel(HistogramOperationViewModel histogramOperationViewModel, AttributeGroupModel attributeGroupModel)
        {
            HistogramOperationViewModel = histogramOperationViewModel;
            AttributeOperationModel = attributeGroupModel;
        }

        public string MainLabel
        {
            get { return _mainLabel; }
            set { SetProperty(ref _mainLabel, value); }
        }

        public HistogramOperationViewModel HistogramOperationViewModel
        {
            get { return _histogramOperationViewModel; }
            set { SetProperty(ref _histogramOperationViewModel, value); }
        }

        public AttributeGroupModel AttributeOperationModel
        {
            get { return _inpuGroupModel; }
            set
            {
                SetProperty(ref _inpuGroupModel, value);
                if (_inpuGroupModel != null)
                    updateLabels();
            }
        }

        public bool IsShadow
        {
            get { return _isShadow; }
            set { SetProperty(ref _isShadow, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelMoved;
        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelDropped;


        public void FireMoved(Rct bounds, AttributeGroupModel attributeGroupModel)
        {
            if (InputGroupViewModelMoved != null)
                InputGroupViewModelMoved(this, new InputGroupViewModelEventArgs(bounds, attributeGroupModel));
        }

        public void FireDropped(Rct bounds, AttributeGroupModel attributeGroupModel)
        {
            if (InputGroupViewModelDropped != null)
                InputGroupViewModelDropped(this, new InputGroupViewModelEventArgs(bounds, attributeGroupModel));
        }

        private void updateLabels()
        {
            MainLabel = _inpuGroupModel.RawName.Replace("_", "");
        }
    }
}