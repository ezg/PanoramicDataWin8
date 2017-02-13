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

        void removeBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            foreach (var mapItem in binMapModels)
                _bczBinMapModels.Remove(mapItem);
        }
        void addBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            if (_helper.ChartType == ChartType.HeatMap)
            {
                _bczBinMapModels = binMapModels;
                return;
            }
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
        }
        public void UpdateBinSortings(List<BczBinMapModel> bczhits)
        {
            if (bczhits.Any(h => _bczBinMapModels.Contains(h)))
            {
                removeBczBinMapModels(bczhits);
                if (false && !bczhits.First().SortUp) // bcz: tri-state toggle of sort axis (turned off because of 'false')
                {
                    bczhits.First().SortUp = true;
                    addBczBinMapModels(bczhits);
                }
            }
            else
            {
                addBczBinMapModels(bczhits);
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
                drawLabelsAndGridLines(canvasArgs, renderLines, true,  sortedXList);   // x labels and grid lines
                drawLabelsAndGridLines(canvasArgs, renderLines, false, sortedYList);   // y labels and grid lines

                _fillRoundedRectGeom?.Dispose();
                _strokeRoundedRectGeom?.Dispose();
                var x = _helper.DataToScreenX((double)_helper.VisualBinRanges[0].AddStep(0)) - _helper.DataToScreenX(0);
                var y = _helper.DataToScreenY((double)_helper.VisualBinRanges[1].AddStep(0), false) - _helper.DataToScreenY(0, false);
                
                _fillRoundedRectGeom = CanvasCachedGeometry.CreateFill(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4));
                _strokeRoundedRectGeom = CanvasCachedGeometry.CreateStroke(CanvasGeometry.CreateRoundedRectangle(canvas, new Rect(0, 0, x, y), 4, 4), 0.5f);
            }
        }

        public virtual List<BinLabel> GetAxisLabels(BinRange range, bool xaxis)
        {
            var labels = new List<BinLabel>();
            if (_bczBinMapModels.Where((m) => m.SortAxis != xaxis).Count() != 0 && ((xaxis && _helper.ChartType == ChartType.HorizontalBar) ||
                (!xaxis && _helper.ChartType == ChartType.VerticalBar)))
            {
                double steps = 6;
                for (double val = range.MinValue; val <= range.MaxValue; val += (range.MaxValue - range.MinValue) / steps)
                {
                    labels.Add(new BinLabel
                    {
                        Value = val,
                        MinValue = val,
                        MaxValue = Math.Min(range.MaxValue, val + (range.MaxValue - range.MinValue) / steps),
                        Label = ((val - range.MinValue) / (range.MaxValue - range.MinValue)).ToString("F2")
                    });
                }
            } else
                labels = range.GetLabels();

            return labels;
        }
        void drawLabelsAndGridLines(Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool renderLines, bool xaxis, Dictionary<int, int> sortedList)
        {
            double xFrom = 0, xTo = 0, yFrom = 0, yTo = 0;
            var white    = Color.FromArgb(255, 255, 255, 255);
            var binRange = _helper.VisualBinRanges[xaxis ? 0 : 1];
            var Labels = GetAxisLabels(binRange, xaxis);
            
            var dim = xaxis ? _helper.DeviceWidth  / (_helper.LabelMetricsX.Width  + 5) :
                              _helper.DeviceHeight / (_helper.LabelMetricsY.Height + 5);
            int mod = (int)Math.Ceiling(Labels.Count / dim );

            foreach (var label in Labels)
            {
                xFrom = _helper.DataToScreenX(xaxis ? label.MinValue : _helper.DataMinX);
                xTo   = _helper.DataToScreenX(xaxis ? label.MaxValue : _helper.DataMaxX);
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

                createAxisFunctionButton(canvasArgs, xaxis, xFrom, xTo, yFrom, yTo, drawLabel);

                if (renderLines)
                {
                    canvasArgs.DrawingSession.DrawLine(CommonExtensions.ToVector2(xFrom, yFrom),
                                                       CommonExtensions.ToVector2(xaxis ? xFrom : xTo, xaxis ? yTo : yFrom), white, 0.5f);
                    if (label == Labels.Last())
                    {
                        canvasArgs.DrawingSession.DrawLine(CommonExtensions.ToVector2(xaxis ? xTo : xFrom, xaxis ? yFrom : yTo),
                                                           CommonExtensions.ToVector2(xTo, yTo), white, 0.5f);
                    }
                }
                if (Labels.IndexOf(label) % mod == 0 && (
                        (_helper.ChartType == ChartType.HeatMap || 
                        true) || //_bczBinMapModels.Where((m) => m.SortAxis != xaxis).Count() == 0) || 
                        (xaxis && _helper.ChartType == ChartType.VerticalBar) ||
                        (!xaxis && _helper.ChartType == ChartType.HorizontalBar)
                    ))
                {
                    var xStart = xaxis ? xFrom + (xTo - xFrom) / 2.0f : xFrom - 10;
                    var yStart = xaxis ? yFrom + 5 : yFrom + (yTo - yFrom) / 2.0f;
                    var text = drawLabel.Label.ToString();
                    if (binRange is QuantitativeBinRange || binRange is DateTimeBinRange)
                    {
                        xStart = xaxis ? xFrom : xStart;
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

        void createAxisFunctionButton(Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, bool xaxis, double xFrom, double xTo, double yFrom, double yTo, BinLabel drawLabel)
        {
            // adds targets on x/Y axis for sorting or normalizing
            var binPrimitive = new BinPrimitive()
            {
                Rect = new Rect(xaxis ? xFrom : xFrom - 20,
                                xaxis ? yFrom + 5 : yTo,
                                xaxis ? xTo - xFrom : 10,
                                xaxis ? 15 : yFrom - yTo),
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
            canvasArgs.DrawingSession.FillRectangle(binPrimitive.Rect, binPrimitive.Color);
        }

        private void renderPlot(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            if (_helper.ChartType == ChartType.HeatMap && _bczBinMapModels.Count > 0)
            {
                _bczBinMapModels.RemoveRange(1, _bczBinMapModels.Count - 1);
            }
            // bcz index sorting
            BczHitTargets.Clear();
            var xAxisSizes = GetAxisSizes(true);
            var yAxisSizes = GetAxisSizes(false);
            var xAxisRanges = GetAxisRanges(true);
            var yAxisRanges = GetAxisRanges(false);
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
                        var binPrimitiveCollection = _helper.GetBinPrimitives(bin, xLabelOrderings, yLabelOrderings,
                                    xAxisRanges , yAxisRanges,
                                     xAxisSizes , yAxisSizes, _bczBinMapModels);
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

        List<Tuple<double,double>> GetAxisRanges(bool sortAxis)
        {
            var axisSizes = new List<Tuple<double, double>>();
            for (int xi = 0; xi < _histogramResult.BinRanges[sortAxis ? 0 : 1].GetBins().Count; xi++)
            {
                double minaxis = double.MaxValue;
                double maxaxis = double.MinValue;
                for (int yi = 0; yi < _histogramResult.BinRanges[sortAxis ? 1 : 0].GetBins().Count; yi++)
                {
                    var binIndex = sortAxis ? new BinIndex(xi, yi) : new BinIndex(yi, xi);
                    if (_histogramResult.Bins.ContainsKey(binIndex)) {
                        double sortValue = _helper.GetBinValue(_histogramResult.Bins[binIndex]);
                        if (sortValue != 0)
                        {
                            if (sortValue < minaxis)
                                minaxis = sortValue;
                            if (sortValue > maxaxis)
                                maxaxis = sortValue;
                        }
                    }
                }
                axisSizes.Add(new Tuple<double, double>(minaxis, maxaxis));
            }
            return axisSizes;
        }
        List<double> GetAxisSizes(bool sortAxis)
        {
            var axisSizes = new List<double>();
            for (int xi = 0; xi < _histogramResult.BinRanges[sortAxis ? 0 : 1].GetBins().Count; xi++)
            {
                double axisSize = 0;
                for (int yi = 0; yi < _histogramResult.BinRanges[sortAxis ? 1 : 0].GetBins().Count; yi++)
                {
                    var binIndex = sortAxis ? new BinIndex(xi, yi) : new BinIndex(yi, xi);
                    if (_histogramResult.Bins.ContainsKey(binIndex))
                    {
                        double sortValue = _helper.GetBinValue(_histogramResult.Bins[binIndex]);
                        axisSize += sortValue;
                    }
                }
                axisSizes.Add(axisSize);
            }
            return axisSizes;
        }
        Dictionary<int,int> SortBinsByValue(bool sortAxis)
        {
            var sortedIndexList = new SortedList<double, int>();
            var binIndexDict = new Dictionary<int, int>();
            int sortBinIndex = -1;
            bool reverse = false;
            if (_helper.ChartType != ChartType.HeatMap)
                foreach (var fm in _bczBinMapModels)
                    if (sortAxis == fm.SortAxis)
                    {
                        reverse = !fm.SortUp;
                        sortBinIndex = 0;// bcz: used to sort HeatMaps by row/col:  _histogramResult.BinRanges[!sortAxis ? 1 : 0].GetIndexFromScaleValue(fm.Value);
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
                double sortValue = sortBinIndex == -1 ? xi : _helper.GetBinValue(_histogramResult.Bins[new BinIndex(newXi, newYi)]);
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
        private double _maxXValue = 0;
        private double _maxYValue = 0;
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


            _maxYValue = double.MinValue;
            foreach (var Brush in _histogramResult.Brushes)
                foreach (var Bin in _histogramResult.Bins.Values)
                {
                    var maxYAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, _histogramResult, Brush.BrushIndex);
                    if (Bin.AggregateResults.ContainsKey(maxYAggregateKey))
                    {
                        var yval = (double)((DoubleValueAggregateResult)Bin.AggregateResults[maxYAggregateKey]).Result;
                        if (yval > _maxYValue)
                            _maxYValue = yval;
                    }
                }
            _maxXValue = double.MinValue;
            foreach (var Brush in _histogramResult.Brushes)
                foreach (var Bin in _histogramResult.Bins.Values)
                {
                    var maxXAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, _histogramResult, Brush.BrushIndex);
                    if (Bin.AggregateResults.ContainsKey(maxXAggregateKey))
                    {
                        var xval = (double)((DoubleValueAggregateResult)Bin.AggregateResults[maxXAggregateKey]).Result;
                        if (xval > _maxXValue)
                            _maxXValue = xval;
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

        public double GetBinValue(Bin bin)
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

        public BinPrimitiveCollection GetBinPrimitives(Bin bin,
            Dictionary<int, int> slistXValues, //mapping of X axis indices (to implement sorting based on Y axis values)
            Dictionary<int, int> slistYValues, //mapping of Y axis indices (to implement sorting based on X axis values)
            List<Tuple<double, double>> xaxisRanges,   // min/max value of other axis values for normalize axis bin
            List<Tuple<double, double>> yaxisRanges,   // min/max value of other axis values for normalize axis bin
            List<double> xaxisSizes,    // sum of elements along an axis
            List<double> yaxisSizes,    // sum of elements along an axis
            List<BczBinMapModel> binMapModels
            )
        {
            var binPrimitiveCollection = new BinPrimitiveCollection();
            binPrimitiveCollection.FilterModel = GetBinFilterModel(bin, _histogramResult.AllBrushIndex());

            var brushFactorSum = 0.0;

            var normalization   = getBinNormalization(binMapModels);
            var binBrushMaxAxis = getBinBrushAxisRange(bin, normalization);
            double binBrushMinValue, binBrushMaxValue;
            getBinBrushValueRange(bin, normalization, out binBrushMinValue, out binBrushMaxValue);

            var orderedBrushes = new List<Brush>(new Brush[] { _histogramResult.Brushes.First() });
            orderedBrushes.Add(_histogramResult.Brushes[_histogramResult.OverlapBrushIndex()]);
            orderedBrushes.AddRange(_histogramResult.Brushes.Where((b) => b.BrushIndex != 0 && b.BrushIndex != _histogramResult.OverlapBrushIndex()));
            foreach (var brush in orderedBrushes)
            {
                var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, brush.BrushIndex);
                double unNormalizedvalue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;

                var mappedXBinIndex = slistXValues[bin.BinIndex.Indices[0]];
                var mappedYBinIndex = slistYValues[bin.BinIndex.Indices[1]];
                
                // read out value depinding on chart type
                if (_chartType == ChartType.HeatMap)
                {
                    var value = getHeatMapBinValue(bin, xaxisRanges, yaxisRanges, xaxisSizes, yaxisSizes, normalization, binBrushMinValue, binBrushMaxValue, brush, ref unNormalizedvalue);

                    createHeatMapBinPrimitives(bin, binPrimitiveCollection, normalization, brush, unNormalizedvalue, mappedXBinIndex, 
                                               mappedYBinIndex, ref brushFactorSum, value);
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    createHorizontalBarChartBinPrimitives(bin, binPrimitiveCollection, normalization, binBrushMaxAxis, brush, unNormalizedvalue,  mappedYBinIndex);
                }
                else if (_chartType == ChartType.VerticalBar)
                {
                    createVerticalBarChartBinPrimitives(bin, binPrimitiveCollection, normalization, binBrushMaxAxis, brush, unNormalizedvalue,  mappedXBinIndex);
                }
                else if (_chartType == ChartType.SinglePoint)
                {
                    createSinglePointBinPrimitives(bin, binPrimitiveCollection, brush, unNormalizedvalue);
                }
            }
            
            // adjust brush rects (stacking or not)
            var allBrushBinPrimitive = binPrimitiveCollection.BinPrimitives.FirstOrDefault(b => b.BrushIndex == _histogramResult.AllBrushIndex());
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

        void createSinglePointBinPrimitives(Bin bin, BinPrimitiveCollection binPrimitiveCollection, Brush brush, double unNormalizedvalue)
        {
            var xAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex);
            var yAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex);
            var xMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, new MarginAggregateParameters() { AggregateFunction = _xIom.AggregateFunction.ToString() }, _histogramResult, brush.BrushIndex);
            var yMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, new MarginAggregateParameters() { AggregateFunction = _yIom.AggregateFunction.ToString() }, _histogramResult, brush.BrushIndex);
            Rect marginRect = Rect.Empty;
            double marginPercentage = 0.0;
            var xValue = ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result;
            var xFrom = DataToScreenX((double)xValue) - 5;
            var xTo = DataToScreenX((double)xValue) + 5;

            var yValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;
            var yFrom = DataToScreenY((double)yValue) + 5;
            var yTo = DataToScreenY((double)yValue);

            var xMargin = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).Margin;
            var xMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;

            var yMargin = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
            var yMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;
            
            createBinPrimitives(bin, binPrimitiveCollection, brush, marginRect, marginPercentage,
                               xFrom, xTo, yFrom, yTo, baseColorFromBrush(brush), unNormalizedvalue);
        }

        void createVerticalBarChartBinPrimitives(Bin bin, BinPrimitiveCollection binPrimitiveCollection, BczNormalization normalization, 
            double binBrushMaxAxis, Brush brush, double unNormalizedvalue, int mappedXBinIndex)
        {
            var yAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, _histogramResult, brush.BrushIndex);
            var yMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_yIom, new MarginAggregateParameters() { AggregateFunction = _yIom.AggregateFunction.ToString() }, _histogramResult, brush.BrushIndex);
            double marginPercentage = 0.0;
            var dataValue = ((DoubleValueAggregateResult)bin.AggregateResults[yAggregateKey]).Result;

            var yValue = normalization.Axis != BczNormalization.axis.X || binBrushMaxAxis == 0 ? dataValue : (dataValue - 0) / (binBrushMaxAxis - 0) * _yScale;
            var yFrom  = DataToScreenY((double)Math.Min(0, yValue));
            var yTo    = DataToScreenY((double)Math.Max(0, yValue));

            var tt    = _histogramResult.BinRanges[0].GetValueFromIndex(mappedXBinIndex);
            var xFrom = DataToScreenX((double)tt);
            var xTo   = DataToScreenX((double)_histogramResult.BinRanges[0].AddStep(tt));

            var yMargin = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).Margin;
            var yMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[yMarginAggregateKey]).AbsolutMargin;

            var marginRect = new Rect(xFrom + (xTo - xFrom) / 2.0 - 2,
                                  DataToScreenY((double)(yValue + yMarginAbsolute)),
                                  4,
                                  DataToScreenY((double)(yValue - yMarginAbsolute)) - DataToScreenY((double)(yValue + yMarginAbsolute)));
            createBinPrimitives(bin, binPrimitiveCollection, brush, marginRect, marginPercentage,
                               xFrom, xTo, yFrom, yTo, baseColorFromBrush(brush), unNormalizedvalue);
        }

        void createHorizontalBarChartBinPrimitives(Bin bin, BinPrimitiveCollection binPrimitiveCollection, BczNormalization normalization, 
            double binBrushMaxAxis, Brush brush, double unNormalizedvalue, int mappedYBinIndex)
        {
            var xAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, _histogramResult, brush.BrushIndex);
            var xMarginAggregateKey = IDEAHelpers.CreateAggregateKey(_xIom, new MarginAggregateParameters() { AggregateFunction = _xIom.AggregateFunction.ToString() }, _histogramResult, brush.BrushIndex);
            double marginPercentage = 0.0;
            var dataValue = ((DoubleValueAggregateResult)bin.AggregateResults[xAggregateKey]).Result;

            var xValue = normalization.Axis != BczNormalization.axis.Y || binBrushMaxAxis == 0 ? dataValue : (dataValue - 0) / (binBrushMaxAxis - 0) * _xScale;
            var xFrom  = DataToScreenX((double)Math.Min(0, xValue));
            var xTo    = DataToScreenX((double)Math.Max(0, xValue));

            var tt    = _histogramResult.BinRanges[1].GetValueFromIndex(mappedYBinIndex);
            var yFrom = DataToScreenY(tt);
            var yTo   = DataToScreenY((double)_histogramResult.BinRanges[1].AddStep(tt));

            var xMargin = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).Margin;
            var xMarginAbsolute = (double)((MarginAggregateResult)bin.AggregateResults[xMarginAggregateKey]).AbsolutMargin;

            var marginRect = new Rect(DataToScreenX((double)(xValue - xMarginAbsolute)),
                                 yTo + (yFrom - yTo) / 2.0 - 2,
                                 DataToScreenX((double)(xValue + xMarginAbsolute)) - DataToScreenX((double)(xValue - xMarginAbsolute)),
                                 4.0);
            createBinPrimitives(bin, binPrimitiveCollection, brush, marginRect, marginPercentage,
                               xFrom, xTo, yFrom, yTo, baseColorFromBrush(brush), unNormalizedvalue);
        }

        void createHeatMapBinPrimitives(Bin bin, BinPrimitiveCollection binPrimitiveCollection, BczNormalization normalization,
            Brush brush, double unNormalizedvalue, int mappedXBinIndex, int mappedYBinIndex, ref double brushFactorSum, double value)
        {
            Rect marginRect = Rect.Empty;
            double marginPercentage = 0.0;
            double xFrom = 0;
            double xTo = 0;
            double yFrom = 0;
            double yTo = 0;

            var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            var allUnNormalizedValue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;
            var brushFactor = (unNormalizedvalue / allUnNormalizedValue);

            var tx = (double)_histogramResult.BinRanges[0].GetValueFromIndex(mappedXBinIndex);
            xFrom = DataToScreenX(tx);
            xTo = DataToScreenX((double)_histogramResult.BinRanges[0].AddStep(tx));

            var ty = (double)_histogramResult.BinRanges[1].GetValueFromIndex(mappedYBinIndex);
            yFrom = DataToScreenY(ty);
            yTo = DataToScreenY((double)_histogramResult.BinRanges[1].AddStep(ty));

            if (allUnNormalizedValue > 0 && unNormalizedvalue > 0)
            {
                brushFactorSum += brushFactor;
                brushFactorSum = (double)Math.Min(brushFactorSum, 1.0);
                var tempRect = new Rect(xFrom, yTo, xTo - xFrom, yFrom - yTo);
                var ratio = (tempRect.Width / tempRect.Height);
                var newHeight = Math.Sqrt((1.0 / ratio) * ((tempRect.Width * tempRect.Height) * brushFactorSum));
                var newWidth = newHeight * ratio;
                xFrom = (double)(tempRect.X + (tempRect.Width - newWidth) / 2.0f);
                yTo = (double)(tempRect.Y + (tempRect.Height - newHeight) / 2.0f);
                xTo = (double)(xFrom + newWidth);
                yFrom = (double)(yTo + newHeight);
                var brushRect = new Rect(tempRect.X + (tempRect.Width - newWidth) / 2.0f,
                    tempRect.Y + (tempRect.Height - newHeight) / 2.0f, newWidth, newHeight);
            }
            if (double.IsNaN(value))
                value = 1;
            var alpha = 0.15f;
            var color = baseColorFromBrush(brush);
            var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), color, (float)(alpha + Math.Pow(value, 1.0 / 3.0) * (1.0 - alpha)));
            var dataColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);

            var marginAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, new MarginAggregateParameters()
            { AggregateFunction = _valueIom.AggregateFunction.ToString() }, _histogramResult, _histogramResult.AllBrushIndex());
            var valueMargin = (MarginAggregateResult)bin.AggregateResults[marginAggregateKey];
            marginPercentage = valueMargin.Margin;
            createBinPrimitives(bin, binPrimitiveCollection, brush, marginRect, marginPercentage, xFrom, xTo, yFrom, yTo, dataColor, unNormalizedvalue);
        }

        double getHeatMapBinValue(Bin bin, List<Tuple<double, double>> xaxisRanges, List<Tuple<double, double>> yaxisRanges, 
            List<double> xaxisSizes, List<double> yaxisSizes, BczNormalization normalization, 
            double binBrushMinValue, double binBrushMaxValue, Brush brush, ref double unNormalizedvalue)
        {
            double value = 0;
            var localminValue = 0.0;
            var localmaxValue = 0.0;
            if (normalization.Axis != BczNormalization.axis.None)
            {
                var binIndex = normalization.Axis == BczNormalization.axis.X ? bin.BinIndex.Indices.First() : bin.BinIndex.Indices.Last();
                if (normalization.Scope == BczNormalization.Scoping.ZeroToSum)
                {
                    localmaxValue = (normalization.Axis == BczNormalization.axis.X ? xaxisSizes : yaxisSizes)[binIndex];
                }
                else if (normalization.Scope == BczNormalization.Scoping.MinToMax)
                {
                    var range = (normalization.Axis == BczNormalization.axis.X ? xaxisRanges : yaxisRanges)[binIndex];
                    localminValue = range.Item1 == 0 ? localminValue : range.Item1;
                    localmaxValue = range.Item2;
                }

            }
            if (unNormalizedvalue != 0)
            {
                if (binBrushMinValue != binBrushMaxValue && normalization.Axis != BczNormalization.axis.None && brush.BrushIndex != _histogramResult.AllBrushIndex())
                {
                    localminValue = binBrushMinValue;
                    localmaxValue = binBrushMaxValue;
                }
                else if (localminValue == localmaxValue)
                {
                    localminValue = _minValue;
                    localmaxValue = _maxValue;
                    if (normalization.Axis != BczNormalization.axis.None)
                        unNormalizedvalue = _maxValue;// bcz: this makes a lone bin in a normalized column/row be a Max value
                }
                value = (unNormalizedvalue - localminValue) / (Math.Abs((localmaxValue - localminValue)) < TOLERANCE ? unNormalizedvalue : (localmaxValue - localminValue));
            }

            return value;
        }

        void createBinPrimitives(Bin bin, BinPrimitiveCollection binPrimitiveCollection, Brush brush, Rect marginRect, double marginPercentage, double xFrom, double xTo, double yFrom, double yTo, Color color, double unNormalizedvalue)
        {
            if (brush.BrushIndex == _histogramResult.AllBrushIndex())
            {
                IGeometry hitGeom = null;

                hitGeom = new Rct(xFrom, yTo, xTo, yFrom).GetPolygon();
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

        double getBinBrushAxisRange(Bin bin, BczNormalization normalization)
        {
            var binBrushMaxAxis = double.MinValue;
            binBrushMaxAxis = double.MinValue;
            foreach (var Brush in _histogramResult.Brushes)
            {
                var maxAggregateKey = IDEAHelpers.CreateAggregateKey(normalization.Axis == BczNormalization.axis.X ? _yIom : _xIom, _histogramResult, Brush.BrushIndex);
                if (bin.AggregateResults.ContainsKey(maxAggregateKey))
                {
                    var val = (double)((DoubleValueAggregateResult)bin.AggregateResults[maxAggregateKey]).Result;
                    if (val > binBrushMaxAxis)
                        binBrushMaxAxis = val;
                    //if (val < binBrushMinAxis)
                    //    binBrushMinAxis = val;
                }
            }
            return binBrushMaxAxis;
        }

        void getBinBrushValueRange(Bin bin, BczNormalization normalization, out double binBrushMinValue, out double binBrushMaxValue)
        {
            binBrushMinValue = double.MaxValue;
            binBrushMaxValue = 0.0;
            foreach (var Brush in _histogramResult.Brushes)
            {
                var maxValAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, Brush.BrushIndex);
                if (bin.AggregateResults.ContainsKey(maxValAggregateKey))
                {
                    var val = (double)((DoubleValueAggregateResult)bin.AggregateResults[maxValAggregateKey]).Result;
                    if (val > binBrushMaxValue)
                        binBrushMaxValue = val;
                    if (val < binBrushMinValue && val != 0)
                        binBrushMinValue = val;
                }
            }
            if (binBrushMinValue == double.MaxValue)
                binBrushMinValue = binBrushMaxValue;
        }

        BczNormalization getBinNormalization(List<BczBinMapModel> binMapModels)
        {
            var normalization = new BczNormalization();
            foreach (var map in binMapModels)
            {  // search through binMapModels to see if we're normalizing the graph
                if (_chartType == ChartType.HeatMap)
                    normalization.Axis = map.SortAxis ? BczNormalization.axis.X : BczNormalization.axis.Y;
                else if (_chartType == ChartType.VerticalBar)
                {
                    if (map.SortAxis)
                        normalization.Axis = BczNormalization.axis.X;
                }
                else if (_chartType == ChartType.HorizontalBar)
                {
                    if (!map.SortAxis)
                        normalization.Axis = BczNormalization.axis.Y;
                }
                // choose whether to normalize from 0-to-SumOfValues  or Min-to-Max value
                normalization.Scope = map.SortUp ? BczNormalization.Scoping.ZeroToSum : BczNormalization.Scoping.MinToMax;
            }

            return normalization;
        }

        Color baseColorFromBrush(Brush brush)
        {
            Color baseColor;
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

            return baseColor;
        }

        public FilterModel GetBinFilterModel(Bin bin, int brushIndex)
        {
            AttributeTransformationModel[] dimensions = new AttributeTransformationModel[] { _xIom, _yIom };
            FilterModel filterModel = new FilterModel();

            var marginAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, new MarginAggregateParameters()
            { AggregateFunction = _valueIom.AggregateFunction.ToString() }, _histogramResult, _histogramResult.AllBrushIndex());
            var valueAggregateKey = IDEAHelpers.CreateAggregateKey(_valueIom, _histogramResult, _histogramResult.AllBrushIndex());
            valueAggregateKey.BrushIndex = brushIndex;
            double unNormalizedvalue = (double)((DoubleValueAggregateResult)bin.AggregateResults[valueAggregateKey]).Result;

            filterModel.Value = unNormalizedvalue;
            for (int i = 0; i < _histogramResult.BinRanges.Count; i++)
            {
                if (!(_histogramResult.BinRanges[i] is AggregateBinRange))
                {
                    var dataFrom = _histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]);
                    var dataTo   = _histogramResult.BinRanges[i].AddStep(_histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]));

                    if (_histogramResult.BinRanges[i] is NominalBinRange)
                    {
                        var tt = _histogramResult.BinRanges[i].GetLabel(bin.BinIndex.Indices[i]);
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