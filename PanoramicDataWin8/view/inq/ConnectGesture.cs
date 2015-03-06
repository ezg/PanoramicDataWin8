using GeoAPI.Geometries;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PanoramicData.view.inq
{
    public class ConnectGesture : IGesture
    {
        private InkableScene _inkableScene = null;

        public ConnectGesture(InkableScene inkableScene)
        {
            this._inkableScene = inkableScene;
        }

        private VisualizationViewModel _fromVisualizationViewModel = null;
        public VisualizationViewModel FromVisualizationViewModel
        {
            get { return _fromVisualizationViewModel; }
        }

        private VisualizationViewModel _toVisualizationViewModel = null;
        public VisualizationViewModel ToVisualizationViewModel
        {
            get { return _toVisualizationViewModel; }
        }

        public bool Recognize(InkStroke inkStroke)
        {
            _fromVisualizationViewModel = null;
            _toVisualizationViewModel = null;

            foreach (VisualizationContainerView view in _inkableScene.Elements.Where(e => e is VisualizationContainerView))
            {
                if (view.Geometry.Contains(inkStroke.Points[0].GetPoint()))
                {
                    _fromVisualizationViewModel = view.DataContext as VisualizationViewModel;
                }
                if (view.Geometry.Contains(inkStroke.Points[inkStroke.Points.Count - 1].GetPoint()) &&
                    _fromVisualizationViewModel != view.DataContext as VisualizationViewModel)
                {
                    _toVisualizationViewModel = view.DataContext as VisualizationViewModel;
                }
            }

            if (_fromVisualizationViewModel != null && _toVisualizationViewModel != null)
            {
                return true;
            }
            return false;
        }
    }
}
