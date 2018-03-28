using System.Collections;
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
using System.Diagnostics;

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
            HypothesesViewController.Instance.RiskOperationModel.PropertyChanged += RiskOperationModel_PropertyChanged;
        }

        private void RiskOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as RiskOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.ModelId) ||
                e.PropertyName == model.GetPropertyName(() => model.RiskControlType))
            {
                foreach (var rovm in _recommenderOperationViewModels)
                {
                    rovm.Key.Result = null;

                }
            }
        }

        public static RecommenderViewController Instance { get; private set; }

        public RecommenderOperationViewModel CreateRecommenderOperationViewModel(HistogramOperationViewModel histogramViewModel)
        {
            RecommenderOperationModel model = new RecommenderOperationModel(histogramViewModel.OperationModel.OriginModel);
            model.Target = histogramViewModel.HistogramOperationModel;

            model.ModelId = HypothesesViewController.Instance.RiskOperationModel.ModelId;
            RecommenderOperationViewModel viewModel = new RecommenderOperationViewModel(model);
            histogramViewModel.RecommenderOperationViewModel = viewModel;
            _recommenderOperationViewModels.Add(model, viewModel);
            _parentHistogramOperationViewModels.Add(viewModel, histogramViewModel);
            model.PropertyChanged += Model_PropertyChanged;
            model.OperationModelUpdated += Model_OperationModelUpdated;
            var attachmentViewModel = histogramViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);


            model.Exlude.CollectionChanged += (sender, args) =>
            {
                updateIncludeExlude(args.OldItems, args.NewItems, attachmentViewModel, histogramViewModel, model, false);
            };
            model.Include.CollectionChanged += (sender, args) =>
            {
                updateIncludeExlude(args.OldItems, args.NewItems, attachmentViewModel, histogramViewModel, model, true);
            };

            return viewModel;
        }

        private  void updateIncludeExlude(IList oldItems, IList newItems, AttachmentViewModel attachmentViewModel, 
            HistogramOperationViewModel histogramViewModel, RecommenderOperationModel model, bool include)
        {
            if (oldItems != null)
            {
                foreach (var oldItem in oldItems)
                {
                    var oldMi = attachmentViewModel.MenuViewModel.MenuItemViewModels
                        .FirstOrDefault(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel &&
                                              (mi.MenuItemComponentViewModel as IncludeExludeMenuItemViewModel)
                                              .IsInclude == include &&
                                              (mi.MenuItemComponentViewModel as IncludeExludeMenuItemViewModel)
                                              .AttributeModel == oldItem);
                    if (oldMi != null)
                    {
                        attachmentViewModel.MenuViewModel.MenuItemViewModels.Remove(oldMi);
                    }
                }
            }
            if (newItems != null)
            {
                foreach (var newItem in newItems)
                {
                    var oldMi = attachmentViewModel.MenuViewModel.MenuItemViewModels
                        .FirstOrDefault(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel &&
                                              (mi.MenuItemComponentViewModel as IncludeExludeMenuItemViewModel)
                                              .IsInclude == include &&
                                              (mi.MenuItemComponentViewModel as IncludeExludeMenuItemViewModel)
                                              .AttributeModel == newItem);
                    if (oldMi == null)
                    {
                        var menuItem = new MenuItemViewModel()
                        {
                            Size = new Vec(54, 54),
                            Position = histogramViewModel.Position,
                            TargetSize = new Vec(54, 54),
                            Row = 0,
                            IsAlwaysDisplayed = true
                        };
                        var includeExcludeModel = new IncludeExludeMenuItemViewModel();
                        includeExcludeModel.AttributeModel = newItem as AttributeModel;
                        includeExcludeModel.IsInclude = include;
                        menuItem.MenuItemComponentViewModel = includeExcludeModel;
                        attachmentViewModel.MenuViewModel.MenuItemViewModels.Add(menuItem);
                    }
                }
            }
            updateLayout(model, attachmentViewModel.MenuViewModel, histogramViewModel);
        }

        private void Model_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            //MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(sender as RecommenderOperationModel, true);
            MainViewController.Instance.MainModel.QueryExecuter.UpdateResultParameters(sender as RecommenderOperationModel);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as RecommenderOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                var viewModel = _recommenderOperationViewModels[model];
                var parent = _parentHistogramOperationViewModels[viewModel];
                var attachmentViewModel =
                    parent.AttachementViewModels.First(
                        avm => avm.AttachmentOrientation == AttachmentOrientation.Right);

                var menuViewModel = attachmentViewModel.MenuViewModel;
                if (model.Result != null)
                {
                    var result = model.Result as RecommenderResult;

                   
                    menuViewModel.MenuItemViewModels.First().IsAlwaysDisplayed = true;

                    var includeExcludeCount = attachmentViewModel.MenuViewModel.MenuItemViewModels
                        .Count(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel);

                    updateRecommendedHistograms(menuViewModel, result, parent, includeExcludeCount);
                    addPagingControls(menuViewModel, result, model, includeExcludeCount);
                    updateLayout(model, menuViewModel, parent);
                }
                else
                {
                    foreach (var mi in menuViewModel.MenuItemViewModels.ToArray()
                        .Where(mi => !(mi.MenuItemComponentViewModel is RecommenderMenuItemViewModel) &&
                                     !(mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel)))
                    {
                        menuViewModel.MenuItemViewModels.Remove(mi);
                    }

                    var includeExcludeCount = attachmentViewModel.MenuViewModel.MenuItemViewModels
                        .Count(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel);
                    updateLayout(model, menuViewModel, parent);
                }
            }
        }

        private void updateLayout(RecommenderOperationModel recommenderOperationModel, MenuViewModel menuViewModel, HistogramOperationViewModel histogramViewModel)
        {
            int row = 0;
            // inlcude excludes
            var attachmentViewModel = histogramViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);

            var newCount = attachmentViewModel.MenuViewModel.MenuItemViewModels
                .Count(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel);

            foreach (var includeExcludeItem in attachmentViewModel.MenuViewModel.MenuItemViewModels
                .Where(mi => mi.MenuItemComponentViewModel is IncludeExludeMenuItemViewModel))
            {
                includeExcludeItem.Row = row;
                row++;
            }
            int c = -(recommenderOperationModel.Exlude.Count + recommenderOperationModel.Include.Count);
            attachmentViewModel.AnkerOffset = new Vec(0, c * 54 + c * 4);

            // standard ones
            var item = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => mi.MenuItemComponentViewModel is RecommenderMenuItemViewModel);
            if (item != null)
            {
                item.Row = row;
            }
            item = menuViewModel.MenuItemViewModels.FirstOrDefault(mi => mi.MenuItemComponentViewModel is RecommenderProgressMenuItemViewModel);
            if (item != null)
            {
                item.Row = row+1;
            }

            // recommended histograms
            int col = 0;
            foreach (var existingModel in menuViewModel.MenuItemViewModels.Where(mi => mi.MenuItemComponentViewModel is RecommendedHistogramMenuItemViewModel).ToArray())
            {
                //existingModel.Column = col + 1;
                existingModel.Row = row;
                col++;
                if (col == 3)
                {
                    col = 0;
                    row += 1;
                }
            }
            if (col != 0)
            {
                row++;
            }


            // paging
            foreach (var pager in menuViewModel.MenuItemViewModels.Where(mi => (mi.MenuItemComponentViewModel is PagingMenuItemViewModel)))
            {
                pager.Row = row;
            }
            menuViewModel.NrRows = row + 1;
        }

        private void updateRecommendedHistograms(MenuViewModel menuViewModel, RecommenderResult result, 
            HistogramOperationViewModel parent, int rowStart)
        {
            int col = 0;
            int row = rowStart;

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
        }

        private void RecHistogramModel_DroppedEvent(object sender, Rct bounds)
        {
            var operationContainerView = new OperationContainerView();
            var width = OperationViewModel.WIDTH;
            var height = OperationViewModel.HEIGHT;

            var model = sender as RecommendedHistogramMenuItemViewModel;
            var attr = IDEAHelpers.GetAttributeModelFromAttribute(model.RecommendedHistogram.XAttribute,
                model.HistogramOperationViewModel.HistogramOperationModel.OriginModel);
            var filterModels = IDEAHelpers.GetFilterModelsFromSelections(model.RecommendedHistogram.Selections,
                model.HistogramOperationViewModel.HistogramOperationModel.OriginModel);
            var operationViewModel = MainViewController.Instance.CreateDefaultHistogramOperationViewModel(attr, bounds.Center - new Vec(width / 2.0, height / 2.0));
            operationViewModel.HistogramOperationModel.AddFilterModels(filterModels);
            FilterLinkViewController.Instance.CreateFilterLinkViewModel(operationViewModel.OperationModel, model.HistogramOperationViewModel.OperationModel);

            var size = new Vec(width, height);
            operationViewModel.Size = size;
            operationContainerView.DataContext = operationViewModel;
            MainViewController.Instance.InkableScene.Add(operationContainerView);

        }

        private void addPagingControls(MenuViewModel menuViewModel, RecommenderResult result, 
            RecommenderOperationModel model, int rowStart)
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
                    newModel.Row = rowStart + 3;
                    newModel.MenuYAlign = MenuYAlign.WithRow;
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
                    newModel.Row = rowStart + 3;
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