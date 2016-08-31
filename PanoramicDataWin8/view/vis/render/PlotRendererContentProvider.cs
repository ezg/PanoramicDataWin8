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
using IDEA_common.aggregates;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using IDEA_common.range;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using NetTopologySuite.Algorithm;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis.render
{
    public class PlotRendererContentProvider : DXSurfaceContentProvider
    {
        private bool _isResultEmpty = false;

        //private float _leftOffset = 40;
        //private float _rightOffset = 20;
        //private float _topOffset = 20;
        //private float _bottomtOffset = 45;

        //private float _deviceWidth = 0;
        //private float _deviceHeight = 0;
        //private float _xScale = 0;
        //private float _yScale = 0;

        private PlotRendererContentProviderHelper _helper = null;

        private Color _textColor;
        private CanvasTextFormat _textFormat;

        private HistogramResult _histogramResult = null;

        private QueryModel _queryModelClone = null;
        private QueryModel _queryModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
        private Dictionary<FilterModel, Rect> _filterModelRects = new Dictionary<FilterModel, Rect>();
        private BinRange _xBinRange = null;
        private BinRange _yBinRange = null;
        private bool _isXAxisAggregated = false;
        private bool _isYAxisAggregated = false;
        private int _xIndex = -1;
        private int _yIndex = -1;
        private InputOperationModel _xIom = null;
        private InputOperationModel _yIom = null;
        private InputOperationModel _valueIom = null;
        private double _minValue = 0;
        private double _maxValue = 0;
        //private Dictionary<BinIndex, List<ProgressiveVisualizationResultItemModel>> _binDictonary = null;
        //private Dictionary<BinIndex, BinPrimitive> _binPrimitives = new Dictionary<BinIndex, BinPrimitive>();

        private CanvasCachedGeometry _fillRoundedRectGeom = null;
        private CanvasCachedGeometry _strokeRoundedRectGeom = null;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        public Dictionary<IGeometry, FilterModel> HitTargets { get; set; }

        public PlotRendererContentProvider()
        {
            HitTargets = new Dictionary<IGeometry, FilterModel>();
        }

        public void UpdateFilterModels(List<FilterModel> filterModels)
        {
            _filterModels = filterModels;
        }

        public void UpdateData(IResult result, QueryModel queryModel, QueryModel queryModelClone)
        {
            _helper = new PlotRendererContentProviderHelper((HistogramResult)result, queryModel, queryModelClone, CompositionScaleX, CompositionScaleY);

            _histogramResult = (HistogramResult)result;
            _queryModelClone = queryModelClone;
            _queryModel = queryModel;
            _xIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.X).FirstOrDefault();
            _yIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.Y).FirstOrDefault();

            var xAggregateKey = QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, _histogramResult.AllBrushIndex());
            var yAggregateKey = QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, _histogramResult.AllBrushIndex());

            
            if (_queryModelClone.GetUsageInputOperationModel(InputUsage.Value).Any())
            {
                _valueIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.Value).First();
            }
            else if (_queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).Any())
            {
                _valueIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).First();
            }
            _minValue = _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult) bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex())]).Result);
            _maxValue = _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex())]).Result);

            if (_histogramResult != null)
            {
                _xIndex = 0;
                _yIndex = 1;
                _isResultEmpty = false;

                if (!(_histogramResult.BinRanges[_xIndex] is AggregateBinRange))
                {
                    _xBinRange = _histogramResult.BinRanges[_xIndex];
                    _isXAxisAggregated = false;
                }
                else
                {
                    double factor = 0.0;
                    if (_histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult) bin.AggregateResults[xAggregateKey]).Result) - 
                        _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult) bin.AggregateResults[xAggregateKey]).Result) == 0)
                    {
                        factor = 0.1;
                        factor *= _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result) < 0 ? -1f : 1f;
                    }
                    _isXAxisAggregated = true;
                    _xBinRange = QuantitativeBinRange.Initialize(
                        _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result) * (1.0 - factor),
                        _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result) * (1.0 + factor), 10, false);
                }

                if (!(_histogramResult.BinRanges[_yIndex] is AggregateBinRange))
                {
                    _yBinRange = _histogramResult.BinRanges[_yIndex];
                    _isYAxisAggregated = false;
                }
                else
                {
                    double factor = 0.0;
                    if (_histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result) -
                        _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result) == 0)
                    {
                        factor = 0.1;
                        factor *= _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result) < 0 ? -1f : 1f;
                    }
                    _isYAxisAggregated = true;
                    _yBinRange = QuantitativeBinRange.Initialize(
                        _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result) * (1.0 - factor),
                        _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result) * (1.0 + factor), 10, false);
                }

                // scale axis to 0 if this is a bar chart
                if (_isXAxisAggregated && !_isYAxisAggregated &&
                    (_xIom.AggregateFunction == AggregateFunction.Count || _xIom.AggregateFunction == AggregateFunction.Sum || _xIom.AggregateFunction == AggregateFunction.Avg || _xIom.AggregateFunction == AggregateFunction.Min || _xIom.AggregateFunction == AggregateFunction.Max))
                {
                    _xBinRange = QuantitativeBinRange.Initialize(Math.Min(0,
                        _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result)), _xBinRange.DataMaxValue, 10, false);
                }
                if (!_isXAxisAggregated && _isYAxisAggregated &&
                    (_yIom.AggregateFunction == AggregateFunction.Count || _yIom.AggregateFunction == AggregateFunction.Sum || _yIom.AggregateFunction == AggregateFunction.Avg || _yIom.AggregateFunction == AggregateFunction.Min || _yIom.AggregateFunction == AggregateFunction.Max))
                {
                    _yBinRange = QuantitativeBinRange.Initialize(Math.Min(0,
                        _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result)), _yBinRange.DataMaxValue, 10, false);
                }
            }
            else
            {
                _isResultEmpty = true;
            }
        }

        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_histogramResult != null && _histogramResult.Bins.Count > 0)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderCell(canvas, canvasArgs);
                }
            }
            if (_isResultEmpty)
            {
                /*_leftOffset = 10;
                _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - _leftOffset - _rightOffset);
                _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - _topOffset - _bottomtOffset);
                DrawString(canvasArgs, _textFormat, _deviceWidth / 2.0f + _leftOffset, _deviceHeight / 2.0f + _topOffset, "no datapoints", _textColor, true, true, false);*/
            }
        }

        private void computeSizesAndRenderLabels(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines)
        {
            _helper.ComputeSizes(canvas, _textFormat);
            var white = Color.FromArgb(255, 255, 255, 255);

            if (_helper.DeviceHeight > 0 && _helper.DeviceWidth > 0)
            {
                float xFrom = 0; 
                float xTo = 0;
                float yFrom = 0;
                float yTo = 0;
                bool lastLabel = false;

                var xLabels = _helper.VisualBinRanges[0].GetLabels();
                // x labels and grid lines
                int mod = (int)Math.Ceiling(1.0 / (Math.Floor((_helper.DeviceWidth / (_helper.LabelMetricsX.Width + 5))) / xLabels.Count));
                int count = 0;
                foreach (var label in xLabels)
                {
                    yFrom = _helper.DataToScreenY(_helper.DataMinY);
                    yTo = _helper.DataToScreenY(_helper.DataMaxY);
                    xFrom = _helper.DataToScreenX((float)label.MinValue);
                    xTo = _helper.DataToScreenX((float)label.MaxValue);
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
                        if (_xBinRange is QuantitativeBinRange)
                        {
                            DrawString(canvasArgs, _textFormat, xFrom, yFrom + 5, double.Parse(label.Label).ToString(), _textColor, true, true, false);
                            if (lastLabel)
                            {
                               // DrawString(canvasArgs, _textFormat, xTo, yFrom + 5, label.MaxValue.ToString(), _textColor, true, true, false);
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
                var yLabels = _helper.VisualBinRanges[1].GetLabels();
                mod = (int)Math.Ceiling(1.0 / (Math.Floor((_helper.DeviceHeight / (_helper.LabelMetricsY.Height + 5))) / yLabels.Count));
                count = 0;
                foreach (var label in yLabels)
                {
                    xFrom = _helper.DataToScreenX(_helper.DataMinX);
                    xTo = _helper.DataToScreenX(_helper.DataMaxX);
                    yFrom = _helper.DataToScreenY((float)label.MinValue);
                    yTo = _helper.DataToScreenY((float)label.MaxValue);
                    lastLabel = count + 1 == yLabels.Count;

                    if (renderLines)
                    {
                        canvasArgs.DrawingSession.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xTo, yFrom), white, 0.5f);
                        if (lastLabel)
                        {
                            canvasArgs.DrawingSession.DrawLine(new Vector2(xFrom, yTo), new Vector2(xTo, yTo), white, 0.5f);
                        }
                    }
                    if (count % mod == 0)
                    {
                        if (_yBinRange is QuantitativeBinRange)
                        {
                            DrawString(canvasArgs, _textFormat, xFrom - 10, yFrom, double.Parse(label.Label).ToString(), _textColor, false, false, true);
                            if (lastLabel)
                            {
                               // DrawString(canvasArgs, _textFormat, xFrom - 10, yTo, label.MaxValue.ToString(), _textColor, false, false, true);
                            }
                        }
                        else
                        {
                            DrawString(canvasArgs, _textFormat, xFrom - 10, yFrom + (yTo - yFrom) / 2.0f, label.Label.ToString(), _textColor, false, false, true);
                        }
                    }
                    count++;
                }

                _fillRoundedRectGeom?.Dispose();
                _strokeRoundedRectGeom?.Dispose();
                var x = _helper.DataToScreenX((float)_xBinRange.AddStep(0)) - _helper.DataToScreenX(0);
                var y = _helper.DataToScreenY((float)_yBinRange.AddStep(0), false) - _helper.DataToScreenY(0, false);

                _fillRoundedRectGeom = CanvasCachedGeometry.CreateFill(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4));
                _strokeRoundedRectGeom = CanvasCachedGeometry.CreateStroke(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4), 0.5f);
            }
        }

        private void renderCell(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            computeSizesAndRenderLabels(canvas, canvasArgs, true);
            
            if (_helper.DeviceHeight < 0 || _helper.DeviceWidth < 0)
            {
                return;
            }

            var white = Color.FromArgb(255, 255, 255, 255);
            var dark = Color.FromArgb(255, 11, 11, 11);

            List<BinPrimitive> allBinPrimitives = new List<BinPrimitive>();
            HitTargets.Clear();
            for (int xi = 0; xi < _histogramResult.BinRanges[_xIndex].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < _histogramResult.BinRanges[_yIndex].GetBins().Count; yi++)
                {
                    BinIndex binIndex = new BinIndex(xi, yi);
                    
                    if (_histogramResult.Bins.ContainsKey(binIndex))
                    {
                        var bin = _histogramResult.Bins[binIndex];
                        var binPrimitives = _helper.GetBinPrimitives(bin);
                        allBinPrimitives.AddRange(binPrimitives);

                        foreach (var binPrimitive in binPrimitives)
                        {
                            if (binPrimitive.Value != 0.0)
                            {
                                canvasArgs.DrawingSession.FillRoundedRectangle(binPrimitive.Rect, 4, 4, binPrimitive.Color);   
                            }
                            if (binPrimitive.FilterModel != null)
                            {
                                HitTargets.Add(binPrimitive.HitGeom, binPrimitive.FilterModel);
                            }
                        }
                    }
                }
            }

            foreach (var binPrimitive in allBinPrimitives)
            {
                if (binPrimitive.FilterModel != null && _filterModels.Contains(binPrimitive.FilterModel))
                {
                    canvasArgs.DrawingSession.DrawRoundedRectangle(binPrimitive.Rect, 4, 4, dark, 1.0f);
                }
            }

            return;
          

            var xBins = _xBinRange.GetBins();
            xBins.Add(_xBinRange.AddStep(xBins.Max()));
            var yBins = _yBinRange.GetBins();
            yBins.Add(_yBinRange.AddStep(yBins.Max()));

            // draw data
            
            _filterModelRects.Clear();
            var rect = new Rect();
            float xFrom = 0;
            float yFrom = 0;
            float xTo = 0;
            float yTo = 0;

            float xFromMargin = 0;
            float yFromMargin = 0;
            float xToMargin = 0;
            float yToMargin = 0;

            for (int xi = 0; xi < _histogramResult.BinRanges[_xIndex].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < _histogramResult.BinRanges[_yIndex].GetBins().Count; yi++)
                {
                    BinIndex binIndex = new BinIndex(xi, yi);
                    if (_histogramResult.Bins.ContainsKey(binIndex))
                    {
                        var bin = _histogramResult.Bins[binIndex];

                        //double xValue = resultItem.Values[_queryModelClone.GetUsageInputOperationModel(InputUsage.X).First()][BrushIndex.ALL];
                        //double yValue = resultItem.Values[_queryModelClone.GetUsageInputOperationModel(InputUsage.Y).First()][BrushIndex.ALL];
                        double value = 0;
                        double unNormalizedvalue = 0;
                        double xMargin = 0;
                        double xMarginAbsolute = 0;
                        double yMargin = 0;
                        double yMarginAbsolute = 0;
                        double valueMargin = 0;
                        double valueMarginAbsolute = 0;

                        unNormalizedvalue = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex())] as DoubleValueAggregateResult).Result;

                        if (_minValue - _maxValue == 0.0)
                        {
                            value = 1.0;
                        }
                        else
                        {
                            value = (unNormalizedvalue - _minValue) / (_maxValue - _minValue);
                        }
                        valueMargin = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, new MarginAggregateParameters(),  _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).Margin;
                        valueMarginAbsolute = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, new MarginAggregateParameters(), _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).AbsolutMargin;

                        if (unNormalizedvalue != 0.0)
                        {
                            double xValue = 0;
                            double yValue = 0;
                            if (_isXAxisAggregated && !_isYAxisAggregated &&
                                (_xIom.AggregateFunction == AggregateFunction.Count || _xIom.AggregateFunction == AggregateFunction.Sum || _xIom.AggregateFunction == AggregateFunction.Avg ||
                                 _xIom.AggregateFunction == AggregateFunction.Min || _xIom.AggregateFunction == AggregateFunction.Max))
                            {
                                xValue = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, _histogramResult.AllBrushIndex())] as DoubleValueAggregateResult).Result;
                                xFrom = _helper.DataToScreenX((float) Math.Min(0, xValue));
                                xMargin = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).Margin;
                                xMarginAbsolute = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).AbsolutMargin;
                            }
                            else
                            {
                                xFrom = _helper.DataToScreenX((float) _xBinRange.GetValueFromIndex(bin.BinIndex.Indices[0]));
                            }

                            if (!_isXAxisAggregated && _isYAxisAggregated &&
                                (_yIom.AggregateFunction == AggregateFunction.Count || _yIom.AggregateFunction == AggregateFunction.Sum || _yIom.AggregateFunction == AggregateFunction.Avg ||
                                 _yIom.AggregateFunction == AggregateFunction.Min || _yIom.AggregateFunction == AggregateFunction.Max))
                            {
                                yValue = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, _histogramResult.AllBrushIndex())] as DoubleValueAggregateResult).Result;
                                yFrom = _helper.DataToScreenY((float) Math.Min(0, yValue));
                                yMargin = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).Margin;
                                yMarginAbsolute = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, _histogramResult.AllBrushIndex())] as MarginAggregateResult).AbsolutMargin;
                            }
                            else
                            {
                                yFrom = _helper.DataToScreenY((float)_yBinRange.GetValueFromIndex(bin.BinIndex.Indices[1]));
                            }

                            if (_xBinRange is NominalBinRange)
                            {
                                xTo = _helper.DataToScreenX((float)_xBinRange.GetValueFromIndex(bin.BinIndex.Indices[0] + 1));
                            }
                            else
                            {
                                if (_isXAxisAggregated)
                                {
                                    if (_isYAxisAggregated)
                                    {
                                        xValue = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, _histogramResult.AllBrushIndex())] as DoubleValueAggregateResult).Result;
                                        xTo = _helper.DataToScreenX((float)xValue);
                                    }
                                    else
                                    {
                                        xTo = _helper.DataToScreenX((float) xValue);
                                        xFromMargin = _helper.DataToScreenX((float) (xValue - xMarginAbsolute));
                                        xToMargin = _helper.DataToScreenX((float) (xValue + xMarginAbsolute));
                                    }
                                }
                                else
                                {
                                    xTo = _helper.DataToScreenX((float)_xBinRange.AddStep(_xBinRange.GetValueFromIndex(bin.BinIndex.Indices[0])));
                                }
                            }

                            if (_yBinRange is NominalBinRange)
                            {
                                yTo = _helper.DataToScreenY((float)_yBinRange.GetValueFromIndex(bin.BinIndex.Indices[1] + 1));
                            }
                            else
                            {
                                if (_isYAxisAggregated)
                                {
                                    if (_isXAxisAggregated)
                                    {
                                        yValue = (bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, _histogramResult.AllBrushIndex())] as DoubleValueAggregateResult).Result;
                                        yTo = _helper.DataToScreenY((float) yValue);
                                    }
                                    else
                                    {
                                        yTo = _helper.DataToScreenY((float) yValue);
                                        yFromMargin = _helper.DataToScreenY((float) (yValue - yMarginAbsolute));
                                        yToMargin = _helper.DataToScreenY((float) (yValue + yMarginAbsolute));
                                    }
                                }
                                else
                                {
                                    yTo = _helper.DataToScreenY((float)_yBinRange.AddStep(_yBinRange.GetValueFromIndex(bin.BinIndex.Indices[1])));
                                }
                            }


                            float alpha = 0.15f;
                            var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) (alpha + Math.Pow(value, 1.0/3.0)*(1.0 - alpha)));
                            var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                            //var dataColor = Color.FromArgb((byte)((0.10 + (Math.Pow(value, 1.0/3.0)) * (1.0-0.10)) * 255), 40, 170, 213);

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
                                mat = mat*currentMat;
                                canvasArgs.DrawingSession.Transform = mat;
                                canvasArgs.DrawingSession.DrawCachedGeometry(_fillRoundedRectGeom, dataColor);
                                canvasArgs.DrawingSession.Transform = currentMat;

                                // draw brush rect
                                if (_queryModel.BrushQueryModels.Count > 0)
                                {
                                    /*var allUnNormalizedValue = resultItem.CountsInterpolated[iom][BrushIndex.ALL];
                                    var brushCount = 0;
                                    foreach (var brushIndex in resultDescriptionModel.BrushIndices.Where(bi => bi != BrushIndex.ALL))
                                    {

                                        Color brushColor = Color.FromArgb(255, 17, 17, 17);
                                        if (_queryModelClone.BrushColors.Count > brushCount)
                                        {
                                            brushColor = _queryModelClone.BrushColors[brushCount];
                                        }

                                        var brushLerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), brushColor, (float) (alpha + Math.Pow(value, 1.0/3.0)*(1.0 - alpha)));
                                        var renderColor = Color.FromArgb(255, brushLerpColor.R, brushLerpColor.G, brushLerpColor.B);
                                        //var renderColor = Color.FromArgb((byte)((0.10 + (Math.Pow(value, 1.0 / 3.0)) * (1.0 - 0.10)) * 255), brushColor.R, brushColor.G, brushColor.B);

                                        var brushUnNormalizedValue = resultItem.CountsInterpolated[iom][brushIndex];
                                        var brushFactor = (brushUnNormalizedValue/allUnNormalizedValue);

                                        var ratio = (rect.Width/rect.Height);
                                        var newHeight = Math.Sqrt((1.0/ratio)*((rect.Width*rect.Height)*brushFactor));
                                        var newWidth = newHeight*ratio;

                                        var brushRect = new Rect(rect.X + (rect.Width - newWidth)/2.0f, rect.Y + (rect.Height - newHeight)/2.0f, newWidth, newHeight);
                                        canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, renderColor);
                                        brushCount++;
                                    }*/
                                }
                                if (valueMargin != 0.0 && _histogramResult.Progress < 1.0)
                                {
                                    //DrawString(canvasArgs, _textFormat,
                                    //    (float)(rect.Left + rect.Width / 2.0f),
                                    //    (float)(rect.Top + rect.Height / 2.0f), '\u00B1' + (valueMargin * 100).ToString("F2") + "%", new Color() { A = 80, R = _textColor.R, G = _textColor.G, B = _textColor.B }, false, true, true);
                                }
                            }
                            else
                            {
                                // draw data rect
                                canvasArgs.DrawingSession.FillRoundedRectangle(rect, 4, 4, Windows.UI.Color.FromArgb(255, 40, 170, 213));
                                //DrawString(canvasArgs, _textFormat, (float) rect.X + _leftOffset, (float)rect.Y + _topOffset, yMargin.Value.ToString("F2"), _textColor, true, true, false);


                                // draw brush rect
                                if (_queryModel.BrushQueryModels.Count > 0)
                                {
                                    /*var allUnNormalizedValue = resultItem.CountsInterpolated[iom][BrushIndex.ALL];
                                    var brushCount = 0;
                                    double sumBrushFactor = 0.0d;
                                    foreach (var brushIndex in resultDescriptionModel.BrushIndices.Where(bi => bi != BrushIndex.ALL))
                                    {
                                        var brushUnNormalizedValue = resultItem.CountsInterpolated[iom][brushIndex];
                                        min = resultDescriptionModel.MinValues[iom][brushIndex];
                                        max = resultDescriptionModel.MaxValues[iom][brushIndex];

                                        var brushValueMargin = resultItem.Margins[iom][brushIndex];
                                        var brushValueMarginAbsolute = resultItem.MarginsAbsolute[iom][brushIndex];

                                        Color brushColor = Color.FromArgb(255, 17, 17, 17);
                                        if (_queryModelClone.BrushColors.Count > brushCount)
                                        {
                                            brushColor = _queryModelClone.BrushColors[brushCount];
                                        }

                                        var brushFactor = (brushUnNormalizedValue/allUnNormalizedValue);
                                        if (_isYAxisAggregated && _isXAxisAggregated)
                                        {
                                            var ratio = (rect.Width/rect.Height);
                                            var newHeight = Math.Sqrt((1.0/ratio)*((rect.Width*rect.Height)*brushFactor));
                                            var newWidth = newHeight*ratio;

                                            var brushRect = new Rect(rect.X + (rect.Width - newWidth)/2.0f, rect.Y + (rect.Height - newHeight)/2.0f, newWidth, newHeight);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);

                                        }
                                        else if (_isYAxisAggregated)
                                        {
                                            var brushRect = new Rect(rect.X, rect.Y + (rect.Height - rect.Height*brushFactor) - rect.Height*sumBrushFactor, rect.Width, rect.Height*brushFactor);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);

                                            double currentY = resultItem.Values[_queryModelClone.GetUsageInputOperationModel(InputUsage.Y).First()][brushIndex];
                                            float pixel = _helper.DataToScreenY((float) (0)) - _helper.DataToScreenY((float) (brushValueMarginAbsolute));

                                            canvasArgs.DrawingSession.DrawLine(
                                                new Vector2((float) (brushRect.X + rect.Width/2.0f), (float) brushRect.Y - pixel),
                                                new Vector2((float) (brushRect.X + rect.Width/2.0f), (float) brushRect.Y + pixel), dark, 1);
                                        }
                                        else if (_isXAxisAggregated)
                                        {
                                            var brushRect = new Rect(rect.X + rect.Width*sumBrushFactor, rect.Y, rect.Width*brushFactor, rect.Height);
                                            canvasArgs.DrawingSession.FillRoundedRectangle(brushRect, 4, 4, brushColor);
                                        }
                                        sumBrushFactor += brushFactor;
                                        brushCount++;
                                    }*/
                                }

                                if (_isYAxisAggregated && !_isXAxisAggregated)
                                {
                                    canvasArgs.DrawingSession.DrawLine(
                                        new Vector2((float) (rect.X + rect.Width/2.0f), (float) yFromMargin),
                                        new Vector2((float) (rect.X + rect.Width/2.0f), (float) yToMargin), dark, 3);
                                }
                                if (_isXAxisAggregated && !_isYAxisAggregated)
                                {
                                    canvasArgs.DrawingSession.DrawLine(
                                        new Vector2((float) xFromMargin, (float) (rect.Y + rect.Height/2.0f)),
                                        new Vector2((float) xToMargin, (float) (rect.Y + rect.Height/2.0f)), dark, 3);
                                }
                            }

                            if (_isXAxisAggregated || _isYAxisAggregated)
                            {
                                //canvasArgs.DrawingSession.DrawRoundedRectangle(rect, 4, 4, white, 0.5f);

                                /*IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
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
                                if (!HitTargets.ContainsKey(hitGeom))
                                {
                                    HitTargets.Add(hitGeom, filterModel);
                                    _filterModelRects.Add(filterModel, rect);
                                }*/
                            }
                        }

                    }
                    xFrom = _helper.DataToScreenX((float)xBins[xi]);
                    yFrom = _helper.DataToScreenY((float)yBins[yi]);
                    xTo = _helper.DataToScreenX((float)xBins[xi + 1]);
                    yTo = _helper.DataToScreenY((float)yBins[yi + 1]);
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

            if (!_isXAxisAggregated && !_isYAxisAggregated)
            {
                /*for (int xi = 0; xi < resultDescriptionModel.BinRanges[_xIndex].GetBins().Count; xi++)
                {
                    for (int yi = 0; yi < resultDescriptionModel.BinRanges[_yIndex].GetBins().Count; yi++)
                    {
                        BinIndex binIndex = new BinIndex(xi, yi);
                        double? unNormalizedvalue = null;
                        if (_binDictonary.ContainsKey(binIndex))
                        {
                            foreach (var resultItem in _binDictonary[binIndex])
                            {
                                if (_queryModelClone.GetUsageInputOperationModel(InputUsage.Value).Any() && resultItem.Values.ContainsKey(_queryModelClone.GetUsageInputOperationModel(InputUsage.Value).First()))
                                {
                                    unNormalizedvalue = (double?)resultItem.Values[_queryModelClone.GetUsageInputOperationModel(InputUsage.Value).First()][BrushIndex.ALL];
                                }
                                else if (_queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).Any() && resultItem.Values.ContainsKey(_queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).First()))
                                {
                                    unNormalizedvalue = (double?)resultItem.Values[_queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).First()][BrushIndex.ALL];
                                }
                            }
                        }

                        xFrom = _helper.DataToScreenX((float)xBins[xi]);
                        yFrom = _helper.DataToScreenY((float)yBins[yi]);
                        xTo = _helper.DataToScreenX((float)xBins[xi + 1]);
                        yTo = _helper.DataToScreenY((float)yBins[yi + 1]);
                        rect = new Rect(
                            xFrom,
                            yTo,
                            xTo - xFrom,
                            yFrom - yTo);

                        IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                        var filterModel = new FilterModel();
                        filterModel.Value = unNormalizedvalue;
                        if (resultDescriptionModel.BinRanges[_xIndex] is NominalBinRange)
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_xIom, Predicate.EQUALS,
                                  resultDescriptionModel.BinRanges[_xIndex].GetLabels()[xi].Label));
                        }
                        else
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_xIom, Predicate.GREATER_THAN_EQUAL, xBins[xi]));
                            filterModel.ValueComparisons.Add(new ValueComparison(_xIom, Predicate.LESS_THAN, xBins[xi + 1]));
                        }
                        if (resultDescriptionModel.BinRanges[_yIndex] is NominalBinRange)
                        {
                            var gg = resultDescriptionModel.BinRanges[_yIndex].GetBins();

                            filterModel.ValueComparisons.Add(new ValueComparison(_yIom, Predicate.EQUALS,
                                  resultDescriptionModel.BinRanges[_yIndex].GetLabels()[yi].Label));
                        }
                        else
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_yIom, Predicate.GREATER_THAN_EQUAL, yBins[yi]));
                            filterModel.ValueComparisons.Add(new ValueComparison(_yIom, Predicate.LESS_THAN, yBins[yi + 1]));
                        }
                        HitTargets.Add(hitGeom, filterModel);

                        if (_filterModels.Contains(filterModel))
                        {
                            canvasArgs.DrawingSession.DrawRoundedRectangle(rect, 4, 4, dark, 1f);
                        }
                    }
                }*/
            }
            else
            {
                foreach (var filterModel in _filterModelRects.Keys)
                {
                    if (_filterModels.Contains(filterModel))
                    {
                        canvasArgs.DrawingSession.DrawRoundedRectangle(_filterModelRects[filterModel], 4, 4, dark, 1.0f);
                    }
                }
            }
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

    public class BinPrimitive
    {
        public float Value { get; set; }
        public Rect Rect { get; set; }
        public Color Color { get; set; }
        public int BrushIndex { get; set; }
        public IGeometry HitGeom { get; set; }
        public FilterModel FilterModel { get; set; }

    }

    public class PlotRendererContentProviderHelper
    {
        private float _compositionScaleX = 1;
        private float _compositionScaleY = 1;

        private HistogramResult _histogramResult = null;
        private QueryModel _queryModelClone = null;
        private QueryModel _queryModel = null;
        private InputOperationModel _xIom = null;
        private InputOperationModel _yIom = null;
        private InputOperationModel _valueIom = null;
        private ChartType _chartType = ChartType.HeatMap;
        public List<BinRange> VisualBinRanges { get; set; } = new List<BinRange>();

        private float _leftOffset = 40;
        private float _rightOffset = 20;
        private float _topOffset = 20;
        private float _bottomtOffset = 45;

        public float DeviceWidth { get; set; } = 0;
        public float DeviceHeight { get; set; } = 0;
        private float _xScale = 0;
        private float _yScale = 0;
        private float _minValue = 0;
        private float _maxValue = 0;
        public float DataMinX { get; set; } = 0;
        public float DataMinY { get; set; } = 0;
        public float DataMaxX { get; set; } = 0;
        public float DataMaxY { get; set; } = 0;

        public Rect LabelMetricsX { get; set; } = Rect.Empty;
        public Rect LabelMetricsY { get; set; } = Rect.Empty;

        private static float TOLERANCE = 0.0001f;

        public PlotRendererContentProviderHelper(HistogramResult histogramResult, QueryModel queryModel, QueryModel queryModelClone, float compositionScaleX, float compositionScaleY)
        {
            _compositionScaleX = compositionScaleX;
            _compositionScaleY = compositionScaleY;
            _histogramResult = histogramResult;
            _queryModelClone = queryModelClone;
            _queryModel = queryModel;
            _xIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.X).FirstOrDefault();
            _yIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.Y).FirstOrDefault();

            if (_queryModelClone.GetUsageInputOperationModel(InputUsage.Value).Any())
            {
                _valueIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.Value).First();
            }
            else if (_queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).Any())
            {
                _valueIom = _queryModelClone.GetUsageInputOperationModel(InputUsage.DefaultValue).First();
            }

            _minValue = (float)_histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex())]).Result);
            _maxValue = (float)_histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex())]).Result);
            
            initializeChartType(_histogramResult.BinRanges);

            var xAggregateKey = QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, _histogramResult.AllBrushIndex());
            var yAggregateKey = QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, _histogramResult.AllBrushIndex());
            VisualBinRanges.Add(createVisualBinRange(_histogramResult.BinRanges[0], xAggregateKey));
            VisualBinRanges.Add(createVisualBinRange(_histogramResult.BinRanges[1], yAggregateKey));
        }

        public void ComputeSizes(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, CanvasTextFormat textFormat)
        {
            var xLabels = VisualBinRanges[0].GetLabels();
            var yLabels = VisualBinRanges[1].GetLabels();
            var maxXLabelLength = xLabels.Max(b => b.Label.Length);
            var maxXLabel = xLabels.First(b => b.Label.Length == maxXLabelLength);
            var maxYLabelLength = yLabels.Max(b => b.Label.Length);
            var maxYLabel = yLabels.First(b => b.Label.Length == maxYLabelLength);

            var layoutX = new CanvasTextLayout(canvas, maxXLabel.Label, textFormat, 1000f, 1000f);
            LabelMetricsX = layoutX.DrawBounds;
            var layoutY = new CanvasTextLayout(canvas, maxYLabel.Label, textFormat, 1000f, 1000f);
            LabelMetricsY = layoutY.DrawBounds;

            _leftOffset = (float)Math.Max(10, LabelMetricsY.Width + 10 + 20);

            DeviceWidth = (float)(canvas.ActualWidth / _compositionScaleX - _leftOffset - _rightOffset);
            DeviceHeight = (float)(canvas.ActualHeight / _compositionScaleY - _topOffset - _bottomtOffset);

            DataMinX = (float)(xLabels.Min(dp => dp.MinValue));
            DataMinY = (float)(yLabels.Min(dp => dp.MinValue));
            DataMaxX = (float)(xLabels.Max(dp => dp.MaxValue));
            DataMaxY = (float)(yLabels.Max(dp => dp.MaxValue));

            _xScale = DataMaxX - DataMinX;
            _yScale = DataMaxY - DataMinY;

            layoutX.Dispose();
            layoutY.Dispose();
        }

        private void initializeChartType(List<BinRange> binRanges)
        {
            if (binRanges[0] is AggregateBinRange && binRanges[1] is AggregateBinRange)
            {
                _chartType = ChartType.SinglePoint;
            }
            else if (binRanges[0] is AggregateBinRange)
            {
                _chartType = ChartType.HorizontalBar;
            }
            else if (binRanges[1] is AggregateBinRange)
            {
                _chartType = ChartType.VerticalBar;
            }
            else
            {
                _chartType = ChartType.HeatMap;
            }
        }

        private BinRange createVisualBinRange(BinRange dataBinRange, AggregateKey aggregateKey)
        {
            BinRange visualBinRange = null;
            if (!(dataBinRange is AggregateBinRange))
            {
                visualBinRange = dataBinRange;
            }
            else
            {
                double factor = 0.0;
                if (_histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result) -
                    _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result) == 0)
                {
                    factor = 0.1;
                    factor *= _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result) < 0 ? -1f : 1f;
                }
                visualBinRange = QuantitativeBinRange.Initialize(
                    _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result) * (1.0 - factor),
                    _histogramResult.Bins.Values.Max(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result) * (1.0 + factor), 10, false);
            }

            if ((_chartType == ChartType.HorizontalBar || _chartType == ChartType.VerticalBar) &&
                dataBinRange is AggregateBinRange)
            {
                visualBinRange = QuantitativeBinRange.Initialize(Math.Min(0,
                    _histogramResult.Bins.Values.Min(bin => ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result)), visualBinRange.DataMaxValue, 10, false);
            }
            return visualBinRange;
        }

        public List<BinPrimitive> GetBinPrimitives(Bin bin)
        {
            List<BinPrimitive> binPrimitives = new List<BinPrimitive>();
            float alpha = 0.15f;
            var baseColor = Colors.White;

            foreach (var brush in _histogramResult.Brushes)
            {
                float xFrom = 0;
                float xTo = 0;
                float yFrom = 0;
                float yTo = 0;
                float value = 0;
                float xMargin = 0;
                float yMargin = 0;
                float xMarginAbsolute = 0;
                float yMarginAbsolute = 0;
                Color color = Colors.White;

                float unNormalizedvalue = (float)((DoubleValueAggregateResult) bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, brush.BrushIndex)]).Result;
                if (unNormalizedvalue != 0)
                {
                    value = (unNormalizedvalue - _minValue)/(Math.Abs((_maxValue - _minValue)) < TOLERANCE ? (unNormalizedvalue - _minValue) : (_maxValue - _minValue));
                }

                if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    baseColor = Windows.UI.Color.FromArgb(255, 40, 170, 213);
                }
                else if (brush.BrushIndex == _histogramResult.OverlapBrushIndex())
                {
                    baseColor = Color.FromArgb(255, 17, 17, 17);
                }
                else
                {
                    //baseColor = _queryModelClone.BrushColors[brush.BrushIndex % _queryModelClone.BrushColors.Count];
                }
                

                // read out value depinding on chart type
                if (_chartType == ChartType.HeatMap)
                {
                    xFrom = DataToScreenX((float)VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0]));
                    xTo = DataToScreenX((float)VisualBinRanges[0].AddStep(VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0])));

                    yFrom = DataToScreenY((float)VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1]));
                    yTo = DataToScreenY((float)VisualBinRanges[1].AddStep(VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1])));

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = dataColor;
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    var xValue = ((DoubleValueAggregateResult) bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex)]).Result;
                    xFrom = DataToScreenX((float) Math.Min(0, xValue));
                    xTo = DataToScreenX((float) Math.Max(0, xValue));

                    yFrom = DataToScreenY((float) VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1]));
                    yTo = DataToScreenY((float) VisualBinRanges[1].AddStep(VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1])));

                    xMargin = (float) ((MarginAggregateResult) bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).Margin;
                    xMarginAbsolute = (float) ((MarginAggregateResult) bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = baseColor;
                }

                else if (_chartType == ChartType.VerticalBar)
                {
                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex)]).Result;
                    yFrom = DataToScreenY((float)Math.Min(0, yValue));
                    yTo = DataToScreenY((float)Math.Max(0, yValue));

                    xFrom = DataToScreenX((float)VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0]));
                    xTo = DataToScreenX((float)VisualBinRanges[0].AddStep(VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0])));

                    yMargin = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).Margin;
                    yMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = baseColor;
                }

                else if (_chartType == ChartType.SinglePoint)
                {
                    var xValue = ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex)]).Result;
                    xFrom = DataToScreenX((float)Math.Min(0, xValue));
                    xTo = DataToScreenX((float)Math.Max(0, xValue));

                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex)]).Result;
                    yFrom = DataToScreenY((float)Math.Min(0, yValue));
                    yTo = DataToScreenY((float)Math.Max(0, yValue));

                    xMargin = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).Margin;
                    xMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).AbsolutMargin;
                    
                    yMargin = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).Margin;
                    yMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex)]).AbsolutMargin;

                    color = baseColor;
                }


                IGeometry hitGeom = null;
                FilterModel filterModel = null;
                if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    InputOperationModel[] dimensions = new InputOperationModel[] {_xIom, _yIom};
                    //if (_chartType != ChartType.HeatMap)
                    {
                        hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                        filterModel = new FilterModel();
                        filterModel.Value = unNormalizedvalue;
                        for (int i = 0; i < _histogramResult.BinRanges.Count; i++)
                        {
                            if (!(_histogramResult.BinRanges[i] is AggregateBinRange))
                            {
                                var dataFrom = _histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]);
                                var dataTo = _histogramResult.BinRanges[i].AddStep(_histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]));

                                if (_histogramResult.BinRanges[i] is NominalBinRange)
                                {
                                    filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.EQUALS,
                                        _histogramResult.BinRanges[i].GetLabel(dataFrom)));
                                }
                                else
                                {
                                    filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.GREATER_THAN_EQUAL, dataFrom));
                                    filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.LESS_THAN, dataTo));
                                }
                            }
                        }
                    }
                }

                BinPrimitive binPrimitive = new BinPrimitive()
                {
                    Rect = new Rect(
                        xFrom,
                        yTo,
                        xTo - xFrom,
                        yFrom - yTo), 
                    BrushIndex = brush.BrushIndex,
                    Color = color,
                    Value = unNormalizedvalue, 
                    FilterModel = filterModel,
                    HitGeom = hitGeom
                };
                binPrimitives.Add(binPrimitive);

            }
            return binPrimitives;
        }

        public float DataToScreenX(float x)
        {
            return ((x - DataMinX) / _xScale) * (DeviceWidth) + (_leftOffset);
        }
        public float DataToScreenY(float y, bool flip = true)
        {
            float retY = ((y - DataMinY) / _yScale) * (DeviceHeight);
            return flip ? (DeviceHeight) - retY + (_topOffset) : retY + (_topOffset);
        }
    }

    public enum ChartType
    {
        HorizontalBar, VerticalBar, HeatMap, SinglePoint
    }
}