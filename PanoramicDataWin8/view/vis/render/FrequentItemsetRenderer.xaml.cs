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
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.common;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class FrequentItemsetRenderer : Renderer
    {
        public FrequentItemsetRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += FrequentItemsetRenderer_Loaded;
        }

        void FrequentItemsetRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            /*_classifierRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _classifierRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _classifierRendererContentProvider;*/
        }

        public override void Dispose()
        {
            base.Dispose();
            if (DataContext != null)
            {
                ((VisualizationViewModel)DataContext).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated -= QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated -= resultModel_ResultModelUpdated;
            }
        }


        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VisualizationViewModel model = ((VisualizationViewModel)DataContext);
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
            }
            if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                e.PropertyName == model.GetPropertyName(() => model.Position))
            {
            }
            mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskType.Replace("_", " ").ToString();
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((VisualizationViewModel)DataContext).PropertyChanged += VisualizationViewModel_PropertyChanged;
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated += resultModel_ResultModelUpdated;
                mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.VisualizationType.ToString();
                mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskType.Replace("_", " ").ToString();
            }
        }

        void resultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
        }


        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
            QueryModel queryModel = ((VisualizationViewModel)DataContext).QueryModel;
            mainLabel.Text = queryModel.TaskType.Replace("_", " ").ToString();
        }

        private void populateData()
        {
            ClassfierResultDescriptionModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel.ResultDescriptionModel as ClassfierResultDescriptionModel;
            /*if (resultModel != null)
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = mainGrid.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = tableGrid.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, tableGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                tableGrid.Opacity = 1;
                mainGrid.Opacity = 0;

               // loadResults(((VisualizationViewModel)DataContext).QueryModel.ResultModel);
               // render();
            }
            else
            {
                // animate between render canvas and label
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = mainGrid.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(300);
                animation.From = tableGrid.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, tableGrid);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();
                tableGrid.Opacity = 0;
                mainGrid.Opacity = 1;
            }*/
        }
    }
}
