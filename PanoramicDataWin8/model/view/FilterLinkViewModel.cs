using System.Collections.ObjectModel;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class FilterLinkViewModel : ExtendedBindableBase
    {
        private ObservableCollection<FilterLinkModel> _filterLinkModels = new ObservableCollection<FilterLinkModel>();
        public ObservableCollection<FilterLinkModel> FilterLinkModels
        {
            get
            {
                return _filterLinkModels;
            }
            set
            {
                this.SetProperty(ref _filterLinkModels, value);
            }
        }

        private ObservableCollection<OperationViewModel> _fromOperationViewModels = new ObservableCollection<OperationViewModel>();
        public ObservableCollection<OperationViewModel> FromOperationViewModels
        {
            get
            {
                return _fromOperationViewModels;
            }
            set
            {
                this.SetProperty(ref _fromOperationViewModels, value);
            }
        }

        private OperationViewModel _toOperationViewModel = null;
        public OperationViewModel ToOperationViewModel
        {
            get
            {
                return _toOperationViewModel;
            }
            set
            {
                this.SetProperty(ref _toOperationViewModel, value);
            }
        }
    }
}
