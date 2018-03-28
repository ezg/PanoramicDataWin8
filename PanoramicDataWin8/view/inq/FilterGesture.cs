using System.Linq;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.view.vis.render;
using PanoramicDataWin8.controller.view;

namespace PanoramicDataWin8.view.inq
{
    public class FilterGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public FilterGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }

        AttributeModel _attributeModel;
        OperationViewModel _operationViewModel;
        public AttributeModel FilterAttributeModel => _attributeModel;
        public OperationViewModel OperationViewModel => _operationViewModel;

        public void CreateFilter(InkStroke stroke)
        {
            var filterOperationViewModel = AddFilterModel(FilterAttributeModel, Predicate.GREATER_THAN, 0, stroke.Points.Last(), controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse);
            filterOperationViewModel.Size = new Vec(OperationViewModel.WIDTH, MainViewController.Instance.MainPage.LastTouchWasMouse ? 50 : OperationViewModel.HEIGHT);
            var operationContainerView = new OperationContainerView() { DataContext = filterOperationViewModel };
            if (operationContainerView.Renderer is FilterRenderer filterRenderer)
            {
                filterRenderer.SetFilter(FilterAttributeModel, Predicate.LESS_THAN, FilterAttributeModel.DataType);
            }
            MainViewController.Instance.InkableScene.Add(operationContainerView);
            FilterLinkViewController.Instance.CreateFilterLinkViewModel(filterOperationViewModel.OperationModel,
                (OperationModel)OperationViewModel.OperationModel);

        }

        public bool Recognize(InkStroke inkStroke)
        {
            if (!inkStroke.IsErase)
            {
                foreach (var operationViewModel in controller.view.MainViewController.Instance.OperationViewModels)
                {
                    foreach (var attachmentViewModel in operationViewModel.AttachementViewModels)
                    {
                        if (attachmentViewModel.MenuViewModel != null)
                            foreach (var menuItemViewModel in attachmentViewModel.MenuViewModel.MenuItemViewModels)
                            {
                                if (menuItemViewModel.MenuItemComponentViewModel is AttributeMenuItemViewModel &&
                                    new Rct(menuItemViewModel.Position, menuItemViewModel.Size).Contains(inkStroke.Points[0]))
                                {
                                    _operationViewModel = operationViewModel;
                                    _attributeModel = (menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel).AttributeViewModel.AttributeModel;

                                    return true;
                                }
                            }
                    }
                }
            }
            return false;
        }
        FilterOperationViewModel AddFilterModel(AttributeModel field, Predicate pred, double value, Pt p, bool useTypingUI)
        {
            var filterModel = new FilterModel();
            filterModel.ValueComparisons.Add(new ValueComparison(new AttributeTransformationModel(field), pred, value));
            return MainViewController.Instance.CreateDefaultFilterOperationViewModel(p, useTypingUI);
        }

    }
    public class GraphFilterGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public GraphFilterGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }

        GraphOperationViewModel GraphOperationViewModel;

        public void CreateFilter(InkStroke stroke)
        {
            var graphFilterVM = MainViewController.Instance.CreateDefaultGraphFilterOperationViewModel(stroke.Points.Last(), true);
            graphFilterVM.Size = new Vec(OperationViewModel.WIDTH, MainViewController.Instance.MainPage.LastTouchWasMouse ? 50 : OperationViewModel.HEIGHT);
            graphFilterVM.GraphFilterOperationModel.TargetGraphOperationModel = GraphOperationViewModel.GraphOperationModel;
            var operationContainerView = new OperationContainerView() { DataContext = graphFilterVM };
            MainViewController.Instance.InkableScene.Add(operationContainerView);
        }

        public bool Recognize(InkStroke inkStroke)
        {
            if (!inkStroke.IsErase)
            {
                foreach (OperationContainerView view in _inkableScene.Elements.Where(e => e is OperationContainerView).Where((ocv)=>ocv.DataContext is GraphOperationViewModel))

                {
                    if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()))
                    {
                        GraphOperationViewModel = view.DataContext as GraphOperationViewModel;
                        return true;
                    }
                }
            }
            return false;
        }
        FilterOperationViewModel AddFilterModel(AttributeModel field, Predicate pred, double value, Pt p, bool useTypingUI)
        {
            var filterModel = new FilterModel();
            filterModel.ValueComparisons.Add(new ValueComparison(new AttributeTransformationModel(field), pred, value));
            return MainViewController.Instance.CreateDefaultFilterOperationViewModel(p, useTypingUI);
        }

    }
}
