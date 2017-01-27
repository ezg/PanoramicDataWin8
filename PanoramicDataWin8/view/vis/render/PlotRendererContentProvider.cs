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
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
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
        
        private HistogramOperationModel _histogramOperationModel = null;
        private List<FilterModel> _filterModels = new List<FilterModel>();
        private List<BczBinMapModel> _bczBinMapModels = new List<BczBinMapModel>();
        private CanvasCachedGeometry _fillRoundedRectGeom = null;
        private CanvasCachedGeometry _strokeRoundedRectGeom = null;

        public double CompositionScaleX { get; set; }
        public double CompositionScaleY { get; set; }
        public Dictionary<IGeometry, FilterModel> HitTargets { get; set; }
        public Dictionary<IGeometry, BczBinMapModel> BczHitTargets { get; set; }

        public PlotRendererContentProvider()
        {
            HitTargets = new Dictionary<IGeometry, FilterModel>();
            BczHitTargets = new Dictionary<IGeometry, BczBinMapModel>();
        }

        public void UpdateFilterModels(List<FilterModel> filterModels)
        {
            _filterModels = filterModels;
        }

        public void RemoveBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            foreach (var mapItem in binMapModels)
                _bczBinMapModels.Remove(mapItem);
        }
        public void UpdateBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            var newbinMaps = new List<BczBinMapModel>();
            foreach (var fm in binMapModels)
            {
                newbinMaps.Clear();
                newbinMaps.Add(fm);
            }
            // remove any existing binMap _bczBinMapModels that has the same sort axis as a new bin map model
            foreach (var binMapModel in _bczBinMapModels)
            {
                bool skip = false;
                foreach (var fm in newbinMaps)
                    if (fm.SortAxis == binMapModel.SortAxis)
                        skip = true;
                if (!skip)
                    newbinMaps.Add(binMapModel);
            }
            _bczBinMapModels.Clear();
            foreach (var fm in newbinMaps)
                _bczBinMapModels.Add(fm);
            _bczBinMapModels = binMapModels;
        }
        public void UpdateBinSortings(List<BczBinMapModel> bczhits)
        {
            if (bczhits.Any(h => _bczBinMapModels.Contains(h)))
            {
                RemoveBczBinMapModels(bczhits);
                if (!bczhits.First().SortUp)
                {
                    bczhits.First().SortUp = true;
                    UpdateBczBinMapModels(bczhits);
                }
            }
            else
            {
                UpdateBczBinMapModels(bczhits);
            }
        }

        public void UpdateData(IResult result, HistogramOperationModel histogramOperationModel, HistogramOperationModel histogramOperationModelClone)
        {
            _histogramResult = (HistogramResult)result;
            _histogramOperationModel = histogramOperationModel;
            
            if (_histogramResult != null && _histogramResult.Bins != null)
            {
                _helper = new PlotRendererContentProviderHelper((HistogramResult)result, histogramOperationModel, histogramOperationModelClone, CompositionScaleX, CompositionScaleY);
                _isResultEmpty = false;
                
            }
            else
            {
                _isResultEmpty = true;
            }
        }

        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(CommonExtensions.ToVector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_histogramResult != null && _histogramResult.Bins != null)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
                {
                    renderPlot(canvas, canvasArgs);
                }
            }
            if (_isResultEmpty)
            {
                var leftOffset = 10;
                var rightOffset = 20;
                var topOffset = 20;
                var bottomtOffset = 45;

                var deviceWidth = (double) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (double)(canvas.ActualHeight / CompositionScaleY - topOffset - bottomtOffset);
                DrawString(canvasArgs, _textFormat, deviceWidth / 2.0f + leftOffset, deviceHeight / 2.0f + topOffset, "no datapoints", _textColor, true, true, false);
            }
        }

        private void  computeSizesAndRenderLabels(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines, Dictionary<int,int> sortedXList, Dictionary<int, int> sortedYList)
        {
            _helper.ComputeSizes(canvas, _textFormat);
            
            if (_helper.DeviceHeight > 0 && _helper.DeviceWidth > 0)
            {
                drawLabelsAndGridLines(canvasArgs, renderLines, true,  sortedXList);   // x labels and grid lines\
                drawLabelsAndGridLines(canvasArgs, renderLines, false, sortedYList);   // y labels and grid lines

                _fillRoundedRectGeom?.Dispose();
                _strokeRoundedRectGeom?.Dispose();
                var x = _helper.DataToScreenX((double)_helper.VisualBinRanges[0].AddStep(0)) - _helper.DataToScreenX(0);
                var y = _helper.DataToScreenY((double)_helper.VisualBinRanges[1].AddStep(0), false) - _helper.DataToScreenY(0, false);
                
                _fillRoundedRectGeom = CanvasCachedGeometry.CreateFill(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4));
                _strokeRoundedRectGeom = CanvasCachedGeometry.CreateStroke(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4), 0.5f);
            }
        }

        void drawLabelsAndGridLines(Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines, bool xaxis, Dictionary<int, int> sortedList)
        {
            double xFrom = 0, xTo = 0, yFrom = 0, yTo = 0;
            var white    = Color.FromArgb(255, 255, 255, 255);
            var binRange = _helper.VisualBinRanges[xaxis ? 0 : 1];
            var Labels   = binRange.GetLabels();
            var dim = xaxis ? _helper.DeviceWidth  / (_helper.LabelMetricsX.Width  + 5) :
                              _helper.DeviceHeight / (_helper.LabelMetricsY.Height + 5);
            int mod = (int)Math.Ceiling(Labels.Count / dim );

            foreach (var label in Labels)
            {
                xFrom = _helper.DataToScreenX(xaxis ? label.MinValue   : _helper.DataMinX);
                xTo   = _helper.DataToScreenX(xaxis ? label.MaxValue   : _helper.DataMaxX);
                yFrom = _helper.DataToScreenY(xaxis ? _helper.DataMinY : label.MinValue);
                yTo   = _helper.DataToScreenY(xaxis ? _helper.DataMaxY : label.MaxValue);
                
                var drawBinIndex = Labels.IndexOf(label);
                foreach (var l in sortedList)
                    if (l.Value == drawBinIndex)
                    {
                        drawBinIndex = l.Key;
                        break;
                    }
                var drawLabel = Labels[drawBinIndex];
                if (// adds targets on x/Y axis for sorting
                    (false && _helper.ChartType == ChartType.HeatMap) || 
                     _helper.ChartType == (xaxis ? ChartType.HorizontalBar : ChartType.VerticalBar))
                {
                    var binPrimitive = new BinPrimitive()
                    {
                        Rect = new Rect(xaxis ? xFrom       : xFrom - 20,
                                        xaxis ? yFrom + 5   : yTo,
                                        xaxis ? xTo - xFrom : 10,
                                        xaxis ? 15          : yFrom - yTo),
                        Color = Color.FromArgb(25, 125, 125, 125)
                    };
                    // add hit targets for sorting by clicking this label
                    var bmc = new BinPrimitiveCollection();
                    bmc.BinPrimitives.Add(binPrimitive);
                    bmc.HitGeom = new Rct(binPrimitive.Rect.Left, binPrimitive.Rect.Top, binPrimitive.Rect.Right, binPrimitive.Rect.Bottom).GetPolygon();
                    bmc.BczBinMapModel = new BczBinMapModel(_helper.ChartType == ChartType.HeatMap ? drawLabel.Value : 0, xaxis);
                    if (_bczBinMapModels.Contains(bmc.BczBinMapModel))
                    {
                        binPrimitive.Color = Color.FromArgb((byte)150, 125, 125, 125);
                        bmc.BczBinMapModel.SortUp = _bczBinMapModels[_bczBinMapModels.IndexOf(bmc.BczBinMapModel)].SortUp;
                    }
                    BczHitTargets.Add(bmc.HitGeom, bmc.BczBinMapModel);

                    // draw hit target
                    canvasArgs.DrawingSession.FillRoundedRectangle(binPrimitive.Rect, 4, 4, binPrimitive.Color);
                }
                
                if (renderLines)
                {
                    canvasArgs.DrawingSession.DrawLine(CommonExtensions.ToVector2(xFrom, yFrom), 
                                                       CommonExtensions.ToVector2(xaxis ? xFrom:xTo, xaxis ? yTo:yFrom), white, 0.5f);
                    if (label == Labels.Last())
                    {
                        canvasArgs.DrawingSession.DrawLine(CommonExtensions.ToVector2(xaxis ? xTo:xFrom, xaxis ? yFrom:yTo), 
                                                           CommonExtensions.ToVector2(xTo, yTo), white, 0.5f);
                    }
                }
                if (Labels.IndexOf(label) % mod == 0)
                {
                    var xStart = xaxis ? xFrom + (xTo - xFrom) / 2.0f : xFrom - 10;
                    var yStart = xaxis ? yFrom + 5                    : yFrom + (yTo - yFrom) / 2.0f;
                    var text = drawLabel.Label.ToString();
                    if (binRange is QuantitativeBinRange || binRange is DateTimeBinRange)
                    {
                        xStart = xaxis ? xFrom  : xStart;
                        yStart = xaxis ? yStart : yFrom;
                        if (binRange is QuantitativeBinRange)
                            text = double.Parse(drawLabel.Label).ToString();
                        if (label == Labels.Last())              
                        {
                            // DrawString(canvasArgs, _textFormat, xTo, yFrom + 5, label.MaxValue.ToString(), _textColor, true, true, false);
                        }
                    }
                    DrawString(canvasArgs, _textFormat, xStart, yStart, text, _textColor, xaxis, xaxis, !xaxis);
                }
            }
        }

        private void renderPlot(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            // bcz index sorting
            BczHitTargets.Clear();
            var xLabelOrderings = SortBinsByValue(false);
            var yLabelOrderings = SortBinsByValue(true);

            computeSizesAndRenderLabels(canvas, canvasArgs, true, xLabelOrderings, yLabelOrderings);
          

            if (_helper.DeviceHeight < 0 || _helper.DeviceWidth < 0)
            {
                return;
            }

            if (_histogramResult.NullValueCount != 0)
            {
                DrawString(canvasArgs, _textFormat, 25, _helper.DeviceHeight + _helper.TopOffset + 25, "null values: " + _histogramResult.NullValueCount, _textColor, true, false, false);
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
                        var binPrimitiveCollection = _helper.GetBinPrimitives(bin,xLabelOrderings, yLabelOrderings);
                        allBinPrimitiveCollections.Add(binPrimitiveCollection);

                        foreach (var binPrimitive in binPrimitiveCollection.BinPrimitives.Where(bp => bp.Value != 0.0 && bp.BrushIndex != _histogramResult.AllBrushIndex()))
                        {
                            canvasArgs.DrawingSession.FillRoundedRectangle(binPrimitive.Rect, 4, 4, binPrimitive.Color);
                            if (binPrimitive.MarginRect != Rect.Empty)
                            {
                                canvasArgs.DrawingSession.FillRectangle(binPrimitive.MarginRect, dark);
                            }

                            if (binPrimitive.MarginPercentage > 0.009)
                            {
                                var percentageText = binPrimitive.MarginPercentage.ToString("F2") + "%";
                                var layout = new CanvasTextLayout(canvas, percentageText, _textFormat, 1000f, 1000f);
                                var layoutBounds = layout.DrawBounds;
                                layout.Dispose();
                                var scale = (double)Math.Min(1.0, Math.Min((binPrimitive.Rect.Height - 5.0) / layoutBounds.Height, (binPrimitive.Rect.Width - 5.0) / layoutBounds.Width));
                                var transMat = Matrix3x2.CreateTranslation(CommonExtensions.ToVector2(
                                    (double)(binPrimitive.Rect.X + binPrimitive.Rect.Width / 2.0),
                                    (double)(binPrimitive.Rect.Y + binPrimitive.Rect.Height / 2.0)));
                                var scaleMat = Matrix3x2.CreateScale(CommonExtensions.ToVector2(scale, scale));
                                var transInvertMat = Matrix3x2.Identity;
                                Matrix3x2.Invert(transMat, out transInvertMat);

                                var mat = transInvertMat * scaleMat * transMat;
                                var oldMat = canvasArgs.DrawingSession.Transform;

                                canvasArgs.DrawingSession.Transform = mat * oldMat;

                                DrawString(canvasArgs, _textFormat,
                                    (double)(binPrimitive.Rect.X + binPrimitive.Rect.Width / 2.0),
                                    (double)(binPrimitive.Rect.Y + binPrimitive.Rect.Height / 2.0),
                                    binPrimitive.MarginPercentage.ToString("F2") + "%", _textColor, false, true, true);
                                canvasArgs.DrawingSession.Transform = oldMat;
                                //canvasArgs.DrawingSession.dra(binPrimitive.MarginRect, dark);
                            }
                        }

                        if (binPrimitiveCollection.FilterModel != null)
                        {
                            HitTargets.Add(binPrimitiveCollection.HitGeom, binPrimitiveCollection.FilterModel);
                        }
                    }
                }
            }

            // highlight selected bars
            foreach (var binPrimitiveCollection in allBinPrimitiveCollections)
            {
                if (binPrimitiveCollection.FilterModel != null && _filterModels.Contains(binPrimitiveCollection.FilterModel))
                {
                    canvasArgs.DrawingSession.DrawRoundedRectangle(binPrimitiveCollection.BinPrimitives.First(bp => bp.BrushIndex == _histogramResult.AllBrushIndex()).Rect, 4, 4, dark, 2.0f);
                    //canvasArgs.DrawingSession.FillRoundedRectangle(binPrimitiveCollection.BinPrimitives.First(bp => bp.BrushIndex == _histogramResult.AllBrushIndex()).Rect, 4, 4, dark);

                }
            }

            // render distributions if needed
            if (_histogramOperationModel.IncludeDistribution)
            {
                //canvasArgs.DrawingSession.FillRectangle(new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight),
                //Color.FromArgb(150, 230, 230, 230));

                List<List<Vector2>> paths = _helper.GetDistribution();
                foreach (var path in paths)
                {
                    if (path.Count > 1)
                    {
                        var pathBuilder = new CanvasPathBuilder(canvas);
                        pathBuilder.BeginFigure(path[0]);
                        foreach (var point in path.Skip(1))
                        {
                            pathBuilder.AddLine(point);
                        }
                        pathBuilder.EndFigure(CanvasFigureLoop.Open);
                        var strokeStyle = new CanvasStrokeStyle
                        {
                            DashStyle = CanvasDashStyle.Solid,
                            DashCap = CanvasCapStyle.Round,
                            StartCap = CanvasCapStyle.Round,
                            EndCap = CanvasCapStyle.Round,
                            LineJoin = CanvasLineJoin.Bevel,
                        };
                        var geometry = CanvasGeometry.CreatePath(pathBuilder);
                        canvasArgs.DrawingSession.DrawGeometry(geometry, Color.FromArgb(150, 230, 230, 230), 4, strokeStyle);
                        canvasArgs.DrawingSession.DrawGeometry(geometry, dark, 1, strokeStyle);
                    }
                }
            }
        }

        Dictionary<int,int> SortBinsByValue(bool sortAxis)
        {
            var sortedIndexList = new SortedList<double, int>();
            var binIndexDict = new Dictionary<int, int>();
            int sortBinIndex = -1;
            bool reverse = false;
            foreach (var fm in _bczBinMapModels)
                if (sortAxis == fm.SortAxis)
                {
                    reverse = !fm.SortUp;
                    sortBinIndex = _histogramResult.BinRanges[!sortAxis ? 1 : 0].GetIndexFromScaleValue(fm.Value);
                }

            for (int xi = 0; xi < _histogramResult.BinRanges[sortAxis ? 1 :0].GetBins().Count; xi++)
            {
                var binValue = _helper.VisualBinRanges[sortAxis ? 1 : 0].GetLabels()[xi].Value;
                var binIndex = _histogramResult.BinRanges[sortAxis ? 1 : 0].GetIndexFromScaleValue(binValue);
                var newXi = binIndex;
                var newYi = sortBinIndex;
                if (sortAxis)
                {
                    newYi = newXi;
                    newXi = sortBinIndex;
                }
                double sortValue = sortBinIndex == -1 ? xi : _helper.GetBinValue(_histogramResult.Bins[new BinIndex(newXi, newYi)], !sortAxis);

                while (sortedIndexList.ContainsKey(sortValue))
                    sortValue += reverse ? -1e-7 : 1e-7;
                sortedIndexList.Add(sortValue, binIndex);
            }
            for (int xi = 0; xi < _histogramResult.BinRanges[sortAxis ? 1 : 0].GetBins().Count; xi++)
            {
                var binValue = _helper.VisualBinRanges[sortAxis ? 1 : 0].GetLabels()[xi].Value;
                var binIndex = _histogramResult.BinRanges[sortAxis ? 1 : 0].GetIndexFromScaleValue(binValue);
                var list = reverse ? new List<int>(sortedIndexList.Values.Reverse()) : sortedIndexList.Values;
                binIndexDict.Add(binIndex, list.IndexOf(binIndex));
            }
            return binIndexDict;
         //   return reverse ? sortedIndexList.Values.Reverse() : sortedIndexList.Values;
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
        public BczBinMapModel BczBinMapModel { get; set; }
    }

    public class BinPrimitive
    {
        public double Value { get; set; }
        public Rect Rect { get; set; }
        public Rect MarginRect { get; set; }
        public double MarginPercentage { get; set; }
        public Color Color { get; set; }
        public int BrushIndex { get; set; }
    }

    public class PlotRendererContentProviderHelper
    {
        private double _compositionScaleX = 1;
        private double _compositionScaleY = 1;

        private HistogramResult _histogramResult = null;
        private HistogramOperationModel _histogramOperationModelClone = null;
        private HistogramOperationModel _histogramOperationModel = null;
        private AttributeTransformationModel _xIom = null;
        private AttributeTransformationModel _yIom = null;
        private AttributeTransformationModel _valueIom = null;
        private ChartType _chartType = ChartType.HeatMap;
        public ChartType ChartType {  get { return _chartType;  } }
        public List<BinRange> VisualBinRanges { get; set; } = new List<BinRange>();

        public double LeftOffset { get; set; } = 40;
        public double RightOffset { get; set; } = 20;
        public double TopOffset { get; set; } = 20;
        public double BottomtOffset { get; set; } = 45;

        public double DeviceWidth { get; set; } = 0;
        public double DeviceHeight { get; set; } = 0;
        
        private double _xScale = 0;
        private double _yScale = 0;
        private double _minValue = 0;
        private double _maxValue = 0;
        public double DataMinX { get; set; } = 0;
        public double DataMinY { get; set; } = 0;
        public double DataMaxX { get; set; } = 0;
        public double DataMaxY { get; set; } = 0;

        public Rect LabelMetricsX { get; set; } = Rect.Empty;
        public Rect LabelMetricsY { get; set; } = Rect.Empty;

        private static double TOLERANCE = 0.0001f;

        public PlotRendererContentProviderHelper(HistogramResult histogramResult, HistogramOperationModel histogramOperationModel, HistogramOperationModel histogramOperationModelClone, double compositionScaleX, double compositionScaleY)
        {
            _compositionScaleX = compositionScaleX;
            _compositionScaleY = compositionScaleY;
            _histogramResult = histogramResult;
            _histogramOperationModelClone = histogramOperationModelClone;
            _histogramOperationModel = histogramOperationModel;
            _xIom = _histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            _yIom = _histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.Y).FirstOrDefault();

            if (_histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.Value).Any())
            {
                _valueIom = _histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.Value).First();
            }
            else if (_histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
            {
                _valueIom = _histogramOperationModelClone.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).First();
            }

            var aggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            _minValue = double.MaxValue;
            _maxValue = double.MinValue;
            foreach (var brush in _histogramResult.Brushes)
            {
                aggregateKey.BrushIndex = brush.BrushIndex;
                foreach (var bin in _histogramResult.Bins.Values)
                {
                    _minValue = (double)Math.Min(_minValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                    _maxValue = (double)Math.Max(_maxValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                }
            }
            
            initializeChartType(_histogramResult.BinRanges);

            VisualBinRanges.Add(createVisualBinRange(_histogramResult.BinRanges[0], _xIom, histogramOperationModel.IncludeDistribution));
            VisualBinRanges.Add(createVisualBinRange(_histogramResult.BinRanges[1], _yIom, histogramOperationModel.IncludeDistribution));
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

            LeftOffset = (double)Math.Max(10, LabelMetricsY.Width + 10 + 20);

            DeviceWidth = (double)(canvas.ActualWidth / _compositionScaleX - LeftOffset - RightOffset);
            DeviceHeight = (double)(canvas.ActualHeight / _compositionScaleY - TopOffset - BottomtOffset);

            DataMinX = (double)(xLabels.Min(dp => dp.MinValue));
            DataMinY = (double)(yLabels.Min(dp => dp.MinValue));
            DataMaxX = (double)(xLabels.Max(dp => dp.MaxValue));
            DataMaxY = (double)(yLabels.Max(dp => dp.MaxValue));

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

        private BinRange createVisualBinRange(BinRange dataBinRange, AttributeTransformationModel atm, bool includeDistribution)
        {
            BinRange visualBinRange = null;
            if (!(dataBinRange is AggregateBinRange))
            {
                if (dataBinRange is QuantitativeBinRange && includeDistribution)
                {
                    var maxDistX = (double) dataBinRange.DataMaxValue;
                    var minDistX = (double) dataBinRange.DataMinValue;
                    foreach (var brush in _histogramResult.Brushes)
                    {
                        var kdeAggregateKey = new AggregateKey
                        {
                            AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                                a => a is KDEAggregateParameters && (a as KDEAggregateParameters).Dimension == atm.AttributeModel.Index)),
                            BrushIndex = brush.BrushIndex
                        };
                        var countAggregateKey = new AggregateKey
                        {
                            AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                                a => a is CountAggregateParameters && (a as CountAggregateParameters).Dimension == atm.AttributeModel.Index)),
                            BrushIndex = brush.BrushIndex
                        };
                        if (_histogramResult.AggregateResults.ContainsKey(countAggregateKey) &&
                            _histogramResult.AggregateResults.ContainsKey(kdeAggregateKey))
                        {
                            var count = (DoubleValueAggregateResult) _histogramResult.AggregateResults[countAggregateKey];
                            PointsAggregateResult points = (PointsAggregateResult) _histogramResult.AggregateResults[kdeAggregateKey];
                            if (points.Points.Any())
                            {
                                maxDistX = Math.Max(maxDistX, (double) points.Points.Max(p => p.X));
                                minDistX = Math.Min(minDistX, (double) points.Points.Min(p => p.X));
                            }
                        }
                    }

                    visualBinRange = dataBinRange.GetUpdatedBinRange(minDistX, maxDistX, new List<object>());
                }
                else
                {
                    visualBinRange = dataBinRange;
                }
            }
            else
            {
                var aggregateKey = IDEAHelpers.CreateAggregateKey(atm, _histogramResult, _histogramResult.AllBrushIndex());
                double factor = 0.0;
                
                var minValue = double.MaxValue;
                var maxValue = double.MinValue;
                foreach (var brush in _histogramResult.Brushes)
                {
                    aggregateKey.BrushIndex = brush.BrushIndex;
                    foreach (var bin in _histogramResult.Bins.Values)
                    {
                        minValue = (double)Math.Min(minValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
                        maxValue = (double)Math.Max(maxValue, ((DoubleValueAggregateResult)bin.AggregateResults[aggregateKey]).Result);
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

        public List<List<Vector2>> GetDistribution()
        {
            List<List<Vector2>> returnList = new List<List<Vector2>>();
            if (_histogramOperationModel.IncludeDistribution && 
                (_chartType == ChartType.HorizontalBar || _chartType == ChartType.VerticalBar))
            {
                foreach (var brush in _histogramResult.Brushes)
                {
                    var xKdeAggregateKey = new AggregateKey
                    {
                        AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                            a => a is KDEAggregateParameters && (a as KDEAggregateParameters).Dimension == _xIom.AttributeModel.Index)),
                        BrushIndex = brush.BrushIndex
                    };
                    var xCountAggregateKey = new AggregateKey
                    {
                        AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                            a => a is CountAggregateParameters && (a as CountAggregateParameters).Dimension == _xIom.AttributeModel.Index)),
                        BrushIndex = brush.BrushIndex
                    };
                    var yKdeAggregateKey = new AggregateKey
                    {
                        AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                            a => a is KDEAggregateParameters && (a as KDEAggregateParameters).Dimension == _yIom.AttributeModel.Index)),
                        BrushIndex = brush.BrushIndex
                    };
                    var yCountAggregateKey = new AggregateKey
                    {
                        AggregateParameterIndex = _histogramResult.GetAggregateParametersIndex(_histogramResult.AggregateParameters.FirstOrDefault(
                             a => a is CountAggregateParameters && (a as CountAggregateParameters).Dimension == _yIom.AttributeModel.Index)),
                        BrushIndex = brush.BrushIndex
                    };

                    if (_chartType == ChartType.HorizontalBar)
                    {
                        List<Vector2> dist = new List<Vector2>();
                        var count = (DoubleValueAggregateResult)_histogramResult.AggregateResults[yCountAggregateKey];
                        PointsAggregateResult points = (PointsAggregateResult)_histogramResult.AggregateResults[yKdeAggregateKey];
                        dist =
                            points.Points.Select(
                                p =>
                                    CommonExtensions.ToVector2(
                                        DataToScreenX((double)p.Y * (double)(count.Result * VisualBinRanges[1].AddStep(0))),
                                        DataToScreenY((double)p.X))).ToList();
                        returnList.Add(dist);

                    }

                    else if (_chartType == ChartType.VerticalBar)
                    {
                        List<Vector2> dist = new List<Vector2>();
                        var count = (DoubleValueAggregateResult)_histogramResult.AggregateResults[xCountAggregateKey];
                        PointsAggregateResult points = (PointsAggregateResult)_histogramResult.AggregateResults[xKdeAggregateKey];
                        dist =
                            points.Points.Select(
                                p =>
                                    CommonExtensions.ToVector2(
                                        DataToScreenX((double)p.X),
                                        DataToScreenY((double)p.Y * (double) (count.Result * VisualBinRanges[0].AddStep(0))))).ToList();
                        returnList.Add(dist);
                    }
                }
            }
            return returnList;
        }

        public double GetBinValue(Bin bin, bool x)
        {
            foreach (var brush in _histogramResult.Brushes)
            {
                if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    double theValue = 0;
                    if (_chartType == ChartType.HeatMap)
                    {
                        var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
                        valueAggregateKey.BrushIndex = _histogramResult.AllBrushIndex();
                        theValue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;
                    }
                    else if (_chartType == ChartType.HorizontalBar)
                    {
                        var xAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex);
                        theValue = ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result;
                    }
                    else if (_chartType == ChartType.VerticalBar)
                    {
                        var yAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex);
                        theValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
                    }
                    return theValue;
                }
            }
            return 0;
        }

        public BinPrimitiveCollection GetBinPrimitives(Bin bin, Dictionary<int,int> slistXValues, Dictionary<int, int> slistYValues)
        {
            BinPrimitiveCollection binPrimitiveCollection = new BinPrimitiveCollection();
            double alpha = 0.15f;
            var baseColor = Colors.White;

            var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            var marginAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, new MarginAggregateParameters()
                {AggregateFunction = _valueIom.AggregateFunction.ToString()}, _histogramResult, _histogramResult.AllBrushIndex());

            var brushFactorSum = 0.0;


            foreach (var brush in _histogramResult.Brushes)
            {
                Rect marginRect = Rect.Empty;
                double marginPercentage = 0.0;
                double xFrom = 0;
                double xTo = 0;
                double yFrom = 0;
                double yTo = 0;
                double value = 0;
                double xMargin = 0;
                double yMargin = 0;
                double xMarginAbsolute = 0;
                double yMarginAbsolute = 0;
                Color color = Colors.White;

                valueAggregateKey.BrushIndex = brush.BrushIndex;
                MarginAggregateResult valueMargin = (MarginAggregateResult)bin.AggregateResults[marginAggregateKey];

                double unNormalizedvalue = (double)((DoubleValueAggregateResult) bin.AggregateResults[valueAggregateKey]).Result;
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
                    baseColor = _histogramOperationModelClone.BrushColors[brush.BrushIndex % _histogramOperationModelClone.BrushColors.Count];
                }

                var xAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex);
                var yAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex);
                var xMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, new MarginAggregateParameters()
                    {AggregateFunction = _xIom.AggregateFunction.ToString()}, _histogramResult, brush.BrushIndex);
                var yMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, new MarginAggregateParameters()
                    {AggregateFunction = _yIom.AggregateFunction.ToString()}, _histogramResult, brush.BrushIndex);

                // read out value depinding on chart type
                if (_chartType == ChartType.HeatMap)
                {
                    valueAggregateKey.BrushIndex = _histogramResult.AllBrushIndex();
                    var allUnNormalizedValue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;
                    var brushFactor = (unNormalizedvalue / allUnNormalizedValue);
                    var mappedXBinIndex = slistXValues[bin.BinIndex.Indices[0]];
                    var mappedYBinIndex = slistYValues[bin.BinIndex.Indices[1]];

                    var tx = (double)_histogramResult.BinRanges[0].GetValueFromIndex(mappedXBinIndex);
                    xFrom = DataToScreenX(tx);
                    xTo = DataToScreenX((double)_histogramResult.BinRanges[0].AddStep(tx));

                    var ty = (double)_histogramResult.BinRanges[1].GetValueFromIndex(mappedYBinIndex); //_histogramResult.BinRanges[1].GetIndexFromScaleValue(VisualBinRanges[1].GetLabels()[slistYValues.IndexOf(bin.BinIndex.Indices[1])].Value);

                    yFrom = DataToScreenY(ty);
                    yTo   = DataToScreenY((double)_histogramResult.BinRanges[1].AddStep(ty));
                    if (allUnNormalizedValue > 0 && unNormalizedvalue > 0)
                    {
                        brushFactorSum += brushFactor;
                        brushFactorSum = (double) Math.Min(brushFactorSum, 1.0);
                        var tempRect = new Rect(xFrom, yTo, xTo - xFrom, yFrom - yTo);
                        var ratio = (tempRect.Width/tempRect.Height);
                        var newHeight = Math.Sqrt((1.0/ratio)*((tempRect.Width*tempRect.Height)* brushFactorSum));
                        var newWidth = newHeight*ratio;
                        xFrom = (double) (tempRect.X + (tempRect.Width - newWidth)/2.0f);
                        yTo = (double) (tempRect.Y + (tempRect.Height - newHeight)/2.0f);
                        xTo = (double) (xFrom + newWidth);
                        yFrom = (double) (yTo + newHeight);
                        var brushRect = new Rect(tempRect.X + (tempRect.Width - newWidth)/2.0f,
                            tempRect.Y + (tempRect.Height - newHeight)/2.0f, newWidth, newHeight);
                    }
                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = dataColor;
                    marginPercentage = valueMargin.Margin;
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    var xValue = ((DoubleValueAggregateResult) bin.AggregateResults[xAggregateKey]).Result;
                    xFrom = DataToScreenX((double) Math.Min(0, xValue));
                    xTo = DataToScreenX((double) Math.Max(0, xValue));

                    var tt = _histogramResult.BinRanges[1].GetValueFromIndex(slistYValues[bin.BinIndex.Indices[1]]); //  slistYValues.IndexOf(bin.BinIndex.Indices[1]));

                    yFrom = DataToScreenY(tt);
                    yTo = DataToScreenY((double)_histogramResult.BinRanges[1].AddStep(tt));

                    xMargin = (double) ((MarginAggregateResult) bin.AggregateResults[xMarginAggregateKey]).Margin;
                    xMarginAbsolute = (double) ((MarginAggregateResult) bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                        color = baseColor;

                    marginRect = new Rect(DataToScreenX((double)(xValue - xMarginAbsolute)),
                                         yTo + (yFrom - yTo) / 2.0 - 2,
                                         DataToScreenX((double)(xValue + xMarginAbsolute)) - DataToScreenX((double)(xValue - xMarginAbsolute)),
                                         4.0);
                }

                else if (_chartType == ChartType.VerticalBar)
                {
                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
                    //yValue = 1.15;//1.2;
                    yFrom = DataToScreenY((double)Math.Min(0, yValue));
                    yTo = DataToScreenY((double)Math.Max(0, yValue));

                    var tt = _histogramResult.BinRanges[0].GetValueFromIndex(slistXValues[bin.BinIndex.Indices[0]]); //  slistXValues.IndexOf(bin.BinIndex.Indices[0]));

                    xFrom = DataToScreenX((double)tt);
                    xTo = DataToScreenX((double)_histogramResult.BinRanges[0].AddStep(tt));

                    yMargin = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
                    yMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), baseColor, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
                    var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);
                    color = baseColor;

                    marginRect = new Rect(xFrom + (xTo - xFrom) / 2.0 - 2,
                                          DataToScreenY((double)(yValue + yMarginAbsolute)),
                                          4,
                                          DataToScreenY((double)(yValue - yMarginAbsolute)) - DataToScreenY((double)(yValue + yMarginAbsolute)));
                }

                else if (_chartType == ChartType.SinglePoint)
                {
                    var xValue = ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result;
                    xFrom = DataToScreenX((double) xValue) - 5;
                    xTo = DataToScreenX((double) xValue) + 5;

                    var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
                    yFrom = DataToScreenY((double)yValue) + 5;
                    yTo = DataToScreenY((double)yValue);

                    xMargin = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).Margin;
                    xMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;
                    
                    yMargin = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
                    yMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;

                    color = baseColor;
                }


                
                if (brush.BrushIndex == _histogramResult.AllBrushIndex())
                {
                    IGeometry hitGeom = null;
                    //if (_chartType != ChartType.HeatMap)

                    hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
                    FilterModel filterModel = GetBinFilterModel(bin, brush.BrushIndex);
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
                    MarginRect = marginRect,
                    MarginPercentage = marginPercentage,
                    BrushIndex = brush.BrushIndex,
                    Color = color,
                    Value = unNormalizedvalue, 
                };
                binPrimitiveCollection.BinPrimitives.Add(binPrimitive);
            }


            // adjust brush rects (stacking or not)
            BinPrimitive allBrushBinPrimitive = binPrimitiveCollection.BinPrimitives.FirstOrDefault(b => b.BrushIndex == _histogramResult.AllBrushIndex());
            double sum = 0.0f;
            double count = binPrimitiveCollection.BinPrimitives.Count(b => b.BrushIndex != _histogramResult.AllBrushIndex() && b.Value != 0.0);
            foreach (var bp in binPrimitiveCollection.BinPrimitives.Where(b => b.BrushIndex != _histogramResult.AllBrushIndex() && b.Value != 0.0))
            {
                if (_chartType == ChartType.VerticalBar)
                {
                    if (_yIom.AggregateFunction == AggregateFunction.Count)
                    {
                        bp.Rect = new Rect(bp.Rect.X, bp.Rect.Y - sum, bp.Rect.Width, bp.Rect.Height);
                        bp.MarginRect = new Rect(bp.MarginRect.X, bp.MarginRect.Y - sum, bp.MarginRect.Width, bp.MarginRect.Height);
                        sum += bp.Rect.Height;
                    }
                    if (_yIom.AggregateFunction == AggregateFunction.Avg)
                    {
                        var w = bp.Rect.Width / 2.0;
                        bp.Rect = new Rect(bp.Rect.X + sum, bp.Rect.Y, bp.Rect.Width / count, bp.Rect.Height);
                        bp.MarginRect = new Rect(bp.MarginRect.X - w + sum + (bp.Rect.Width / 2.0), bp.MarginRect.Y, bp.MarginRect.Width, bp.MarginRect.Height);
                        sum += bp.Rect.Width;
                    }
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    if (_xIom.AggregateFunction == AggregateFunction.Count)
                    {
                        bp.Rect = new Rect(bp.Rect.X + sum, bp.Rect.Y, bp.Rect.Width, bp.Rect.Height);
                        bp.MarginRect = new Rect(bp.MarginRect.X + sum, bp.MarginRect.Y, bp.MarginRect.Width, bp.MarginRect.Height);
                        sum += bp.Rect.Width;
                    }
                    if (_xIom.AggregateFunction == AggregateFunction.Avg)
                    {
                        var h = bp.Rect.Height / 2.0;
                        bp.Rect = new Rect(bp.Rect.X, bp.Rect.Y + sum, bp.Rect.Width, bp.Rect.Height / count);
                        bp.MarginRect = new Rect(bp.MarginRect.X, bp.MarginRect.Y - h + sum + (bp.Rect.Height / 2.0), bp.MarginRect.Width, bp.MarginRect.Height);
                        sum += bp.Rect.Height;
                    }
                }
                else if (_chartType == ChartType.HeatMap)
                {
                    //var brushFactor = (bp.Value / allBrushBinPrimitive.Value);
                    //bp.Rect = new Rect(bp.Rect.X + sum, bp.Rect.Y, bp.Rect.Width, bp.Rect.Height);
                    //sum += bp.Rect.Width;
                }
            }
            binPrimitiveCollection.BinPrimitives.Reverse();
            return binPrimitiveCollection;
        }

        public FilterModel GetBinFilterModel(Bin bin, int brushIndex)
        {
            AttributeTransformationModel[] dimensions = new AttributeTransformationModel[] { _xIom, _yIom };
            FilterModel filterModel;
            filterModel = new FilterModel();

            var marginAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, new MarginAggregateParameters()
            { AggregateFunction = _valueIom.AggregateFunction.ToString() }, _histogramResult, _histogramResult.AllBrushIndex());
            var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            valueAggregateKey.BrushIndex = brushIndex;
            MarginAggregateResult valueMargin = (MarginAggregateResult)bin.AggregateResults[marginAggregateKey];
            double unNormalizedvalue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;

            filterModel.Value = unNormalizedvalue;
            for (int i = 0; i < _histogramResult.BinRanges.Count; i++)
            {
                if (!(_histogramResult.BinRanges[i] is AggregateBinRange))
                {
                    var dataFrom = _histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]);
                    var dataTo = _histogramResult.BinRanges[i].AddStep(_histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]));

                    if (_histogramResult.BinRanges[i] is NominalBinRange)
                    {
                        var dd = _histogramResult.BinRanges[i].GetIndexFromScaleValue(bin.BinIndex.Indices[i]);
                        var tt = _histogramResult.BinRanges[i].GetLabel(dd);
                        var xx = _histogramResult.BinRanges[i].GetLabel(dataFrom);

                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.EQUALS, tt));
                    }
                    else if (_histogramResult.BinRanges[i] is AlphabeticBinRange)
                    {
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.STARTS_WITH,
                            _histogramResult.BinRanges[i].GetLabel(dataFrom)));
                    }
                    else
                    {
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.GREATER_THAN_EQUAL, dataFrom));
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.LESS_THAN, dataTo));
                    }
                }
            }
            return filterModel;
        }

        public double DataToScreenX(double x)
        {
            return (double)(((x - DataMinX) / _xScale) * (DeviceWidth) + (LeftOffset));
        }
        public double DataToScreenY(double y, bool flip = true)
        {
            double retY = ((y - DataMinY) / _yScale) * (DeviceHeight);
            return (double)(flip ? (DeviceHeight) - retY + (TopOffset) : retY + (TopOffset));
        }
    }

    public enum ChartType
    {
        HorizontalBar, VerticalBar, HeatMap, SinglePoint
    }
}