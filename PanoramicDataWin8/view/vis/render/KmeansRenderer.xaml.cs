using PanoramicData.controller.data;
using PanoramicData.model.data;
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
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.PropertyChanged -= QueryResultModel_PropertyChanged;
            }
        }

        void KmeansRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.PropertyChanged += QueryResultModel_PropertyChanged;
                if (resultModel.QueryResultItemModels != null)
                {
                    resultModel.QueryResultItemModels.CollectionChanged += QueryResultItemModels_CollectionChanged;
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

        void QueryResultItemModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            populateData();
        }

        void QueryResultModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
            if (e.PropertyName == resultModel.GetPropertyName(() => resultModel.QueryResultItemModels))
            {
                resultModel.QueryResultItemModels.CollectionChanged += QueryResultItemModels_CollectionChanged;
                populateData();
            }
        }
        private void populateData()
        {
            QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
            if (resultModel.QueryResultItemModels.Count > 0)
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
                storyboard.Completed += (s, e) =>
                {
                    mainLabel.Visibility = Visibility.Collapsed;
                };

                animation = new DoubleAnimation();
                animation.From = renderCanvas.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, renderCanvas);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                _toLoad = resultModel.QueryResultItemModels.Count;
                _loaded = 0;
                _clusterCenters.Clear();
                _samples.Clear();
                foreach (var queryResultItemModel in resultModel.QueryResultItemModels)
                {
                    queryResultItemModel.PropertyChanged += queryResultItemModel_PropertyChanged;
                }

            }
        }

        void queryResultItemModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Data")
            {
                var dataWrapper = sender as DataWrapper<QueryResultItemModel>;
                if (!dataWrapper.IsLoading && dataWrapper.Data != null)
                {
                    loadQueryResultItemModel(dataWrapper.Data);
                }
            }
        }

        void loadQueryResultItemModel(QueryResultItemModel queryResultItemModel)
        {
            if (queryResultItemModel.JobResultValues.ContainsKey(JobTypeResult.ClusterX))
            {
                Vec cluster = new Vec(
                    double.Parse(queryResultItemModel.JobResultValues[JobTypeResult.ClusterX].Value.ToString()),
                    double.Parse(queryResultItemModel.JobResultValues[JobTypeResult.ClusterY].Value.ToString()));
                _clusterCenters.Add(cluster);
            }
            else if (queryResultItemModel.JobResultValues.ContainsKey(JobTypeResult.SampleX))
            {
                Vec sample = new Vec(
                    double.Parse(queryResultItemModel.JobResultValues[JobTypeResult.SampleX].Value.ToString()),
                    double.Parse(queryResultItemModel.JobResultValues[JobTypeResult.SampleY].Value.ToString()));
                _samples.Add(sample);
            }
            _loaded++;
            if (_toLoad == _loaded)
            {
                render();
            }
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
