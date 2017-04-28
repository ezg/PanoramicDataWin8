using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using IDEA_common.operations.risk;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.view
{
    public class RecommenderViewController
    {
         private readonly MainModel _mainModel;


        private RecommenderViewController(MainModel mainModel, ObservableCollection<OperationViewModel> operationViewModel)
        {
            _mainModel = mainModel;
            //MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model, false);
        }

        public static RecommenderViewController Instance { get; private set; }

        public RecommenderOperationViewModel CreateRecommenderOperationViewModel(HistogramOperationViewModel histogramViewModel)
        {
            RecommenderOperationModel model = new RecommenderOperationModel(_mainModel.SchemaModel);
            RecommenderOperationViewModel viewModel = new RecommenderOperationViewModel(model);
            MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model, true);
            return viewModel;
        }
        
        public static void CreateInstance(MainModel mainModel, ObservableCollection<OperationViewModel> operationViewModel)
        {
            Instance = new RecommenderViewController(mainModel, operationViewModel);
          
        }

    }
}