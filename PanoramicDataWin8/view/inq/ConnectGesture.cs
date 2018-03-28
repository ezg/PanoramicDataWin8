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
using Windows.Foundation;

namespace PanoramicDataWin8.view.inq
{
    public class ConnectGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public ConnectGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }


        IFilterProviderOperationModel _filterProviderOperationViewModel = null;
        IFilterConsumerOperationModel _filterConsumerOperationViewModel = null;
        public IFilterProviderOperationModel FilterProviderOperationViewModel =>  _filterProviderOperationViewModel; 
        public IFilterConsumerOperationModel FilterConsumerOperationViewModel => _filterConsumerOperationViewModel;

        public void CreateConnection(InkStroke stroke)
        {

            bool created = false;
            if (FilterConsumerOperationViewModel == null)
            {
                created = true;
                CreateConsumer(stroke.Clone());
            }
            if (created && FilterProviderOperationViewModel is FilterOperationModel &&
                FilterConsumerOperationViewModel is FilterOperationModel)
            {
                FilterLinkViewController.Instance.CreateFilterLinkViewModel(FilterConsumerOperationViewModel,
                                                                            FilterProviderOperationViewModel);
            }
            else
                FilterLinkViewController.Instance.CreateFilterLinkViewModel(FilterProviderOperationViewModel,
                                                                            FilterConsumerOperationViewModel);
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
}