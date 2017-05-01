﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using IDEA_common.operations.recommender;
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
            MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(model, true);
            return viewModel;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as RecommenderOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
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
                        var recHistogramModel = new RecommendedHistogramMenuItemViewModel();
                        recHistogramModel.Id = recommendedHistogram.Id;
                        newModel.MenuItemComponentViewModel = recHistogramModel;

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

        private void addPagingControls(MenuViewModel menuViewModel, RecommenderResult result, RecommenderOperationModel model)
        {
            var left = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => (mi.MenuItemComponentViewModel as PagingMenuItemViewModel)?.PagingDirection == PagingDirection.Left);
            var right = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => (mi.MenuItemComponentViewModel as PagingMenuItemViewModel)?.PagingDirection == PagingDirection.Right);

            if (model.Page > 0 && left == null)
            {
                var newModel = new MenuItemViewModel()
                {
                    Size = new Vec(25, 25),
                    TargetSize = new Vec(25, 25),
                    IsAlwaysDisplayed = true
                };
                var paging = new PagingMenuItemViewModel() { PagingDirection = PagingDirection.Left };
                paging.PagingEvent += Paging_PagingEvent;
                newModel.MenuItemComponentViewModel = paging;

                newModel.Column = 1;
                newModel.Row = 3;
                menuViewModel.MenuItemViewModels.Add(newModel);
            }
            else if (left != null)
            {
                menuViewModel.MenuItemViewModels.Remove(left);
            }

            if (model.Page * model.PageSize < result.TotalCount && right == null)
            {
                var newModel = new MenuItemViewModel()
                {
                    Size = new Vec(25, 25),
                    TargetSize = new Vec(25, 25),
                    IsAlwaysDisplayed = true
                };
                var paging = new PagingMenuItemViewModel() {PagingDirection = PagingDirection.Right};
                paging.PagingEvent += Paging_PagingEvent;
                newModel.MenuItemComponentViewModel = paging;

                newModel.Column = 3;
                newModel.Row = 3;
                newModel.MenuXAlign = MenuXAlign.WithColumn;
                newModel.MenuYAlign = MenuYAlign.WithRow;

                menuViewModel.MenuItemViewModels.Add(newModel);
            }
            else if (right != null)
            {
                menuViewModel.MenuItemViewModels.Remove(left);
            }
        }

        private void Paging_PagingEvent(object sender, PagingDirection pagingDirection)
        {
            throw new System.NotImplementedException();
        }

        public static void CreateInstance(MainModel mainModel, ObservableCollection<OperationViewModel> operationViewModel)
        {
            Instance = new RecommenderViewController(mainModel, operationViewModel);
          
        }

    }
}