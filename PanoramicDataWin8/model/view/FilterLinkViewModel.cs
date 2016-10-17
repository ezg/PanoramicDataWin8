using System.Collections.ObjectModel;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class FilterLinkViewModel : ExtendedBindableBase
    {
        private ObservableCollection<FilterLinkModel> _filterLinkModels = new ObservableCollection<FilterLinkModel>();

        private ObservableCollection<OperationViewModel> _fromOperationViewModels = new ObservableCollection<OperationViewModel>();

        private OperationViewModel _toOperationViewModel;

        public ObservableCollection<FilterLinkModel> FilterLinkModels
        {
            get { return _filterLinkModels; }
            set { SetProperty(ref _filterLinkModels, value); }
        }

        public ObservableCollection<OperationViewModel> FromOperationViewModels
        {
            get { return _fromOperationViewModels; }
            set { SetProperty(ref _fromOperationViewModels, value); }
        }

        public OperationViewModel ToOperationViewModel
        {
            get { return _toOperationViewModel; }
            set { SetProperty(ref _toOperationViewModel, value); }
        }
    }
}