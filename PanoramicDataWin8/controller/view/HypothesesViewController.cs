using System.Collections.Generic;
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
        private readonly Dictionary<ComparisonId, HypothesisViewModel> _comparisonIdToHypothesisViewModels = new Dictionary<ComparisonId, HypothesisViewModel>();
        private MainModel _mainModel = null;
        private static int _nextComparisonOrder = 0;


        private HypothesesViewController(MainModel mainModel)
        {
            _riskOperationModel = new RiskOperationModel(null);
            _riskOperationModel.PropertyChanged += _riskOperationModel_PropertyChanged;
            _mainModel = mainModel;
            _mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _mainModel.GetPropertyName(() => _mainModel.QueryExecuter))
            {
                _mainModel.QueryExecuter.ExecuteOperationModel(_riskOperationModel);
            }
        }

        public static HypothesesViewController Instance { get; private set; }

        public RiskOperationModel RiskOperationModel
        {
            get { return _riskOperationModel; }
        }

        public HypothesesViewModel HypothesesViewModel { get; } = new HypothesesViewModel();

        public static void CreateInstance(MainModel mainModel)
        {
            Instance = new HypothesesViewController(mainModel);
        }
        

        private void StatisticalComparisonOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var statOpModel = (StatisticalComparisonOperationModel) sender;
            if (e.PropertyName == statOpModel.GetPropertyName(() => statOpModel.Result))
            {
                var res = (AddComparisonResult) statOpModel.Result;
                if (res != null)
                {
                    if (statOpModel.StatisticalComparisonDecisionOperationModel == null)
                    {
                        statOpModel.StatisticalComparisonDecisionOperationModel = new StatisticalComparisonDecisionOperationModel(statOpModel.SchemaModel);
                        statOpModel.StatisticalComparisonDecisionOperationModel.PropertyChanged += StatisticalComparisonDecisionOperationModel_PropertyChanged;
                        statOpModel.StatisticalComparisonDecisionOperationModel.Parent = statOpModel;
                    }
                    statOpModel.StatisticalComparisonDecisionOperationModel.ModelId = statOpModel.ModelId;
                    statOpModel.StatisticalComparisonDecisionOperationModel.ComparisonId = res.ComparisonId;
                    statOpModel.StatisticalComparisonDecisionOperationModel.RiskControlType = _riskOperationModel.RiskControlType;
                    _mainModel.QueryExecuter.ExecuteOperationModel(statOpModel.StatisticalComparisonDecisionOperationModel);

                    if (!_comparisonIdToHypothesisViewModels.ContainsKey(res.ComparisonId))
                    {
                        var vm = new HypothesisViewModel();
                        HypothesesViewModel.HypothesisViewModels.Add(vm);
                        _comparisonIdToHypothesisViewModels.Add(res.ComparisonId, vm);
                    }
                }
            }
        } 

        private void StatisticalComparisonDecisionOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var statDesOpModel = (StatisticalComparisonDecisionOperationModel)sender;
            if (e.PropertyName == statDesOpModel.GetPropertyName(() => statDesOpModel.Result) && statDesOpModel.Result != null)
            {
                var des = ((GetDecisionsResult) statDesOpModel.Result).Decisions.FirstOrDefault(d => d.ComparisonId != null && d.ComparisonId.Equals(statDesOpModel.ComparisonId));
                _comparisonIdToHypothesisViewModels[statDesOpModel.ComparisonId].Decision = des;
            }
        }

        private void StatisticalComparisonOperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            var model = (StatisticalComparisonOperationModel) sender;
            if (model.StatisticallyComparableOperationModels.Count == 2 && !(e is BrushOperationModelUpdatedEventArgs))
            {
                model.ComparisonOrder = _nextComparisonOrder++;
                MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model);
            }
        }

        public void AddStatisticalComparisonOperationModel(StatisticalComparisonOperationModel model)
        {
            model.ModelId = _riskOperationModel.ModelId;

            model.OperationModelUpdated += StatisticalComparisonOperationModel_OperationModelUpdated;
            model.PropertyChanged += StatisticalComparisonOperationModel_PropertyChanged;
            model.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }

        public void RemoveStatisticalComparisonOperationModel(StatisticalComparisonOperationModel model)
        {
            model.OperationModelUpdated -= StatisticalComparisonOperationModel_OperationModelUpdated;
            model.PropertyChanged -= StatisticalComparisonOperationModel_PropertyChanged;
        }

        private void _riskOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _riskOperationModel.GetPropertyName(() => _riskOperationModel.Result) && _riskOperationModel.Result != null)
            {
                _riskOperationModel.ModelId = ((NewModelOperationResult) _riskOperationModel.Result).ModelId;
            }
        }
    }
}