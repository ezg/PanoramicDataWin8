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

        private double _startWealth = -1;
        public double StartWealth
        {
            get { return _startWealth; }
            set { SetProperty(ref _startWealth, value); }
        }
        
        private double _wealth = -1;
        public double Wealth
        {
            get { return _wealth; }
            set { SetProperty(ref _wealth, value); }
        }
    }
}
