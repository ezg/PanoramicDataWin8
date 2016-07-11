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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class ComparisonView : UserControl
    {
        private ComparisonViewModel _model = null;
        private Storyboard _pulsingOpeningStoryboard = null;
        private Storyboard _closingStoryboard = null;

        public ComparisonView()
        {
            this.InitializeComponent();
            this.DataContextChanged += ComparisonViewModel_DataContextChanged;
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

        void ComparisonViewModel_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                foreach (var vis in _model.VisualizationViewModels)
                {
                    vis.PropertyChanged -= VisModel_PropertyChanged;
                    vis.QueryModel.ResultModel.ResultModelUpdated -= ResultModel_ResultModelUpdated;
                }
            }
            if (args.NewValue != null)
            {
                _model = (ComparisonViewModel)args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                foreach (var vis in _model.VisualizationViewModels)
                {
                    vis.PropertyChanged += VisModel_PropertyChanged;
                    vis.QueryModel.ResultModel.ResultModelUpdated += ResultModel_ResultModelUpdated;
                }
                updateRendering();
                updateResult();
            }
        }

        private void ResultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            updateResult();
        }

        private void updateResult()
        {
            var res1 = _model.VisualizationViewModels[0].QueryModel.ResultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;
            var res2 = _model.VisualizationViewModels[1].QueryModel.ResultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;

            if (res1 != null && res2 != null)
            {
                var dim = _model.VisualizationViewModels[0].QueryModel.GetUsageInputOperationModel(InputUsage.X).First();
                var n1 = res1.OverallCount[dim.InputModel.Name];
                var n2 = res2.OverallCount[dim.InputModel.Name];
                var m1 = res1.OverallMeans[dim.InputModel.Name];
                var m2 = res2.OverallMeans[dim.InputModel.Name];
                var v1 = Math.Sqrt(res1.OverallSampleStandardDeviations[dim.InputModel.Name]);
                var v2 = Math.Sqrt(res2.OverallSampleStandardDeviations[dim.InputModel.Name]);

                var t = (m1 - m2)/Math.Sqrt((v1/n1) + (v2/n2));
            }
        }

        private void VisModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }


        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.ComparisonViewModelState))
            {
                if (_model.ComparisonViewModelState == ComparisonViewModelState.Closing)
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
                else if (_model.ComparisonViewModelState == ComparisonViewModelState.Opened)
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

            _model.Size = new Vec(80,80);
            var size = 80;
            brushRectangle.Width = size;
            brushRectangle.Height = size;
            brushRectangle.Fill = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
            brushRectangle.RenderTransform = new TranslateTransform() { X = 0 };
            
            _model.Position =
                (((left.Bounds.Center.GetVec() + new Vec(left.Size.X / 2.0, 0)) +
                  (right.Bounds.Center.GetVec() - new Vec(right.Size.X / 2.0, 0))) / 2.0 - _model.Size / 2.0).GetWindowsPoint();
        }

    }
}
