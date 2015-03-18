using PanoramicData.controller.data;
using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicDataWin8.view.common;
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
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class BarRenderer : Renderer
    {
        private BarRendererContentProvider _barRendererContentProvider;

        private List<Vec> _clusterCenters = new List<Vec>();
        private List<Vec> _samples = new List<Vec>();

        public BarRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += BarRenderer_DataContextChanged;
            this.Loaded += BarRenderer_Loaded;
        }

        void BarRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            _barRendererContentProvider = new BarRendererContentProvider();
            dxSurface.ContentProvider = _barRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (DataContext != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.QueryResultModelUpdated -= resultModel_QueryResultModelUpdated;
            }
            dxSurface.Dispose();
        }

        void BarRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                QueryResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.QueryResultModel;
                resultModel.QueryResultModelUpdated += resultModel_QueryResultModelUpdated;
            }
        }

        void resultModel_QueryResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                render();
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

                animation = new DoubleAnimation();
                animation.From = dxSurface.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurface);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                _clusterCenters.Clear();
                _samples.Clear();

                loadQueryResultItemModels(resultModel);
                render();
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
                animation.From = dxSurface.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, dxSurface);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();
            }
        }

        void loadQueryResultItemModels(QueryResultModel resultModel)
        {
            foreach (var queryResultItemModel in resultModel.QueryResultItemModels)
            {
                 
            }

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
            //_loaded++;
            //if (_toLoad == _loaded)
            {
                render();
            }
        }

        void render()
        {
            dxSurface.Redraw();
        }
    }

    public class BarRendererContentProvider : DXSurfaceContentProvider
    {
        public override void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(230, 230, 230));
        }

        public override void Draw(D2D.DeviceContext d2dDeviceContext)
        {
           
        }
        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
        }
    }
}
