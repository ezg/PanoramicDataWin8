using System.Linq;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;
using System;
using IDEA_common.catalog;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.view.vis.render;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.view.vis.menu;

namespace PanoramicDataWin8.view.inq
{
    public class ConnectGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public ConnectGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }


        private IFilterProviderOperationModel _filterProviderOperationViewModel = null;
        public IFilterProviderOperationModel FilterProviderOperationViewModel
        {
            get { return _filterProviderOperationViewModel; }
        }

        private IFilterConsumerOperationModel _filterConsumerOperationViewModel = null;
        public IFilterConsumerOperationModel FilterConsumerOperationViewModel
        {
            get { return _filterConsumerOperationViewModel; }
        }

        public bool Recognize(InkStroke inkStroke)
        {
            if (!inkStroke.IsErase)
            {
                _filterProviderOperationViewModel = null;
                _filterConsumerOperationViewModel = null;

                
                foreach (OperationContainerView view in _inkableScene.Elements.Where(e => e is OperationContainerView))
                {
                    var operationModel = ((OperationViewModel) view.DataContext).OperationModel;

                    bool withinView = false;
                    if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()) && operationModel is IFilterProviderOperationModel)
                    {
                        _filterProviderOperationViewModel = operationModel as IFilterProviderOperationModel;
                        withinView = true;
                    }
                    if (view.Geometry.Contains(inkStroke.Points[inkStroke.Points.Count - 1].GetPoint()))
                    {
                        if (operationModel is IFilterConsumerOperationModel && _filterProviderOperationViewModel != operationModel)
                            _filterConsumerOperationViewModel = operationModel as IFilterConsumerOperationModel;
                        else if (withinView)
                            return false;
                    }
                }

                if (_filterProviderOperationViewModel != null && (_filterProviderOperationViewModel is FilterOperationModel || inkStroke.IsPause || _filterConsumerOperationViewModel != null)) // && _filterProviderOperationViewModel is IFilterConsumerOperationModel)
                {
                    return true;
                }
            }
            return false;
        }
        public OperationContainerView CreateConsumer(InkStroke inkStroke)
        {
            foreach (OperationContainerView view in _inkableScene.Elements.Where(e => e is OperationContainerView))
            {
                var operationModel = ((OperationViewModel)view.DataContext).OperationModel;

                if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()) && operationModel is IFilterProviderOperationModel)
                {
                    var operationContainerView = PanoramicDataWin8.controller.view.MainViewController.Instance.CopyOperationViewModel(
                        view.DataContext as OperationViewModel, inkStroke.Points.Last());
                    _filterConsumerOperationViewModel = (operationContainerView.DataContext as OperationViewModel).OperationModel as IFilterConsumerOperationModel;
                    return operationContainerView;
                }
            }
            return null;
        }
    }
    public class FilterGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public FilterGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
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
                                    var attr = (menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel).AttributeViewModel.AttributeModel;
                                    var filterOperationViewModel = AddFilterModel(attr.RawName, Predicate.GREATER_THAN, 0, inkStroke.Points.Last(), controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse);
                                    var size = new Vec(OperationViewModel.WIDTH, MainViewController.Instance.MainPage.LastTouchWasMouse ? 50 : OperationViewModel.HEIGHT);
                                    var operationContainerView = new OperationContainerView();
                                    filterOperationViewModel.Size = size;
                                    operationContainerView.DataContext = filterOperationViewModel;
                                    MainViewController.Instance.InkableScene.Add(operationContainerView);
                                    if (FilterRenderer.FieldType(attr.RawName) ==   DataType.String)
                                        (operationContainerView.Renderer as FilterRenderer).SetFilter(attr.RawName, Predicate.LESS_THAN, "a");
                                    else
                                        (operationContainerView.Renderer as FilterRenderer).SetFilter(attr.RawName, Predicate.LESS_THAN, 0);
                                    FilterLinkViewController.Instance.CreateFilterLinkViewModel(filterOperationViewModel.OperationModel,
                                        (OperationModel)operationViewModel.OperationModel);

                                    return true;
                                }
                            }
                    }
                }
            }
            return false;
        }
        private FilterOperationViewModel AddFilterModel(string field, Predicate pred, double value, Pt p, bool useTypingUI)
        {
            var schemaModel = (controller.view.MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel;
            var inputModels = schemaModel.OriginModels.First().InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;
            var attributeTransformationModel = new AttributeTransformationModel(inputModels.First() as AttributeModel);
            foreach (var im in inputModels)
                if (im.RawName.ToLower() == field)
                    attributeTransformationModel = new AttributeTransformationModel(im as AttributeModel);
            
            var filterModel = new FilterModel();
            filterModel.ValueComparisons.Add(new ValueComparison(attributeTransformationModel, pred, value));
            return controller.view.MainViewController.Instance.CreateDefaultFilterOperationViewModel(p, useTypingUI);
        }
        
    }
}