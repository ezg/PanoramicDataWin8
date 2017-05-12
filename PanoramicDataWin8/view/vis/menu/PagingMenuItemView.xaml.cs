using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class PagingMenuItemView : UserControl
    {
        private PagingMenuItemViewModel _model = null;
        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();
        private Pt _mainPointerManagerStartPoint = new Point();

        public PagingMenuItemView()
        {
            this.InitializeComponent();
            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
            this.DataContextChanged += RecommenderHandleView_DataContextChanged;
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
                _model = ((MenuItemViewModel)this.DataContext).MenuItemComponentViewModel as PagingMenuItemViewModel;
                _model.PropertyChanged += _model_PropertyChanged;
                updateRendering();
            }
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        private void updateRendering()
        {
            left.Visibility = _model.PagingDirection == PagingDirection.Left ? Visibility.Visible : Visibility.Collapsed;
            right.Visibility = _model.PagingDirection == PagingDirection.Right ? Visibility.Visible : Visibility.Collapsed;
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
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (_model != null)
            {
                _model.FirePagingEvent();
            }
        }

    }
}
