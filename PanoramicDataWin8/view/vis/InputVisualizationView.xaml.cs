using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class InputVisualizationView : UserControl
    {
        private InputVisualizationViewModel _model = null;
        private Storyboard _pulsingOpeningStoryboard = null;
        private Storyboard _closingStoryboard = null;

        public InputVisualizationView()
        {
            this.InitializeComponent();
            this.DataContextChanged += InputVisualizationView_DataContextChanged;
            this.Loaded += SetOperationView_Loaded;
        }

        void SetOperationView_Loaded(object sender, RoutedEventArgs e)
        {
            CubicEase easingFunction = new CubicEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(1000));
            _pulsingOpeningStoryboard = new Storyboard();

            DoubleAnimation animation = new DoubleAnimation();
            animation.EnableDependentAnimation = true;
            animation.Duration = duration;
            animation.From = 0;
            animation.To = 1;
            animation.EasingFunction = easingFunction;
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, "Opacity");
            _pulsingOpeningStoryboard.Children.Add(animation);

            _pulsingOpeningStoryboard.Begin();
        }

        void InputVisualizationView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                foreach (var vis in _model.VisualizationViewModels)
                {
                    vis.PropertyChanged -= VisModel_PropertyChanged;
                }
            }
            if (args.NewValue != null)
            {
                _model = (InputVisualizationViewModel)args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                foreach (var vis in _model.VisualizationViewModels)
                {
                    vis.PropertyChanged += VisModel_PropertyChanged;
                }
                updateRendering();
            }
        }

        private void VisModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }
        

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.InputVisualizationViewModelState))
            {
                if (_model.InputVisualizationViewModelState == InputVisualizationViewModelState.Closing)
                {
                    if (_closingStoryboard != null)
                    {
                        _closingStoryboard.Stop();
                    }
                    _pulsingOpeningStoryboard.Stop();

                    CubicEase easingFunction = new CubicEase();
                    easingFunction.EasingMode = EasingMode.EaseInOut;
                    _closingStoryboard = new Storyboard();

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.EnableDependentAnimation = true;
                    animation.Duration = new Duration(TimeSpan.FromMilliseconds(400));
                    animation.From = this.Opacity;
                    animation.To = 0;
                    animation.EasingFunction = easingFunction;
                    Storyboard.SetTarget(animation, this);
                    Storyboard.SetTargetProperty(animation, "Opacity");
                    _closingStoryboard.Children.Add(animation);

                    _closingStoryboard.Begin();
                }
                else if (_model.InputVisualizationViewModelState == InputVisualizationViewModelState.Opened)
                {
                    /* CubicEase easingFunction = new CubicEase();
                     easingFunction.EasingMode = EasingMode.EaseInOut;
                     Duration duration = new Duration(TimeSpan.FromMilliseconds(400));
                     Storyboard storyboard = new Storyboard();

                     DoubleAnimation animation = new DoubleAnimation();
                     animation.EnableDependentAnimation = true;
                     animation.Duration = duration;
                     animation.From = fullView.Opacity;
                     animation.To = 1;
                     animation.EasingFunction = easingFunction;
                     Storyboard.SetTarget(animation, fullView);
                     Storyboard.SetTargetProperty(animation, "Opacity");
                     storyboard.Children.Add(animation);

                     animation = new DoubleAnimation();
                     animation.EnableDependentAnimation = true;
                     animation.Duration = duration;
                     animation.From = ellipse.Opacity;
                     animation.To = 0;
                     animation.EasingFunction = easingFunction;
                     Storyboard.SetTarget(animation, ellipse);
                     Storyboard.SetTargetProperty(animation, "Opacity");
                     storyboard.Children.Add(animation);

                     storyboard.Begin();*/
                }
            }
            updateRendering();
        }



        private void updateRendering()
        {
            this.SendToFront();

            var left = _model.VisualizationViewModels[0];
            var right = _model.VisualizationViewModels[1];

            if (left.Bounds.Left > right.Bounds.Left)
            {
                var temp = right;
                right = left;
                left = temp;
            }

            var lineFrom = (new Pt(left.Bounds.Right, left.Bounds.Center.Y) - _model.Position).GetWindowsPoint();
            var lineTo = (new Pt(right.Bounds.Left, right.Bounds.Center.Y) - _model.Position).GetWindowsPoint();

            line.X1 = lineFrom.X;
            line.Y1 = lineFrom.Y;
            line.X2 = lineTo.X;
            line.Y2 = lineTo.Y;
            line.Visibility = Visibility.Collapsed;

            var gap = 54;
            if (left == _model.From)
            {
                brushRectangle.Width = 10;
                brushRectangle.Fill = new SolidColorBrush(_model.Color);
                brushRectangle.Height = (left.Bounds.Bottom - left.Bounds.Top - gap);
                brushRectangle.RenderTransform = new TranslateTransform() { X = left.Bounds.Right - _model.Position.X, Y = left.Bounds.Top - _model.Position.Y + gap };
            }
            else if (right == _model.From)
            {
                brushRectangle.Width = 10;
                brushRectangle.Fill = new SolidColorBrush(_model.Color);
                brushRectangle.Height = (right.Bounds.Bottom - right.Bounds.Top - gap);
                brushRectangle.RenderTransform = new TranslateTransform() { X = right.Bounds.Left - brushRectangle.Width - _model.Position.X, Y = right.Bounds.Top - _model.Position.Y + gap };
            }


            _model.Position =
                (((left.Bounds.Center.GetVec() + new Vec(left.Size.X / 2.0, 0)) +
                  (right.Bounds.Center.GetVec() - new Vec(right.Size.X / 2.0, 0))) / 2.0 - _model.Size / 2.0).GetWindowsPoint();
        }

    }
}
