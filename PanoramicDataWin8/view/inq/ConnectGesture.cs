using System.Linq;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;
using System;
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

                
                foreach (var operationViewModel in MainViewController.Instance.OperationViewModels)
                {
                    foreach (var attachmentViewModel in operationViewModel.AttachementViewModels)      
                    {
                        foreach (var menuItemViewModel in attachmentViewModel.MenuViewModel.MenuItemViewModels)
                        {
                            if (menuItemViewModel.MenuItemComponentViewModel is AttributeTransformationMenuItemViewModel &&
                                menuItemViewModel.Bounds.Contains(inkStroke.Points[0]))
                            {
                                var attr = (menuItemViewModel.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                                var name = attr.AttributeModel.RawName;
                            }
                        }
                    }
                }

                var hits = MainViewController.Instance.OperationViewModels
                    .SelectMany(ovm => ovm.AttachementViewModels)
                    .Select(att => att.MenuViewModel)
                    .SelectMany(mvm => mvm.MenuItemViewModels)
                    .Where(mivm => mivm.MenuItemComponentViewModel is AttributeTransformationMenuItemViewModel && mivm.Bounds.Contains(inkStroke.Points[0]))
                    .Select(mivm => mivm.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel);

                var tt = hits.ToArray();

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

                if (_filterProviderOperationViewModel != null && (inkStroke.IsPause || _filterConsumerOperationViewModel != null) && _filterProviderOperationViewModel is IFilterConsumerOperationModel)
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