using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
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
            HeaderObjects = new ObservableCollection<HeaderObject>();
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
                updateSize(null);
            }
            if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                e.PropertyName == model.GetPropertyName(() => model.Position))
            {
            }
            mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskModel.Name.Replace("_", " ").ToString();
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
                mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskModel.Name.Replace("_", " ").ToString();
            }
        }

        void resultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
        }


        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
            QueryModel queryModel = ((VisualizationViewModel)DataContext).QueryModel;
            mainLabel.Text = queryModel.TaskModel.Name.Replace("_", " ").ToString();
        }

        private void populateData()
        {
            ResultModel resultModel = ((VisualizationViewModel) DataContext).QueryModel.ResultModel;
            
            if (resultModel != null && resultModel.ResultItemModels.Count > 0)
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
               render();
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
            }
        }

        public ObservableCollection<HeaderObject> HeaderObjects { get; set; }
        private void render()
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            List<InputOperationModel> inputOperationModels = new List<InputOperationModel>();

            //InputModel pattern = new TuppleWareFieldInputModel("pattern", InputDataTypeConstants.NVARCHAR, InputVisualizationTypeConstants.ENUM) { OriginModel = (model.QueryModel.SchemaModel.OriginModels[0] as TuppleWareOriginModel) };
            //InputModel minsupport = new TuppleWareFieldInputModel("support", InputDataTypeConstants.NVARCHAR, InputVisualizationTypeConstants.ENUM) { OriginModel = (model.QueryModel.SchemaModel.OriginModels[0] as TuppleWareOriginModel) };
            
           // inputOperationModels.Add(new InputOperationModel(pattern));
            //inputOperationModels.Add(new InputOperationModel(minsupport));

            List<HeaderObject> headerObjects = new List<HeaderObject>();

            foreach (var inputOperationModel in inputOperationModels)
            {
                HeaderObject ho = new HeaderObject();
                ho.InputFieldViewModel = new InputFieldViewModel(model, inputOperationModel)
                {
                    AttachmentOrientation = AttachmentOrientation.Top, 
                    IsDraggable = false,
                    IsDraggableByPen = false,
                };
                ho.Width = 100;
                if (HeaderObjects.Any(hoo => hoo.InputFieldViewModel != null && hoo.InputFieldViewModel.InputOperationModel == ho.InputFieldViewModel.InputOperationModel))
                {
                    var oldHo = HeaderObjects.First(hoo => hoo.InputFieldViewModel.InputOperationModel == ho.InputFieldViewModel.InputOperationModel);
                    ho.Width = oldHo.Width;
                }
                headerObjects.Add(ho);
            }
            if (headerObjects.Count > 0)
            {
                headerObjects.First().IsFirst = true;
                headerObjects.Last().IsLast = true;
            }
            headerItemsControl.ItemsSource = headerObjects;

            // header template
            StringBuilder sb = new StringBuilder();
            sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" >");
            sb.Append("<Grid>");
            //sb.Append("<Border >");
            sb.Append(" <StackPanel Orientation=\"Horizontal\">");
            sb.Append("     <local:DataGridResizer Margin=\"0,0,0,0\" Width=\"4\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/> ");
            sb.Append("     <Grid Width=\"{Binding Width}\">");
            sb.Append("         <local:InputFieldView DataContext=\"{Binding InputFieldViewModel}\" Width=\"{Binding Width}\" RawName=\"inputFieldView\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
            sb.Append("     </Grid>");
            sb.Append("     <local:DataGridResizer Width=\"4\" xmlns:local=\"using:PanoramicDataWin8.view.common\" IsResizer=\"True\"/> ");
            sb.Append(" </StackPanel>");
            //sb.Append("</Border>");
            sb.Append("</Grid>");
            sb.Append("</DataTemplate>");
            DataTemplate datatemplate = (DataTemplate)XamlReader.Load(sb.ToString());
            headerItemsControl.ItemTemplate = datatemplate;

            foreach (var ho in HeaderObjects)
            {
                ho.Resized -= headerObject_Resized;
            }
            HeaderObjects.Clear();

            foreach (var ho in headerObjects)
            {
                ho.Resized += headerObject_Resized;
                ho.NrElements = headerObjects.Count;
                HeaderObjects.Add(ho);
            }
            updateSize(null);

            // list view template
            sb = new StringBuilder();
            sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" >");
            sb.Append("<StackPanel Orientation=\"Horizontal\" Background=\"{StaticResource lightBrush}\">");
            int count = 0;
            foreach (var inputOperationModel in inputOperationModels)
            {
                sb.Append("     <local:DataGridCell HeaderObject=\"{Binding ElementName=_this, Path=HeaderObjects[" + count + "]}\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
                //sb.Append("     <TextBlock Text=\"asdfasfds}\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
                count++;
            }


            sb.Append("</StackPanel>");
            sb.Append("</DataTemplate>");
            datatemplate = (DataTemplate)XamlReader.Load(sb.ToString());
            listView.ItemTemplate = datatemplate;

            listView.ItemsSource = model.QueryModel.ResultModel.ResultItemModels;
        }

        void headerObject_Resized(object sender, EventArgs e)
        {
            updateSize(sender as HeaderObject);
        }

        private void updateSize(HeaderObject exclude)
        {
            double totalHeaderWidth = HeaderObjects.Sum(ho => ho.Width);
            double availableWidth = (DataContext as VisualizationViewModel).Size.X - (HeaderObjects.Count()) * 8;

            if (exclude != null)
            {
                totalHeaderWidth -= exclude.Width;
                availableWidth -= exclude.Width;

                exclude.Width = Math.Min(exclude.Width, (DataContext as VisualizationViewModel).Size.X - (HeaderObjects.Count() - 1) * 28);

                double ratio = availableWidth / totalHeaderWidth;
                HeaderObjects.Where(ho => ho != exclude).ToList().ForEach(ho => ho.Value.Width *= ratio);
            }
            else
            {
                double ratio = availableWidth / totalHeaderWidth;
                HeaderObjects.ForEach(ho => ho.Value.Width *= ratio);
            }
            //setMenuViewModelAnkerPosition();
        }
    }
}
