using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using GeoAPI.Geometries;
using IDEA_common.catalog;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.data.progressive;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class AttributeMenuItemView : UserControl, AttributeViewModelEventHandler
    {
        private AttributeFieldView _shadow = null;
        private long _manipulationStartTime = 0;
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        public AttributeMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += AttributeMenuItemView_DataContextChanged;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }

        void AttributeMenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (args.NewValue as MenuItemViewModel).PropertyChanged += AttributeMenuItemView_PropertyChanged;
            }
        }

        private void AttributeMenuItemView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as MenuItemViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                if ((model.MenuItemComponentViewModel as AttributeMenuItemViewModel).TextAngle == 270)
                {
                    txtBlock.MaxWidth = model.Size.Y;
                }
                if ((model.MenuItemComponentViewModel as AttributeMenuItemViewModel).TextAngle == 0)
                {
                    txtBlock.MaxWidth = model.Size.X;
                }
            }
        }

        bool? _isHighlighted = false;
        DateTime _highlightStoryboardStart = DateTime.MinValue;
        DateTime _unhighlightStoryboardStart = DateTime.MinValue;
        void toggleHighlighted(bool isHighlighted)
        {
            if (isHighlighted == _isHighlighted)
                return;
            if (isHighlighted && _highlightStoryboardStart != DateTime.MinValue && _highlightStoryboardStart > _unhighlightStoryboardStart)
                return;
            if (!isHighlighted &&  _unhighlightStoryboardStart != DateTime.MinValue && _unhighlightStoryboardStart > _highlightStoryboardStart)
                return;
            var model = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);

            ExponentialEase easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            ColorAnimation backgroundAnimation = new ColorAnimation();
            backgroundAnimation.EasingFunction = easingFunction;
            backgroundAnimation.Duration = TimeSpan.FromMilliseconds(300);
            backgroundAnimation.From = (mainGrid.Background as SolidColorBrush).Color;

            if (isHighlighted)
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                txtBlock.Foreground = (Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush);
            }
            else
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush).Color;
                txtBlock.Foreground = model.TextBrush;
            }

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(backgroundAnimation);
            Storyboard.SetTarget(backgroundAnimation, mainGrid);
            Storyboard.SetTargetProperty(backgroundAnimation, "(Border.Background).(SolidColorBrush.Color)");
            //Storyboard.SetTargetProperty(foregroundAnimation, "(TextBlock.Foreground).Color");

            if (isHighlighted)
            {
                _isHighlighted = null;
                _highlightStoryboardStart = DateTime.Now;
                storyboard.Completed += (sender, args) => {
                    _highlightStoryboardStart = DateTime.MinValue;
                    _isHighlighted = _unhighlightStoryboardStart == null ? (bool?) true : null;
                };
            }
            else
            {
                _isHighlighted = null;
                _unhighlightStoryboardStart = DateTime.Now;
                storyboard.Completed += (sender, args) => {
                    _unhighlightStoryboardStart = DateTime.MinValue;
                    _isHighlighted = _highlightStoryboardStart == null ? (bool?)false : null;
                };
            }
            storyboard.Begin();
        }

        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            var model = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);
            if (model.CanDelete)
            {
                if (e.TriggeringPointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    var properties = e.StartContacts[e.TriggeringPointer.PointerId].Properties;
                    if (properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
                    {
                        ((MenuItemViewModel)DataContext).FireDeleted();
                        return;
                    }
                }
                return;
            }

            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                _manipulationStartTime = DateTime.Now.Ticks;
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            var model = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);

            if (e.NumActiveContacts == 1 && model.CanDrag)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = gt.TransformPoint(e.StartContacts[e.TriggeringPointer.PointerId].Position).GetVec() - currentPoint.GetVec();

                if (delta.Length > 10 && _shadow == null)
                {
                    createShadow(currentPoint, e);
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
                        model.AttributeViewModel.FireMoved(bounds, model.AttributeViewModel, e);
                    }
                }

                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            var model = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);
            if (!model.CanDrag)
            {
                return;
            }
            if (_shadow == null &&
                _manipulationStartTime + TimeSpan.FromSeconds(0.5).Ticks > DateTime.Now.Ticks)
            {
                var attrModel = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);
                if (attrModel != null && attrModel.TappedTriggered != null)
                {
                    attrModel.TappedTriggered(e);
                }
            }

            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;

                Rct bounds = _shadow.GetBounds(inkableScene);

                model.AttributeViewModel.FireDropped(bounds, model.AttributeViewModel, e);

                inkableScene.Remove(_shadow);
                _shadow = null;
            }

            _manipulationStartTime = 0;
        }

        public void createShadow(Point fromInkableScene, PointerManagerEvent e)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            var attributeViewModel = ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel).AttributeViewModel;

            if (inkableScene != null && attributeViewModel != null)
            {
                _currentFromInkableScene = fromInkableScene;
                _shadow = new AttributeFieldView();
                _shadow.DataContext = new AttributeViewModel(null, attributeViewModel.AttributeModel)
                {
                    IsNoChrome = false,
                    IsMenuEnabled = true,
                    IsShadow = true
                };

                _shadow.Measure(new Size(double.PositiveInfinity,
                                         double.PositiveInfinity));

                double add = attributeViewModel.IsNoChrome ? 30 : 0;
                //_shadow.Width = this.ActualWidth + add;
                //_shadow.Height = _shadow.DesiredSize.Height;

                _shadow.RenderTransform = new TranslateTransform()
                {
                    X = fromInkableScene.X - _shadow.Width / 2.0,
                    Y = fromInkableScene.Y - _shadow.Height
                };


                inkableScene.Add(_shadow);
                _shadow.SendToFront();
                
                attributeViewModel.FireMoved(_shadow.GetBounds(inkableScene), attributeViewModel, e);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public void AttributeViewModelMoved(AttributeViewModel sender,
            AttributeViewModelEventArgs e, bool overElement)
        {
            var model = (AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
            if (e.PointerArgs.Timestamp > _lastDropTime && model.CanDrop)
            {
                if (overElement)
                {
                    toggleHighlighted(true);

                }
                else if (!overElement)
                {
                    toggleHighlighted(false);
                }
            }
        }

        DateTime _lastDropTime = DateTime.MinValue;

        public void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            var model = (AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;

            if (model.CanDrop)
            {
                _lastDropTime = e.PointerArgs.Timestamp;
                toggleHighlighted(false);

                if (overElement)
                {
                    if (model != null && model.DroppedTriggered != null)
                    {
                        model.DroppedTriggered(sender);
                    }
                }
            }
        }

        public AttributeModel CurrentAttributeModel
        {
            get
            {
                return ((AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
            }
        }

        private void TextInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextInputBox.Text != "")
            {
                ChangeCodeRawName(TextInputBox.Text);
            }
            var model = (AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
            model.Editing = Visibility.Collapsed;
        }

        private void TextInputBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (TextInputBox.Text != "" && e.Key == Windows.System.VirtualKey.Enter)
                ChangeCodeRawName(TextInputBox.Text);
        }
        AttributeMenuItemViewModel ChangeCodeRawName(string newName)
        {
            var model = (AttributeMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
            var attr = AttributeTransformationModel.MatchesExistingField(newName, true);
            // if attribute label doesn't match any known attribute and this is a calculation operation, 
            // then set the name of the Calculation operation to the attribute label 
            if (attr == null && !IDEAAttributeModel.NameExists(newName, model.AttributeViewModel.AttributeModel.OriginModel))
            {
                var compModel = model.AttributeViewModel.OperationViewModel.OperationModel as ComputationalOperationModel;
                if (compModel != null)
                    compModel.RefactorFunctionName(newName);
            }

            return model;
        }
    }
}
