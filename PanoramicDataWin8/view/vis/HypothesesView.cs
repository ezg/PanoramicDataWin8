using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis
{
    public class HypothesesView : UserControl
    {
        private HypothesesViewModel _hypothesesViewModel = null;
        private Canvas _contentCanvas = null;

        private Dictionary<HypothesisViewModel, HypothesisView> _views = new Dictionary<HypothesisViewModel, HypothesisView>();

        public HypothesesView()
        {
            DataContextChanged += HypothesesView_DataContextChanged;
            _contentCanvas = new Canvas();
            this.Content = _contentCanvas;
        }

        private void HypothesesView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_hypothesesViewModel != null)
            {
                _hypothesesViewModel.HypothesisViewModels.CollectionChanged -= HypothesisViewModels_CollectionChanged;
            }
            if (args.NewValue != null && args.NewValue is HypothesesViewModel)
            {
                _hypothesesViewModel = (HypothesesViewModel) args.NewValue;
                _hypothesesViewModel.HypothesisViewModels.CollectionChanged += HypothesisViewModels_CollectionChanged;
            }
        }

        private void HypothesisViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    var hypo = (HypothesisViewModel) item;
                    if (_views.ContainsKey(hypo))
                    {
                        _contentCanvas.Children.Remove(_views[hypo]);
                        _views.Remove(hypo);
                    }
                }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    var hypo = (HypothesisViewModel)item;
                    var v = new HypothesisView();
                    v.DataContext = hypo;
                    _contentCanvas.Children.Add(v);
                    _views.Add(hypo, v);
                }
        }
    }
}
