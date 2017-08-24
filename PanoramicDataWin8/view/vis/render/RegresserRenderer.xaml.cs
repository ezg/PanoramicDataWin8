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
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class RegresserRenderer : Renderer, IScribbable
    {
        // private ExampleRendererContentProvider _exampleRendererContentProvider = new ExampleRendererContentProvider();


        public RegresserRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += PlotRenderer_Loaded;
        }

        void PlotRenderer_Loaded(object sender, RoutedEventArgs e)
        {

            //_exampleRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            //_exampleRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            //dxSurface.ContentProvider = _exampleRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as RegresserOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as RegresserOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
            if (dxSurface != null)
            {
                dxSurface.Dispose();
            }
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as RegresserOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as RegresserOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as RegresserOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as RegresserOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;

                var result = (DataContext as RegresserOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
                else
                {
                    RegresserOperationModel operationModel = (RegresserOperationModel)((OperationViewModel)DataContext).OperationModel;
                    if (!operationModel.AttributeUsageTransformationModels.Any())
                    {
                        viewBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RegresserOperationModel operationModel = (RegresserOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as RegresserOperationViewModel).OperationModel.Result;
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
            RegresserOperationViewModel model = (DataContext as RegresserOperationViewModel);
            //_exampleRendererContentProvider.UpdateData(result,
            //    (ExampleOperationModel)model.OperationModel,
            //    (ExampleOperationModel)model.OperationModel.ResultCauserClone);
        }


        public override void StartSelection(Windows.Foundation.Point point)
        {

        }

        public override void MoveSelection(Windows.Foundation.Point point)
        {
        }

        public override bool EndSelection()
        {
            return false;
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
                RegresserOperationViewModel model = this.DataContext as RegresserOperationViewModel;

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