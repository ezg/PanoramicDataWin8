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
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using WinRTXamlToolkit.Controls.Extensions;

namespace PanoramicDataWin8.view.vis.render
{
    public class ClassifierRendererContentProvider : DXSurfaceContentProvider
    {
        private float _leftOffset = 10;
        private float _rightOffset = 10;
        private float _topOffset = 10;
        private float _bottomtOffset = 20;

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
        private DW.TextFormat _textFormatBig;
        
        private ResultModel _resultModel = null;
        private ClassfierResultDescriptionModel _classfierResultDescriptionModel = null;

        private QueryModel _queryModel = null;
        private BinRange _xBinRange = null;
        private BinRange _yBinRange = null;
        private bool _isXAxisAggregated = false;
        private bool _isYAxisAggregated = false;
        private int _xIndex = -1;
        private int _yIndex = -1;
        private InputOperationModel _xAom = null;
        private InputOperationModel _yAom = null;
        
        private int _labelIndex = 0;

        public int LabelIndex
        {
            get { return _labelIndex; }
            set { _labelIndex = value; }
        }

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        

        public ClassifierRendererContentProvider()
        {
        }
        
        public void UpdateData(ResultModel resultModel, QueryModel queryModel, int labelIndex )
        {
            _resultModel = resultModel;
            _queryModel = queryModel;
            _labelIndex = labelIndex;

            _classfierResultDescriptionModel = _resultModel.ResultDescriptionModel as ClassfierResultDescriptionModel;
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
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 0f, 0f, 1f));
            if (_resultModel != null && _classfierResultDescriptionModel != null)
            {
                computeSizes(d2dDeviceContext, dwFactory);

                if (_deviceHeight < 0 || _deviceWidth < 0)
                {
                    return;
                }
                var centerX =  _deviceWidth/2.0f + _leftOffset;
                var centerY =  _deviceHeight/2.0f + _topOffset;

                string label = _labelIndex != -1 ? _classfierResultDescriptionModel.Labels[_labelIndex].Name : "avg across labels";
                drawString(d2dDeviceContext, dwFactory, _textFormatBig, centerX, _topOffset, label, false, true, false);

                var w = (_deviceWidth - 20)/3.0f;
                var h = Math.Max(0, _deviceHeight/2.0f - 50);
                var yOff = 40f;

                if ((_deviceHeight <= 200 || _deviceWidth <= 180) || _labelIndex == -1)
                {
                    //yOff = centerY - h /2.0f;
                    h = Math.Max(0, _deviceHeight - 50);
                }

                renderGauge(d2dDeviceContext, dwFactory,
                    _leftOffset,
                    yOff,
                    w,
                    h,
                    _labelIndex != -1 ? (float)_classfierResultDescriptionModel.Precisions[_classfierResultDescriptionModel.Labels[_labelIndex]] :(float) _classfierResultDescriptionModel.AvgPrecision, 
                    "precision");

                renderGauge(d2dDeviceContext, dwFactory,
                   _leftOffset + w + 10,
                   yOff,
                   w,
                   h,
                   _labelIndex != -1 ? (float)_classfierResultDescriptionModel.Recalls[_classfierResultDescriptionModel.Labels[_labelIndex]] :(float) _classfierResultDescriptionModel.AvRecall, 
                   "recall");

                renderGauge(d2dDeviceContext, dwFactory,
                   _leftOffset + (w + 10)  * 2,
                   yOff,
                   w,
                   h,
                   _labelIndex != -1 ? (float)_classfierResultDescriptionModel.F1s[_classfierResultDescriptionModel.Labels[_labelIndex]] : (float)_classfierResultDescriptionModel.AvgF1, 
                   "f1");

                if (_deviceHeight > 200 && _deviceWidth > 180 && _labelIndex != -1)
                {
                    renderConfusionMatrix(d2dDeviceContext, dwFactory,
                        _leftOffset, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                    renderRoc(d2dDeviceContext, dwFactory,
                        centerX + 10, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                }
            };
        }

        private void renderGauge(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory,
            float xStart, float yStart, float width, float height, float value, string name)
        {
            value = (float) Math.Min(0.9999, Math.Max(0.0, value));
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var blue = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(41f / 255f, 170f / 255f, 213f / 255f, 255f / 255f));

            var roundedRect = new D2D.RoundedRectangle();

            var rect = new RectangleF(xStart,
                yStart,
                width,
                height);
            //d2dDeviceContext.DrawRectangle(rect, white);
                  height -= 25;
            var radius = Math.Min(width / 2.0f, height / 2.0f) - 4;
            drawString(d2dDeviceContext, dwFactory, _textFormat, width / 2.0f + xStart, (height / 2.0f) + radius+ yStart + 10, name, false, true, false);
            
            if (width > 0 && height > 0 && radius > 15)
            {
                D2D.Ellipse ell = new D2D.Ellipse(new Vector2(
                    (width / 2.0f) + xStart,
                    (height / 2.0f) + yStart), radius, radius);
                d2dDeviceContext.DrawEllipse(ell, white, 4);

                var thickness = 4.0f;

                float angle = 2.0f * (float)Math.PI * value - (float) Math.PI / 2.0f;

                D2D.PathGeometry geom = new D2D.PathGeometry(d2dDeviceContext.Factory);
                using (var sink = geom.Open())
                {
                    sink.BeginFigure(new Vector2((width/2.0f) + xStart, (height/2.0f) + yStart - (radius)), D2D.FigureBegin.Hollow);
                    var arc = new D2D.ArcSegment();
                    arc.ArcSize = value > 0.5 ? D2D.ArcSize.Large : D2D.ArcSize.Small;
                    float x = (width/2.0f) + xStart;
                    float y = (height/2.0f) + yStart;
                    arc.Point = new Vector2(
                        (float) Math.Cos(angle)*(radius) + x,
                        (float) Math.Sin(angle)*(radius) + y);
                    arc.SweepDirection = D2D.SweepDirection.Clockwise;
                    arc.Size = new Size2F((radius), (radius));
                    sink.AddArc(arc);
                    sink.EndFigure(D2D.FigureEnd.Open);
                    sink.Close();
                }
                d2dDeviceContext.DrawGeometry(geom, blue, 4);
            }
            drawString(d2dDeviceContext, dwFactory, _textFormat,
                 (width / 2.0f) + xStart,
                 (height / 2.0f) + yStart,
                 (value * 100.0).ToString("F1") + "%", false, true, true);
            blue.Dispose();
            white.Dispose();
        }


        private void renderConfusionMatrix(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory,
            float xStart, float yStart, float width, float height)
        {
           // width -= 20;
            height -= 20;
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var roundedRect = new D2D.RoundedRectangle();
            
            var rect = new RectangleF(xStart,
                       yStart,
                       width,
                       height);
            //d2dDeviceContext.DrawRectangle(rect, white);

            drawString(d2dDeviceContext, dwFactory, _textFormatBig, xStart + width / 2.0f + 20, yStart, "confusion matrix", false, true, false);

            yStart += 20;
            drawString(d2dDeviceContext, dwFactory, _textFormat, xStart + width / 2.0f + 20, yStart, "predicted", false, true, false);

            var oldTransform = d2dDeviceContext.Transform;
            var mat = Matrix3x2.Rotation((-90f * (float)Math.PI) / 180.0f, new Vector2(xStart, yStart + height / 2.0f + 10f));
            d2dDeviceContext.Transform = mat * oldTransform;
            drawString(d2dDeviceContext, dwFactory, _textFormat, xStart, yStart + height / 2.0f + 10, "actual", false, true, false);
            d2dDeviceContext.Transform = oldTransform;

            var xOff = xStart + 40;
            var yOff = yStart + 40;
            var h = (height - 60) / 2.0f;
            var w = (width - 40) / 2.0f;

            for (int r = 0; r < _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]].Count; r++)
            {
                drawString(d2dDeviceContext, dwFactory, _textFormat, xOff + w / 2.0f + (r * w), yOff - 20, (1 - r) + "", false, true, false);
                drawString(d2dDeviceContext, dwFactory, _textFormat, xOff - 10, yOff + h / 2.0f + (r * h), (1 - r) + "", false, true, true);

                var row = _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]][r];
                var total = _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]].SelectMany(t => t).Sum();
                var valueSum = (float) row.Sum();
                for (int c = 0; c < row.Count; c++)
                {
                    var value = total == 0.0 ? 0.0 : (float)row[c] / total;
                    var yFrom = ((float)r * h + yOff);
                    var xFrom = (float)c * w + xOff;
                    
                    roundedRect.Rect = new RectangleF(
                        xFrom,
                        yFrom,
                        w,
                        h);
                    roundedRect.RadiusX = roundedRect.RadiusY = 4;

                    if (value > 0)
                    {
                        var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) (value));
                        var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R/255f, lerpColor.G/255f, lerpColor.B/255f, 1f));

                        d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                        binColor.Dispose();
                    }
                    drawString(d2dDeviceContext, dwFactory, _textFormat, xFrom + w / 2.0f, yFrom + h / 2.0f, row[c].ToString("N0"), false, true, true);

                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                }
            }
            white.Dispose();
        }

        private void renderRoc(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory,
            float xStart, float yStart, float width, float height)
        {
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var blue = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(41f / 255f, 170f / 255f, 213f / 255f, 255f / 255f));

            drawString(d2dDeviceContext, dwFactory, _textFormatBig, xStart + width / 2.0f + 5, yStart, "roc curve", false, true, false);

            yStart += 60;

            float xOff = xStart + 20f;
            float yOff = yStart;
            float h = (height - 80f);
            float w = (width - 20f);
            drawString(d2dDeviceContext, dwFactory, _textFormat, xOff + w / 2.0f, yStart + h + 5, "fpr", false, true, false);

            var oldTransform = d2dDeviceContext.Transform;
            var mat = Matrix3x2.Rotation((-90f * (float)Math.PI) / 180.0f, new Vector2(xStart + 5, yStart + h / 2.0f));
            d2dDeviceContext.Transform = mat * oldTransform;
            drawString(d2dDeviceContext, dwFactory, _textFormat, xStart + 5, yStart + h / 2.0f, "tpr", false, true, false);
            d2dDeviceContext.Transform = oldTransform;


            var rect = new RectangleF(xOff,
                        yOff,
                        w,
                        h);
            d2dDeviceContext.DrawRectangle(rect, white);
            d2dDeviceContext.DrawLine(new Vector2(xOff, yOff + h), new Vector2(xOff + w, yOff), white, 0.5f);

            Pt last = new Pt(0,0);
            foreach (var pt in _classfierResultDescriptionModel.RocCurves[_classfierResultDescriptionModel.Labels[_labelIndex]])
            {
                d2dDeviceContext.DrawLine(
                    new Vector2(
                        (float) (last.X*w + xOff),
                        (float) ((1.0 - last.Y) * h + yOff)), 
                    new Vector2(
                        (float)pt.X * w + xOff,
                        (float)(1.0 - pt.Y) * h + yOff), blue, 1f);
                last = pt;
            }

            var auc = _classfierResultDescriptionModel.AUCs[_classfierResultDescriptionModel.Labels[_labelIndex]];
            drawString(d2dDeviceContext, dwFactory, _textFormat, xOff + w - 4, yOff + h - 15, "auc: " + auc.ToString("F2"), false, false, false);
            
            blue.Dispose();
            white.Dispose();
        }

        private void drawString(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory, DW.TextFormat textFormat, float x, float y, string text,
            bool leftAligned,
            bool horizontallyCentered, bool verticallyCentered)
        {
            var layout = new DW.TextLayout(dwFactory, text, textFormat, 1000f, 1000f);
            var metrics = layout.Metrics;

            if (horizontallyCentered)
            {
                x += metrics.Width / 2.0f;
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

        private void computeSizes(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            _deviceWidth = (float)(d2dDeviceContext.Size.Width / CompositionScaleX - _leftOffset - _rightOffset);
            _deviceHeight = (float)(d2dDeviceContext.Size.Height / CompositionScaleY - _topOffset - _bottomtOffset);

            _minX = (float) 0;
            _minY = (float) 0;
            _maxX = (float) 100;
            _maxY = (float) 100;

            _xScale = _maxX - _minX;
            _yScale = _maxY - _minY;
        }

        public override void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {
            // reusable structure representing a text font with size and style
            _textFormat = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 11f));
            _textFormatBig = disposeCollector.Collect(new DW.TextFormat(dwFactory, "Abel", SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, 16f));

            // reusable brush structure
            _textBrush = disposeCollector.Collect(new D2D.SolidColorBrush(d2dDeviceContext, new Color(17, 17, 17)));

            // prebaked text - useful for constant labels as it greatly improves performance
            //_textLayout = disposeCollector.Collect(new DW.TextLayout(dwFactory, "Demo DirectWrite text here.", _textFormat, 100f, 100f));
        }
    }
}
