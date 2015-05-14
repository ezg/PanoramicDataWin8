using PanoramicData.controller.data;
using PanoramicData.model.data;
using PanoramicData.model.data.result;
using PanoramicData.model.view;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class KmeansRenderer : Renderer
    {
        private int _toLoad = 0;
        private int _loaded = 0;

        private List<Vec> _clusterCenters = new List<Vec>();
        private List<Vec> _samples = new List<Vec>();

        public KmeansRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += KmeansRenderer_DataContextChanged;
        }
        public override void Dispose()
        {
            base.Dispose();
            if (DataContext != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
                resultModel.PropertyChanged -= ResultModel_PropertyChanged;
            }
        }

        void KmeansRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
                resultModel.PropertyChanged += ResultModel_PropertyChanged;
                if (resultModel.ResultItemModels != null)
                {
                    resultModel.ResultItemModels.CollectionChanged += ResultItemModels_CollectionChanged;
                    populateData();
                }
            }
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                render();
            }
        }

        void ResultItemModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            populateData();
        }

        void ResultModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
            if (e.PropertyName == resultModel.GetPropertyName(() => resultModel.ResultItemModels))
            {
                resultModel.ResultItemModels.CollectionChanged += ResultItemModels_CollectionChanged;
                populateData();
            }
        }


        private void populateData()
        {
            ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
            if (resultModel.ResultItemModels.Count > 0)
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = mainLabel.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.From = renderCanvas.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, renderCanvas);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                _toLoad = resultModel.ResultItemModels.Count;
                _loaded = 0;
                _clusterCenters.Clear();
                _samples.Clear();
                foreach (var resultItemModel in resultModel.ResultItemModels)
                {
                    resultItemModel.PropertyChanged += resultItemModel_PropertyChanged;
                }
            }
            else
            {
                // animate between render canvas and label
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = mainLabel.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainLabel);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.From = renderCanvas.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, renderCanvas);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();
            }
        }

        void resultItemModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Data")
            {
                var dataWrapper = sender as DataWrapper<ResultItemModel>;
                if (!dataWrapper.IsLoading && dataWrapper.Data != null)
                {
                    loadResultItemModel(dataWrapper.Data);
                }
            }
        }

        void loadResultItemModel(ResultItemModel resultItemModel)
        {
            /*if (resultItemModel.JobResultValues.ContainsKey(JobResult.ClusterX))
            {
                Vec cluster = new Vec(
                    double.Parse(resultItemModel.JobResultValues[JobResult.ClusterX].Value.ToString()),
                    double.Parse(resultItemModel.JobResultValues[JobResult.ClusterY].Value.ToString()));
                _clusterCenters.Add(cluster);
            }
            else if (resultItemModel.JobResultValues.ContainsKey(JobResult.SampleX))
            {
                Vec sample = new Vec(
                    double.Parse(resultItemModel.JobResultValues[JobResult.SampleX].Value.ToString()),
                    double.Parse(resultItemModel.JobResultValues[JobResult.SampleY].Value.ToString()));
                _samples.Add(sample);
            }
            _loaded++;
            if (_toLoad == _loaded)
            {
                render();
            }*/
        }

        void render()
        {
            double minX = Math.Min(_clusterCenters.Min(v => v.X), _samples.Min(v => v.X));
            double minY = Math.Min(_clusterCenters.Min(v => v.Y), _samples.Min(v => v.Y));
            double maxX = Math.Max(_clusterCenters.Max(v => v.X), _samples.Max(v => v.X));
            double maxY = Math.Max(_clusterCenters.Max(v => v.Y), _samples.Max(v => v.Y));
            

            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            double xOffset = model.Size.X * 0.15;
            double yOffset = model.Size.Y * 0.15;
            double scaleX = (model.Size.X * 0.7) / (maxX - minX);
            double scaleY = (model.Size.Y * 0.7) / (maxY - minY);

            renderCanvas.Children.Clear();

            foreach (var c in _clusterCenters)
            {
                Rectangle r = new Rectangle();
                r.Fill = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush;
                r.Width = r.Height = 10;
                r.RenderTransform = new TranslateTransform()
                {
                    X = c.X * scaleX + xOffset - 5,
                    Y = c.Y * scaleY + yOffset - 5
                };
                renderCanvas.Children.Add(r);
            }
            foreach (var s in _samples)
            {
                Rectangle r = new Rectangle();
                r.Fill = Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush;
                r.Width = r.Height = 10;
                r.RenderTransform = new TranslateTransform()
                {
                    X = s.X * scaleX + xOffset - 5,
                    Y = s.Y * scaleY + yOffset - 5
                };
                renderCanvas.Children.Add(r);
            }
        }
    }
}
