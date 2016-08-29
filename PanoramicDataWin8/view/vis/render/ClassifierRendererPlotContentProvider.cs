using NetTopologySuite.Geometries;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking.Sockets;
using Windows.UI;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis.render
{
    public class ClassifierRendererPlotContentProvider : DXSurfaceContentProvider
    {
        private bool _isResultEmpty = false;

        private float _leftOffset = 40;
        private float _rightOffset = 20;
        private float _topOffset = 20;
        private float _bottomOffset = 45;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }

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
        
        private BinRange _xBinRange = null;
        private BinRange _yBinRange = null;
        private bool _isXAxisAggregated = false;
        private bool _isYAxisAggregated = false;
        private int _xIndex = -1;
        private int _yIndex = -1;
        private InputOperationModel _xAom = null;
        private InputOperationModel _yAom = null;
        private InputOperationModel _vAom = null;
        private Dictionary<BinIndex, List<ProgressiveVisualizationResultItemModel>> _binDictonary = null;
        private Dictionary<BinIndex, BinPrimitive> _binPrimitives = new Dictionary<BinIndex, BinPrimitive>();

        private CanvasCachedGeometry _fillRoundedRectGeom = null;
        private CanvasCachedGeometry _strokeRoundedRectGeom = null;
        

        public ClassifierRendererPlotContentProvider()
        {
        }

        public void UpdateData(ResultModel resultModel, InputOperationModel xAom, InputOperationModel yAom, InputOperationModel vAom)
        {
            _resultModel = resultModel;
            _xAom = xAom;
            _yAom = yAom;
            _vAom = vAom;

            _visualizationDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;

            if (resultModel.ResultItemModels.Count > 0)
            {
                _xIndex = _visualizationDescriptionModel.Dimensions.IndexOf(xAom);
                _yIndex = _visualizationDescriptionModel.Dimensions.IndexOf(yAom);
                _isResultEmpty = false;

                if (!(_visualizationDescriptionModel.BinRanges[_xIndex] is AggregateBinRange))
                {
                    _xBinRange = _visualizationDescriptionModel.BinRanges[_xIndex];
                    _isXAxisAggregated = false;
                }
                else
                {
                    double factor = 0.0;
                    if (_visualizationDescriptionModel.MinValues[xAom][BrushIndex.ALL] - _visualizationDescriptionModel.MaxValues[xAom][BrushIndex.ALL] == 0)
                    {
                        factor = 0.1;
                        factor *= _visualizationDescriptionModel.MinValues[xAom][BrushIndex.ALL] < 0 ? -1f : 1f;
                    }
                    _isXAxisAggregated = true;
                    _xBinRange = QuantitativeBinRange.Initialize(_visualizationDescriptionModel.MinValues[xAom][BrushIndex.ALL] * (1.0 - factor), _visualizationDescriptionModel.MaxValues[xAom][BrushIndex.ALL] * (1.0 + factor), 10, false);
                }

                if (!(_visualizationDescriptionModel.BinRanges[_yIndex] is AggregateBinRange))
                {
                    _yBinRange = _visualizationDescriptionModel.BinRanges[_yIndex];
                    _isYAxisAggregated = false;
                }
                else
                {
                    double factor = 0.0;
                    if (_visualizationDescriptionModel.MinValues[yAom][BrushIndex.ALL] - _visualizationDescriptionModel.MaxValues[yAom][BrushIndex.ALL] == 0)
                    {
                        factor = 0.1;
                        factor *= _visualizationDescriptionModel.MinValues[yAom][BrushIndex.ALL] < 0 ? -1f : 1f;
                    }
                    _isYAxisAggregated = true;
                    _yBinRange = QuantitativeBinRange.Initialize(_visualizationDescriptionModel.MinValues[yAom][BrushIndex.ALL] * (1.0 - factor), _visualizationDescriptionModel.MaxValues[yAom][BrushIndex.ALL] * (1.0 + factor), 10, false);
                }

                // scale axis to 0 if this is a bar chart
                if (_isXAxisAggregated && !_isYAxisAggregated &&
                    (xAom.AggregateFunction == AggregateFunction.Count || xAom.AggregateFunction == AggregateFunction.Sum || xAom.AggregateFunction == AggregateFunction.Avg || xAom.AggregateFunction == AggregateFunction.Min || xAom.AggregateFunction == AggregateFunction.Max))
                {
                    _xBinRange = QuantitativeBinRange.Initialize(Math.Min(0, _visualizationDescriptionModel.MinValues[xAom][BrushIndex.ALL]), _xBinRange.DataMaxValue, 10, false);
                }
                if (!_isXAxisAggregated && _isYAxisAggregated &&
                    (yAom.AggregateFunction == AggregateFunction.Count || yAom.AggregateFunction == AggregateFunction.Sum || yAom.AggregateFunction == AggregateFunction.Avg || yAom.AggregateFunction == AggregateFunction.Min || yAom.AggregateFunction == AggregateFunction.Max))
                {
                    _yBinRange = QuantitativeBinRange.Initialize(Math.Min(0, _visualizationDescriptionModel.MinValues[yAom][BrushIndex.ALL]), _yBinRange.DataMaxValue, 10, false);
                }

                // create bin dictionary
                var resultDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;
                _binDictonary = new Dictionary<BinIndex, List<ProgressiveVisualizationResultItemModel>>();
                foreach (var resultItem in _resultModel.ResultItemModels.Select(ri => ri as ProgressiveVisualizationResultItemModel))
                {
                    if (resultItem.Values.ContainsKey(xAom) && resultItem.Values.ContainsKey(yAom))
                    {
                        double xValue = (double)resultItem.Values[xAom][BrushIndex.ALL];
                        double yValue = (double)resultItem.Values[yAom][BrushIndex.ALL];

                        BinIndex binIndex = new BinIndex(
                            resultDescriptionModel.BinRanges[_xIndex].GetIndex(xValue),
                            resultDescriptionModel.BinRanges[_yIndex].GetIndex(yValue));
                        if (!_binDictonary.ContainsKey(binIndex))
                        {
                            _binDictonary.Add(binIndex, new List<ProgressiveVisualizationResultItemModel>());
                        }
                        _binDictonary[binIndex].Add(resultItem);
                    }
                }
            }
            else if (resultModel.ResultItemModels.Count == 0 && resultModel.Progress == 1.0)
            {
                _isResultEmpty = _resultModel.ResultType != ResultType.Clear; ;
            }
        }

        public void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
        }

        public void render(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, 
            float leftOffset, float rightOffset, float topOffset, float bottomOffset, float deviceWidth, float deviceHeight)
        {
            this._leftOffset = leftOffset;
            this._rightOffset = rightOffset;
            this._topOffset = topOffset;
            this._bottomOffset = bottomOffset;
            this._deviceWidth = deviceWidth;
            this._deviceHeight = deviceHeight;

            _textFormat = new CanvasTextFormat()
            {
                FontSize = 11,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textColor = Color.FromArgb(255, 17, 17, 17);

            if (_resultModel != null && _resultModel.ResultItemModels.Count > 0)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderCell(canvas, canvasArgs);
                }
            }
            if (_isResultEmpty)
            {
                _leftOffset = 10;
                _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - _leftOffset - _rightOffset);
                _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - _topOffset - _bottomOffset);
                DrawString(canvasArgs, _textFormat, _deviceWidth / 2.0f + _leftOffset, _deviceHeight / 2.0f + _topOffset, "no datapoints", _textColor, true, true, false);
            }
        }

        private void computeSizesAndRenderLabels(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines)
        {
            //var xLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelX.TrimTo(20), MinValue = bin.MinX, MaxValue = bin.MaxX }).Distinct().ToList();
            //var yLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelY.TrimTo(20), MinValue = bin.MinY, MaxValue = bin.MaxY }).Distinct().ToList();
            var xLabels = _xBinRange.GetLabels();
            var yLabels = _yBinRange.GetLabels();
            var maxXLabelLength = xLabels.Max(b => b.Label.Length);
            var maxXLabel = xLabels.First(b => b.Label.Length == maxXLabelLength);
            var maxYLabelLength = yLabels.Max(b => b.Label.Length);
            var maxYLabel = yLabels.First(b => b.Label.Length == maxYLabelLength);

            var layoutX = new CanvasTextLayout(canvas, maxXLabel.Label, _textFormat, 1000f, 1000f);
            var metricsX = layoutX.DrawBounds;
            var layoutY = new CanvasTextLayout(canvas, maxYLabel.Label, _textFormat, 1000f, 1000f);
            var metricsY = layoutY.DrawBounds;

            _leftOffset = 20;

            _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - _leftOffset - _rightOffset);
            _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - _topOffset - _bottomOffset);

            _minX = (float)(xLabels.Min(dp => dp.MinValue));
            _minY = (float)(yLabels.Min(dp => dp.MinValue));
            _maxX = (float)(xLabels.Max(dp => dp.MaxValue));
            _maxY = (float)(yLabels.Max(dp => dp.MaxValue));

            _xScale = _maxX - _minX;
            _yScale = _maxY - _minY;

            var white = Color.FromArgb(255, 255, 255, 255);

            float xFrom = 0;
            float xTo = 0;
            float yFrom = 0;
            float yTo = 0;
            bool lastLabel = false;

            if (_deviceWidth > 0 && _deviceHeight > 0)
            {
                // x labels and grid lines
                int mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceWidth / (metricsX.Width + 5))) / xLabels.Count));
                int count = 0;
                foreach (var label in xLabels)
                {
                    yFrom = toScreenY(_minY);
                    yTo = toScreenY(_maxY);
                    xFrom = toScreenX((float)label.MinValue);
                    xTo = toScreenX((float)label.MaxValue);
                    lastLabel = count + 1 == xLabels.Count;

                    if (renderLines)
                    {
                        canvasArgs.DrawingSession.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xFrom, yTo), white, 0.5f);
                        if (lastLabel)
                        {
                            canvasArgs.DrawingSession.DrawLine(new Vector2(xTo, yFrom), new Vector2(xTo, yTo), white, 0.5f);
                        }
                    }
                    if (count % mod == 0)
                    {
                        if (_visualizationDescriptionModel.AxisTypes[_xIndex] == AxisType.Quantitative)
                        {
                            DrawString(canvasArgs, _textFormat, xFrom, yFrom + 5, double.Parse(label.Label).ToString(), _textColor, true, true, false);
                            if (lastLabel)
                            {
                                DrawString(canvasArgs, _textFormat, xTo, yFrom + 5, label.MaxValue.ToString(), _textColor, true, true, false);
                            }
                        }
                        else
                        {
                            DrawString(canvasArgs, _textFormat, xFrom + (xTo - xFrom) / 2.0f, yFrom + 5, label.Label.ToString(), _textColor, true, true, false);
                        }
                    }
                    count++;
                }

                // y labels and grid lines
                mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceHeight / (metricsY.Height + 0))) / yLabels.Count));
                count = 0;
                foreach (var label in yLabels)
                {
                    xFrom = toScreenX(_minX);
                    xTo = toScreenX(_maxX);
                    yFrom = toScreenY((float)label.MinValue);
                    yTo = toScreenY((float)label.MaxValue);
                    lastLabel = count + 1 == yLabels.Count;

                    if (renderLines)
                    {
                        canvasArgs.DrawingSession.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xTo, yFrom), white, 0.5f);
                        if (lastLabel)
                        {
                            canvasArgs.DrawingSession.DrawLine(new Vector2(xFrom, yTo), new Vector2(xTo, yTo), white, 0.5f);
                        }
                    }
                    
                    count++;
                }

                if (_fillRoundedRectGeom != null)
                {
                    _fillRoundedRectGeom.Dispose();
                }
                if (_strokeRoundedRectGeom != null)
                {
                    _strokeRoundedRectGeom.Dispose();
                }
                var x = toScreenX((float)_xBinRange.AddStep(0)) - toScreenX(0);
                var y = toScreenY((float)_yBinRange.AddStep(0), false) - toScreenY(0, false);

                _fillRoundedRectGeom = CanvasCachedGeometry.CreateFill(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4));
                _strokeRoundedRectGeom = CanvasCachedGeometry.CreateStroke(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4), 0.5f);
            }

            layoutX.Dispose();
            layoutY.Dispose();
        }

        private void renderCell(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            computeSizesAndRenderLabels(canvas, canvasArgs, true);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var white = Color.FromArgb(255, 255, 255, 255);
            var dark = Color.FromArgb(255, 11, 11, 11);

            var xBins = _xBinRange.GetBins();
            xBins.Add(_xBinRange.AddStep(xBins.Max()));
            var yBins = _yBinRange.GetBins();
            yBins.Add(_yBinRange.AddStep(yBins.Max()));

            // draw data
            var resultDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;
            var rect = new Rect();
            float xFrom = 0;
            float yFrom = 0;
            float xTo = 0;
            float yTo = 0;

            float xFromMargin = 0;
            float yFromMargin = 0;
            float xToMargin = 0;
            float yToMargin = 0;

            for (int xi = 0; xi < resultDescriptionModel.BinRanges[_xIndex].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < resultDescriptionModel.BinRanges[_yIndex].GetBins().Count; yi++)
                {
                    BinIndex binIndex = new BinIndex(xi, yi);
                    if (_binDictonary.ContainsKey(binIndex))
                    {
                        foreach (var resultItem in _binDictonary[binIndex])
                        {
                            double xValue = resultItem.Values[_xAom][BrushIndex.ALL];
                            double yValue = resultItem.Values[_yAom][BrushIndex.ALL];
                            double value = 0;
                            double unNormalizedvalue = 0;
                            double xMargin = 0;
                            double xMarginAbsolute = 0;
                            double yMargin = 0;
                            double yMarginAbsolute = 0;
                            double valueMargin = 0;
                            double valueMarginAbsolute = 0;

                            InputOperationModel iom = _vAom;
                            unNormalizedvalue = resultItem.Values[iom][BrushIndex.ALL];
                            double min = resultDescriptionModel.MinValues[iom][BrushIndex.ALL];
                            double max = resultDescriptionModel.MaxValues[iom][BrushIndex.ALL];

                            if (min - max == 0.0)
                            {
                                value = 1.0;
                            }
                            else
                            {
                                value = (unNormalizedvalue - min) / (max - min);
                            }
                            valueMargin = resultItem.Margins[iom][BrushIndex.ALL];
                            valueMarginAbsolute = resultItem.MarginsAbsolute[iom][BrushIndex.ALL];

                            if (value != null && unNormalizedvalue != 0.0)
                            {
                                if (_isXAxisAggregated && !_isYAxisAggregated &&
                                    (_xAom.AggregateFunction == AggregateFunction.Count || _xAom.AggregateFunction == AggregateFunction.Sum || _xAom.AggregateFunction == AggregateFunction.Avg ||
                                     _xAom.AggregateFunction == AggregateFunction.Min || _xAom.AggregateFunction == AggregateFunction.Max))
                                {
                                    xFrom = toScreenX((float)Math.Min(0, xValue));
                                    xMargin = resultItem.Margins[_xAom][BrushIndex.ALL];
                                    xMarginAbsolute = resultItem.MarginsAbsolute[_xAom][BrushIndex.ALL];
                                }
                                else
                                {
                                    xFrom = toScreenX((float)xBins[_xBinRange.GetDisplayIndex(xValue)]);
                                }

                                if (!_isXAxisAggregated && _isYAxisAggregated &&
                                    (_yAom.AggregateFunction == AggregateFunction.Count || _yAom.AggregateFunction == AggregateFunction.Sum || _yAom.AggregateFunction == AggregateFunction.Avg ||
                                     _yAom.AggregateFunction == AggregateFunction.Min || _yAom.AggregateFunction == AggregateFunction.Max))
                                {
                                    yFrom = toScreenY((float)Math.Min(0, yValue));
                                    yMargin = resultItem.Margins[_yAom][BrushIndex.ALL];
                                    yMarginAbsolute = resultItem.MarginsAbsolute[_yAom][BrushIndex.ALL];
                                }
                                else
                                {
                                    yFrom = toScreenY((float)yBins[_yBinRange.GetDisplayIndex(yValue)]);
                                }

                                if (_xBinRange is NominalBinRange)
                                {
                                    xTo = toScreenX((float)xBins[_xBinRange.GetDisplayIndex(xValue) + 1]);
                                }
                                else
                                {
                                    if (_isXAxisAggregated)
                                    {
                                        xTo = toScreenX((float)xValue);
                                        if (!_isYAxisAggregated)
                                        {
                                            xFromMargin = toScreenX((float)(xValue - xMarginAbsolute));
                                            xToMargin = toScreenX((float)(xValue + xMarginAbsolute));
                                        }
                                    }
                                    else
                                    {
                                        xTo = toScreenX((float)xBins[_xBinRange.GetDisplayIndex(_xBinRange.AddStep(xValue))]);
                                    }
                                }

                                if (_yBinRange is NominalBinRange)
                                {
                                    yTo = toScreenY((float)yBins[_yBinRange.GetDisplayIndex(yValue) + 1]);
                                }
                                else
                                {
                                    if (_isYAxisAggregated)
                                    {
                                        yTo = toScreenY((float)yValue);
                                        if (!_isXAxisAggregated)
                                        {
                                            yFromMargin = toScreenY((float)(yValue - yMarginAbsolute));
                                            yToMargin = toScreenY((float)(yValue + yMarginAbsolute));
                                        }
                                    }
                                    else
                                    {
                                        yTo = toScreenY((float)yBins[_yBinRange.GetDisplayIndex(_yBinRange.AddStep(yValue))]);
                                    }
                                }


                                float alpha = 0.15f;
                                var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                                var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                                //var dataColor = Color.FromArgb((byte)((0.10 + (Math.Pow(value, 1.0 / 3.0)) * (1.0 - 0.10)) * 255), 40, 170, 213);

                                //var brushBaseColor = Windows.UI.Color.FromArgb(178, 77, 148, 125);
                                //var brushBaseColor = Windows.UI.Color.FromArgb(255, 178, 77, 148);
                                //lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 240, 219, 232), brushBaseColor, (float)Math.Sqrt(value));
                                //var brushColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);

                                rect = new Rect(
                                    xFrom,
                                    yTo,
                                    xTo - xFrom,
                                    yFrom - yTo);

                                if (!_isXAxisAggregated && !_isYAxisAggregated)
                                {
                                    // draw data rect
                                    var currentMat = canvasArgs.DrawingSession.Transform;
                                    var mat = Matrix3x2.CreateTranslation(new Vector2(xFrom, yTo));
                                    mat = mat * currentMat;
                                    canvasArgs.DrawingSession.Transform = mat;
                                    canvasArgs.DrawingSession.DrawCachedGeometry(_fillRoundedRectGeom, dataColor);
                                    canvasArgs.DrawingSession.Transform = currentMat;
                                }
                                else
                                {
                                    //var tt = xValue;
                                    // draw data rect
                                    //canvasArgs.DrawingSession.FillRoundedRectangle(rect, 4, 4, Windows.UI.Color.FromArgb(255, 40, 170, 213));
                                    //DrawString(canvasArgs, _textFormat, (float) rect.X + _leftOffset, (float)rect.Y + _topOffset, yMargin.Value.ToString("F2"), _textColor, true, true, false);


                                    // draw brush rect
                                    var allUnNormalizedValue = resultItem.CountsInterpolated[iom][BrushIndex.ALL];
                                    var brushCount = 0;
                                    double sumBrushFactor = 0.0d;
                                    List<BrushIndex> brushIndices = new List<BrushIndex>() {new BrushIndex("0"), new BrushIndex("1"), new BrushIndex("2"), new BrushIndex("3") };
                                    foreach (var brushIndex in resultDescriptionModel.BrushIndices.Where(bi => bi != BrushIndex.ALL))
                                    {
                                        var brushUnNormalizedValue = resultItem.CountsInterpolated[iom][brushIndex];
                                        min = resultDescriptionModel.MinValues[iom][brushIndex];
                                        max = resultDescriptionModel.MaxValues[iom][brushIndex];

                                        var brushValueMargin = resultItem.Margins[iom][brushIndex];
                                        var brushValueMarginAbsolute = resultItem.MarginsAbsolute[iom][brushIndex];

                                        //0 ['actual and predicted', 'not actual and predicted', 'not actual and not predicted', 'actual and not predicted']
                                        Color brushColor = Color.FromArgb(255, 17, 17, 17);
                                        if (brushIndex.Equals(new BrushIndex("0")))
                                        {
                                            brushColor = Color.FromArgb(255, 178, 77, 148); 
                                        }
                                        else if (brushIndex.Equals(new BrushIndex("1")))
                                        {
                                            brushColor = Color.FromArgb(125, 41, 170, 213);
                                        }
                                        else if (brushIndex.Equals(new BrushIndex("2")))
                                        {
                                            brushColor = Color.FromArgb(255, 41, 170, 213);
                                        }
                                        else if (brushIndex.Equals(new BrushIndex("3")))
                                        {
                                            brushColor = Color.FromArgb(125, 178, 77, 148);
                                        }

                                        var brushFactor = (brushUnNormalizedValue / allUnNormalizedValue);
                                        if (_isYAxisAggregated && _isXAxisAggregated)
                                        {
                                            var ratio = (rect.Width / rect.Height);
                                            var newHeight = Math.Sqrt((1.0 / ratio) * ((rect.Width * rect.Height) * brushFactor));
                                            var newWidth = newHeight * ratio;

                                            var brushRect = new Rect(rect.X + (rect.Width - newWidth) / 2.0f, rect.Y + (rect.Height - newHeight) / 2.0f, newWidth, newHeight);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);

                                        }
                                        else if (_isYAxisAggregated)
                                        {
                                            var brushRect = new Rect(rect.X, rect.Y + (rect.Height - rect.Height * brushFactor) - rect.Height * sumBrushFactor, rect.Width, rect.Height * brushFactor);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);
                                        }
                                        else if (_isXAxisAggregated)
                                        {
                                            var brushRect = new Rect(rect.X + rect.Width * sumBrushFactor, rect.Y, rect.Width * brushFactor, rect.Height);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);
                                        }
                                        sumBrushFactor += brushFactor;
                                        brushCount++;
                                    }
                                }

                                if (_isXAxisAggregated || _isYAxisAggregated)
                                {
                                    //canvasArgs.DrawingSession.DrawRoundedRectangle(rect, 4, 4, white, 0.5f);

                                    IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                                    var filterModel = new FilterModel();
                                    for (int i = 0; i < resultDescriptionModel.BinRanges.Count; i++)
                                    {
                                        if (!(resultDescriptionModel.BinRanges[i] is AggregateBinRange))
                                        {
                                            double binRangeValue = resultItem.Values[resultDescriptionModel.Dimensions[i]][BrushIndex.ALL];

                                            var bins = resultDescriptionModel.BinRanges[i].GetBins();
                                            bins.Add(resultDescriptionModel.BinRanges[i].AddStep(bins.Max()));
                                            var v = resultDescriptionModel.BinRanges[i].GetIndex(binRangeValue);
                                            filterModel.Value = unNormalizedvalue;
                                            if (resultDescriptionModel.BinRanges[i] is NominalBinRange)
                                            {
                                                filterModel.ValueComparisons.Add(new ValueComparison(resultDescriptionModel.Dimensions[i], Predicate.EQUALS,
                                                        resultDescriptionModel.BinRanges[i].GetLabel(v)));
                                            }
                                            else
                                            {
                                                filterModel.ValueComparisons.Add(new ValueComparison(resultDescriptionModel.Dimensions[i], Predicate.GREATER_THAN_EQUAL, bins[v]));
                                                filterModel.ValueComparisons.Add(new ValueComparison(resultDescriptionModel.Dimensions[i], Predicate.LESS_THAN, bins[v + 1]));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    xFrom = toScreenX((float)xBins[xi]);
                    yFrom = toScreenY((float)yBins[yi]);
                    xTo = toScreenX((float)xBins[xi + 1]);
                    yTo = toScreenY((float)yBins[yi + 1]);
                    if (!_isXAxisAggregated && !_isYAxisAggregated)
                    {
                        // d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                        var currentMat = canvasArgs.DrawingSession.Transform;
                        var mat = Matrix3x2.CreateTranslation(new Vector2(xFrom, yTo));
                        mat = mat * currentMat;
                        canvasArgs.DrawingSession.Transform = mat;
                        canvasArgs.DrawingSession.DrawCachedGeometry(_strokeRoundedRectGeom, white);
                        canvasArgs.DrawingSession.Transform = currentMat;
                    }
                }
            }
        }

        public override void Load(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs canvasArgs)
        {
            
        }

        private float toScreenX(float x)
        {
            return ((x - _minX) / _xScale) * (_deviceWidth) + (_leftOffset);
        }
        private float toScreenY(float y, bool flip = true)
        {
            float retY = ((y - _minY) / _yScale) * (_deviceHeight);
            return flip ? (_deviceHeight) - retY + (_topOffset) : retY + (_topOffset);
        }
    }
}