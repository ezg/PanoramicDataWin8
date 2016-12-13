using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using IDEA_common.operations.risk;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.controller.view
{
    public class HypothesesViewController
    {
        private readonly RiskOperationModel _riskOperationModel;
        private readonly ObservableCollection<StatisticalComparisonOperationModel> _statisticalComparisonOperationModels = new ObservableCollection<StatisticalComparisonOperationModel>();
        private MainModel _mainModel = null;


        private HypothesesViewController(MainModel mainModel)
        {
            _riskOperationModel = new RiskOperationModel(null);
            _riskOperationModel.PropertyChanged += _riskOperationModel_PropertyChanged;
            _mainModel = mainModel;
            _mainModel.PropertyChanged += MainModel_PropertyChanged;

            _statisticalComparisonOperationModels.CollectionChanged += _statisticalComparisonOperationModels_CollectionChanged;
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _mainModel.GetPropertyName(() => _mainModel.QueryExecuter))
            {
                _mainModel.QueryExecuter.ExecuteOperationModel(_riskOperationModel);
            }
        }

        public static HypothesesViewController Instance { get; private set; }

        public HypothesesViewModel HypothesesViewModel { get; } = new HypothesesViewModel();

        public static void CreateInstance(MainModel mainModel)
        {
            Instance = new HypothesesViewController(mainModel);
        }

        private void _statisticalComparisonOperationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                    var current = (StatisticalComparisonOperationModel) item;
                    current.OperationModelUpdated += OperationModel_OperationModelUpdated;
                    current.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
            }
        }

        private void OperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            var model = (OperationModel) sender;
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

        private void _riskOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _riskOperationModel.GetPropertyName(() => _riskOperationModel.Result))
            {
                _riskOperationModel.ModelId = ((NewModelOperationResult) _riskOperationModel.Result).ModelId;
            }
        }
    }
}