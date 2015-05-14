using System.Collections.ObjectModel;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class LinkViewModel : ExtendedBindableBase
    {
        private ObservableCollection<LinkModel> _linkModels = new ObservableCollection<LinkModel>();
        public ObservableCollection<LinkModel> LinkModels
        {
            get
            {
                return _linkModels;
            }
            set
            {
                this.SetProperty(ref _linkModels, value);
            }
        }

        private ObservableCollection<VisualizationViewModel> _fromVisualizationViewModels = new ObservableCollection<VisualizationViewModel>();
        public ObservableCollection<VisualizationViewModel> FromVisualizationViewModels
        {
            get
            {
                return _fromVisualizationViewModels;
            }
            set
            {
                this.SetProperty(ref _fromVisualizationViewModels, value);
            }
        }

        private VisualizationViewModel _toVisualizationViewModel = null;
        public VisualizationViewModel ToVisualizationViewModel
        {
            get
            {
                return _toVisualizationViewModel;
            }
            set
            {
                this.SetProperty(ref _toVisualizationViewModel, value);
            }
        }
    }
}
