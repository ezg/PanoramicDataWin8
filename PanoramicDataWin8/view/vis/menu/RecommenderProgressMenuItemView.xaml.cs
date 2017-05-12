using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class RecommenderProgressMenuItemView : UserControl
    {
        private readonly PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint;
        private Pt _mainPointerManagerStartPoint = new Point();
        private RecommenderProgressMenuItemViewModel _model;

        public RecommenderProgressMenuItemView()
        {
            InitializeComponent();
            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
            DataContextChanged += RecommenderHandleView_DataContextChanged;
        }

        private void RecommenderHandleView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                _model = null;
            }
            if (args.NewValue != null)
            {
                _model = ((MenuItemViewModel) DataContext).MenuItemComponentViewModel as RecommenderProgressMenuItemViewModel;
                _model.HistogramOperationViewModel.RecommenderOperationViewModel.RecommenderOperationModel.PropertyChanged += RecommenderOperationModel_PropertyChanged;
                _model.PropertyChanged += _model_PropertyChanged;
                updateRendering();
                updateProgress();
            }
        }

        private void RecommenderOperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as RecommenderOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                updateProgress();
            }
        }

        private void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void updateRendering()
        {
            //  left.Visibility = _model.PagingDirection == PagingDirection.Left ? Visibility.Visible : Visibility.Collapsed;
            //right.Visibility = _model.PagingDirection == PagingDirection.Right ? Visibility.Visible : Visibility.Collapsed;
        }


        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var gt = TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                _mainPointerManagerStartPoint = _mainPointerManagerPreviousPoint;
            }
        }

        private void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
        }

        private void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (_model != null)
            {
            }
        }

        private void updateProgress()
        {
            var resultModel = _model.HistogramOperationViewModel.RecommenderOperationViewModel?.RecommenderOperationModel?.Result;
            var progress = resultModel?.Progress ?? 0;
            progressGrid.Visibility = Visibility.Visible;
            // progress
            double size = 14;
            double thickness = 2;

            var percentage = Math.Min(progress, 0.999999);
            if (percentage > 0.5)
            {
                arcSegement1.IsLargeArc = true;
            }
            else
            {
                arcSegement1.IsLargeArc = false;
            }
            var angle = 2 * Math.PI * percentage - Math.PI / 2.0;
            var x = size / 2.0;
            var y = size / 2.0;

            var p = new Point(Math.Cos(angle) * (size / 2.0 - thickness / 2.0) + x, Math.Sin(angle) * (size / 2.0 - thickness / 2.0) + y);
            arcSegement1.Point = p;
            if (size / 2.0 - thickness / 2.0 > 0.0)
            {
                arcSegement1.Size = new Size(size / 2.0 - thickness / 2.0, size / 2.0 - thickness / 2.0);
            }
        }
    }
}