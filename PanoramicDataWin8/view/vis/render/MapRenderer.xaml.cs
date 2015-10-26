using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using GeoAPI.Geometries;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.style;
using PointCollection = Esri.ArcGISRuntime.Geometry.PointCollection;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class MapRenderer : Renderer
    {
        private PointerManager _pointerManager = null;
        private IOneFingerListener _oneFingerListener = null;

        private MapInertiaHandler _mapInertiaHandler = null;
        private Pointer scrollPointer = null;

        private Viewpoint _previousViewpoint = null;

        private Location _startCenter = new Location();
        private Vec _previousFingerDiff = new Vec();
        private Vec _previousFingerCenter = new Vec();

        private double _startZoom = 0;

        public MapRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
        }

        void MapView_Loaded(object sender, RoutedEventArgs args)
        {
            (sender as MapView).Map.Layers.Remove("tiledLayer");
            WebTiledLayer tiledLayer = new WebTiledLayer() { ID = "tiledLayer" };
            tiledLayer.TemplateUri = "https://api.mapbox.com/v4/ezgraggen.41f60101/{level}/{col}/{row}@2x.png?access_token=pk.eyJ1IjoiZXpncmFnZ2VuIiwiYSI6ImNpZm9tbmdteWhpeTRzNG03M3J1bnpneHAifQ.tTw2Dwj64wUza2n_dJPk9A";
            tiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
            (sender as MapView).Map.Layers.Add(tiledLayer);

            _pointerManager = new PointerManager();
            _pointerManager.Added += _pointerManager_Added;
            _pointerManager.Moved += _pointerManager_Moved;
            _pointerManager.Removed += _pointerManager_Removed;
            _pointerManager.Attach(this);

            _oneFingerListener = this.GetAncestors().FirstOrDefault(a => a is IOneFingerListener) as IOneFingerListener;

            _mapInertiaHandler = new MapInertiaHandler(sender as MapView);

            xyRenderer.Render += render;
            xyRenderer.LoadResultItemModels += loadResultItemModels;
        }

        void render(bool sizeChanged = false)
        {
            if (!sizeChanged)
            {
                VisualizationViewModel model = (DataContext as VisualizationViewModel);
                var graphicsOverlay = _mapInertiaHandler.MapView.GraphicsOverlays["graphicsOverlay"];
                List<Graphic> graphics = new List<Graphic>();

                if (model.QueryModel.ResultModel != null && model.QueryModel.ResultModel.ResultItemModels.Any())
                {
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

                                double? value = null;
                                double? unNormalizedvalue = null;
                                if (model.QueryModel.GetUsageInputOperationModel(InputUsage.Value).Any() && resultItem.Values.ContainsKey(model.QueryModel.GetUsageInputOperationModel(InputUsage.Value).First()))
                                {
                                    unNormalizedvalue = (double?) resultItem.Values[model.QueryModel.GetUsageInputOperationModel(InputUsage.Value).First()].Value;
                                    value = (double?) resultItem.Values[model.QueryModel.GetUsageInputOperationModel(InputUsage.Value).First()].NoramlizedValue;
                                }
                                else if (model.QueryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).Any() && resultItem.Values.ContainsKey(model.QueryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()))
                                {
                                    unNormalizedvalue = (double?) resultItem.Values[model.QueryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].Value;
                                    value = (double?) resultItem.Values[model.QueryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].NoramlizedValue;
                                }

                                if (value != null)
                                {
                                    //if (MyMapView.WrapAround)
                                    //mapPoint = GeometryEngine.NormalizeCentralMeridian(mapPoint) as MapPoint;

                                    PointCollection coords = new PointCollection();
                                    coords.Add(new MapPoint(xFrom, yFrom));
                                    coords.Add(new MapPoint(xTo, yFrom));
                                    coords.Add(new MapPoint(xTo, yTo));
                                    coords.Add(new MapPoint(xFrom, yTo));
                                    coords.Add(new MapPoint(xFrom, yFrom));

                                    Polygon polygon = new Polygon(
                                        new List<PointCollection> {coords},
                                        SpatialReferences.Wgs84);



                                    float alpha = 0.1f*(float) Math.Log10(value.Value) + 1f;
                                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) Math.Sqrt(value.Value));
                                    lerpColor.A = 150;
                                    var redSymbol = new SimpleFillSymbol() {Color = lerpColor, Outline = new SimpleLineSymbol() {Color = Colors.White, Width = 0.5}};

                                    graphics.Add(new Graphic() {Geometry = polygon, Symbol = redSymbol});
                                }
                            }
                        }
                    }
                    graphicsOverlay.GraphicsSource = graphics;
                }
            }
        }

        private Point ToGeographic(Point pnt)
        {
            double mercatorX_lon = pnt.X;
            double mercatorY_lat = pnt.Y;
            if (Math.Abs(mercatorX_lon) < 180 && Math.Abs(mercatorY_lat) < 90)
                return pnt;

            if ((Math.Abs(mercatorX_lon) > 20037508.3427892) || (Math.Abs(mercatorY_lat) > 20037508.3427892))
                return pnt;

            double x = mercatorX_lon;
            double y = mercatorY_lat;
            double num3 = x / 6378137.0;
            double num4 = num3 * 57.295779513082323;
            double num5 = Math.Floor((double)((num4 + 180.0) / 360.0));
            double num6 = num4 - (num5 * 360.0);
            double num7 = 1.5707963267948966 - (2.0 * Math.Atan(Math.Exp((-1.0 * y) / 6378137.0)));
            mercatorX_lon = num6;
            mercatorY_lat = num7 * 57.295779513082323;
            return new Point(mercatorX_lon, mercatorY_lat);
        }

        private Point ToWebMercator(Point pnt)
        {
            double mercatorX_lon = pnt.X;
            double mercatorY_lat = pnt.Y;
            if ((Math.Abs(mercatorX_lon) > 180 || Math.Abs(mercatorY_lat) > 90))
                return pnt;

            double num = mercatorX_lon * 0.017453292519943295;
            double x = 6378137.0 * num;
            double a = mercatorY_lat * 0.017453292519943295;

            mercatorX_lon = x;
            mercatorY_lat = 3189068.5 * Math.Log((1.0 + Math.Sin(a)) / (1.0 - Math.Sin(a)));
            return new Point(mercatorX_lon, mercatorY_lat);
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
                _previousViewpoint = _mapInertiaHandler.MapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                
                var tg = this.TransformToVisual(_mapInertiaHandler.MapView);
                _previousFingerDiff = ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() - ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec();
                _previousFingerCenter = (((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() + ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec()) / 2.0f;

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

                var tg = this.TransformToVisual(_mapInertiaHandler.MapView);
                Vec currentFingerDiff = ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() - ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec();
                Vec currentFingerCenter = (((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[0].PointerId].Position)).GetVec() + ((Pt)tg.TransformPoint(e.CurrentContacts[e.CurrentPointers[1].PointerId].Position)).GetVec()) / 2.0f;

                Vec delta = (_previousFingerCenter - currentFingerCenter);
                var zoomFactor = _previousFingerDiff.Length / currentFingerDiff.Length;

                var previousCenterInPixel = _mapInertiaHandler.MapView.LocationToScreen(_previousViewpoint.TargetGeometry as MapPoint);
               
                Mat mat = Mat.Translate(1 * currentFingerCenter) * Mat.Scale(zoomFactor, zoomFactor) * Mat.Translate(-1 * currentFingerCenter) * Mat.Translate(delta);
                Pt newCenterInPixel = mat * previousCenterInPixel;

                var newCenterMapPoint = _mapInertiaHandler.MapView.ScreenToLocation(newCenterInPixel);
                if (newCenterMapPoint != null)
                {
                    Viewpoint currentViewpoint = new ViewpointCenter(newCenterMapPoint, _previousViewpoint.TargetScale.Value*zoomFactor);
                    _mapInertiaHandler.MapView.SetView(currentViewpoint);

                    _previousViewpoint = currentViewpoint;
                    _previousFingerDiff = currentFingerDiff;
                    _previousFingerCenter = currentFingerCenter;
                }

                _mapInertiaHandler.InertiaActive = false;
            }
        }

        void _pointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts < 2)
            {
                _mapInertiaHandler.InertiaActive = true;
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

        private MapView _mapView;
        public MapView MapView
        {
            get { return _mapView; }
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

        public MapInertiaHandler(MapView mapView)
        {
            this._mapView = mapView;
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
                    /*Point pixel = new Point();
                    _map.TryLocationToPixel(_center, out pixel);

                    Location newLocation = new Location();
                    if (_map.TryPixelToLocation((pixel.GetVec() + velocity).GetWindowsPoint(), out newLocation))
                    {
                        _map.Center = newLocation;
                        _center = newLocation;
                    }
                    velocity *= 0.95;*/
                }
            }
        }

        public void Dispose()
        {
            _animationTimer.Stop();
        }
    }
}
