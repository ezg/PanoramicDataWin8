using PanoramicDataWin8.view.vis.render;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.style;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class OperationContainerView : UserControl, IScribbable, AttributeTransformationViewModelEventHandler, IOneFingerListener
    {
        private Point _previousPoint = new Point();
        private Point _initialPoint = new Point();
        private Stopwatch _tapStart = new Stopwatch();
        private bool _movingStarted = false;
        private bool _fingerDown = false;

        private PointerManager _resizePointerManager = new PointerManager();
        private Point _resizePointerManagerPreviousPoint = new Point();

        private Renderer _renderer = null;

        public OperationContainerView()
        {
            this.InitializeComponent();

            this.Loaded += VisualizationContainerView_Loaded;
            this.DataContextChanged += visualizationContainerView_DataContextChanged;
            MainViewController.Instance.InkableScene.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(InkableScene_PointerPressed), true);
        }

        public void Dispose()
        {
            if (_renderer != null)
            {
                _renderer.Dispose();
            }
        }


        private void InkableScene_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ancestors = (e.OriginalSource as FrameworkElement).GetAncestors();
            if (!ancestors.Contains(this) && _fingerDown)
            {
                bool found = false;
                foreach (var ancest in ancestors)
                {
                    if (ancest is OperationContainerView)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    MainViewController.Instance.CopyOperationViewModel(this.DataContext as OperationViewModel, e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position);
                }
            }
        }

        void VisualizationContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.PointerPressed += VisualizationContainerView_PointerPressed;

            _resizePointerManager.Added += resizePointerManager_Added;
            _resizePointerManager.Moved += resizePointerManager_Moved;
            _resizePointerManager.Removed += resizePointerManager_Removed;
            _resizePointerManager.Attach(resizeGrid);
        }

        void visualizationContainerView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                OperationViewModel model = ((OperationViewModel) args.NewValue);
                model.OperationModel.PropertyChanged += QueryModel_PropertyChanged;
                visualizationTypeUpdated();
            }
        }

        void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel operationModel = (HistogramOperationModel) ((OperationViewModel) DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.VisualizationType))
            {
                visualizationTypeUpdated();
            }
        }

        void visualizationTypeUpdated()
        {
            OperationViewModel operationViewModel = (DataContext as OperationViewModel);
            if (contentGrid.Children.Count == 1)
            {
                (contentGrid.Children.First() as Renderer).Dispose();
            }
            contentGrid.Children.Clear();

            if (operationViewModel.OperationModel is HistogramOperationModel)
            {
                var histogramOperationModel = (HistogramOperationModel) operationViewModel.OperationModel;

                if (histogramOperationModel.VisualizationType == VisualizationType.table)
                {
                    _renderer = new render.TableRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (histogramOperationModel.VisualizationType == VisualizationType.plot)
                {
                    _renderer = new PlotRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (histogramOperationModel.VisualizationType == VisualizationType.map)
                {
                    _renderer = new MapRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (histogramOperationModel.VisualizationType == VisualizationType.county)
                {
                    _renderer = new SVGRenderer("county.json");
                    contentGrid.Children.Add(_renderer);
                }


            }
            else if (operationViewModel.OperationModel is ClassificationOperationModel)
            {
                var classificationOperationModel = (ClassificationOperationModel)operationViewModel.OperationModel;
                if (classificationOperationModel.TaskModel.Name != "frequent_itemsets")
                {
                    _renderer = new ClassifierRenderer();
                    contentGrid.Children.Add(_renderer);
                }
               /* else if (classificationOperationModel.TaskModel.Name == "frequent_itemsets")
                {
                    _renderer = new FrequentItemsetRenderer();
                    contentGrid.Children.Add(_renderer);
                }*/
            }
        }

        public void Pressed(FrameworkElement sender, PointerManagerEvent e)
        {
            _tapStart.Restart();
            var trans = sender.TransformToVisual(MainViewController.Instance.InkableScene);
            _previousPoint = trans.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
            _initialPoint = _previousPoint;
            _movingStarted = false;
            _fingerDown = true;

            this.SendToFront();
            OperationViewModel model = (DataContext as OperationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = true;
            }
        }

        public void TwoFingerMoved()
        {
            _tapStart.Restart();
        }

        public void Moved(FrameworkElement sender, PointerManagerEvent e)
        {
            var trans = sender.TransformToVisual(MainViewController.Instance.InkableScene);
            var currentPoint = trans.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
            if (((_initialPoint.GetVec() - currentPoint.GetVec()).Length2 > 100 || _movingStarted) &&
                _tapStart.ElapsedMilliseconds > 50)
            {
                _movingStarted = true;
                Vec delta = _previousPoint.GetVec() - currentPoint.GetVec();
                OperationViewModel model = (DataContext as OperationViewModel);
                model.Position -= delta;

            }
            _previousPoint = currentPoint;
        }

        public void Released(FrameworkElement sender, PointerManagerEvent e, bool isRightMouse)
        {
            if (_movingStarted)
            {
                _movingStarted = false;
            }
            else if (_tapStart.ElapsedMilliseconds < 300)
            {
                var trans = sender.TransformToVisual(MainViewController.Instance.InkableScene);
                if (e.CurrentContacts.ContainsKey(e.TriggeringPointer.PointerId))
                {
                    _renderer.StartSelection(trans.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position));
                    _renderer.EndSelection();
                }
            }
            _fingerDown = false;

            OperationViewModel model = (DataContext as OperationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = false;
            }

            if (isRightMouse)
            {
                MainViewController.Instance.RemoveOperationViewModel(this);
            }
        }

        void VisualizationContainerView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen ||
                (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                return;
            }
            _tapStart.Restart();
            _previousPoint = e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position;
            _initialPoint = _previousPoint;
            _movingStarted = false;
            e.Handled = true;
            this.CapturePointer(e.Pointer);
            this.PointerMoved += VisualizationContainerView_PointerMoved;
            this.PointerReleased += VisualizationContainerView_PointerReleased;
            _fingerDown = true;

            this.SendToFront();
            OperationViewModel model = (DataContext as OperationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = true;
            }

            ((OperationViewModel) DataContext).FireOperationViewModelTapped();
        }

        async void VisualizationContainerView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position;
            if ((_initialPoint.GetVec() - currentPoint.GetVec()).Length2 > 100 || _movingStarted)
            {
                _movingStarted = true;
                Vec delta = _previousPoint.GetVec() - currentPoint.GetVec();
                OperationViewModel model = (DataContext as OperationViewModel);
                model.Position -= delta;
                
            }
            _previousPoint = currentPoint;
            e.Handled = true;
        }

        void VisualizationContainerView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_movingStarted)
            {
                _movingStarted = false;
            }
            else if (_tapStart.ElapsedMilliseconds < 300)
            {
                _renderer.StartSelection(e.GetCurrentPoint(MainViewController.Instance.InkableScene).Position);
                _renderer.EndSelection();
            }
            _fingerDown = false;
            this.ReleasePointerCapture(e.Pointer);
            this.PointerMoved -= VisualizationContainerView_PointerMoved;
            this.PointerReleased -= VisualizationContainerView_PointerReleased;


            OperationViewModel model = (DataContext as OperationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = false;
            }

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
                {
                    MainViewController.Instance.RemoveOperationViewModel(this);
                }
            }
        }


        void resizePointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = resizeGrid.TransformToVisual(MainViewController.Instance.InkableScene);
                _resizePointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                //OperationViewModel model = (DataContext as OperationViewModel);
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
                OperationViewModel model = (DataContext as OperationViewModel);
                model.Size = new Vec(Math.Max(model.Size.X - delta.X, OperationViewModel.MIN_WIDTH), Math.Max(model.Size.Y - delta.Y, OperationViewModel.MIN_HEIGHT));
                _resizePointerManagerPreviousPoint = currentPoint;
            }
        }

        void resizePointerManager_Removed(object sender, PointerManagerEvent e)
        {
            //OperationViewModel model = (DataContext as OperationViewModel);
            //model.AttachementViewModels.ForEach(avm => avm.Value.IsDisplayed = false);
        }

        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                OperationViewModel model = this.DataContext as OperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get
            {
                IScribbable scribbable = _renderer as IScribbable;
                if (scribbable != null)
                {
                    return new List<IScribbable>() { scribbable };
                }

                return new List<IScribbable>();
            }
        }

        public void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            AttributeTransformationViewModelEventHandler inputModelEventHandler = _renderer as AttributeTransformationViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.AttributeTransformationViewModelMoved(sender, e, overElement);
            }
        }

        public void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            AttributeTransformationViewModelEventHandler inputModelEventHandler = _renderer as AttributeTransformationViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.AttributeTransformationViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                AttributeTransformationViewModelEventHandler inputModelEventHandler = _renderer as AttributeTransformationViewModelEventHandler;
                if (inputModelEventHandler != null)
                {
                    return inputModelEventHandler.BoundsGeometry;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }

        public bool IsDeletable
        {
            get { return true; }
        }
    }
}
