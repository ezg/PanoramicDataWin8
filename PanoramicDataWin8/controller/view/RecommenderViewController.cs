using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using IDEA_common.operations;
using IDEA_common.operations.recommender;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class RecommenderViewController
    {
         private readonly MainModel _mainModel;
        private Dictionary<RecommenderOperationModel, RecommenderOperationViewModel> _recommenderOperationViewModels = new Dictionary<RecommenderOperationModel, RecommenderOperationViewModel>();
        private Dictionary<RecommenderOperationViewModel, HistogramOperationViewModel> _parentHistogramOperationViewModels = new Dictionary<RecommenderOperationViewModel, HistogramOperationViewModel>();


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
            histogramViewModel.RecommenderOperationViewModel = viewModel;
            _recommenderOperationViewModels.Add(model, viewModel);
            _parentHistogramOperationViewModels.Add(viewModel, histogramViewModel);


            model.PropertyChanged += Model_PropertyChanged;
            model.OperationModelUpdated += Model_OperationModelUpdated;
            MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model, true);
            return viewModel;
        }

        private void Model_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            //MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(sender as RecommenderOperationModel, true);
            MainViewController.Instance.MainModel.QueryExecuter.UpdateResultParameters(sender as RecommenderOperationModel);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as RecommenderOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result) && model.Result != null)
            {
                var result = model.Result as RecommenderResult;
                var viewModel = _recommenderOperationViewModels[model];
                var parent = _parentHistogramOperationViewModels[viewModel];

                var attachmentViewModel = parent.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
                var menuViewModel = attachmentViewModel.MenuViewModel;
                menuViewModel.MenuItemViewModels.First().IsAlwaysDisplayed = true;

                int col = 0;
                int row = 0;
                
                foreach (var existingModel in menuViewModel.MenuItemViewModels.Where(mi => mi.MenuItemComponentViewModel is RecommendedHistogramMenuItemViewModel).ToArray())
                {
                    if (result.RecommendedHistograms.Any(rh => rh.Id == (existingModel.MenuItemComponentViewModel as RecommendedHistogramMenuItemViewModel).Id))
                    {
                        existingModel.Column = col + 1;
                        existingModel.Row = row;
                        col++;
                        if (col == 3)
                        {
                            col = 0;
                            row += 1;
                        }
                    }
                    else
                    {
                        (existingModel.MenuItemComponentViewModel as RecommendedHistogramMenuItemViewModel).DroppedEvent -= RecHistogramModel_DroppedEvent;
                        menuViewModel.MenuItemViewModels.Remove(existingModel);
                    }
                }

                foreach (var recommendedHistogram in result.RecommendedHistograms.ToArray())
                {
                    if (!menuViewModel.MenuItemViewModels.Where(mi => mi.MenuItemComponentViewModel is RecommendedHistogramMenuItemViewModel)
                        .Any(mi => (mi.MenuItemComponentViewModel as RecommendedHistogramMenuItemViewModel).Id == recommendedHistogram.Id))
                    {
                        var newModel = new MenuItemViewModel()
                        {
                            Position = menuViewModel.MenuItemViewModels.First().Position,
                            Size = new Vec(54, 54),
                            TargetSize = new Vec(54, 54),
                            IsAlwaysDisplayed = true
                        };
                        var recHistogramModel = new RecommendedHistogramMenuItemViewModel
                        {
                            Id = recommendedHistogram.Id,
                            RecommendedHistogram = recommendedHistogram,
                            HistogramOperationViewModel = parent
                        };
                        newModel.MenuItemComponentViewModel = recHistogramModel;
                        recHistogramModel.DroppedEvent += RecHistogramModel_DroppedEvent;

                        newModel.Column = col + 1;
                        newModel.Row = row;
                        col++;
                        if (col == 3)
                        {
                            col = 0;
                            row += 1;
                        }
                        menuViewModel.MenuItemViewModels.Add(newModel);
                    }
                }

                addPagingControls(menuViewModel, result, model);
            }
        }

        private void RecHistogramModel_DroppedEvent(object sender, Rct bounds)
        {
            var operationContainerView = new OperationContainerView();
            var width = OperationViewModel.WIDTH;
            var height = OperationViewModel.HEIGHT;

            var model = sender as RecommendedHistogramMenuItemViewModel;
            var attr = IDEAHelpers.GetAttributeModelFromAttribute(model.RecommendedHistogram.XAttribute);
            var filterModels = IDEAHelpers.GetFilterModelsFromSelections(model.RecommendedHistogram.Selections);
            attr.OriginModel = MainViewController.Instance.MainModel.SchemaModel.OriginModels.First();
            var operationViewModel = MainViewController.Instance.CreateDefaultHistogramOperationViewModel(attr, bounds.Center - new Vec(width / 2.0, height / 2.0));
            operationViewModel.HistogramOperationModel.AddFilterModels(filterModels);
            FilterLinkViewController.Instance.CreateFilterLinkViewModel(operationViewModel.OperationModel, model.HistogramOperationViewModel.OperationModel);

            var size = new Vec(width, height);
            operationViewModel.Size = size;
            operationContainerView.DataContext = operationViewModel;
            MainViewController.Instance.InkableScene.Add(operationContainerView);

        }

        private void addPagingControls(MenuViewModel menuViewModel, RecommenderResult result, RecommenderOperationModel model)
        {
            var left = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => (mi.MenuItemComponentViewModel as PagingMenuItemViewModel)?.PagingDirection == PagingDirection.Left);
            var right = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => (mi.MenuItemComponentViewModel as PagingMenuItemViewModel)?.PagingDirection == PagingDirection.Right);

            if (model.Page > 0)
            {
                if (left == null)
                {
                    var newModel = new MenuItemViewModel()
                    {
                        Size = new Vec(25, 25),
                        TargetSize = new Vec(25, 25),
                        IsAlwaysDisplayed = true
                    };
                    var paging = new PagingMenuItemViewModel() {PagingDirection = PagingDirection.Left};
                    paging.PagingEvent += (sender, direction) =>
                    {
                        model.Page -= 1;
                        model.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    };
                    newModel.MenuItemComponentViewModel = paging;

                    newModel.Column = 1;
                    newModel.Row = 3;
                    menuViewModel.MenuItemViewModels.Add(newModel);
                }
            }
            else if (left != null)
            {
                menuViewModel.MenuItemViewModels.Remove(left);
            }

            if (model.Page * model.PageSize + model.PageSize < result.TotalCount)
            {
                if (right == null)
                {
                    var newModel = new MenuItemViewModel()
                    {
                        Size = new Vec(25, 25),
                        TargetSize = new Vec(25, 25),
                        IsAlwaysDisplayed = true
                    };
                    var paging = new PagingMenuItemViewModel() {PagingDirection = PagingDirection.Right};
                    paging.PagingEvent += (sender, direction) =>
                    {
                        model.Page += 1;
                        model.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    };
                    newModel.MenuItemComponentViewModel = paging;

                    newModel.Column = 3;
                    newModel.Row = 3;
                    newModel.MenuXAlign = MenuXAlign.WithColumn | MenuXAlign.Right;
                    newModel.MenuYAlign = MenuYAlign.WithRow;

                    menuViewModel.MenuItemViewModels.Add(newModel);
                }
            }
            else if (right != null)
            {
                menuViewModel.MenuItemViewModels.Remove(right);
            }
        }

        public static void CreateInstance(MainModel mainModel, ObservableCollection<OperationViewModel> operationViewModel)
        {
            Instance = new RecommenderViewController(mainModel, operationViewModel);
          
        }

    }
}