using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class AttachmentHeaderViewModel : ExtendedBindableBase
    {
        public Action<AttributeTransformationModel> AddedTriggered { get; set; }
        public Action<AttachmentItemViewModel> RemovedTriggered { get; set; }

        private Vec _preferedItemSize = new Vec(50, 50);
        public Vec PreferedItemSize
        {
            get
            {
                return _preferedItemSize;
            }
            set
            {
                this.SetProperty(ref _preferedItemSize, value);
            }
        }

        private ObservableCollection<AttachmentItemViewModel> _attachmentItemViewModels = new ObservableCollection<AttachmentItemViewModel>();
        public ObservableCollection<AttachmentItemViewModel> AttachmentItemViewModels
        {
            get
            {
                return _attachmentItemViewModels;
            }
            set
            {
                this.SetProperty(ref _attachmentItemViewModels, value);
            }
        }

        private AddAttachmentItemViewModel _addAttachmentItemViewModel = null;
        public AddAttachmentItemViewModel AddAttachmentItemViewModel
        {
            get
            {
                return _addAttachmentItemViewModel;
            }
            set
            {
                this.SetProperty(ref _addAttachmentItemViewModel, value);
            }
        }

        private bool _acceptsInputModels = true;
        public bool AcceptsInputModels
        {
            get
            {
                return _acceptsInputModels;
            }
            set
            {
                this.SetProperty(ref _acceptsInputModels, value);
            }
        }

        private bool _acceptsInputGroupModels = false;
        public bool AcceptsInputGroupModels
        {
            get
            {
                return _acceptsInputGroupModels;
            }
            set
            {
                this.SetProperty(ref _acceptsInputGroupModels, value);
            }
        }

        private AttributeUsage? _attributeUsage = null;
        public AttributeUsage? AttributeUsage
        {
            get
            {
                return _attributeUsage;
            }
            set
            {
                this.SetProperty(ref _attributeUsage, value);
            }
        }
    }
}
