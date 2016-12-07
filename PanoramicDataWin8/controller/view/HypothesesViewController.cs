using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class HypothesesViewController
    {
        private static HypothesesViewController _instance;
        private RiskOperationModel _riskOperationModel = null;
        private ObservableCollection<StatisticalComparisonOperationModel> _statisticalComparisonOperationModels = new ObservableCollection<StatisticalComparisonOperationModel>();

        private HypothesesViewController()
        {
            _riskOperationModel = new RiskOperationModel(null);
            _riskOperationModel.PropertyChanged += _riskOperationModel_PropertyChanged;
            //MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(_riskOperationModel);

            _statisticalComparisonOperationModels.CollectionChanged += _statisticalComparisonOperationModels_CollectionChanged;
        }

        private void _statisticalComparisonOperationModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = (StatisticalComparisonOperationModel) item;
                    current.OperationModelUpdated -= OperationModel_OperationModelUpdated;
                    foreach (var m in current.StatisticallyComparableOperationModels.ToArray())
                    {
                        current.RemoveStatisticallyComparableOperationModel(m);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = (StatisticalComparisonOperationModel)item;
                    current.OperationModelUpdated += OperationModel_OperationModelUpdated;
                    current.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
            }
        }
        private void OperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            var model = (OperationModel)sender;
            MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model);
        }

        public void AddStatisticalComparisonOperationModel(StatisticalComparisonOperationModel model)
        {
            _statisticalComparisonOperationModels.Add(model);
        }

        public void RemoveStatisticalComparisonOperationModel(StatisticalComparisonOperationModel model)
        {
            _statisticalComparisonOperationModels.Remove(model);
        }
        
        private void _riskOperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _riskOperationModel.GetPropertyName(() => _riskOperationModel.Result))
            {
               
            }
        }

        public static HypothesesViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HypothesesViewController();
                return _instance;
            }
        }

        private HypothesesViewModel _hypothesesViewModel = new HypothesesViewModel();
        public HypothesesViewModel HypothesesViewModel
        {
            get
            {
                return _hypothesesViewModel;
            }
        }
    }
}
