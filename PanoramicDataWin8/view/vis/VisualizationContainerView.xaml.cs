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
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class VisualizationContainerView : UserControl, IScribbable, InputFieldViewModelEventHandler
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

            if (visualizationViewModel.QueryModel.JobType == JobType.DB)
            {
                if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.bar)
                {
                    _renderer = new PlotRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.table)
                {
                    _renderer = new TableRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.plot)
                {
                    _renderer = new PlotRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.line)
                {
                    _renderer = new PlotRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                else if (visualizationViewModel.QueryModel.VisualizationType == VisualizationType.map)
                {
                    _renderer = new PlotRenderer();
                    contentGrid.Children.Add(_renderer);
                }
                
                
            }
            else if (visualizationViewModel.QueryModel.JobType == JobType.Kmeans)
            {
                _renderer = new KmeansRenderer();
                contentGrid.Children.Add(_renderer);
            }
        }

        private int _status = 0; // 0: unknonw, 1: single, 2: double
        private bool _moved = false;
        private Stopwatch _firstDownTime = new Stopwatch();

        void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.CurrentPointers.First().PointerId].Position);
                _firstDownTime.Restart();
            }
            else if (e.NumActiveContacts == 2)
            {
                _firstDownTime.Stop();
            }
            _moved = false;
            this.SendToFront();
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            foreach (var avm in model.AttachementViewModels)
            {
                avm.IsDisplayed = true;
            }
        }
        
        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            _moved = true;
            GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
            if (e.NumActiveContacts == 2 && e.TriggeringPointer.PointerId == e.CurrentPointers.First().PointerId)
            {
                if (_status != 2 && _firstDownTime.ElapsedMilliseconds < 50)
                {
                    _status = 2;
                }
                if (_status == 2)
                {
                    performMoved(e.CurrentContacts[e.CurrentPointers.First().PointerId]);
                }
            }
            else if (e.NumActiveContacts == 1)
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
                if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    performMoved(e.CurrentContacts[e.CurrentPointers.First().PointerId]);
                }
                else
                {
                    if (_status != 1 && _firstDownTime.ElapsedMilliseconds > 50)
                    {
                        _status = 1;
                        Point currentPoint =
                            gt.TransformPoint(e.CurrentContacts[e.CurrentPointers.First().PointerId].Position);
                        _renderer.StartSelection(currentPoint);
                        _renderer.MoveSelection(currentPoint);
                    }
                    if (_status == 1)
                    {
                        Point currentPoint =
                            gt.TransformPoint(e.CurrentContacts[e.CurrentPointers.First().PointerId].Position);
                        _renderer.MoveSelection(currentPoint);
                    }
                }
            }
        }

        void performMoved(PointerPoint pp)
        {
            GeneralTransform gt = this.TransformToVisual(MainViewController.Instance.InkableScene);
            Point currentPoint = gt.TransformPoint(pp.Position);

            Vec delta = _mainPointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();
            VisualizationViewModel model = (DataContext as VisualizationViewModel);
            model.Position -= delta;
            _mainPointerManagerPreviousPoint = currentPoint;
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (_status == 0)
            {
                if (_firstDownTime.IsRunning && !_moved)
                {
                    _renderer.StartSelection(_mainPointerManagerPreviousPoint);
                    _renderer.EndSelection();
                }
            }
            if (_status == 1)
            {
                _renderer.EndSelection();
                _status = 0;
            }
            if (_status == 2)
            {
                _status = 0;
            }

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

        public void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _renderer as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelMoved(sender, e, overElement);
            }
        }

        public void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _renderer as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                InputFieldViewModelEventHandler inputModelEventHandler = _renderer as InputFieldViewModelEventHandler;
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
    }
}
