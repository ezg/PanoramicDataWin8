using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class HypothesesViewModel : ExtendedBindableBase
    {
        private ObservableCollection<HypothesisViewModel> _hypothesisViewModels = new ObservableCollection<HypothesisViewModel>();

        public ObservableCollection<HypothesisViewModel> HypothesisViewModels
        {
            get { return _hypothesisViewModels; }
            set { SetProperty(ref _hypothesisViewModels, value); }
        }
    }
}
