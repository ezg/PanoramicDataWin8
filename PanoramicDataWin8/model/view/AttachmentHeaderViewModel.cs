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
        public Action<InputOperationModel> AddedTriggered { get; set; }
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

        private InputUsage? _inputUsage = null;
        public InputUsage? InputUsage
        {
            get
            {
                return _inputUsage;
            }
            set
            {
                this.SetProperty(ref _inputUsage, value);
            }
        }
    }
}
