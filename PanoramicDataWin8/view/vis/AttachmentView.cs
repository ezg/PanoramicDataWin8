using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Core;
using Windows.System;
using Windows.Devices.Input;

namespace PanoramicDataWin8.view.vis
{
    public class AttachmentView : UserControl
    {
        private readonly DispatcherTimer _activeTimer = new DispatcherTimer();

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;


        public AttachmentView()
        {
            this.DataContextChanged += AttachmentView_DataContextChanged;

            _activeTimer.Interval = TimeSpan.FromMilliseconds(10);
            _activeTimer.Tick += _activeTimer_Tick;
            _activeTimer.Start();
        }

        void _activeTimer_Tick(object sender, object e)
        {
            var model = ((AttachmentViewModel) DataContext);
            if (_menuViewModel != null)
            {
                if (model.ActiveStopwatch.Elapsed > TimeSpan.FromSeconds(4) && _menuViewModel.IsDisplayed)
                {
                    _menuViewModel.IsDisplayed = false;
                }
                if (model.ActiveStopwatch.Elapsed.Ticks > 0 && model.ActiveStopwatch.Elapsed < TimeSpan.FromSeconds(2.5) && !_menuViewModel.IsDisplayed)
                {
                    _menuViewModel.IsDisplayed = true;
                }
            }
        }

        private IDisposable _disposable = null;
        void AttachmentView_DataContextChanged(FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs e)
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
            }
            if (e.NewValue != null)
            {
                var model = (e.NewValue as AttachmentViewModel);

                _menuViewModel = model.MenuViewModel;
                if (_menuViewModel != null && _menuView == null)
                {
                    _menuView = new MenuView()
                    {
                        DataContext = _menuViewModel
                    };
                    setMenuViewModelAnkerPosition();
                    MainViewController.Instance.InkableScene.Add(_menuView);
                    //_menuViewModel.IsDisplayed = true;

                }
                model.OperationViewModel.PropertyChanged += OperationViewModel_PropertyChanged;
                _disposable = Observable.FromEventPattern<PropertyChangedEventArgs>(model, "PropertyChanged")
                    .Sample(TimeSpan.FromMilliseconds(50))
                    .Subscribe(async arg =>
                    {
                        var dispatcher = this.Dispatcher;
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            OperationViewModel_PropertyChanged(arg.Sender, arg.EventArgs);
                        });
                    });

            }
        }

        void OperationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var attach = (DataContext as AttachmentViewModel);
            var visModel = attach.OperationViewModel;
            if (e.PropertyName == visModel.GetPropertyName(() => visModel.Size) ||
                e.PropertyName == visModel.GetPropertyName(() => visModel.Position))
            {
                setMenuViewModelAnkerPosition();
            }
            else if (e.PropertyName == attach.GetPropertyName(() => attach.AnkerOffset))
            {
                setMenuViewModelAnkerPosition();
            }
        }

        public void Dispose()
        {
            if (_menuViewModel != null)
            {
                MainViewController.Instance.InkableScene.Remove(_menuView);
            }
        }
        
        private void setMenuViewModelAnkerPosition()
        {
            if (_menuViewModel != null)
            {
                AttachmentViewModel model = (DataContext as AttachmentViewModel);

                if (model.AttachmentOrientation == AttachmentOrientation.Left)
                {
                    _menuViewModel.AnkerPosition = new Pt(model.OperationViewModel.Position.X,
                        model.OperationViewModel.Position.Y) + model.AnkerOffset;
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.TopRight)
                {
                    _menuViewModel.AnkerPosition = new Pt(model.OperationViewModel.Position.X + model.OperationViewModel.Size.X,
                        model.OperationViewModel.Position.Y) + model.AnkerOffset;
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.Right)
                {
                    _menuViewModel.AnkerPosition = new Pt(model.OperationViewModel.Position.X + model.OperationViewModel.Size.X,
                        model.OperationViewModel.Position.Y) + model.AnkerOffset;
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.Top || model.AttachmentOrientation == AttachmentOrientation.TopStacked)
                {
                    _menuViewModel.AnkerPosition = new Pt(model.OperationViewModel.Position.X,
                        model.OperationViewModel.Position.Y) + model.AnkerOffset;
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.Bottom || model.AttachmentOrientation == AttachmentOrientation.TopStacked)
                {
                    _menuViewModel.AnkerPosition = new Pt(model.OperationViewModel.Position.X,
                        model.OperationViewModel.Position.Y + model.OperationViewModel.Size.Y) + model.AnkerOffset;
                }

                foreach (var menuItemViewModel in _menuViewModel.MenuItemViewModels.Where(m => m.IsHeightBoundToParent))
                {
                    menuItemViewModel.TargetSize =
                        menuItemViewModel.TargetSize = new Vec(menuItemViewModel.TargetSize.X, model.OperationViewModel.Size.Y)
                                                       + model.AnkerOffset;
                }
                foreach (var menuItemViewModel in _menuViewModel.MenuItemViewModels.Where(m => m.IsWidthBoundToParent))
                {
                    menuItemViewModel.TargetSize =
                        menuItemViewModel.TargetSize = new Vec(model.OperationViewModel.Size.X, menuItemViewModel.TargetSize.Y)
                                                       + model.AnkerOffset;
                }
            }
        }
    }

}
