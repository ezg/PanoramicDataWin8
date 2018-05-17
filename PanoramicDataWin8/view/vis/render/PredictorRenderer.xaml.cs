using PanoramicDataWin8.utils;
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
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Input;
using System.Diagnostics;
using GeoAPI.Geometries;
using IDEA_common.operations;
using IDEA_common.operations.ml.optimizer;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class PredictorRenderer : Renderer, IScribbable
    {
        private PredictorRendererContentProvider _predictorRendererContentProvider = new PredictorRendererContentProvider();
        
        public PredictorRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += Renderer_Loaded;
            this.SizeChanged += (sender, e) => Relayout();
        }

        void Relayout()
        {
            var model = ((DataContext as PredictorOperationViewModel).OperationModel as PredictorOperationModel);
        }
        void Renderer_Loaded(object sender, RoutedEventArgs e)
        {

            _predictorRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _predictorRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _predictorRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as PredictorOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as PredictorOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
            if (dxSurface != null)
            {
                dxSurface.Dispose();
            }
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as PredictorOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as PredictorOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as PredictorOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as PredictorOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;

                var result = (DataContext as PredictorOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
                else
                {
                    PredictorOperationModel operationModel = (PredictorOperationModel)((OperationViewModel)DataContext).OperationModel;
                    if (!operationModel.AttributeTransformationModelParameters.Any())
                    {
                        viewBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PredictorOperationModel operationModel = (PredictorOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as PredictorOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
            }
        }

        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            if (e is VisualOperationModelUpdatedEventArgs)
            {
                render();
            }
        }

        void loadResult(IResult result)
        {
            if (result is OptimizerResult)
            {
                PredictorOperationViewModel model = (DataContext as PredictorOperationViewModel);
                var operationModel = (PredictorOperationModel) model.OperationModel;
                operationModel.UpdateBackendOperatorId((result as OptimizerResult).TopKSolutions.First().SolutionId);

                _predictorRendererContentProvider.UpdateData(result,
                    (PredictorOperationModel) model.OperationModel,
                    (PredictorOperationModel) model.OperationModel.ResultCauserClone);
            }
            else if (result is ErrorResult)
            {
                ErrorHandler.HandleError((result as ErrorResult).Message);
            }
        }

        private List<Windows.Foundation.Point> _selectionPoints = new List<Windows.Foundation.Point>();
        public override void StartSelection(Windows.Foundation.Point point)
        {
            GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            _selectionPoints = new List<Windows.Foundation.Point> { gt.TransformPoint(point) };
        }

        public override void MoveSelection(Windows.Foundation.Point point)
        {
            GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(dxSurface);
            _selectionPoints.Add(gt.TransformPoint(point));
        }

        public override bool EndSelection()
        {
         
            if (_predictorRendererContentProvider.SubmitHitTarget != null &&
                _predictorRendererContentProvider.SubmitHitTarget.Contains(_selectionPoints.First().GetPoint()))
            {
                submitProblem();
            }
            else if (_predictorRendererContentProvider.SpecifyProblemHitTarget != null &&
                _predictorRendererContentProvider.SpecifyProblemHitTarget.Contains(_selectionPoints.First().GetPoint()))
            {
                specifyProblem();
            }
            return false;
        }

        async void submitProblem()
        {
            ContentDialog locationPromptDialog = new ContentDialog
            {
                Title = "TASK 2: Submit solution?",
                Content = "Submit this prediction as a solution? The application will exit after you press OK.\nThis means you are done with Task 2. \n\nNever use this for Task 1, rather use the \"specify problem?\" button in that case.",
                SecondaryButtonText = "Cancel",
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await locationPromptDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                PredictorOperationViewModel model = (DataContext as PredictorOperationViewModel);
                var operationModel = (PredictorOperationModel)model.OperationModel;

                var catalogCommand = new SubmitProblemCommand();
                await catalogCommand.SumbitResult(operationModel);
                Application.Current.Exit();
            }
        }

        async void specifyProblem()
        {
            TextBlock tb = new TextBlock();
            tb.Text = "Sumbit this a new problem specification. Please provide a brief description or comments.";

            RichEditBox inputTextBox = new RichEditBox();
            inputTextBox.AcceptsReturn = true;
            inputTextBox.TextWrapping = TextWrapping.Wrap;
            inputTextBox.Height = 3 * 32;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Vertical;
            
            sp.Children.Add(tb);
            sp.Children.Add(inputTextBox);

            ContentDialog dialog = new ContentDialog();
            dialog.Content = sp;
            dialog.Title = "TASK 1: Specify problem?";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "OK";
            dialog.SecondaryButtonText = "Cancel";

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                PredictorOperationViewModel model = (DataContext as PredictorOperationViewModel);
                var operationModel = (PredictorOperationModel) model.OperationModel;

                string userComment = string.Empty;
                inputTextBox.Document.GetText(Windows.UI.Text.TextGetOptions.None, out userComment);

                var catalogCommand = new SpecifyProblemCommand();
                await catalogCommand.SpecifyProblem(operationModel, userComment);
            }
        }


        void render(bool sizeChanged = false)
        {
            viewBox.Visibility = Visibility.Collapsed;
            dxSurface?.Redraw();
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                PredictorOperationViewModel model = this.DataContext as PredictorOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return true;
        }
    }
}