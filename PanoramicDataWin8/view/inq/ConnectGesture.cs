using System.Linq;
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

        private OperationViewModel _fromOperationViewModel = null;
        public OperationViewModel FromOperationViewModel
        {
            get { return _fromOperationViewModel; }
        }

        private OperationViewModel _toOperationViewModel = null;
        public OperationViewModel ToOperationViewModel
        {
            get { return _toOperationViewModel; }
        }

        public bool Recognize(InkStroke inkStroke)
        {
            if (!inkStroke.IsErase)
            {
                _fromOperationViewModel = null;
                _toOperationViewModel = null;

                foreach (OperationContainerView view in _inkableScene.Elements.Where(e => e is OperationContainerView))
                {
                    if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()))
                    {
                        _fromOperationViewModel = view.DataContext as HistogramOperationViewModel;
                    }
                    if (view.Geometry.Contains(inkStroke.Points[inkStroke.Points.Count - 1].GetPoint()) &&
                        _fromOperationViewModel != view.DataContext as HistogramOperationViewModel)
                    {
                        _toOperationViewModel = view.DataContext as HistogramOperationViewModel;
                    }
                }

                if (_fromOperationViewModel != null && _toOperationViewModel != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
