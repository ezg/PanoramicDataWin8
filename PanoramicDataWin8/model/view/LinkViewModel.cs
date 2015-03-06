using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.model.data;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.view
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
