using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using IDEA_common.operations.risk;
using IDEA_common.util;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.controller.view
{
    public class HypothesesViewController
    {
        private readonly DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();
        private readonly RiskOperationModel _riskOperationModel;
        private readonly Dictionary<ComparisonId, HypothesisViewModel> _comparisonIdToHypothesisViewModels = new Dictionary<ComparisonId, HypothesisViewModel>();
        private readonly Dictionary<StatisticalComparisonOperationModel, StatisticalComparisonSaveViewModel> _modelToSaveViewModel = new Dictionary<StatisticalComparisonOperationModel, StatisticalComparisonSaveViewModel>();
        private MainModel _mainModel = null;
        private static int _nextComparisonOrder = 0;


        private HypothesesViewController(MainModel mainModel)
        {
            _riskOperationModel = new RiskOperationModel(null);
            _riskOperationModel.PropertyChanged += _riskOperationModel_PropertyChanged;
            _mainModel = mainModel;
            _mainModel.PropertyChanged += MainModel_PropertyChanged;

            _operationViewMovingTimer.Interval = TimeSpan.FromSeconds(2);
            _operationViewMovingTimer.Tick += operationViewMovingTimer_Tick;
            //_operationViewMovingTimer.Start();
        }

        // periodically update all decisions
        private void operationViewMovingTimer_Tick(object sender, object e)
        {
            getAllDecisions();
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _mainModel.GetPropertyName(() => _mainModel.QueryExecuter))
            {
                _mainModel.QueryExecuter.ExecuteOperationModel(_riskOperationModel, true);
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
                    if (!_comparisonIdToHypothesisViewModels.ContainsKey(res.ComparisonId))
                    {
                        var vm = new HypothesisViewModel();
                        vm.StatisticalComparisonSaveViewModel = _modelToSaveViewModel[statOpModel];
                        HypothesesViewModel.HypothesisViewModels.Add(vm);
                        _comparisonIdToHypothesisViewModels.Add(res.ComparisonId, vm);
                    }
                    
                    _comparisonIdToHypothesisViewModels[res.ComparisonId].Decision = res.Decision[_riskOperationModel.RiskControlType];
                    Debug.WriteLine(statOpModel.ExecutionId + ", " + statOpModel.ResultExecutionId);
                    if (statOpModel.ExecutionId == statOpModel.ResultExecutionId)
                    {
                        statOpModel.Decision = res.Decision[_riskOperationModel.RiskControlType];
                        _comparisonIdToHypothesisViewModels[res.ComparisonId].ViewOrdering = HypothesesViewModel.HypothesisViewModels.Count == 0
                            ? 0
                            : HypothesesViewModel.HypothesisViewModels.Max(h => h.ViewOrdering) + 1;
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        private void StatisticalComparisonDecisionOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var statDesOpModel = (StatisticalComparisonDecisionOperationModel) sender;
            if (e.PropertyName == statDesOpModel.GetPropertyName(() => statDesOpModel.Result) && statDesOpModel.Result != null)
            {
                foreach (var decision in ((GetDecisionsResult) statDesOpModel.Result).Decisions)
                {
                    _comparisonIdToHypothesisViewModels[decision.ComparisonId].Decision = decision;
                }
            }
        }

        private void StatisticalComparisonOperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            var model = (StatisticalComparisonOperationModel) sender;
            if (model.StatisticallyComparableOperationModels.Count == 2 && !(e is BrushOperationModelUpdatedEventArgs))
            {
                model.ComparisonOrder = _nextComparisonOrder++;
                _modelToSaveViewModel[model] = new StatisticalComparisonSaveViewModel();

                var filter = "";
                var filterModels = new List<FilterModel>();
                filter = FilterModel.GetFilterModelsRecursive(model.StatisticallyComparableOperationModels[0], new List<IFilterProviderOperationModel>(), filterModels, true);
                _modelToSaveViewModel[model].FilterDist0 = filter;

                filter = "";
                filterModels = new List<FilterModel>();
                filter = FilterModel.GetFilterModelsRecursive(model.StatisticallyComparableOperationModels[1], new List<IFilterProviderOperationModel>(), filterModels, true);
                _modelToSaveViewModel[model].FilterDist1 = filter;

                model.ExecutionId += 1;
                MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model, false);
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

        public void ClearAllStatisticalComparison()
        {
            foreach (var ci in _comparisonIdToHypothesisViewModels.Keys.ToArray())
            {
                HypothesesViewModel.HypothesisViewModels.Remove(_comparisonIdToHypothesisViewModels[ci]);
                _comparisonIdToHypothesisViewModels.Remove(ci);
            }
        }

        private void getAllDecisions()
        {
            var comparisonIds = _comparisonIdToHypothesisViewModels.Keys.ToList();

            var opModel = new StatisticalComparisonDecisionOperationModel(_riskOperationModel.SchemaModel);
            opModel.PropertyChanged += StatisticalComparisonDecisionOperationModel_PropertyChanged;

            opModel.ModelId = _riskOperationModel.ModelId;
            opModel.ComparisonIds = comparisonIds;
            opModel.RiskControlType = _riskOperationModel.RiskControlType;
            _mainModel.QueryExecuter.ExecuteOperationModel(opModel, true);
        }

        private void _riskOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _riskOperationModel.GetPropertyName(() => _riskOperationModel.Result) && _riskOperationModel.Result != null)
            {
                _riskOperationModel.ModelId = ((NewModelOperationResult) _riskOperationModel.Result).ModelId;
            }
            else if (e.PropertyName == _riskOperationModel.GetPropertyName(() => _riskOperationModel.RiskControlType))
            {
                getAllDecisions();
            }
        }
    }
}