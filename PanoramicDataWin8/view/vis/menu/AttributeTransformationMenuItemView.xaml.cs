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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class AttributeTransformationMenuItemView : UserControl, AttributeTransformationViewModelEventHandler
    {
        private AttributeFieldView _shadow = null;
        private long _manipulationStartTime = 0;
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        public AttributeTransformationMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += AttributeTransformationMenuItemView_DataContextChanged;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }
        
        void AttributeTransformationMenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (args.NewValue as MenuItemViewModel).PropertyChanged += AttributeTransformationMenuItemView_PropertyChanged;
            }
        }

        private void AttributeTransformationMenuItemView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as MenuItemViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                if ((model.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel).TextAngle == 270)
                {
                    txtBlock.MaxWidth = model.Size.Y;
                }
                if ((model.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel).TextAngle == 0)
                {
                    txtBlock.MaxWidth = model.Size.X;
                }
            }
        }

        private bool _isHighlighted = false;
        void toggleHighlighted(bool isHighlighted)
        {
            var model = ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);

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

            storyboard.Begin();
        }


        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            var model = ((AttributeTransformationMenuItemViewModel) ((MenuItemViewModel) DataContext).MenuItemComponentViewModel);
            if (!model.CanDrag)
            {
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
            var model = ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);

            if (e.NumActiveContacts == 1 && model.CanDrag)
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
                        model.AttributeTransformationViewModel.FireMoved(bounds, model.AttributeTransformationViewModel.AttributeTransformationModel);
                    }
                }

                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            var model = ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel);
            if (!model.CanDrag)
            {
                return;
            }
            if (_shadow == null &&
                _manipulationStartTime + TimeSpan.FromSeconds(0.5).Ticks > DateTime.Now.Ticks)
            {
                var attrModel =
                ((AttributeTransformationMenuItemViewModel)
                    ((MenuItemViewModel) DataContext).MenuItemComponentViewModel);
                if (attrModel != null && attrModel.TappedTriggered != null)
                {
                    attrModel.TappedTriggered();
                }
            }

            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;

                Rct bounds = _shadow.GetBounds(inkableScene);
                
                model.AttributeTransformationViewModel.FireDropped(bounds,
                    new AttributeTransformationModel(model.AttributeTransformationViewModel.AttributeTransformationModel.AttributeModel)
                    {
                        AggregateFunction = model.AttributeTransformationViewModel.AttributeTransformationModel.AggregateFunction
                    });

                inkableScene.Remove(_shadow);
                _shadow = null;
            }

            _manipulationStartTime = 0;
        }

        public void createShadow(Point fromInkableScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            var model = ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel).AttributeTransformationViewModel;

            if (inkableScene != null && model != null)
            {
                _currentFromInkableScene = fromInkableScene;
                _shadow = new AttributeFieldView();
                _shadow.DataContext = new AttributeTransformationViewModel(null, model.AttributeTransformationModel)
                {
                    IsNoChrome = false,
                    IsMenuEnabled = true,
                    IsShadow = true
                };

                _shadow.Measure(new Size(double.PositiveInfinity,
                                         double.PositiveInfinity));

                double add = model.IsNoChrome ? 30 : 0;
                //_shadow.Width = this.ActualWidth + add;
                //_shadow.Height = _shadow.DesiredSize.Height;

                _shadow.RenderTransform = new TranslateTransform()
                {
                    X = fromInkableScene.X - _shadow.Width / 2.0,
                    Y = fromInkableScene.Y - _shadow.Height
                };


                inkableScene.Add(_shadow);
                _shadow.SendToFront();

                Rct bounds = _shadow.GetBounds(inkableScene);
                model.FireMoved(bounds, model.AttributeTransformationModel);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender,
            AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            var model = (AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;

            if (model.CanDrop)
            {
                if (overElement && !_isHighlighted)
                {
                    toggleHighlighted(true);
                    _isHighlighted = true;

                }
                else if (!overElement && _isHighlighted)
                {
                    toggleHighlighted(false);
                    _isHighlighted = false;
                }
            }
        }

        public void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender,
            AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            var model = (AttributeTransformationMenuItemViewModel) ((MenuItemViewModel) DataContext).MenuItemComponentViewModel;

            if (model.CanDrop) { 
                if (_isHighlighted)
                {
                    toggleHighlighted(false);
                    _isHighlighted = false;
                }

                if (overElement)
                {
                    if (model != null && model.DroppedTriggered != null)
                    {
                        model.DroppedTriggered(new AttributeTransformationModel(e.AttributeTransformationModel.AttributeModel));
                    }
                }
            }
        }

        public AttributeTransformationModel CurrentAttributeTransformationModel
        {
            get
            {
                var model = (AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
                return model?.AttributeTransformationViewModel?.AttributeTransformationModel;
            }
        }

        private void TextInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var newName = TextInputBox.Text;
            var model = (AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
            var attr = AttributeTransformationModel.MatchesExistingField(newName, true);
            // if attribute label doesn't match any known attribute and this is a calculation operation, 
            // then set the name of the Calculation operation to the attribute label 
            if (attr == null && !IDEAAttributeComputedFieldModel.NameExists(newName)) {
                (model.AttributeTransformationViewModel.OperationViewModel.OperationModel as CalculationOperationModel)?.SetRawName(model.Label);
                (model.AttributeTransformationViewModel.OperationViewModel.OperationModel as DefinitionOperationModel)?.SetRawName(model.Label);
                (model.AttributeTransformationViewModel.OperationViewModel.OperationModel as PredictorOperationModel)?.SetRawName(model.Label);
            }

            model.Editing = Visibility.Collapsed;
        }

        private void TextInputBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var model = (AttributeTransformationMenuItemViewModel)((MenuItemViewModel)DataContext).MenuItemComponentViewModel;
            var attr = AttributeTransformationModel.MatchesExistingField(TextInputBox.Text, true);
            if (attr == null)
            {
                model.Label = TextInputBox.Text;
            }
        }
    }
}
