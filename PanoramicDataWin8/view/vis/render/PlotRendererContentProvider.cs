using PanoramicData.controller.view;
using PanoramicData.model.data;
using PanoramicData.model.view;
using PanoramicData.utils;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.controller.data.sim.binrange;
using PanoramicDataWin8.view.common;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

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

        private QueryResultModel _queryResultModel = null;
        private QueryModel _queryModel = null;
        private BinRange _xBinRange = null;
        private BinRange _yBinRange = null;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }

        public void UpdateData(QueryResultModel queryResultModel, QueryModel queryModel, AttributeOperationModel xAom, AttributeOperationModel yAom)
        {
            _queryResultModel = queryResultModel;
            _queryModel = queryModel;
            if (!(_queryResultModel.XBinRange is AggregateBinRange))
            {
                _xBinRange = _queryResultModel.XBinRange;
            }
            else
            {
                _xBinRange = QuantitativeBinRange.Initialize(_queryResultModel.MinValues[xAom], _queryResultModel.MaxValues[xAom], 10);
            }

            if (!(_queryResultModel.YBinRange is AggregateBinRange))
            {
                _yBinRange = _queryResultModel.YBinRange;
            }
            else
            {
                _yBinRange = QuantitativeBinRange.Initialize(_queryResultModel.MinValues[yAom], _queryResultModel.MaxValues[yAom], 10);
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

            if (_queryResultModel != null && _queryResultModel.QueryResultItemModels.Count > 0)
            {
                if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Grid)
                {
                    renderGrid(d2dDeviceContext, dwFactory);
                }
                else if (MainViewController.Instance.MainModel.GraphRenderOption == GraphRenderOptions.Cell)
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
            var maxXLabel = xLabels.Where(b => b.Label.Length == maxXLabelLength).First();
            var maxYLabelLength = yLabels.Max(b => b.Label.Length);
            var maxYLabel = yLabels.Where(b => b.Label.Length == maxYLabelLength).First();

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

            // x labels and grid lines
            int mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceWidth / (metricsX.Width + 5))) / xLabels.Count));
            int count = 0;
            foreach (var label in xLabels)
            {
                float yFrom = toScreenY(_minY);
                float yTo = toScreenY(_maxY);
                float xFrom = toScreenX((float)label.MinValue);
                float xTo = toScreenX((float)label.MaxValue);
                bool lastLabel = count + 1 == xLabels.Count;

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
                    if (_queryResultModel.XAxisType == AxisType.Quantitative)
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
            mod = (int)Math.Ceiling(1.0 / (Math.Floor((_deviceHeight / (metricsY.Height + 5))) / yLabels.Count));
            count = 0;
            foreach (var label in yLabels)
            {
                float xFrom = toScreenX(_minX);
                float xTo = toScreenX(_maxX);
                float yFrom = toScreenY((float)label.MinValue);
                float yTo = toScreenY((float)label.MaxValue);
                bool lastLabel = count + 1 == yLabels.Count;

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
                    if (_queryResultModel.YAxisType == AxisType.Quantitative)
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
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

            var xBins = _xBinRange.GetBins();
            xBins.Add(_xBinRange.AddStep(xBins.Max()));
            var yBins = _yBinRange.GetBins();
            yBins.Add(_yBinRange.AddStep(yBins.Max()));

            // draw data
            foreach (var resultItem in _queryResultModel.QueryResultItemModels)
            {
                double? xValue = (double?)resultItem.AttributeValues[_queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).First()].Value;
                double? yValue = (double?)resultItem.AttributeValues[_queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).First()].Value;
                double? value = (double?)resultItem.AttributeValues[_queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).First()].Value;

                var roundedRect = new D2D.RoundedRectangle();
                float xFrom = toScreenX((float)xBins[_xBinRange.GetIndex(xValue.Value)]);
                float yFrom = toScreenY((float)yBins[_yBinRange.GetIndex(yValue.Value)]);
                float xTo = toScreenX((float)xBins[_xBinRange.GetIndex(_xBinRange.AddStep(xValue.Value))]);
                float yTo = toScreenY((float)yBins[_yBinRange.GetIndex(_yBinRange.AddStep(yValue.Value))]);
                float w = (float)Math.Max((xTo - xFrom) * (float)value.Value, 5.0);
                float h = (float)Math.Max((yFrom - yTo) * (float)value.Value, 5.0);

                float alpha = 0.1f * (float)Math.Log10(value.Value) + 1f;
                var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 230, 230, 230), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)Math.Sqrt(value.Value));
                var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R / 255f, lerpColor.G / 255f, lerpColor.B / 255f, 1f));

                roundedRect.Rect = new RectangleF(
                    xFrom,
                    yTo,
                    xTo - xFrom,
                    yFrom - yTo);
                roundedRect.RadiusX = roundedRect.RadiusY = 4;
                d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                if (_queryResultModel.QueryResultItemModels.Count < 10000)
                {
                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                }


                binColor.Dispose();
            }
            white.Dispose();
        }

        private void renderGrid(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            computeSizesAndRenderLabels(d2dDeviceContext, dwFactory, _queryResultModel.QueryResultItemModels.Count < 10000);
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color(40, 170, 213));
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));

            // draw data
            /*foreach (var bin in _queryResultModel.QueryResultItemModels.Select(item => item.Bin).Where(bin => bin.Values.Any() && bin.Values.First().HasValue))
            {
                var roundedRect = new D2D.RoundedRectangle();
                float xFrom = toScreenX((float)bin.MinX);
                float yFrom = toScreenY((float)bin.MinY);
                float xTo = toScreenX((float)bin.MaxX);
                float yTo = toScreenY((float)bin.MaxY);
                float w = (float)Math.Max((xTo - xFrom) * (float)bin.Values.First().Value, 5.0);
                float h = (float)Math.Max((yFrom - yTo) * (float)bin.Values.First().Value, 5.0);

                roundedRect.Rect = new RectangleF(
                    xFrom + ((xTo - xFrom) - w) / 2.0f,
                    yTo + ((yFrom - yTo) - h) / 2.0f,
                    w,
                    h);
                roundedRect.RadiusX = roundedRect.RadiusY = 4;

                if (bin.Values.First().Value > 0)
                {
                    d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 1f);

                }
            }*/
            binColor.Dispose();
            white.Dispose();
        }

        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            // reusable structure representing a text font with size and style
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));

            // reusable brush structure
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));

            // prebaked text - useful for constant labels as it greatly improves performance
            //_textLayout = disposeCollector.Collect(new DW.TextLayout(dwFactory, "Demo DirectWrite text here.", _textFormat, 100f, 100f));
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
}
