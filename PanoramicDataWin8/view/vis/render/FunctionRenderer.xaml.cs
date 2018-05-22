using GeoAPI.Geometries;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Controls;
using GraphSharp.Sample;
using IDEA_common.operations.ml.optimizer;
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

        PocGraphView _lastPg = null;
        void dataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            NameTextBox.Text = "";
            MinHeight = OperationViewModel.MIN_HEIGHT / 2;
            var viewModel = (DataContext as FunctionOperationViewModel);
            if (viewModel != null)
            {
                if (this.FunctionOperationViewModel.FunctionOperationModel is PipelineFunctionModel)
                {
                    viewModel.FunctionOperationModel.OperationModelUpdated -= updatePipeline;
                    viewModel.FunctionOperationModel.OperationModelUpdated += updatePipeline;
                    viewModel.FunctionOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
                else
                {
                    NameTextBox.Text = FunctionOperationViewModel.FunctionOperationModel.GetAttributeModel().DisplayName;
                    viewModel.FunctionOperationModel.OperationModelUpdated += (s, e) => viewModel.FunctionOperationModel.UpdateName();
                }
                FunctionOperationViewModel.OperationViewModelTapped -= OperationViewModelTapped;
                FunctionOperationViewModel.OperationViewModelTapped += OperationViewModelTapped;
            }
        }

        void updatePipeline(object sender, OperationModelUpdatedEventArgs e)
        {
            var pipelineFunctionModel = this.FunctionOperationViewModel.FunctionOperationModel as PipelineFunctionModel;
            if (_lastPg != null)
                GraphCanvas.Children.Remove(_lastPg);
            _lastPg = new PocGraphView()
            {
                Graph = convertPipelineDescriptionToGraph(pipelineFunctionModel.PipelineDescription),
                LayoutAlgorithmType = StandardLayoutAlgorithmFactory<PocVertex, PocEdge, PocGraph>.AlgorithmTypeNames.Circular.ToString(),
                IsAnimationEnabled = false
            };
            GraphCanvas.Children.Add(_lastPg);
            GraphCanvas.LayoutUpdated += (ss, ee) =>
            {
                double minLeft = double.MaxValue;
                double maxLeft = double.MinValue;
                double minTop = double.MaxValue;
                double maxTop = double.MinValue;
                bool any = false;
                foreach (var ch in _lastPg.Children.OfType<VertexControl>())
                {
                    any = true;
                    minLeft = Math.Min(minLeft, Canvas.GetLeft(ch));
                    maxLeft = Math.Max(maxLeft, Canvas.GetLeft(ch) + ch.ActualWidth);
                    minTop = Math.Min(minTop, Canvas.GetTop(ch));
                    maxTop = Math.Max(maxTop, Canvas.GetTop(ch) + ch.ActualHeight);
                }
                GraphCanvas.Height = any ? maxTop - minTop : 100;
                GraphCanvas.Width = any ? maxLeft - minLeft : 100;
                GraphCanvas.RenderTransform = new TranslateTransform() { X = any ? -minLeft : 0, Y = any ? -minTop : 0 };
            };
        }

        public static string ValueToString(IDEA_common.operations.ml.optimizer.Value value)
        {
            if (value is IDEA_common.operations.ml.optimizer.BoolValue bval)
                return bval.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.ErrorValue eval)
                return eval.Message;
            if (value is IDEA_common.operations.ml.optimizer.BytesValue byteVal)
                return byteVal.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.CsvUriValue cval)
                return cval.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.DatasetUriValue dval)
                return dval.Value;
            if (value is IDEA_common.operations.ml.optimizer.DoubleValue doubleVal)
                return doubleVal.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.LongValue lval)
                return lval.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.PickleBlobValue pval)
                return pval.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.PickleUriValue puriVal)
                return puriVal.Value;
            if (value is IDEA_common.operations.ml.optimizer.PlasmaIdValue plasmaVal)
                return plasmaVal.Value.ToString();
            if (value is IDEA_common.operations.ml.optimizer.StringValue sval)
                return sval.Value;
            return null;
        }
        static PocGraph convertPipelineDescriptionToGraph(PipelineDescription pipelineDescription)
        {
            var graph = new PocGraph();
            if (pipelineDescription.Steps != null)
            {
                int cnt = -1;
                foreach (var p in pipelineDescription.Steps)
                {
                    int start = 0;
                    var parms = p.Outputs.Select((o) => o.Id).ToList();
                    if (p is PrimitivePipelineDescriptionStep primStep)
                    {
                        var args = primStep.Hyperparams.Select((a) =>
                        {
                            var header = a.Key;
                            if (a.Value is IDEA_common.operations.ml.optimizer.ValueArgument varg)
                                header += "=" + ValueToString(varg.Data);
                            return header;
                        }).ToList();
                        if (cnt >= start)
                            graph.AddVertex(new PocVertex(primStep.Primitive.Name, primStep.Outputs.FirstOrDefault()?.Id ?? "<none>", args, parms));
                    }
                    else if (p is SubpipelinePipelineDescriptionStep subStep)
                    {
                        if (cnt >= start)
                            graph.AddVertex(new PocVertex(subStep.PipelineDescription.Name, subStep.Outputs.FirstOrDefault()?.Id ?? "<none>", subStep.Inputs.Select((s) => s.Data).ToList(), parms));
                    }
                    else if (p is PlaceholderPipelineDescriptionStep placeStep)
                    {
                        if (cnt >= start)
                            graph.AddVertex(new PocVertex("<placeholder>", placeStep.Outputs.FirstOrDefault()?.Id ?? "<none>", placeStep.Inputs.Select((s) => s.Data).ToList(), parms));
                    }
                    // new List<string>(i % 3 == 0 ? new string[] { "i1", "i2", "i3" } : i % 3 == 1 ? new string[] { "i6", "i3" } : new string[] { "i8" })));
                    if (cnt > start)
                        graph.AddEdge(new PocEdge("", graph.Vertices.ElementAt(cnt - start), graph.Vertices.ElementAt(cnt-1 - start)));
                    cnt++;
                }
            }

            
            //foreach (var p in pipelineDescription.Steps)
            //{
            //    foreach (var g in p.Outputs)
            //        graph.AddEdge("" + p.ToString() + "To" + g.Id, graph.VertexAdded.ElementsAt(g.I)
            //}
            //graph.AddEdge(new PocEdge("1to2", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(2)));
            //graph.AddEdge(new PocEdge("2to3", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(3)));
            //graph.AddEdge(new PocEdge("2to4", graph.Vertices.ElementAt(2), graph.Vertices.ElementAt(4)));
            //graph.AddEdge(new PocEdge("0to5", graph.Vertices.ElementAt(0), graph.Vertices.ElementAt(5)));
            //graph.AddEdge(new PocEdge("1to7", graph.Vertices.ElementAt(1), graph.Vertices.ElementAt(7)));
            //graph.AddEdge(new PocEdge("4to6", graph.Vertices.ElementAt(4), graph.Vertices.ElementAt(6)));
            //graph.AddEdge(new PocEdge("7to4", graph.Vertices.ElementAt(7), graph.Vertices.ElementAt(4)));
            return graph;
        }
        public override void Dispose()
        {
            var viewModel = (DataContext as FunctionOperationViewModel);
            viewModel.FunctionOperationModel.OperationModelUpdated -= updatePipeline;
            base.Dispose();
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
