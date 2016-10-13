using System.Linq;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;

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

                    if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()) && operationModel is IFilterProviderOperationModel)
                    {
                        _filterProviderOperationViewModel = operationModel as IFilterProviderOperationModel;
                    }
                    if (view.Geometry.Contains(inkStroke.Points[inkStroke.Points.Count - 1].GetPoint()) &&
                        _filterProviderOperationViewModel != operationModel && operationModel is IFilterConsumerOperationModel)  
                    {
                        _filterConsumerOperationViewModel = operationModel as IFilterConsumerOperationModel;
                    }
                }

                if (_filterProviderOperationViewModel != null && _filterConsumerOperationViewModel != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}