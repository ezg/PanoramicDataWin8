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
using WinRTXamlToolkit.Tools;

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
                    vis.HistogramOperationModel.PropertyChanged -= QueryModel_PropertyChanged;
                }
            }
            if (args.NewValue != null)
            {
                _model = (ComparisonViewModel)args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                foreach (var vis in _model.VisualizationViewModels)
                {
                    vis.PropertyChanged += VisModel_PropertyChanged;
                    vis.HistogramOperationModel.PropertyChanged += QueryModel_PropertyChanged;
                }
                updateRendering();
                updateResult();
            }
        }

        private void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel model = (DataContext as HistogramOperationViewModel).HistogramOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                updateResult();
            }
        }
        
        private void updateResult()
        {
            /*var res1 = _model.OperationViewModels[0].OperationModel.Result.ResultDescriptionModel as VisualizationResultDescriptionModel;
            var res2 = _model.OperationViewModels[1].OperationModel.Result.ResultDescriptionModel as VisualizationResultDescriptionModel;

            if (res1 != null && res2 != null)
            {
                var dim = _model.OperationViewModels[0].OperationModel.GetUsageAttributeTransformationModel(InputUsage.X).First();
                if (_model.OperationViewModels[1].OperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Any(i => i.AttributeModel == dim.AttributeModel))
                {
                    var n1 = res1.OverallCount[dim.AttributeModel.RawName];
                    var n2 = res2.OverallCount[dim.AttributeModel.RawName];
                    var m1 = res1.OverallMeans[dim.AttributeModel.RawName];
                    var m2 = res2.OverallMeans[dim.AttributeModel.RawName];
                    var v1 = Math.Sqrt(res1.OverallSampleStandardDeviations[dim.AttributeModel.RawName]);
                    var v2 = Math.Sqrt(res2.OverallSampleStandardDeviations[dim.AttributeModel.RawName]);

                    var df = Math.Min(n1, n2);
                    var t = (m1 - m2)/Math.Sqrt((v1/n1) + (v2/n2));
                    var p = tToP(t, df);

                    var r = Math.Sqrt((t * t) / ((t * t) + (df * 1)));
                    var d = (t * 2) / (Math.Sqrt(df));

                    if (p < 0.001)
                    {
                        tbPValue.Text = "p < 0.001";
                    }
                    else if (p < 0.01)
                    {
                        tbPValue.Text = "p < 0.01";
                    }
                    else if (p < 0.05)
                    {
                        tbPValue.Text = "p < 0.05";
                    }
                    else if (p >= 0.05)
                    {
                        tbPValue.Text = "p > 0.05";
                    }
                    tbPValue.FontSize = 16;

                    tbDValue.Text = "d = " + Math.Abs(d).ToString("F2");
                    tbDValue.FontSize = 16;
                }
            }*/
        }

        private double tToP(double t, double df)
        {
            var abst = Math.Abs(t);
            var tsq = t*t;
            var p = 0.0;
            if (df == 1)
            {
                p = 1 - 2.0*Math.Atan(abst)/Math.PI;
            }
            else if (df == 2)
                p = 1 - abst/Math.Sqrt(tsq + 2);
            else if (df == 3)
                p = 1 - 2*(Math.Atan(abst/Math.Sqrt(3)) + abst*Math.Sqrt(3)/(tsq + 3))/Math.PI;
            else if (df == 4)
                p = 1 - abst*(1 + 2/(tsq + 4))/Math.Sqrt(tsq + 4);
            else
            {
                var z = tToZ(abst, df);
                if (df > 4)
                    p = normP(z);
            }
            return p;
        }

        private double tToZ(double t, double df)
        {
            var A9 = df - 0.5;
            var B9 = 48*A9*A9;
            var T9 = t*t/df;
            var Z8 = 0.0;
            var P7 = 0.0;
            var B7 = 0.0;
            var z = 0.0;

            if (T9 >= 0.04)
                Z8 = A9*Math.Log(1 + T9);
            else
            {
                Z8 = A9*(((1 - T9*0.75)*T9/3 - 0.5)*T9 + 1)*T9;
            }
            P7 = ((0.4 * Z8 + 3.3) * Z8 + 24) * Z8 + 85.5;
            B7 = 0.8 * Math.Pow(Z8, 2) + 100 + B9;
            z = (1 + (-P7 / B7 + Z8 + 3) / B9) * Math.Sqrt(Z8);
            return z;
        }

        private double normP(double z)
        {
            var absz = Math.Abs(z);
            var a1 = 0.0000053830;
            var a2 = 0.0000488906;
            var a3 = 0.0000380036;
            var a4 = 0.0032776263;
            var a5 = 0.0211410061;
            var a6 = 0.0498673470;
            var p = (((((a1*absz + a2)*absz + a3)*absz + a4)*absz + a5)*absz + a6)*absz + 1;
            p = Math.Pow(p, -16);
            return p;
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

            _model.Size = new Vec(100,100);
            var size = 100;
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
