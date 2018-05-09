using GeoAPI.Geometries;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Controls;
using GraphSharp.Sample;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class FunctionRenderer : Renderer, IScribbable
    {
        DispatcherTimer _keyboardTimer = new DispatcherTimer();
        public FunctionRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += dataContextChanged;
            this.InitializeComponent();
        }

        public bool IsDeletable { get { return false; } }

        public FunctionOperationViewModel FunctionOperationViewModel => DataContext as FunctionOperationViewModel;

        public IGeometry Geometry => new Rct(FunctionOperationViewModel.Position, FunctionOperationViewModel.Size).GetPolygon();

        public List<IScribbable> Children => new List<IScribbable>();
        public bool Consume(InkStroke inkStroke) { return false; }

        void dataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            NameTextBox.Text = "";
            MinHeight = OperationViewModel.MIN_HEIGHT / 2;
            var viewModel = (DataContext as FunctionOperationViewModel);
            if (viewModel != null)
            {
                if (this.FunctionOperationViewModel.FunctionOperationModel is PipelineFunctionModel pipelineFunctionModel)
                {
                    var pg = new PocGraphView()
                    {
                        Graph = convertPipelineDescriptionToGraph(pipelineFunctionModel.PipelineDescription),
                        LayoutAlgorithmType = StandardLayoutAlgorithmFactory<PocVertex, PocEdge, PocGraph>.AlgorithmTypeNames.EfficientSugiyama.ToString(),
                        IsAnimationEnabled = false
                    };

                    GraphCanvas.Children.Add(pg);
                    GraphCanvas.LayoutUpdated += (s, e) =>
                    {
                        double minLeft = double.MaxValue;
                        double maxLeft = double.MinValue;
                        double minTop = double.MaxValue;
                        double maxTop = double.MinValue;
                        foreach (var ch in pg.Children.OfType<VertexControl>())
                        {
                            minLeft = Math.Min(minLeft, Canvas.GetLeft(ch));
                            maxLeft = Math.Max(maxLeft, Canvas.GetLeft(ch) + ch.ActualWidth);
                            minTop = Math.Min(minTop, Canvas.GetTop(ch));
                            maxTop = Math.Max(maxTop, Canvas.GetTop(ch) + ch.ActualHeight);
                        }
                        GraphCanvas.Height = maxTop - minTop;
                        GraphCanvas.Width = maxLeft - minLeft;
                        GraphCanvas.RenderTransform = new TranslateTransform() { X = -minLeft, Y = -minTop };
                    };
                }
                else
                {
                    NameTextBox.Text = FunctionOperationViewModel.FunctionOperationModel.GetAttributeModel().DisplayName;
                }
                FunctionOperationViewModel.OperationViewModelTapped -= OperationViewModelTapped;
                FunctionOperationViewModel.OperationViewModelTapped += OperationViewModelTapped;
                viewModel.FunctionOperationModel.OperationModelUpdated += (s, e) => viewModel.FunctionOperationModel.UpdateName();
            }
        }

        static PocGraph convertPipelineDescriptionToGraph(object pipelineDescription)
        {
            var graph = new PocGraph();
            for (int i = 0; i < 8; i++)
            {
                graph.AddVertex(new PocVertex(i.ToString(),
                    new List<string>(i % 3 == 0 ? new string[] { "i1", "i2", "i3" } : i % 3 == 1 ? new string[] { "i6", "i3" } : new string[] { "i8" })));
            }

            graph.AddEdge(new PocEdge("0to1", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(1)));
            graph.AddEdge(new PocEdge("1to2", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(2)));
            graph.AddEdge(new PocEdge("2to3", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(3)));
            graph.AddEdge(new PocEdge("2to4", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(4)));
            graph.AddEdge(new PocEdge("0to5", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(5)));
            graph.AddEdge(new PocEdge("1to7", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(7)));
            graph.AddEdge(new PocEdge("4to6", graph.Vertices.ElementAt(4), graph.Vertices.ElementAt(6)));
            graph.AddEdge(new PocEdge("7to4", graph.Vertices.ElementAt(7), graph.Vertices.ElementAt(4)));
            return graph;
        }

        private void OperationViewModelTapped(PointerRoutedEventArgs e)
        {
            //NameTextBox.IsEnabled = true;
            //NameTextBox.Focus(FocusState.Keyboard);
        }


        private void NameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (e.Key == Windows.System.VirtualKey.Enter)
            //{

            //    e.Handled = true;
            //}
            //else
            //    _keyboardTimer.Start();
        }

        private void NameTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            if (!NameTextBox.GetBounds().Contains(e.GetCurrentPoint(NameTextBox).Position))
            {
                //var model = (this.DataContext as FunctionOperationViewModel).OperationModel as FunctionOperationModel;
                //NameTextBox.IsEnabled = false;
                //model.SetRawName(NameTextBox.Text);
                //MainViewController.Instance.MainPage.addAttributeButton.Focus(FocusState.Pointer);
                //MainViewController.Instance.MainPage.clearAndDisposeMenus();
            }
        }
    }
}
