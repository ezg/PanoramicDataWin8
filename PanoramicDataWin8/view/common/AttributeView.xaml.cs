using PanoramicData.controller.view;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicData.view.inq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PanoramicData.model.data;
using Windows.UI;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class AttributeView : UserControl
    {
        private AttributeView _shadow = null;
        private long _manipulationStartTime = 0;
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        public AttributeView()
        {
            this.InitializeComponent();
            this.DataContextChanged += AttributeView_DataContextChanged;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }

        void AttributeView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null && args.NewValue is AttributeViewModel)
            {
                (args.NewValue as AttributeViewModel).PropertyChanged += AttributeView_PropertyChanged;
                updateRendering();
            }
        }

        void AttributeView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void updateRendering()
        {
            AttributeViewModel model = DataContext as AttributeViewModel;

            if (model.IsShadow)
            {
                mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                border.BorderThickness = new Thickness(4);
            }
            else
            {
                mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                border.BorderThickness = new Thickness(0);
            }
        }

        void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                _manipulationStartTime = DateTime.Now.Ticks;
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
                    InkableScene inkableScene = MainViewController.Instance.InkableScene;
                    _shadow.RenderTransform = new TranslateTransform()
                    {
                        X = currentPoint.X - _shadow.Width / 2.0,
                        Y = currentPoint.Y - _shadow.Height
                    };
                    if (inkableScene != null)
                    {
                        inkableScene.Add(_shadow);

                        Rct bounds = _shadow.GetBounds(inkableScene);
                        (DataContext as AttributeViewModel).FireMoved(bounds,
                            new AttributeOperationModel((DataContext as AttributeViewModel).AttributeOperationModel.AttributeModel),
                            AttributeViewModelEventArgType.Default);
                    }
                }

                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {

            if (_shadow == null &&
                _manipulationStartTime + TimeSpan.FromSeconds(0.5).Ticks > DateTime.Now.Ticks)
            {
                if ((DataContext as AttributeViewModel).IsMenuEnabled)
                {
                    DisplayMenu(_mainPointerManagerPreviousPoint);
                }
            }

            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;

                Rct bounds = _shadow.GetBounds(inkableScene);
                (DataContext as AttributeViewModel).FireDropped(bounds, AttributeViewModelEventArgType.Default,
                    new AttributeOperationModel((DataContext as AttributeViewModel).AttributeOperationModel.AttributeModel));

                inkableScene.Remove(_shadow);
                _shadow = null;
            }

            _manipulationStartTime = 0;
        }

        public void createShadow(Point fromInkableScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            if (inkableScene != null && DataContext != null && (DataContext as AttributeViewModel).AttributeOperationModel != null)
            {
                _currentFromInkableScene = fromInkableScene;
                _shadow = new AttributeView();
                _shadow.DataContext = new AttributeViewModel(null, (DataContext as AttributeViewModel).AttributeOperationModel)
                {
                    IsNoChrome = false,
                    IsMenuEnabled = true,
                    IsShadow = true
                };

                _shadow.Measure(new Size(double.PositiveInfinity,
                                         double.PositiveInfinity));

                double add = (DataContext as AttributeViewModel).IsNoChrome ? 30 : 0;
                _shadow.Width = this.ActualWidth + add;
                _shadow.Height = _shadow.DesiredSize.Height;

                _shadow.RenderTransform = new TranslateTransform()
                {
                    X = fromInkableScene.X - _shadow.Width / 2.0,
                    Y = fromInkableScene.Y - _shadow.Height
                };
                    
                    
                inkableScene.Add(_shadow);
                _shadow.SendToFront();

                Rct bounds = _shadow.GetBounds(inkableScene);
                (DataContext as AttributeViewModel).FireMoved(bounds,
                    new AttributeOperationModel((DataContext as AttributeViewModel).AttributeOperationModel.AttributeModel),
                    AttributeViewModelEventArgType.Default);
            }
        }
        
        private void DisplayMenu(Point fromInkableScene)
        {
            AttributeViewModel model = (DataContext as AttributeViewModel);
            bool createNew = true;

            if (_menuViewModel != null)
            {
                createNew = _menuViewModel.AttributeViewModel != model;
                foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                {
                    menuItem.TargetPosition = model.VisualizationViewModel.Bounds.Center;
                }
                _menuViewModel.IsToBeRemoved = true;
                _menuViewModel.IsDisplayed = false;
                _menuViewModel = null;
            }

            if (createNew)
            {
                var menuViewModel = model.CreateMenuViewModel();
                if (menuViewModel.MenuItemViewModels.Count > 0)
                {
                    _menuViewModel = menuViewModel;
                    _menuView = new MenuView()
                    {
                        DataContext = _menuViewModel
                    };
                    setMenuViewModelAnkerPosition();
                    MainViewController.Instance.InkableScene.Add(_menuView);
                    _menuViewModel.IsDisplayed = true;
                }
            }
        }

        private void setMenuViewModelAnkerPosition()
        {
            if (_menuViewModel != null)
            {
                AttributeViewModel model = (DataContext as AttributeViewModel);

                if (model.AttachmentOrientation == AttachmentOrientation.Top)
                {
                    Rct bounds = this.GetBounds(MainViewController.Instance.InkableScene);
                    _menuViewModel.AnkerPosition = bounds.TopLeft;
                }
            }
        }

    }
}
