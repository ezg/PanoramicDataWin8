using PanoramicData.controller.view;
using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicData.view.inq;
using PanoramicDataWin8.view.vis.render;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class VisualizationContainerView : UserControl, IScribbable, AttributeViewModelEventHandler
    {
        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        private PointerManager _resizePointerManager = new PointerManager();
        private Point _resizePointerManagerPreviousPoint = new Point();

        private Renderer _renderer = null;

        public VisualizationContainerView()
        {
            this.InitializeComponent();

            this.Loaded += VisualizationContainerView_Loaded;
            this.DataContextChanged += visualizationContainerView_DataContextChanged;
        }

        void VisualizationContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);

            _resizePointerManager.Added += resizePointerManager_Added;
            _resizePointerManager.Moved += resizePointerManager_Moved;
            _resizePointerManager.Removed += resizePointerManager_Removed;
            _resizePointerManager.Attach(resizeGrid);
        }

        void visualizationContainerView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                VisualizationViewModel model = (args.NewValue as VisualizationViewModel);
                model.QueryModel.PropertyChanged += QueryModel_PropertyChanged;
                visualizationTypeUpdated();
            }
        }

        void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            QueryModel queryModel = (DataContext as VisualizationViewModel).QueryModel;
            if (e.PropertyName == queryModel.GetPropertyName(() => queryModel.VisualizationType))
            {
                visualizationTypeUpdated();
            }
        }

        void visualizationTypeUpdated()
        {
            VisualizationViewModel visualizationViewModel = (DataContext as VisualizationViewModel);
            if (contentGrid.Children.Count == 1)
            {
                (contentGrid.Children.First() as Renderer).Dispose();
            }
            contentGrid.Children.Clear();

            if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.Bar)
            {
               /* Renderer fRenderer = new XYRenderer()
                {
                    RenderContent = new Direct2dXYRendererContent()
                    {
                        Scene = new ScatterPlotScene()
                    }
                };
                contentGrid.Children.Add(fRenderer);*/
            }
            else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.Table)
            {
                /*Renderer renderer = new TableRenderer();
                contentGrid.Children.Add(fRenderer);*/
            }
            else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.Plot)
            {
                //PlotFilterRenderer4 fRenderer = new PlotFilterRenderer4(false);
                //(_front.Content as Front).SetContent(fRenderer);
            }
            else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.Line)
            {
                //PlotFilterRenderer4 fRenderer = new PlotFilterRenderer4(false);
                //(_front.Content as Front).SetContent(fRenderer);
            }
            else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.Map)
            {
                //MapFilterRenderer2 fRenderer = new MapFilterRenderer2(false);
                //(_front.Content as Front).SetContent(fRenderer);
            }

            _renderer = new TableRenderer();
            contentGrid.Children.Add(_renderer);
        }


        void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                this.SendToFront();
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
                VisualizationViewModel model = (DataContext as VisualizationViewModel);
                foreach (var avm in model.AttachementViewModels)
                {
                    avm.IsDisplayed = true;
                }
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = _mainPointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();
                VisualizationViewModel model = (DataContext as VisualizationViewModel);
                model.Position -= delta;
                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = false;
            }
        }

        void resizePointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = resizeGrid.TransformToVisual(MainViewController.Instance.InkableScene);
                _resizePointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                //VisualizationViewModel model = (DataContext as VisualizationViewModel);
                //model.AttachementViewModels.ForEach(avm => avm.Value.IsDisplayed = true);
            }
        }

        void resizePointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = resizeGrid.TransformToVisual(MainViewController.Instance.InkableScene);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = _resizePointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();
                VisualizationViewModel model = (DataContext as VisualizationViewModel);
                model.Size = new Vec(Math.Max(model.Size.X - delta.X, VisualizationViewModel.MIN_WIDTH), Math.Max(model.Size.Y - delta.Y, VisualizationViewModel.MIN_HEIGHT));
                _resizePointerManagerPreviousPoint = currentPoint;
            }
        }

        void resizePointerManager_Removed(object sender, PointerManagerEvent e)
        {
            //VisualizationViewModel model = (DataContext as VisualizationViewModel);
            //model.AttachementViewModels.ForEach(avm => avm.Value.IsDisplayed = false);
        }

        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                VisualizationViewModel model = this.DataContext as VisualizationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get
            {
                return new List<IScribbable>();
            }
        }

        public void AttributeViewModelMoved(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            AttributeViewModelEventHandler attributeViewModelEventHandler = _renderer as AttributeViewModelEventHandler;
            if (attributeViewModelEventHandler != null)
            {
                attributeViewModelEventHandler.AttributeViewModelMoved(sender, e, overElement);
            }
        }

        public void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            AttributeViewModelEventHandler attributeViewModelEventHandler = _renderer as AttributeViewModelEventHandler;
            if (attributeViewModelEventHandler != null)
            {
                attributeViewModelEventHandler.AttributeViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                AttributeViewModelEventHandler attributeViewModelEventHandler = _renderer as AttributeViewModelEventHandler;
                if (attributeViewModelEventHandler != null)
                {
                    return attributeViewModelEventHandler.BoundsGeometry;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
