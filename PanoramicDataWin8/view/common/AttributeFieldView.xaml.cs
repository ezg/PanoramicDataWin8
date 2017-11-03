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
using Windows.UI;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using System.Diagnostics;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Animation;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class AttributeFieldView : UserControl
    {
        public delegate void InputFieldViewModelTappedHandler(object sender, EventArgs e);
        public static event InputFieldViewModelTappedHandler InputFieldViewModelTapped;

        private AttributeFieldView _shadow = null;
        private long _manipulationStartTime = 0;
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();
    
        public AttributeFieldView()
        {
            this.InitializeComponent();
            this.DataContextChanged += InputFieldView_DataContextChanged;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }

        void InputFieldView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                if (args.NewValue is AttributeViewModel)
                {
                    (args.NewValue as AttributeViewModel).PropertyChanged += InputFieldView_PropertyChanged;
                }
                if (args.NewValue is AttributeTransformationViewModel)
                {
                    (args.NewValue as AttributeTransformationViewModel).PropertyChanged += InputFieldView_PropertyChanged;
                }
            }
            updateRendering();
        }

        void InputFieldView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void updateRendering()
        {
            var model = DataContext as AttributeViewModel;
            txtBlock.Inlines.Clear();
            Run r = new Run { Text = model.MainLabel };
            if (model.AttributeModel.IsTarget)
            {
                Underline ul = new Underline();
                ul.Inlines.Add(r);
                txtBlock.Inlines.Add(ul);
            }
            else
            {
                txtBlock.Inlines.Add(r);
            }


            if (model.IsShadow)
            {
                mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                border.BorderThickness = new Thickness(4);
            }
            else
            {
                mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                border.BorderThickness = model.BorderThicknes;
            }

            if (model.TextAngle == 0)
            {
                txtBlock.MaxWidth = model.Size.X;
            }
            else
            {
                txtBlock.MaxWidth = model.Size.Y;
            }

            toggleHighlighted(model.IsHighlighted);

        }

        void toggleHighlighted(bool isHighlighted)
        {
            AttributeViewModel model = DataContext as AttributeViewModel;

            ExponentialEase easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            ColorAnimation backgroundAnimation = new ColorAnimation();
            backgroundAnimation.EasingFunction = easingFunction;
            backgroundAnimation.Duration = TimeSpan.FromMilliseconds(300);
            backgroundAnimation.From = (mainGrid.Background as SolidColorBrush).Color;

            if (isHighlighted)
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                txtBlock.Foreground = model.HighlightBrush;
            }
            else
            {
                backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush).Color;
                txtBlock.Foreground = model.NormalBrush;
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
                        (DataContext as AttributeViewModel).FireMoved(bounds, (DataContext as AttributeViewModel).AttributeModel);
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
                if ((DataContext as AttributeTransformationViewModel).IsMenuEnabled && InputFieldViewModelTapped != null)
                {
                    Debug.WriteLine("--TAPP");
                    InputFieldViewModelTapped(this, new EventArgs());
                }
            }

            if (_shadow != null)
            {
                InkableScene inkableScene = MainViewController.Instance.InkableScene;

                Rct bounds = _shadow.GetBounds(inkableScene);
                if (DataContext is AttributeTransformationViewModel)
                    (DataContext as AttributeTransformationViewModel).FireDropped(bounds, // (DataContext as AttributeTransformationViewModel).AttributeTransformationModel
                        new AttributeTransformationModel((DataContext as AttributeTransformationViewModel).AttributeModel)
                        {
                            AggregateFunction = (DataContext as AttributeTransformationViewModel).AttributeTransformationModel.AggregateFunction
                        });
                else (DataContext as AttributeViewModel).FireDropped(bounds, (DataContext as AttributeViewModel).AttributeModel);

                inkableScene.Remove(_shadow);
                _shadow = null;
            }

            _manipulationStartTime = 0;
        }

        public void createShadow(Point fromInkableScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            if (inkableScene != null && DataContext != null && (DataContext as AttributeViewModel).AttributeModel != null)
            {
                _currentFromInkableScene = fromInkableScene;
                _shadow = new AttributeFieldView();
                var shadowAttributeViewModel = (DataContext as AttributeViewModel).Clone();
                shadowAttributeViewModel.IsNoChrome = false;
                shadowAttributeViewModel.IsMenuEnabled = true;
                shadowAttributeViewModel.IsShadow = true;
                _shadow.DataContext = shadowAttributeViewModel;
                _shadow.Measure(new Size(double.PositiveInfinity,
                                         double.PositiveInfinity));

                double add = (DataContext as AttributeViewModel).IsNoChrome ? 30 : 0;
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
                (DataContext as AttributeViewModel).FireMoved(bounds, (DataContext as AttributeViewModel).AttributeModel);
            }
        }
    }
}
