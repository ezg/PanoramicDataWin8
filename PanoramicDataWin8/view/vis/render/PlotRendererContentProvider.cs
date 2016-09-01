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

        private PlotRendererContentProviderHelper _helper = null;

        private Color _textColor;
        private CanvasTextFormat _textFormat;

        private HistogramResult _histogramResult = null;

        private QueryModel _queryModelClone = null;
        private QueryModel _queryModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
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
            

            _histogramResult = (HistogramResult)result;
            _queryModelClone = queryModelClone;
            _queryModel = queryModel;
            
            if (_histogramResult != null && _histogramResult.Bins != null)
            {
                _helper = new PlotRendererContentProviderHelper((HistogramResult)result, queryModel, queryModelClone, CompositionScaleX, CompositionScaleY);
                _isResultEmpty = false;
                
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

            if (_histogramResult != null && _histogramResult.Bins != null)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderCell(canvas, canvasArgs);
                }
            }
            if (_isResultEmpty)
            {
                var leftOffset = 10;
                var rightOffset = 20;
                var topOffset = 20;
                var bottomtOffset = 45;

                var deviceWidth = (float) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - topOffset - bottomtOffset);
                DrawString(canvasArgs, _textFormat, deviceWidth / 2.0f + leftOffset, deviceHeight / 2.0f + topOffset, "no datapoints", _textColor, true, true, false);
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
                        if (_helper.VisualBinRanges[0] is QuantitativeBinRange)
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
                        if (_helper.VisualBinRanges[1] is QuantitativeBinRange)
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
                var x = _helper.DataToScreenX((float)_helper.VisualBinRanges[0].AddStep(0)) - _helper.DataToScreenX(0);
                var y = _helper.DataToScreenY((float)_helper.VisualBinRanges[1].AddStep(0), false) - _helper.DataToScreenY(0, false);

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

            List<BinPrimitiveCollection> allBinPrimitiveCollections = new List<BinPrimitiveCollection>();
            HitTargets.Clear();
            for (int xi = 0; xi < _histogramResult.BinRanges[0].GetBins().Count; xi++)
            {
                for (int yi = 0; yi < _histogramResult.BinRanges[1].GetBins().Count; yi++)
                {
                    BinIndex binIndex = new BinIndex(xi, yi);
                    
                    if (_histogramResult.Bins.ContainsKey(binIndex))
                    {
                        var bin = _histogramResult.Bins[binIndex];
                        var binPrimitiveCollection = _helper.GetBinPrimitives(bin);
                        allBinPrimitiveCollections.Add(binPrimitiveCollection);

                        foreach (var binPrimitive in binPrimitiveCollection.BinPrimitives.Where(bp => bp.Value != 0.0 && bp.BrushIndex != _histogramResult.AllBrushIndex()))
                        {
                            canvasArgs.DrawingSession.FillRoundedRectangle(binPrimitive.Rect, 4, 4, binPrimitive.Color);
                        }

                        if (binPrimitiveCollection.FilterModel != null)
                        {
                            HitTargets.Add(binPrimitiveCollection.HitGeom, binPrimitiveCollection.FilterModel);
                        }
                    }
                }
            }

            foreach (var binPrimitiveCollection in allBinPrimitiveCollections)
            {
                if (binPrimitiveCollection.FilterModel != null && _filterModels.Contains(binPrimitiveCollection.FilterModel))
                {
                    canvasArgs.DrawingSession.DrawRoundedRectangle(binPrimitiveCollection.BinPrimitives.First(bp => bp.BrushIndex == _histogramResult.AllBrushIndex()).Rect, 4, 4, dark, 1.0f);
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

    public class BinPrimitiveCollection
    {
        public List<BinPrimitive>  BinPrimitives { get; set; } = new List<BinPrimitive>();
        public IGeometry HitGeom { get; set; }
        public FilterModel FilterModel { get; set; }
    }

    public class BinPrimitive
    {
        public float Value { get; set; }
        public Rect Rect { get; set; }
        public Color Color { get; set; }
        public int BrushIndex { get; set; }
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

            var aggregateKey = QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            _minValue = float.MaxValue;
            _maxValue = float.MinValue;
            foreach (var brush in _histogramResult.Brushes)
            {
                aggregateKey.BrushIndex = brush.BrushIndex;
                foreach (var bin in _histogramResult.Bins.Values)
                {
                    _minValue = (float)Math.Min(_minValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                    _maxValue = (float)Math.Max(_maxValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                }
            }
            
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
                
                var minValue = float.MaxValue;
                var maxValue = float.MinValue;
                foreach (var brush in _histogramResult.Brushes)
                {
                    aggregateKey.BrushIndex = brush.BrushIndex;
                    foreach (var bin in _histogramResult.Bins.Values)
                    {
                        minValue = (float)Math.Min(minValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                        maxValue = (float)Math.Max(maxValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                    }
                }

                if (minValue - maxValue == 0)
                {
                    factor = 0.1;
                    factor *= minValue < 0 ? -1f : 1f;
                }
                visualBinRange = QuantitativeBinRange.Initialize(minValue*(1.0 - factor), maxValue*(1.0 + factor), 10, false);
                if (_chartType == ChartType.HorizontalBar || _chartType == ChartType.VerticalBar)
                {
                    visualBinRange = QuantitativeBinRange.Initialize(Math.Min(0, minValue), visualBinRange.DataMaxValue, 10, false);
                }
            }
            
            return visualBinRange;
        }

        public BinPrimitiveCollection GetBinPrimitives(Bin bin)
        {
            BinPrimitiveCollection binPrimitiveCollection = new BinPrimitiveCollection();
            float alpha = 0.15f;
            var baseColor = Colors.White;

            var valueAggregateKey = QueryModelHelper.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());

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

                valueAggregateKey.BrushIndex = brush.BrushIndex;
                float unNormalizedvalue = (float)((DoubleValueAggregateResult) bin.AggregateResults[valueAggregateKey]).Result;
                if (unNormalizedvalue != 0)
                {
                    value = (unNormalizedvalue - _minValue)/(Math.Abs((_maxValue - _minValue)) < TOLERANCE ? (unNormalizedvalue - _minValue) : (_maxValue - _minValue));
                }

                if (brush.BrushIndex == _histogramResult.RestBrushIndex())
                {
                    baseColor = Windows.UI.Color.FromArgb(255, 40, 170, 213);
                }
                else if (brush.BrushIndex == _histogramResult.OverlapBrushIndex())
                {
                    baseColor = Color.FromArgb(255, 17, 17, 17);
                }
                else if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    baseColor = Windows.UI.Color.FromArgb(255, 255, 0, 0);
                }
                else
                {
                    baseColor = _queryModelClone.BrushColors[brush.BrushIndex % _queryModelClone.BrushColors.Count];
                }

                var xAggregateKey = QueryModelHelper.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex);
                var yAggregateKey = QueryModelHelper.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex);
                var xMarginAggregateKey = QueryModelHelper.CreateAggregateKey(_xIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex);
                var yMarginAggregateKey = QueryModelHelper.CreateAggregateKey(_yIom, new MarginAggregateParameters(), _histogramResult, brush.BrushIndex);

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
                    var xValue = ((DoubleValueAggregateResult) bin.AggregateResults[xAggregateKey]).Result;
                    xFrom = DataToScreenX((float) Math.Min(0, xValue));
                    xTo = DataToScreenX((float) Math.Max(0, xValue));

                    yFrom = DataToScreenY((float) VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1]));
                    yTo = DataToScreenY((float) VisualBinRanges[1].AddStep(VisualBinRanges[1].GetValueFromIndex(bin.BinIndex.Indices[1])));

                    xMargin = (float) ((MarginAggregateResult) bin.AggregateResults[xMarginAggregateKey]).Margin;
                    xMarginAbsolute = (float) ((MarginAggregateResult) bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = baseColor;
                }

                else if (_chartType == ChartType.VerticalBar)
                {
                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
                    yFrom = DataToScreenY((float)Math.Min(0, yValue));
                    yTo = DataToScreenY((float)Math.Max(0, yValue));

                    xFrom = DataToScreenX((float)VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0]));
                    xTo = DataToScreenX((float)VisualBinRanges[0].AddStep(VisualBinRanges[0].GetValueFromIndex(bin.BinIndex.Indices[0])));

                    yMargin = (float)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
                    yMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = baseColor;
                }

                else if (_chartType == ChartType.SinglePoint)
                {
                    var xValue = ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result;
                    xFrom = DataToScreenX((float) xValue) - 5;
                    xTo = DataToScreenX((float) xValue) + 5;

                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
                    yFrom = DataToScreenY((float)yValue) + 5;
                    yTo = DataToScreenY((float)yValue);

                    xMargin = (float)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).Margin;
                    xMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;
                    
                    yMargin = (float)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
                    yMarginAbsolute = (float)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;

                    color = baseColor;
                }


                
                if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    IGeometry hitGeom = null;
                    FilterModel filterModel = null;
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
                    binPrimitiveCollection.FilterModel = filterModel;
                    binPrimitiveCollection.HitGeom = hitGeom;
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
                };
                binPrimitiveCollection.BinPrimitives.Add(binPrimitive);
            }


            // adjust brush rects (stacking or not)
            BinPrimitive allBrushBinPrimitive = binPrimitiveCollection.BinPrimitives.FirstOrDefault(b => b.BrushIndex == _histogramResult.AllBrushIndex());
            double sum = 0.0f;
            foreach (var bp in binPrimitiveCollection.BinPrimitives.Where(b => b.BrushIndex != _histogramResult.AllBrushIndex()))
            {
                if (_chartType == ChartType.VerticalBar)
                {
                    if (_yIom.AggregateFunction == AggregateFunction.Count)
                    {
                        //var brushFactor = (bp.Value / allBrushBinPrimitive.Value);
                        bp.Rect = new Rect(bp.Rect.X, bp.Rect.Y - sum, bp.Rect.Width, bp.Rect.Height);
                        sum += bp.Rect.Height;
                    }
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    if (_xIom.AggregateFunction == AggregateFunction.Count)
                    {
                        //var brushFactor = (bp.Value / allBrushBinPrimitive.Value);
                        bp.Rect = new Rect(bp.Rect.X + sum, bp.Rect.Y, bp.Rect.Width, bp.Rect.Height);
                        sum += bp.Rect.Width;
                    }
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    //var brushFactor = (bp.Value / allBrushBinPrimitive.Value);
                    bp.Rect = new Rect(bp.Rect.X + sum, bp.Rect.Y, bp.Rect.Width, bp.Rect.Height);
                    sum += bp.Rect.Width;
                }
            }
            binPrimitiveCollection.BinPrimitives.Reverse();
            return binPrimitiveCollection;
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