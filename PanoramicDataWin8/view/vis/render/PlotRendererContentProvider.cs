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
        private DataScaler _dataScaler = new DataScaler();
        private Stopwatch _frameStopwatch = new Stopwatch();
        private D2D.Brush _textBrush;
        private DW.TextFormat _textFormat;

        private ResultModel _resultModel = null;
        private VisualizationResultDescriptionModel _visualizationDescriptionModel = null;

        private QueryModel _queryModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
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

        public void ResetData()
        {
            _binPrimitives.Clear();
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
                }
                _isYAxisAggregated = true;
                _yBinRange = QuantitativeBinRange.Initialize(_visualizationDescriptionModel.MinValues[yAom] * (1.0 - factor), _visualizationDescriptionModel.MaxValues[yAom] * (1.0 + factor), 10, false);
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

            _dataScaler.LeftOffset = Math.Max(10, metricsY.Width + 10 + 20);

            _dataScaler.DeviceWidth = (float)(d2dDeviceContext.Size.Width / CompositionScaleX - _dataScaler.LeftOffset - _dataScaler.RightOffset);
            _dataScaler.DeviceHeight = (float)(d2dDeviceContext.Size.Height / CompositionScaleY - _dataScaler.TopOffset - _dataScaler.BottomtOffset);

            _dataScaler.MinX = (float)(xLabels.Min(dp => dp.MinValue));
            _dataScaler.MinY = (float)(yLabels.Min(dp => dp.MinValue));
            _dataScaler.MaxX = (float)(xLabels.Max(dp => dp.MaxValue));
            _dataScaler.MaxY = (float)(yLabels.Max(dp => dp.MaxValue));

            _dataScaler.XScale = _dataScaler.MaxX - _dataScaler.MinX;
            _dataScaler.YScale = _dataScaler.MaxY - _dataScaler.MinY;

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

            float xFrom = 0;
            float xTo = 0;
            float yFrom = 0;
            float yTo = 0;
            bool lastLabel = false;

            // x labels and grid lines
            int mod = (int)Math.Ceiling(1.0 / (Math.Floor((_dataScaler.DeviceWidth / (metricsX.Width + 5))) / xLabels.Count));
            int count = 0;
            foreach (var label in xLabels)
            {
                yFrom = _dataScaler.ToScreenY(_dataScaler.MinY);
                yTo = _dataScaler.ToScreenY(_dataScaler.MaxY);
                xFrom = _dataScaler.ToScreenX((float)label.MinValue);
                xTo = _dataScaler.ToScreenX((float)label.MaxValue);
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
            mod = (int)Math.Ceiling(1.0 / (Math.Floor((_dataScaler.DeviceHeight / (metricsY.Height + 0))) / yLabels.Count));
            count = 0;
            foreach (var label in yLabels)
            {
                xFrom = _dataScaler.ToScreenX(_dataScaler.MinX);
                xTo = _dataScaler.ToScreenX(_dataScaler.MaxX);
                yFrom = _dataScaler.ToScreenY((float)label.MinValue);
                yTo = _dataScaler.ToScreenY((float)label.MaxValue);
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

            white.Dispose();
            layoutX.Dispose();
            layoutY.Dispose();
        }

        private void renderCell(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            computeSizesAndRenderLabels(d2dDeviceContext, dwFactory, false);
            if (_dataScaler.DeviceHeight < 0 || _dataScaler.DeviceWidth < 0)
            {
                return;
            }

            _frameStopwatch.Stop();

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var dark = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(11f / 255f, 11f / 255f, 11f / 255f, 1f));

            var xBins = _xBinRange.GetBins();
            xBins.Add(_xBinRange.AddStep(xBins.Max()));
            var yBins = _yBinRange.GetBins();
            yBins.Add(_yBinRange.AddStep(yBins.Max()));

            // draw data
            HitTargets.Clear();
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
                            BinIndex renderBinIndex = new BinIndex();
                            for (int d = 0; d < resultDescriptionModel.Dimensions.Count; d++)
                            {
                                renderBinIndex.Indices.Add(resultDescriptionModel.BinRanges[d].GetIndex((double) resultItem.Values[resultDescriptionModel.Dimensions[d]].Value));
                            }
                            BinPrimitive binPrimitive = null;
                            if (_binPrimitives.ContainsKey(renderBinIndex))
                            {
                                binPrimitive = _binPrimitives[renderBinIndex];
                                binPrimitive.Update(resultItem, _frameStopwatch.ElapsedMilliseconds, _queryModel, _xBinRange, _yBinRange, xBins, yBins, _dataScaler);
                            }
                            else
                            {
                                binPrimitive = new BinPrimitive();
                                //binPrimitive.Initialize(resultItem, _frameStopwatch.ElapsedMilliseconds, _queryModel, _xBinRange, _yBinRange, xBins, yBins, _dataScaler);
                                _binPrimitives.Add(renderBinIndex, binPrimitive);
                            }
                            binPrimitive.Animate(_frameStopwatch.ElapsedMilliseconds);

                            var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)Math.Sqrt(binPrimitive.A));
                            var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R / 255f, lerpColor.G / 255f, lerpColor.B / 255f, 1f));
                            binPrimitive.Render(d2dDeviceContext, binColor, true);
                             
                            binColor.Dispose();
                        }
                    }

                    if (!_isXAxisAggregated && !_isYAxisAggregated)
                    {
                        xFrom = _dataScaler.ToScreenX((float) xBins[xi]);
                        yFrom = _dataScaler.ToScreenY((float) yBins[yi]);
                        xTo = _dataScaler.ToScreenX((float) xBins[xi + 1]);
                        yTo = _dataScaler.ToScreenY((float) yBins[yi + 1]);
                        roundedRect.Rect = new RectangleF(
                            xFrom,
                            yTo,
                            xTo - xFrom,
                            yFrom - yTo);
                        roundedRect.RadiusX = roundedRect.RadiusY = 4;


                        IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                        var filterModel = new FilterModel();
                        filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.GREATER_THAN_EQUAL,
                            xBins[xi]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.LESS_THAN, xBins[xi + 1]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.GREATER_THAN_EQUAL,
                            yBins[yi]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.LESS_THAN, yBins[yi + 1]));
                        HitTargets.Add(hitGeom, filterModel);

                        d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                    }
                }
            }

            for (int xi = 0; xi < resultDescriptionModel.BinRanges[_xIndex].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < resultDescriptionModel.BinRanges[_yIndex].GetBins().Count; yi++)
                {
                    if (!_isXAxisAggregated && !_isYAxisAggregated)
                    {
                        xFrom = _dataScaler.ToScreenX((float)xBins[xi]);
                        yFrom = _dataScaler.ToScreenY((float)yBins[yi]);
                        xTo = _dataScaler.ToScreenX((float)xBins[xi + 1]);
                        yTo = _dataScaler.ToScreenY((float)yBins[yi + 1]);
                        roundedRect.Rect = new RectangleF(
                                    xFrom,
                                    yTo,
                                    xTo - xFrom,
                                    yFrom - yTo);
                        roundedRect.RadiusX = roundedRect.RadiusY = 4;

                        IGeometry hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                        var filterModel = new FilterModel();
                        filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.GREATER_THAN_EQUAL, xBins[xi]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_xAom, Predicate.LESS_THAN, xBins[xi + 1]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.GREATER_THAN_EQUAL, yBins[yi]));
                        filterModel.ValueComparisons.Add(new ValueComparison(_yAom, Predicate.LESS_THAN, yBins[yi + 1]));

                        if (_filterModels.Contains(filterModel))
                        {
                            d2dDeviceContext.DrawRoundedRectangle(roundedRect, dark, 0.5f);
                        }
                    }
                }
            }
            dark.Dispose();
            white.Dispose();

            _frameStopwatch.Start();
        }

        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));
        }

        
    }

    public class DataScaler
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

        public float LeftOffset
        {
            get { return _leftOffset; }
            set { _leftOffset = value; }
        }

        public float RightOffset
        {
            get { return _rightOffset; }
            set { _rightOffset = value; }
        }

        public float TopOffset
        {
            get { return _topOffset; }
            set { _topOffset = value; }
        }

        public float BottomtOffset
        {
            get { return _bottomtOffset; }
            set { _bottomtOffset = value; }
        }

        public float DeviceWidth
        {
            get { return _deviceWidth; }
            set { _deviceWidth = value; }
        }

        public float DeviceHeight
        {
            get { return _deviceHeight; }
            set { _deviceHeight = value; }
        }

        public float XScale
        {
            get { return _xScale; }
            set { _xScale = value; }
        }

        public float YScale
        {
            get { return _yScale; }
            set { _yScale = value; }
        }

        public bool FlipY
        {
            get { return _flipY; }
            set { _flipY = value; }
        }

        public float MinX
        {
            get { return _minX; }
            set { _minX = value; }
        }

        public float MinY
        {
            get { return _minY; }
            set { _minY = value; }
        }

        public float MaxX
        {
            get { return _maxX; }
            set { _maxX = value; }
        }

        public float MaxY
        {
            get { return _maxY; }
            set { _maxY = value; }
        }

        public float ToScreenX(float x)
        {
            return ((x - _minX) / _xScale) * (_deviceWidth) + (_leftOffset);
        }

        public float ToScreenY(float y)
        {
            float retY = ((y - _minY) / _yScale) * (_deviceHeight);
            return _flipY ? (_deviceHeight) - retY + (_topOffset) : retY + (_topOffset);
        }
    }

    public class BinPrimitive
    {
        private double? _xValue = null;
        private double? _yValue  = null;
        private double? _value = null;
        private List<double> _xBins = null;
        private List<double> _yBins = null;

        public float X { get; set; }
        public float Y { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float StartX { get; set; }
        public float StartY { get; set; } 
        public float W { get; set; }
        public float H { get; set; }
        public float TargetW { get; set; }
        public float TargetH { get; set; }
        public float StartW { get; set; }
        public float StartH { get; set; }
        public float A { get; set; }
        public float TargetA { get; set; }
        public float StartA { get; set; }
        public long XAninmationStartTime { get; set; }
        public long YAninmationStartTime { get; set; }
        public long AAninmationStartTime { get; set; }

        public bool IsInitialized { get; set; }

        public void Initialize(VisualizationItemResultModel resultItem, long elapsedTime, QueryModel queryModel, BinRange xBinRange, BinRange yBinRange, List<double> xBins, List<double> yBins, DataScaler dataScaler)
        {
            //Update(resultItem, elapsedTime, queryModel, xBinRange, yBinRange, xBins, yBins, dataScaler);

            IsInitialized = true;
            A = StartA = 0;
            X = StartX = TargetX;
            Y = StartY = TargetY;
            H = StartH = TargetH;
            W = StartW = TargetW;
            _xBins = xBins;
            _yBins = yBins;
        }

        public void Update(VisualizationItemResultModel resultItem, long elapsedTime, QueryModel queryModel, BinRange xBinRange, BinRange yBinRange, List<double> xBins, List<double> yBins, DataScaler dataScaler)
        {
            double? newXValue = null;
            double? newYValue = null;
            double? newValue = null;
            
            getRawValues(out newXValue, out newYValue, out newValue, resultItem, queryModel);
            float xFrom = 0;
            float yFrom = 0;
            float xTo = 0;
            float yTo = 0;
            float a = 0;

            getScreenValues(out xFrom, out yFrom, out xTo, out yTo, out a, xBinRange, yBinRange, xBins, yBins, newXValue, newYValue, newValue, dataScaler);

            if (newXValue.HasValue && newYValue.HasValue && newValue.HasValue)
            {
                if (newXValue != _xValue || _xBins.Count != xBins.Count || _yBins.Count != yBins.Count)
                {
                    _xValue = newXValue;
                    _xBins = xBins;
                    XAninmationStartTime = elapsedTime;
                    TargetX = xFrom;
                    TargetW = xTo - xFrom;
                    StartX = X;
                    StartW = W;
                }
                if (newYValue != _yValue || _yBins.Count != yBins.Count || _xBins.Count != xBins.Count)
                {
                    _yValue = newYValue;
                    _yBins = yBins;
                    YAninmationStartTime = elapsedTime;
                    TargetY = yTo;
                    TargetH = yFrom - yTo;
                    StartY = Y;
                    StartH = H;
                }
                if (newValue != _value)
                {
                    _value = newValue;
                    AAninmationStartTime = elapsedTime;
                    TargetA = (float) a;
                    StartA = A;
                }

                if (!IsInitialized)
                {
                    Initialize(resultItem, elapsedTime, queryModel, xBinRange, yBinRange, xBins, yBins, dataScaler);
                }
            }
        }

        private void getRawValues(out double? xValue, out double? yValue, out double? value,
            VisualizationItemResultModel resultItem, QueryModel queryModel)
        {
            xValue = (double?) resultItem.Values[queryModel.GetUsageInputOperationModel(InputUsage.X).First()].Value;
            yValue = (double?) resultItem.Values[queryModel.GetUsageInputOperationModel(InputUsage.Y).First()].Value;
            value = null;
            if (queryModel.GetUsageInputOperationModel(InputUsage.Value).Any() && resultItem.Values.ContainsKey(queryModel.GetUsageInputOperationModel(InputUsage.Value).First()))
            {
                value = (double?) resultItem.Values[queryModel.GetUsageInputOperationModel(InputUsage.Value).First()].NoramlizedValue;
            }
            else if (queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).Any() &&
                     resultItem.Values.ContainsKey(queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()))
            {
                value = (double?) resultItem.Values[queryModel.GetUsageInputOperationModel(InputUsage.DefaultValue).First()].NoramlizedValue;
            }
        }

        private bool getScreenValues(out float xFrom, out float yFrom, out float xTo, out float yTo, out float a,
            BinRange xBinRange, BinRange yBinRange, List<double> xBins, List<double> yBins,
            double? xValue, double? yValue, double? value, DataScaler dataScaler)
        {
            xFrom = 0;
            yFrom = 0;
            xTo = 0;
            yTo = 0;
            a = 0;

            if (value.HasValue && xValue.HasValue && yValue.HasValue)
            {
                xFrom = dataScaler.ToScreenX((float) xBins[xBinRange.GetIndex(xValue.Value)]);
                yFrom = dataScaler.ToScreenY((float) yBins[yBinRange.GetIndex(yValue.Value)]);
                xTo = dataScaler.ToScreenX((float) xBins[xBinRange.GetIndex(xBinRange.AddStep(xValue.Value))]);
                yTo = dataScaler.ToScreenY((float) yBins[yBinRange.GetIndex(yBinRange.AddStep(yValue.Value))]);
                a = (float)Math.Sqrt(value.Value);
                return true;
            }
            return false;
        }

        public void Render(D2D.DeviceContext d2dDeviceContext, D2D.SolidColorBrush brush, bool fill)
        {
            if (_value.HasValue && _xValue.HasValue && _yValue.HasValue)
            {
                var roundedRect = new D2D.RoundedRectangle {Rect = new RectangleF(X, Y, W, H)};
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

        public void Animate(long elapsedMilliseconds)
        {
            long animationDuration = 500;
            A = QuadraticEaseInOut(elapsedMilliseconds - AAninmationStartTime, StartA, TargetA - StartA, animationDuration);
            X = QuadraticEaseInOut(elapsedMilliseconds - XAninmationStartTime, StartX, TargetX - StartX, animationDuration);
            Y = QuadraticEaseInOut(elapsedMilliseconds - YAninmationStartTime, StartY, TargetY - StartY, animationDuration);
            H = QuadraticEaseInOut(elapsedMilliseconds - YAninmationStartTime, StartH, TargetH - StartH, animationDuration);
            W = QuadraticEaseInOut(elapsedMilliseconds - XAninmationStartTime, StartW, TargetW - StartW, animationDuration);
        }

        private float QuadraticEaseInOut(long t, float b, float c, long d)
        {
           /* t /= d / 2;
            if (t < 1) return c / 2.0f * t * t + b;
            t--;
            return -c / 2.0f * (t * (t - 2) - 1) + b;*/
            return c * (float) Math.Min(1.0, (float)t / (float)d) + b;
        }
    }
}
