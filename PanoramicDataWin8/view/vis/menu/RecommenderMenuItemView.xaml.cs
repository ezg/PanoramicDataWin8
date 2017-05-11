using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class RecommenderMenuItemView : UserControl
    {
        private RecommenderHandleView _shadow = null;
        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();
        private Pt _mainPointerManagerStartPoint = new Point();

        public RecommenderMenuItemView()
        {
            this.InitializeComponent();
            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
            Loaded += RecommenderMenuItemView_Loaded;
        }

        private void RecommenderMenuItemView_Loaded(object sender, RoutedEventArgs e)
        {
            budgetView.DataContext = new BudgetViewModel()
            {
                DefaultLabel = "rec"
            };
        }

        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                _mainPointerManagerStartPoint = _mainPointerManagerPreviousPoint;
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = gt.TransformPoint(e.StartContacts[e.TriggeringPointer.PointerId].Position).GetVec() - currentPoint.GetVec();

                if (delta.Length > 10 && _shadow == null)
                {
                    createShadow(currentPoint);
                }

                if (_shadow != null)
                {
                    var shadowModel = _shadow.DataContext as RecommenderHandleViewModel;
                    InkableScene inkableScene = MainViewController.Instance.InkableScene;
                    shadowModel.Position = new Pt(currentPoint.X - shadowModel.Size.X / 2.0, currentPoint.Y - shadowModel.Size.Y);

                    if (inkableScene != null)
                    {
                        inkableScene.Add(_shadow);
                    }
                }

                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;

                Rct bounds = _shadow.GetBounds(inkableScene);
                (budgetView.DataContext as BudgetViewModel).BudgetToSpend = 0;
                (_shadow.DataContext as RecommenderHandleViewModel).PropertyChanged -= ShadowModel_PropertyChanged;
                inkableScene.Remove(_shadow);
                _shadow = null;

                var model = ((MenuItemViewModel) this.DataContext).MenuItemComponentViewModel as RecommenderMenuItemViewModel;
                var dist = (_mainPointerManagerStartPoint.GetVec() - _mainPointerManagerPreviousPoint.GetVec()).Length;
                if (model != null && dist > 50)
                {
                    model.FireCreateRecommendationEvent(bounds);
                }
            }
        }

        public void createShadow(Point fromInkableScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            if (inkableScene != null)
            {
                var model = ((MenuItemViewModel)this.DataContext).MenuItemComponentViewModel as RecommenderMenuItemViewModel;
                _shadow = new RecommenderHandleView();
                var shadowModel = new RecommenderHandleViewModel()
                {
                    AttachmentViewModel = model.AttachmentViewModel
                };
                _shadow.DataContext = shadowModel;
                shadowModel.PropertyChanged += ShadowModel_PropertyChanged;
                
                shadowModel.Size = new Vec(this.ActualWidth, this.ActualHeight);
                shadowModel.Position = new Pt(fromInkableScene.X - shadowModel.Size.X / 2.0, fromInkableScene.Y - shadowModel.Size.Y);
                shadowModel.StartPosition = shadowModel.Position;
                shadowModel.StartPercentage = shadowModel.Percentage = 20;

                inkableScene.Add(_shadow);
                _shadow.SendToFront();
            }
        }

        private void ShadowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as RecommenderHandleViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Percentage))
            {
                (budgetView.DataContext as BudgetViewModel).BudgetToSpend = model.Percentage / 100.0;
            }
        }
    }
}
