using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Bing.Maps;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.style;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class MapRenderer : Renderer
    {
        private PointerManager _pointerManager = null;
        private IOneFingerListener _oneFingerListener = null;

        private MapInertiaHandler _mapInertiaHandler = null;
        private Pointer scrollPointer = null;
        private Location _startCenter = new Location();
        private Point _startCenterPixels = new Point();
        private Vec _initalFingerDiff = new Vec();
        private Vec _initialFingerCenter = new Vec();
        private double _startZoom = 0;


        private Vec _startFingerCenterPoint = new Vec();

        public MapRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
        }

        void MapRenderer_Loaded(object sender, RoutedEventArgs args)
        {
            (sender as Map).TileLayers.Clear();
            MapTileLayer layer2 = new MapTileLayer();
            layer2.GetTileUri += (s, e) =>
            {
                //e.Uri = new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", e.LevelOfDetail, e.X, e.Y));
                e.Uri = new Uri(string.Format("https://api.mapbox.com/v4/ezgraggen.41f60101/{0}/{1}/{2}@2x.png?access_token=pk.eyJ1IjoiZXpncmFnZ2VuIiwiYSI6ImNpZm9tbmdteWhpeTRzNG03M3J1bnpneHAifQ.tTw2Dwj64wUza2n_dJPk9A", e.LevelOfDetail, e.X, e.Y));
            };

            (sender as Map).TileLayers.Add(layer2);

            (sender as Map).PointerPressedOverride += map_PointerPressedOverride;
            (sender as Map).PointerMovedOverride += map_PointerMovedOverride;
            (sender as Map).PointerReleasedOverride += map_PointerReleasedOverride;

            (sender as Map).IsHitTestVisible = false;

            _pointerManager = new PointerManager();
            _pointerManager.Added += _pointerManager_Added;
            _pointerManager.Moved += _pointerManager_Moved;
            _pointerManager.Removed += _pointerManager_Removed;
            _pointerManager.Attach(this);

            _oneFingerListener = this.GetAncestors().FirstOrDefault(a => a is IOneFingerListener) as IOneFingerListener;

            _mapInertiaHandler = new MapInertiaHandler(sender as Map);

            xyRenderer.Render += render;
            xyRenderer.LoadResultItemModels += loadResultItemModels;
        }

        void render()
        {
            VisualizationViewModel model = (DataContext as VisualizationViewModel);

            _mapInertiaHandler.Map.ShapeLayers.Clear();
            if (model.QueryModel.ResultModel != null && model.QueryModel.ResultModel.ResultItemModels.Any())
            {
                MapShapeLayer shapeLayer = new MapShapeLayer();
                var resultDescriptionModel = model.QueryModel.ResultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;

                var xAom = model.QueryModel.GetUsageInputOperationModel(InputUsage.X).FirstOrDefault();
                var yAom = model.QueryModel.GetUsageInputOperationModel(InputUsage.Y).FirstOrDefault();

                var xIndex = resultDescriptionModel.Dimensions.IndexOf(xAom);
                var yIndex = resultDescriptionModel.Dimensions.IndexOf(yAom);

                var xBinRange = resultDescriptionModel.BinRanges[xIndex];
                var yBinRange = resultDescriptionModel.BinRanges[yIndex];

                var xBins = xBinRange.GetBins();
                xBins.Add(xBinRange.AddStep(xBins.Max()));
                var yBins = yBinRange.GetBins();
                yBins.Add(yBinRange.AddStep(yBins.Max()));

                if (xAom != null && yAom != null)
                {
                    foreach (var resultItem in model.QueryModel.ResultModel.ResultItemModels.Select(ri => ri as VisualizationItemResultModel))
                    {
                        double? xValue = (double?) resultItem.Values[xAom].Value;
                        double? yValue = (double?) resultItem.Values[yAom].Value;

                        if (xValue.HasValue && yValue.HasValue)
                        {
                            var xFrom = xBins[xBinRange.GetDisplayIndex(xValue.Value)];
                            var xTo = xBins[xBinRange.GetDisplayIndex(xValue.Value) + 1];

                            var yFrom = yBins[yBinRange.GetDisplayIndex(yValue.Value)];
                            var yTo = yBins[yBinRange.GetDisplayIndex(yValue.Value) + 1];

                            MapPolygon polygon = new MapPolygon();
                            polygon.Locations = new LocationCollection() 
                            {   
                                new Location(xFrom, yFrom), 
                                new Location(xTo, yFrom),
                                new Location(xTo, yTo), 
                                new Location(xFrom, yTo) 
                            };
                            var color = Windows.UI.Colors.Orange;
                            color.A = 150;

                            polygon.FillColor = color;

                            shapeLayer.Shapes.Add(polygon);
                        }
                    }
                }

                _mapInertiaHandler.Map.ShapeLayers.Add(shapeLayer);
            }
        }
        
        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (DataContext as VisualizationViewModel).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
            }
        }

        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
        }

        void loadResultItemModels(ResultModel resultModel)
        {
            render();
        }


        void map_PointerReleasedOverride(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        void map_PointerMovedOverride(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        void map_PointerPressedOverride(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            xyRenderer.Dispose();
            if (_pointerManager != null)
            {
                _pointerManager.Detach();
            }
        }

        void _pointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1 && _oneFingerListener != null)
            {
                _oneFingerListener.Pressed(this, e);
            }
            else if (e.NumActiveContacts == 2 && scrollPointer == null)
            {
                scrollPointer = e.TriggeringPointer;
                _mapInertiaHandler.Map.TryLocationToPixel(_mapInertiaHandler.Map.Center, out _startCenterPixels);
                _startCenter = _mapInertiaHandler.Map.Center;
                _startZoom = _mapInertiaHandler.Map.ZoomLevel;

                _initalFingerDiff = ((Pt)e.CurrentContacts[e.CurrentPointers[0].PointerId].Position).GetVec() - ((Pt)e.CurrentContacts[e.CurrentPointers[1].PointerId].Position).GetVec();

                var tg = this.TransformToVisual(_mapInertiaHandler.Map);
                 _startFingerCenterPoint = (((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() + ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec()) / 2.0f;
   
                _mapInertiaHandler.Map.TryLocationToPixel(_startCenter, out _startCenterPixels);
                _mapInertiaHandler.Center = _mapInertiaHandler.Map.Center;
                _mapInertiaHandler.StartCenter = _mapInertiaHandler.Map.Center;

                _initialFingerCenter = (((Pt)e.CurrentContacts[e.CurrentPointers[0].PointerId].Position).GetVec() + ((Pt)e.CurrentContacts[e.CurrentPointers[1].PointerId].Position).GetVec()) / 2.0f;

                _mapInertiaHandler.InertiaActive = false;
            }
        }

        void _pointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1 && _oneFingerListener != null)
            {
                _oneFingerListener.Moved(this, e);
            }
            else if (e.NumActiveContacts == 2 && scrollPointer != null && e.CurrentPointers.Contains(scrollPointer) && e.TriggeringPointer.PointerId == scrollPointer.PointerId)
            {
                _oneFingerListener.TwoFingerMoved();

                var tg = this.TransformToVisual(_mapInertiaHandler.Map);

                _mapInertiaHandler.Map.Center = _startCenter;
                _mapInertiaHandler.Map.ZoomLevel = _startZoom;

                Vec currentFingerDiff = ((Pt) tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() - ((Pt) tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec();
                Vec currentFingerCenterPoint = (((Pt) tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() + ((Pt) tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec())/2.0f;

                Vec delta = (_startFingerCenterPoint - currentFingerCenterPoint);

                Location newLocation = new Location();
                _mapInertiaHandler.Map.TryPixelToLocation((_startCenterPixels.GetVec()  + delta).GetWindowsPoint(), out newLocation);
                _mapInertiaHandler.Map.Center = newLocation;


                var zoomFactor = Math.Sqrt(currentFingerDiff.Length / _initalFingerDiff.Length);
                _mapInertiaHandler.Map.SetZoomLevelAroundPoint(_startZoom * zoomFactor, currentFingerCenterPoint.GetWindowsPoint(), MapAnimationDuration.None);


                _mapInertiaHandler.InertiaActive = false;
            }
        }

        void _pointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts < 2)
            {
                //_mapInertiaHandler.InertiaActive = true;
                if (e.NumActiveContacts < 1 && _oneFingerListener != null)
                {
                    _oneFingerListener.Released(this, e, e.IsRightMouse);
                }
            }
            if (scrollPointer != null && e.TriggeringPointer.PointerId == scrollPointer.PointerId)
            {
                scrollPointer = null;
            }
        }
    }



    class MapInertiaHandler : IDisposable
    {
        private DispatcherTimer _animationTimer;

        private Location _center;
        public Location Center
        {
            get { return _center; }
            set { _center = value; }
        }

        private Location _startCenter;
        public Location StartCenter
        {
            get { return _startCenter; }
            set { _startCenter = value; }
        }

        private Map _map;
        public Map Map
        {
            get { return _map; }
        }

        private double _zoomLevel;
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { _zoomLevel = value; }
        }

        private bool inertiaActive;
        public bool InertiaActive
        {
            get { return inertiaActive; }
            set { inertiaActive = value; }
        }

        private Vec velocity;
        public Vec Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public MapInertiaHandler(Map map)
        {
            this._map = map;
            this.InertiaActive = false;
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _animationTimer.Tick += HandleWorldTimerTick;
            _animationTimer.Start();
        }

        private void HandleWorldTimerTick(object sender, object e)
        {
            if (InertiaActive)
            {
                if (velocity.Length > 1)
                {
                    Point pixel = new Point();
                    _map.TryLocationToPixel(_center, out pixel);

                    Location newLocation = new Location();
                    if (_map.TryPixelToLocation((pixel.GetVec() + velocity).GetWindowsPoint(), out newLocation))
                    {
                        _map.Center = newLocation;
                        _center = newLocation;
                    }
                    velocity *= 0.95;
                }
            }
        }

        public void Dispose()
        {
            _animationTimer.Stop();
        }
    }
}
