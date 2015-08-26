using NetTopologySuite.Geometries;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.view.common;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Networking.Sockets;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis.render
{
    public class PlotRendererContentProvider : DXSurfaceContentProvider
    {
        private float _leftOffset = 40;
        private float _rightOffset = 10;
        private float _topOffset = 10;
        private float _bottomtOffset = 45;

        private float _deviceWidth = 0;
        private float _deviceHeight = 0;
        private float _xScale = 0;
        private float _yScale = 0;
        private bool _flipY = true;
        private float _minX = 0;
        private float _minY = 0;
        private float _maxX = 0;
        private float _maxY = 0;

        private D2D.Brush _textBrush;
        private DW.TextFormat _textFormat;

        private ResultModel _resultModel = null;
        private VisualizationResultDescriptionModel _visualizationDescriptionModel = null;

        private QueryModel _queryModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
        private Dictionary<FilterModel, D2D.RoundedRectangle> _filterModelRects = new Dictionary<FilterModel, D2D.RoundedRectangle>(); 
        private BinRange _xBinRange = null;
        private BinRange _yBinRange = null;
        private bool _isXAxisAggregated = false;
        private bool _isYAxisAggregated = false;
        private int _xIndex = -1;
        private int _yIndex = -1;
        private InputOperationModel _xAom = null;
        private InputOperationModel _yAom = null;
        private Dictionary<BinIndex, List<VisualizationItemResultModel>> _binDictonary = null;
        private Dictionary<BinIndex, BinPrimitive> _binPrimitives = new Dictionary<BinIndex, BinPrimitive>();

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

        public void UpdateData(ResultModel resultModel, QueryModel queryModel, InputOperationModel xAom, InputOperationModel yAom)
        {
            _resultModel = resultModel;
            _queryModel = queryModel;
            _xAom = xAom;
            _yAom = yAom;

            _visualizationDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;

            _xIndex = _visualizationDescriptionModel.Dimensions.IndexOf(xAom);
            _yIndex = _visualizationDescriptionModel.Dimensions.IndexOf(yAom);

            if (!(_visualizationDescriptionModel.BinRanges[_xIndex] is AggregateBinRange))
            {
                _xBinRange = _visualizationDescriptionModel.BinRanges[_xIndex];
                _isXAxisAggregated = false;
            }
            else
            {
                double factor = 0.0;
                if (_visualizationDescriptionModel.MinValues[xAom] - _visualizationDescriptionModel.MaxValues[xAom] == 0)
                {
                    factor = 0.1;
                    factor *= _visualizationDescriptionModel.MinValues[xAom] < 0 ? -1f : 1f;
                }
                _isXAxisAggregated = true;
                _xBinRange = QuantitativeBinRange.Initialize(_visualizationDescriptionModel.MinValues[xAom] * (1.0 - factor), _visualizationDescriptionModel.MaxValues[xAom] * (1.0 + factor), 10, false);
            }

            if (!(_visualizationDescriptionModel.BinRanges[_yIndex] is AggregateBinRange))
            {
                _yBinRange = _visualizationDescriptionModel.BinRanges[_yIndex];
                _isYAxisAggregated = false;
            }
            else
            {
                double factor = 0.0;
                if (_visualizationDescriptionModel.MinValues[yAom] - _visualizationDescriptionModel.MaxValues[yAom] == 0)
                {
                    factor = 0.1;
                    factor *= _visualizationDescriptionModel.MinValues[yAom] < 0 ? -1f : 1f;
                }
                _isYAxisAggregated = true;
                _yBinRange = QuantitativeBinRange.Initialize(_visualizationDescriptionModel.MinValues[yAom] * (1.0 - factor), _visualizationDescriptionModel.MaxValues[yAom] * (1.0 + factor), 10, false);
            }

            // scale axis to 0 if this is a bar chart
            if (_isXAxisAggregated && !_isYAxisAggregated &&
                (xAom.AggregateFunction == AggregateFunction.Count || xAom.AggregateFunction == AggregateFunction.Sum || xAom.AggregateFunction == AggregateFunction.Avg || xAom.AggregateFunction == AggregateFunction.Min || xAom.AggregateFunction == AggregateFunction.Max))
            {
                _xBinRange = QuantitativeBinRange.Initialize(Math.Min(0, _visualizationDescriptionModel.MinValues[xAom]), _xBinRange.DataMaxValue, 10, false);
            }
            if (!_isXAxisAggregated && _isYAxisAggregated &&
               (yAom.AggregateFunction == AggregateFunction.Count || yAom.AggregateFunction == AggregateFunction.Sum || yAom.AggregateFunction == AggregateFunction.Avg || yAom.AggregateFunction == AggregateFunction.Min || yAom.AggregateFunction == AggregateFunction.Max))
            {
                _yBinRange = QuantitativeBinRange.Initialize(Math.Min(0, _visualizationDescriptionModel.MinValues[yAom]), _yBinRange.DataMaxValue, 10, false);
            }

            // create bin dictionary
            var resultDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;
            _binDictonary = new Dictionary<BinIndex, List<VisualizationItemResultModel>>();
            foreach (var resultItem in _resultModel.ResultItemModels.Select(ri => ri as VisualizationItemResultModel))
            {
                if (resultItem.Values.ContainsKey(xAom) && resultItem.Values.ContainsKey(yAom))
                {
                    double? xValue = (double?)resultItem.Values[xAom].Value;
                    double? yValue = (double?)resultItem.Values[yAom].Value;

                    if (xValue.HasValue && yValue.HasValue)
                    {
                        BinIndex binIndex = new BinIndex(
                            resultDescriptionModel.BinRanges[_xIndex].GetIndex(xValue.Value),
                            resultDescriptionModel.BinRanges[_yIndex].GetIndex(yValue.Value));
                        if (!_binDictonary.ContainsKey(binIndex))
                        {
                            _binDictonary.Add(binIndex, new List<VisualizationItemResultModel>());
                        }
                        _binDictonary[binIndex].Add(resultItem);
                    }
                }
            }
        }

        public override void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(new Color(230, 230, 230));
        }

        public override void Draw(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            var mat = Matrix3x2.Identity;
            mat.ScaleVector = new Vector2(CompositionScaleX, CompositionScaleY);
            d2dDeviceContext.Transform = mat;

            if (_resultModel != null && _resultModel.ResultItemModels.Count > 0)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderCell(d2dDeviceContext, dwFactory);
                }
            }
        }

        private void drawString(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory, float x, float y, string text,
            bool leftAligned,
            bool horizontallyCentered, bool verticallyCentered)
        {
            var layout = new DW.TextLayout(dwFactory, text, _textFormat, 1000f, 1000f);
            var metrics = layout.Metrics;

            if (horizontallyCentered)
            {
                x -= metrics.Width / 2.0f;
            }
            if (verticallyCentered)
            {
                y -= metrics.Height / 2.0f;
            }
            if (!leftAligned)
            {
                x -= metrics.Width;
            }

            d2dDeviceContext.DrawTextLayout(new Vector2(x, y), layout, _textBrush);
        }

        private void computeSizesAndRenderLabels(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory, bool renderLines)
        {
            //var xLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelX.TrimTo(20), MinValue = bin.MinX, MaxValue = bin.MaxX }).Distinct().ToList();
            //var yLabels = BinnedDataPoints.Select(bin => new { Label = bin.LabelY.TrimTo(20), MinValue = bin.MinY, MaxValue = bin.MaxY }).Distinct().ToList();
            var xLabels = _xBinRange.GetLabels();
            var yLabels = _yBinRange.GetLabels();
            var maxXLabelLength = xLabels.Max(b => b.Label.Length);
            var maxXLabel = xLabels.First(b => b.Label.Length == maxXLabelLength);
            var maxYLabelLength = yLabels.Max(b => b.Label.Length);
            var maxYLabel = yLabels.First(b => b.Label.Length == maxYLabelLength);

            var layoutX = new DW.TextLayout(dwFactory, maxXLabel.Label, _textFormat, 1000f, 1000f);
            var metricsX = layoutX.Metrics;
            var layoutY = new DW.TextLayout(dwFactory, maxYLabel.Label, _textFormat, 1000f, 1000f);
            var metricsY = layoutY.Metrics;

            _leftOffset = Math.Max(10, metricsY.Width + 10 + 20);

            _deviceWidth = (float)(d2dDeviceContext.Size.Width / CompositionScaleX - _leftOffset - _rightOffset);
            _deviceHeight = (float)(d2dDeviceContext.Size.Height / CompositionScaleY - _topOffset - _bottomtOffset);

            _minX = (float)(xLabels.Min(dp => dp.MinValue));
            _minY = (float)(yLabels.Min(dp => dp.MinValue));
            _maxX = (float)(xLabels.Max(dp => dp.MaxValue));
            _maxY = (float)(yLabels.Max(dp => dp.MaxValue));

            _xScale = _maxX - _minX;
            _yScale = _maxY - _minY;

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

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
                        d2dDeviceContext.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xFrom, yTo), white, 0.5f);
                        if (lastLabel)
                        {
                            d2dDeviceContext.DrawLine(new Vector2(xTo, yFrom), new Vector2(xTo, yTo), white, 0.5f);
                        }
                    }
                    if (count % mod == 0)
                    {
                        if (_visualizationDescriptionModel.AxisTypes[_xIndex] == AxisType.Quantitative)
                        {
                            drawString(d2dDeviceContext, dwFactory, xFrom, yFrom + 5, label.Label.ToString(), true, true, false);
                        }
                        else
                        {
                            drawString(d2dDeviceContext, dwFactory, xFrom + (xTo - xFrom) / 2.0f, yFrom + 5, label.Label.ToString(), true, true, false);
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
                        d2dDeviceContext.DrawLine(new Vector2(xFrom, yFrom), new Vector2(xTo, yFrom), white, 0.5f);
                        if (lastLabel)
                        {
                            d2dDeviceContext.DrawLine(new Vector2(xFrom, yTo), new Vector2(xTo, yTo), white, 0.5f);
                        }
                    }
                    if (count % mod == 0)
                    {
                        if (_visualizationDescriptionModel.AxisTypes[_yIndex] == AxisType.Quantitative)
                        {
                            drawString(d2dDeviceContext, dwFactory, xFrom - 10, yFrom, label.Label.ToString(), false, false, true);
                        }
                        else
                        {
                            drawString(d2dDeviceContext, dwFactory, xFrom - 10, yFrom + (yTo - yFrom) / 2.0f, label.Label.ToString(), false, false, true);
                        }
                    }
                    count++;
                }

                /*foreach (var x in _xBinRange.GetBins())
                {
                    foreach (var y in _yBinRange.GetBins())
                    {
                        var roundedRect = new D2D.RoundedRectangle();
                        xFrom = toScreenX((float)x);
                        yFrom = toScreenY((float)y);
                        xTo = toScreenX((float)_xBinRange.AddStep(x));
                        yTo = toScreenY((float)_yBinRange.AddStep(y));
                        roundedRect.Rect = new RectangleF(
                            xFrom,
                            yTo,
                            xTo - xFrom,
                            yFrom - yTo);
                        roundedRect.RadiusX = roundedRect.RadiusY = 4;
                   
                        d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                    }
                }    */
            }

            white.Dispose();
            layoutX.Dispose();
            layoutY.Dispose();
        }

        private void renderCell(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            computeSizesAndRenderLabels(d2dDeviceContext, dwFactory, false);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var dark = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(11f / 255f, 11f / 255f, 11f / 255f, 1f));

            var xBins = _xBinRange.GetBins();
            xBins.Add(_xBinRange.AddStep(xBins.Max()));
            var yBins = _yBinRange.GetBins();
            yBins.Add(_yBinRange.AddStep(yBins.Max()));

            // draw data
            HitTargets.Clear();
            _filterModelRects.Clear();
            var resultDescriptionModel = _resultModel.ResultDescriptionModel as VisualizationResultDescriptionModel;
            var roundedRect = new D2D.RoundedRectangle();
            float xFrom = 0;
            float yFrom = 0;
            float xTo = 0;
            float yTo = 0;
            for (int xi = 0; xi < resultDescriptionModel.BinRanges[_xIndex].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < resultDescriptionModel.BinRanges[_yIndex].GetBins().Count; yi++)
                {
                    BinIndex binIndex = new BinIndex(xi, yi);
                    if (_binDictonary.ContainsKey(binIndex))
                    {
                        foreach (var resultItem in _binDictonary[binIndex])
                        {
                            double? xValue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.X).First()].Value;
                            double? yValue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.Y).First()].Value;
                            double? value = null;
                            double? unNormalizedvalue = null;
                            if (_queryModel.GetUsageInputOperationModel(InputUsage.Value).Any() && resultItem.Values.ContainsKey(_queryModel.GetUsageInputOperationModel(InputUsage.Value).First()))
                            {
                                unNormalizedvalue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.Value).First()].Value;
                                value = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.Value).First()].NoramlizedValue;
                            }
                            else if (_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).Any() && resultItem.Values.ContainsKey(_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()))
                            {
                                unNormalizedvalue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].Value;
                                value = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].NoramlizedValue;
                            }

                            if (value != null)
                            {
                                if (_isXAxisAggregated && !_isYAxisAggregated &&
                                    (_xAom.AggregateFunction == AggregateFunction.Count || _xAom.AggregateFunction == AggregateFunction.Sum || _xAom.AggregateFunction == AggregateFunction.Avg ||
                                     _xAom.AggregateFunction == AggregateFunction.Min || _xAom.AggregateFunction == AggregateFunction.Max))
                                {
                                    xFrom = toScreenX((float)Math.Min(0, xValue.Value));
                                }
                                else
                                {
                                    xFrom = toScreenX((float) xBins[_xBinRange.GetDisplayIndex(xValue.Value)]);
                                }

                                if (!_isXAxisAggregated && _isYAxisAggregated &&
                                    (_yAom.AggregateFunction == AggregateFunction.Count || _yAom.AggregateFunction == AggregateFunction.Sum || _yAom.AggregateFunction == AggregateFunction.Avg ||
                                     _yAom.AggregateFunction == AggregateFunction.Min || _yAom.AggregateFunction == AggregateFunction.Max))
                                {
                                    yFrom = toScreenY((float)Math.Min(0, yValue.Value));
                                }
                                else
                                {
                                    yFrom = toScreenY((float) yBins[_yBinRange.GetDisplayIndex(yValue.Value)]);
                                }

                                if (_xBinRange is NominalBinRange)
                                {
                                    xTo = toScreenX((float) xBins[_xBinRange.GetDisplayIndex(xValue.Value) + 1]);
                                }
                                else
                                {
                                    xTo = toScreenX((float) xBins[_xBinRange.GetDisplayIndex(_xBinRange.AddStep(xValue.Value))]);
                                }

                                if (_yBinRange is NominalBinRange)
                                {
                                    yTo = toScreenY((float) yBins[_yBinRange.GetDisplayIndex(yValue.Value) + 1]);
                                }
                                else
                                {
                                    yTo = toScreenY((float) yBins[_yBinRange.GetDisplayIndex(_yBinRange.AddStep(yValue.Value))]);
                                }


                                float alpha = 0.1f * (float)Math.Log10(value.Value) + 1f;
                                var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)Math.Sqrt(value.Value));
                                var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R / 255f, lerpColor.G / 255f, lerpColor.B / 255f, 1f));

                                roundedRect.Rect = new RectangleF(
                                    xFrom,
                                    yTo,
                                    xTo - xFrom,
                                    yFrom - yTo);
                                roundedRect.RadiusX = roundedRect.RadiusY = 4;
                                d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                                if (_isXAxisAggregated || _isYAxisAggregated)
                                {
                                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                                    
                                    IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                                    var filterModel = new FilterModel();
                                    for (int i = 0; i < resultDescriptionModel.BinRanges.Count; i++)
                                    {
                                        if (!(resultDescriptionModel.BinRanges[i] is AggregateBinRange))
                                        {
                                            double? binRangeValue = (double?) resultItem.Values[resultDescriptionModel.Dimensions[i]].Value;
                                            if (binRangeValue.HasValue)
                                            {
                                                var bins = resultDescriptionModel.BinRanges[i].GetBins();
                                                bins.Add(resultDescriptionModel.BinRanges[i].AddStep(bins.Max()));
                                                var v = resultDescriptionModel.BinRanges[i].GetIndex(binRangeValue.Value);
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
                                    if (!HitTargets.ContainsKey(hitGeom))
                                    {
                                        HitTargets.Add(hitGeom, filterModel);
                                        _filterModelRects.Add(filterModel, roundedRect);
                                    }
                                }
                            }
                        }
                    }
                    xFrom = toScreenX((float)xBins[xi]);
                    yFrom = toScreenY((float)yBins[yi]);
                    xTo = toScreenX((float)xBins[xi + 1]);
                    yTo = toScreenY((float)yBins[yi + 1]);
                    roundedRect.Rect = new RectangleF(
                                xFrom,
                                yTo,
                                xTo - xFrom,
                                yFrom - yTo);
                    roundedRect.RadiusX = roundedRect.RadiusY = 4;
                    if (!_isXAxisAggregated && !_isYAxisAggregated)
                    {
                        d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                    }
                }
            }

            if (!_isXAxisAggregated && !_isYAxisAggregated)
            {
                for (int xi = 0; xi < resultDescriptionModel.BinRanges[_xIndex].GetBins().Count; xi++)
                {
                    for (int yi = 0; yi < resultDescriptionModel.BinRanges[_yIndex].GetBins().Count; yi++)
                    {
                        BinIndex binIndex = new BinIndex(xi, yi);
                        double? unNormalizedvalue = null;
                        if (_binDictonary.ContainsKey(binIndex))
                        {
                            foreach (var resultItem in _binDictonary[binIndex])
                            {
                                if (_queryModel.GetUsageInputOperationModel(InputUsage.Value).Any() && resultItem.Values.ContainsKey(_queryModel.GetUsageInputOperationModel(InputUsage.Value).First()))
                                {
                                    unNormalizedvalue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.Value).First()].Value;
                                }
                                else if (_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).Any() && resultItem.Values.ContainsKey(_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()))
                                {
                                    unNormalizedvalue = (double?)resultItem.Values[_queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].Value;
                                }
                            }
                        }

                        xFrom = toScreenX((float) xBins[xi]);
                        yFrom = toScreenY((float) yBins[yi]);
                        xTo = toScreenX((float) xBins[xi + 1]);
                        yTo = toScreenY((float) yBins[yi + 1]);
                        roundedRect.Rect = new RectangleF(
                            xFrom,
                            yTo,
                            xTo - xFrom,
                            yFrom - yTo);
                        roundedRect.RadiusX = roundedRect.RadiusY = 4;

                        IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                        var filterModel = new FilterModel();
                        filterModel.Value = unNormalizedvalue;
                        if (resultDescriptionModel.BinRanges[_xIndex] is NominalBinRange)
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.EQUALS,
                                  resultDescriptionModel.BinRanges[_xIndex].GetLabels()[xi].Label));
                        }
                        else
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.GREATER_THAN_EQUAL, xBins[xi]));
                            filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.LESS_THAN, xBins[xi + 1]));
                        }
                        if (resultDescriptionModel.BinRanges[_yIndex] is NominalBinRange)
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.EQUALS,
                                  resultDescriptionModel.BinRanges[_yIndex].GetLabels()[yi].Label));
                        }
                        else
                        {
                            filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.GREATER_THAN_EQUAL, yBins[yi]));
                            filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.LESS_THAN, yBins[yi + 1]));
                        }
                        HitTargets.Add(hitGeom, filterModel);

                        if (_filterModels.Contains(filterModel))
                        {
                            d2dDeviceContext.DrawRoundedRectangle(roundedRect, dark, 0.5f);
                        }
                    }
                }
            }
            else
            {
                foreach (var filterModel in _filterModelRects.Keys)
                {
                    if (_filterModels.Contains(filterModel))
                    {
                        d2dDeviceContext.DrawRoundedRectangle(_filterModelRects[filterModel], dark, 0.5f);
                    }
                }
            }
            dark.Dispose();
            white.Dispose();
        }

        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));
        }

        private float toScreenX(float x)
        {
            return ((x - _minX) / _xScale) * (_deviceWidth) + (_leftOffset);
        }
        private float toScreenY(float y)
        {
            float retY = ((y - _minY) / _yScale) * (_deviceHeight);
            return _flipY ? (_deviceHeight) - retY + (_topOffset) : retY + (_topOffset);
        }
    }

    public class BinPrimitive
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public float TargetW { get; set; }
        public float TargetH { get; set; }
        public float A { get; set; }
        public float TargetA { get; set; }

        public void Render(D2D.DeviceContext d2dDeviceContext, D2D.SolidColorBrush brush, bool fill)
        {
            var roundedRect = new D2D.RoundedRectangle { Rect = new RectangleF(X, Y, W, H) };
            roundedRect.RadiusX = roundedRect.RadiusY = 4;
            if (fill)
            {
                d2dDeviceContext.FillRoundedRectangle(roundedRect, brush);
            }
            else
            {
                d2dDeviceContext.DrawRoundedRectangle(roundedRect, brush);
            }
        }
    }
}