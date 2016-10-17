using NetTopologySuite.Geometries;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking.Sockets;
using Windows.UI;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis.render
{
    public class SVGRendererContentProvider : DXSurfaceContentProvider
    {
        private string _svgFilePath = "";

        private bool _isResultEmpty = false;

        private float _leftOffset = 40;
        private float _rightOffset = 20;
        private float _topOffset = 20;
        private float _bottomtOffset = 45;

        private float _deviceWidth = 0;
        private float _deviceHeight = 0;
        private float _xScale = 0;
        private float _yScale = 0;

        private Color _textColor;
        private CanvasTextFormat _textFormat;

        private IResult _result = null;
        //private VisualizationResultDescriptionModel _visualizationDescriptionModel = null;

        private HistogramOperationModel _histogramOperationModelClone = null;
        private HistogramOperationModel _histogramOperationModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
        private Dictionary<FilterModel, Rect> _filterModelRects = new Dictionary<FilterModel, Rect>();
        private AttributeTransformationModel _xAom = null;
        private AttributeTransformationModel _yAom = null;
        private Dictionary<string, Dictionary<Brush, double>> _values = new Dictionary<string, Dictionary<Brush, double>>();
        private Dictionary<string, int> _index = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<Brush, double>> _countsInterpolated = new Dictionary<string, Dictionary<Brush, double>>();

        private CanvasCachedGeometry _fillRoundedRectGeom = null;
        private CanvasCachedGeometry _strokeRoundedRectGeom = null;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        public Dictionary<IGeometry, FilterModel> HitTargets { get; set; }

        private Dictionary<string, List<List<Vector2>>> _svgShapes = new Dictionary<string, List<List<Vector2>>>();
        private Dictionary<string, List<CanvasCachedGeometry>> _cachedGeometriesFilled = null;
        private Dictionary<string, Dictionary<float, List<CanvasCachedGeometry>>> _cachedGeometriesOutline = null;
        private MinMax2D _minMax = new MinMax2D();
        private List<float> _strokeScales = null;

        private static Dictionary<string, Dictionary<string, List<List<Vector2>>>>  _svgShapesCache = new Dictionary<string, Dictionary<string, List<List<Vector2>>>>();
        private static Dictionary<string, MinMax2D>  _minMaxCache = new Dictionary<string, MinMax2D>();

        public SVGRendererContentProvider(string svgFilePath)
        {
            _svgFilePath = svgFilePath;
            HitTargets = new Dictionary<IGeometry, FilterModel>();
            loadSVGData();
        }

        private async void loadSVGData()
        {
            if (!_svgShapesCache.ContainsKey(_svgFilePath) || !_minMaxCache.ContainsKey(_svgFilePath))
            {
                var minMax = new MinMax2D();
                var svgShapes = new Dictionary<string, List<List<Vector2>>>();

                _minMaxCache.Add(_svgFilePath, minMax);
                _svgShapesCache.Add(_svgFilePath, svgShapes);

                var installedLoc = Package.Current.InstalledLocation;
                string content = await installedLoc.GetFileAsync(@"Assets\svg\" + _svgFilePath).AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;
                JObject result = JObject.Parse(content);
                foreach (var obj in result)
                {
                    var key = obj.Key;
                    if (!svgShapes.ContainsKey(key))
                    {
                        svgShapes.Add(key, new List<List<Vector2>>());
                    }
                    List<List<Vector2>> shapes = svgShapes[key];
                    foreach (var shapeString in (JArray) obj.Value)
                    {
                        var data = shapeString.ToString();
                        var entries = data.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);

                        List<Vector2> currentShape = null;

                        foreach (var entry in entries)
                        {
                            var ptString = entry.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                            float x = 0;
                            float y = 0;

                            if (entry == "M")
                            {
                                currentShape = new List<Vector2>();
                                shapes.Add(currentShape);
                            }
                            else if (entry == "L")
                            {

                            }
                            else if (float.TryParse(ptString[0], out x) && float.TryParse(ptString[1], out y))
                            {
                                var pt = new Vector2(x, y);
                                currentShape.Add(pt);
                                minMax.Update(x, y);
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            _svgShapes = _svgShapesCache[_svgFilePath];
            _minMax = _minMaxCache[_svgFilePath];

           
        }

        public  void UpdateFilterModels(List<FilterModel> filterModels)
        {
            _filterModels = filterModels;
        }

        public async void UpdateData(IResult result, HistogramOperationModel histogramOperationModel, HistogramOperationModel histogramOperationModelClone, AttributeTransformationModel xAom, AttributeTransformationModel yAom)
        {
            _result = result;
            _histogramOperationModelClone = histogramOperationModelClone;
            _histogramOperationModel = histogramOperationModel;
            _xAom = xAom;
            _yAom = yAom;

            /*_visualizationDescriptionModel = _result.ResultDescriptionModel as VisualizationResultDescriptionModel;

            if (resultModel.ResultItemModels.Count > 0)
            {
                var yIndex = _visualizationDescriptionModel.Dimensions.IndexOf(yAom);
                var yBinRange = _visualizationDescriptionModel.BinRanges[yIndex];

                _values.Clear();
                _countsInterpolated.Clear();
                _index.Clear();
                var labels = yBinRange.GetLabels();
                foreach (var resultItem in _result.ResultItemModels.Select(ri => ri as ProgressiveVisualizationResultItemModel))
                {
                    var xValues = resultItem.Values[xAom];
                    var yValues = resultItem.Values[yAom];
                    var label = (yBinRange as NominalBinRange).LabelsValue[(int)yValues[BrushIndex.ALL]];

                    _index.Add(label, (int)yValues[BrushIndex.ALL]);
                    _values.Add(label, xValues);
                    _countsInterpolated.Add(label, resultItem.CountsInterpolated[xAom]);
                }
            }
            else if (resultModel.ResultItemModels.Count == 0 && resultModel.Progress == 1.0)
            {
                _isResultEmpty = _result.ResultType != ResultType.Clear; ;
            }*/
        }

        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_result != null)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderSVG(canvas, canvasArgs);
                }
            }

            //renderSVG(canvas, canvasArgs);

            if (_isResultEmpty)
            {
                _leftOffset = 10;
                _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - _leftOffset - _rightOffset);
                _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - _topOffset - _bottomtOffset);
                DrawString(canvasArgs, _textFormat, _deviceWidth / 2.0f + _leftOffset, _deviceHeight / 2.0f + _topOffset, "no datapoints", _textColor, true, true, false);
            }
            Debug.WriteLine("time to render county map : " + sw.ElapsedMilliseconds);
        }

        private void renderSVG(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            computeSizesAndRenderLabels(canvas, canvasArgs, true);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

       
            if (_cachedGeometriesFilled == null || _cachedGeometriesOutline == null)
            {
                _strokeScales = new List<float>();
                _strokeScales.Add(1);
                _strokeScales.Add(0.2f);

                _cachedGeometriesFilled = new Dictionary<string, List<CanvasCachedGeometry>>();
                _cachedGeometriesOutline = new Dictionary<string, Dictionary<float, List<CanvasCachedGeometry>>>();
                foreach (var key in _svgShapes.Keys)
                {
                    _cachedGeometriesOutline.Add(key, new Dictionary<float, List<CanvasCachedGeometry>>());
                    foreach (var strokeScale in _strokeScales)
                    {
                        _cachedGeometriesOutline[key].Add(strokeScale, new List<CanvasCachedGeometry>());
                    }
                    _cachedGeometriesFilled.Add(key, new List<CanvasCachedGeometry>());
                    foreach (var path in _svgShapes[key])
                    {
                        var geom = CanvasGeometry.CreatePolygon(canvas, path.Select(v => new Vector2(v.X - _minMax.MinX, v.Y - _minMax.MinY)).ToArray());
                        var fill = CanvasCachedGeometry.CreateFill(geom, CanvasGeometry.ComputeFlatteningTolerance(300, 5));
                        foreach (var strokeScale in _strokeScales)
                        {
                            _cachedGeometriesOutline[key][strokeScale].Add(CanvasCachedGeometry.CreateStroke(geom, strokeScale, new CanvasStrokeStyle()));
                        }
                        _cachedGeometriesFilled[key].Add(fill);

                    }
                }
            }

            var currentMat = canvasArgs.DrawingSession.Transform;
            var transMat = Matrix3x2.CreateTranslation(new Vector2(_leftOffset, _topOffset));
            var scaleMat = Matrix3x2.CreateScale(_xScale, _yScale);
            var mat = currentMat * scaleMat * transMat;
            canvasArgs.DrawingSession.Transform = mat;

            var white = Color.FromArgb(255, 255, 255, 255);
            var dark = Color.FromArgb(255, 11, 11, 11);

            //var x = _xScale;
            //float scaleIndex = _strokeScales.Select((s, i) => new { v = s, d = Math.Abs(s - x), i = i }).OrderBy(e => e.d).First().v;
            //int scaleIndex = _strokeScales.Select(s => s -  );


            foreach (var key in _svgShapes.Keys)
            {
                for (int i = 0; i < _svgShapes[key].Count; i++)
                {
                    //var geom = CanvasGeometry.CreatePolygon(canvas, path.Select(v => new Vector2(v.X - _minMax.MinX, v.Y - _minMax.MinY)).ToArray());
                    if (_values.ContainsKey("F" + key))
                    {/*
                        var value = _values["F" + key][BrushIndex.ALL];

                        double min = _visualizationDescriptionModel.MinValues[_xAom][BrushIndex.ALL];
                        double max = _visualizationDescriptionModel.MaxValues[_xAom][BrushIndex.ALL];

                        if (min - max == 0.0)
                        {
                            value = 1.0;
                        }
                        else
                        {
                            value = (value - min)/(max - min);
                        }
                        float alpha = 0.15f;
                        var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) (alpha + Math.Pow(value, 1.0/3.0)*(1.0 - alpha)));
                        var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                        //var dataColor = Color.FromArgb((byte)((0.10 + (Math.Pow(value, 1.0 / 3.0)) * (1.0 - 0.10)) * 255), 40, 170, 213);

                        canvasArgs.DrawingSession.DrawCachedGeometry(_cachedGeometriesFilled[key][i], dataColor);

                        var brushCount = 0;
                        foreach (var brushIndex in _visualizationDescriptionModel.BrushIndices.Where(bi => bi != BrushIndex.ALL))
                        {

                            Color brushColor = Color.FromArgb(255, 17, 17, 17);
                            if (_histogramOperationModelClone.BrushColors.Count > brushCount)
                            {
                                brushColor = _histogramOperationModelClone.BrushColors[brushCount];
                            }

                            var brushLerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), brushColor, (float) (alpha + Math.Pow(value, 1.0/3.0)*(1.0 - alpha)));
                            var renderColor = Color.FromArgb(255, brushLerpColor.R, brushLerpColor.G, brushLerpColor.B);
                            //var renderColor = Color.FromArgb((byte)((0.10 + (Math.Pow(value, 1.0 / 3.0)) * (1.0 - 0.10)) * 255), brushColor.R, brushColor.G, brushColor.B);

                            var allUnNormalizedValue = _countsInterpolated["F" + key][BrushIndex.ALL];
                            var brushUnNormalizedValue = _countsInterpolated["F" + key][brushIndex];
                            var brushFactor = (brushUnNormalizedValue/allUnNormalizedValue);

                          
                            if (brushFactor > 0)
                            {
                                //canvasArgs.DrawingSession.FillGeometry(geom, renderColor);
                                canvasArgs.DrawingSession.DrawCachedGeometry(_cachedGeometriesFilled[key][i], renderColor);
                            }
                            brushCount++;
                        }
                        //canvasArgs.DrawingSession.DrawGeometry(geom, white, 0.1f/_xScale);
                        canvasArgs.DrawingSession.DrawCachedGeometry(_cachedGeometriesOutline[key][0.2f][i], white);*/
                    }
                    else
                    {
                        //canvasArgs.DrawingSession.DrawGeometry(geom, white, 1f / _xScale);
                        canvasArgs.DrawingSession.DrawCachedGeometry(_cachedGeometriesOutline[key][0.2f][i], white);
                    }

                }
            }
            HitTargets.Clear();
            foreach (var key in _svgShapes.Keys)
            {
                for (int i = 0; i < _svgShapes[key].Count; i++)
                {
                    var path = _svgShapes[key][i];
                    var label = "F" + key;
                    if (_index.ContainsKey(label))
                    {
                        var index = _index[label];

                        if (path.Count > 4)
                        {
                            IGeometry hitGeom = path.Select(v => new Pt((v.X - _minMax.MinX)*_xScale + _leftOffset, (v.Y - _minMax.MinY)*_yScale + _topOffset)).GetPolygon();
                            var filterModel = new FilterModel();
                            filterModel.Value = index;
                            filterModel.GroupAggregateComparisons = "f";

                            filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.EQUALS, label));
                            if (!HitTargets.ContainsKey(hitGeom))
                            {
                                HitTargets.Add(hitGeom, filterModel);


                                if (_filterModels.Contains(filterModel))
                                {
                                    //var geom = CanvasGeometry.CreatePolygon(canvas, path.Select(v => new Vector2(v.X - _minMax.MinX, v.Y - _minMax.MinY)).ToArray());
                                    //canvasArgs.DrawingSession.DrawGeometry(geom, dark, 1f/_xScale);
                                    canvasArgs.DrawingSession.DrawCachedGeometry(_cachedGeometriesOutline[key][1][i], dark);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void computeSizesAndRenderLabels(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines)
        {
            _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - 10 - 10);
            _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - 10 - 10);

            _xScale = _deviceWidth / (_minMax.MaxX - _minMax.MinX);
            _yScale = _deviceHeight / (_minMax.MaxY - _minMax.MinY);

            _xScale = Math.Min(_xScale, _yScale);
            _yScale = Math.Min(_xScale, _yScale);

            float xOff = _deviceWidth - ((_minMax.MaxX - _minMax.MinX) *_xScale);
            _leftOffset = 10f + xOff/2.0f;

            float yOff = _deviceHeight - ((_minMax.MaxY - _minMax.MinY) * _xScale);
            _topOffset = 10f + yOff / 2.0f;
        }

        public override void Load(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs canvasArgs)
        {
            _textFormat = new CanvasTextFormat()
            {
                FontSize = 11,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textColor = Color.FromArgb(255, 17, 17, 17);
        }
    }

    public class MinMax2D
    {
        private float _minX = float.MaxValue;
        public float MinX
        {
            get { return _minX; }
            set { _minX = value; }
        }

        private float _minY = float.MaxValue;
        public float MinY
        {
            get { return _minY; }
            set { _minY = value; }
        }

        private float _maxX = float.MinValue;
        public float MaxX
        {
            get { return _maxX; }
            set { _maxX = value; }
        }

        private float _maxY = float.MinValue;
        public float MaxY
        {
            get { return _maxY; }
            set { _maxY = value; }
        }

        public void Update(float x, float y)
        {
            _minX = Math.Min(_minX, x);
            _minY = Math.Min(_minY, y);

            _maxX = Math.Max(_maxX, x);
            _maxY = Math.Max(_maxY, y);
        }
    }
}
