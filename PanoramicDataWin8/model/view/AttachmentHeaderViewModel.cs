using PanoramicData.model.data;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class AttachmentHeaderViewModel : ExtendedBindableBase
    {
        public Action<AttributeOperationModel> AddedTriggered { get; set; }
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

        private AttributeFunction? _attributeFunction = null;
        public AttributeFunction? AttributeFunction
        {
            get
            {
                return _attributeFunction;
            }
            set
            {
                this.SetProperty(ref _attributeFunction, value);
            }
        }
    }
}
