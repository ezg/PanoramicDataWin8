using NetTopologySuite.Geometries;
using PanoramicDataWin8.controller.data.sim;
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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
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
        private float _minX = 0;
        private float _minY = 0;
        private float _maxX = 0;
        private float _maxY = 0;

        private Color _textColor;
        private CanvasTextFormat _textFormat;

        private ResultModel _resultModel = null;
        private VisualizationResultDescriptionModel _visualizationDescriptionModel = null;

        private QueryModel _queryModelClone = null;
        private QueryModel _queryModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
        private Dictionary<FilterModel, Rect> _filterModelRects = new Dictionary<FilterModel, Rect>();
        private InputOperationModel _xAom = null;
        private Dictionary<string, Dictionary<BrushIndex, double>> _values = new Dictionary<string, Dictionary<BrushIndex, double>>();
        private Dictionary<string, Dictionary<BrushIndex, double>> _countsInterpolated = new Dictionary<string, Dictionary<BrushIndex, double>>();

        private CanvasCachedGeometry _fillRoundedRectGeom = null;
        private CanvasCachedGeometry _strokeRoundedRectGeom = null;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        public Dictionary<IGeometry, FilterModel> HitTargets { get; set; }

        private Dictionary<string, List<List<Vector2>>> _svgShapes = new Dictionary<string, List<List<Vector2>>>();

        public SVGRendererContentProvider(string svgFilePath)
        {
            _svgFilePath = svgFilePath;
            HitTargets = new Dictionary<IGeometry, FilterModel>();
            loadSVGData();
        }

        private async void loadSVGData()
        {
            _minX = float.MaxValue;
            _minY = float.MaxValue;
            _maxX = float.MinValue;
            _maxY = float.MinValue;

            var installedLoc = Package.Current.InstalledLocation;
            string content = await installedLoc.GetFileAsync(@"Assets\svg\" + _svgFilePath).AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;
            JObject result = JObject.Parse(content);
            foreach (var obj in result)
            {
                var key = obj.Key;
                if (!_svgShapes.ContainsKey(key))
                {
                    _svgShapes.Add(key, new List<List<Vector2>>());
                }
                List<List<Vector2>> shapes = _svgShapes[key];
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
                            _minX = Math.Min(_minX, x);
                            _minY = Math.Min(_minY, y);

                            _maxX = Math.Max(_maxX, x);
                            _maxY = Math.Max(_maxY, y);
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
        }

        public void UpdateFilterModels(List<FilterModel> filterModels)
        {
            _filterModels = filterModels;
        }

        public async void UpdateData(ResultModel resultModel, QueryModel queryModel, QueryModel queryModelClone, InputOperationModel xAom, InputOperationModel yAom)
        {
            _resultModel = resultModel;
            _queryModelClone = queryModelClone;
            _queryModel = queryModel;
            _xAom = xAom;

            _visualizationDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;

            if (resultModel.ResultItemModels.Count > 0)
            {
                var yIndex = _visualizationDescriptionModel.Dimensions.IndexOf(yAom);
                var yBinRange = _visualizationDescriptionModel.BinRanges[yIndex];

                _values.Clear();
                _countsInterpolated.Clear();
                var labels = yBinRange.GetLabels();
                foreach (var resultItem in _resultModel.ResultItemModels.Select(ri => ri as ProgressiveVisualizationResultItemModel))
                {
                    var xValues = resultItem.Values[xAom];
                    var yValues = resultItem.Values[yAom];
                    var label = (yBinRange as NominalBinRange).LabelsValue[(int)yValues[BrushIndex.ALL]];

                    _values.Add(label, xValues);
                    _countsInterpolated.Add(label, resultItem.CountsInterpolated[xAom]);
                }
            }
            else if (resultModel.ResultItemModels.Count == 0 && resultModel.Progress == 1.0)
            {
                _isResultEmpty = _resultModel.ResultType != ResultType.Clear; ;
            }
        }

        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_resultModel != null && _resultModel.ResultItemModels.Count > 0)
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
        }

        private void renderSVG(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            computeSizesAndRenderLabels(canvas, canvasArgs, true);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var currentMat = canvasArgs.DrawingSession.Transform;
            var transMat = Matrix3x2.CreateTranslation(new Vector2(_leftOffset, _topOffset));
            var scaleMat = Matrix3x2.CreateScale(_xScale, _yScale);
            var mat = currentMat * scaleMat * transMat;
            canvasArgs.DrawingSession.Transform = mat;

            var white = Color.FromArgb(255, 255, 255, 255);
            var dark = Color.FromArgb(255, 11, 11, 11);

            foreach (var key in _svgShapes.Keys)
            {
                foreach (var path in _svgShapes[key])
                {
                    var geom = CanvasGeometry.CreatePolygon(canvas, path.Select(v => new Vector2(v.X - _minX, v.Y - _minY)).ToArray());
                    if (_values.ContainsKey("F" + key))
                    {
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
                        var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) Math.Sqrt(value));
                        var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);

                        canvasArgs.DrawingSession.FillGeometry(geom, dataColor);

                         

                        var brushCount = 0;
                        foreach (var brushIndex in _visualizationDescriptionModel.BrushIndices.Where(bi => bi != BrushIndex.ALL))
                        {

                            Color brushColor = Color.FromArgb(255, 17, 17, 17);
                            if (_queryModelClone.BrushColors.Count > brushCount)
                            {
                                brushColor = _queryModelClone.BrushColors[brushCount];
                            }

                            var brushLerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), brushColor, (float) Math.Sqrt(value));
                            var renderColor = Color.FromArgb(255, brushLerpColor.R, brushLerpColor.G, brushLerpColor.B);

                            var allUnNormalizedValue = _countsInterpolated["F" + key][BrushIndex.ALL];
                            var brushUnNormalizedValue = _countsInterpolated["F" + key][brushIndex];
                            var brushFactor = (brushUnNormalizedValue/allUnNormalizedValue);

                            /*var ratio = (rect.Width/rect.Height);
                            var newHeight = Math.Sqrt((1.0/ratio)*((rect.Width*rect.Height)*brushFactor));
                            var newWidth = newHeight*ratio;

                            var brushRect = new Rect(rect.X + (rect.Width - newWidth)/2.0f, rect.Y + (rect.Height - newHeight)/2.0f, newWidth, newHeight);
                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, renderColor);*/
                            if (brushFactor > 0)
                            {
                                canvasArgs.DrawingSession.FillGeometry(geom, renderColor);
                            }
                            brushCount++;
                        }
                        canvasArgs.DrawingSession.DrawGeometry(geom, white, 0.1f/_xScale);
                    }
                    else
                    {
                        canvasArgs.DrawingSession.DrawGeometry(geom, white, 1f / _xScale);
                    }
                    
                }
            }
            

        }

        private void computeSizesAndRenderLabels(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines)
        {
            _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - 10 - 10);
            _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - 10 - 10);

            _xScale = _deviceWidth / (_maxX - _minX);
            _yScale = _deviceHeight / (_maxY - _minY);

            _xScale = Math.Min(_xScale, _yScale);
            _yScale = Math.Min(_xScale, _yScale);

            float xOff = _deviceWidth - ((_maxX - _minX)*_xScale);
            _leftOffset = 10f + xOff/2.0f;

            float yOff = _deviceHeight - ((_maxY - _minY) * _xScale);
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
}