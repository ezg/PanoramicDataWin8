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
        private float _bottomtOffset = 10;

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

                drawString(d2dDeviceContext, dwFactory, _textFormatBig, toScreenX(50), toScreenY(90), _classfierResultDescriptionModel.Labels[_labelIndex].Name, false, true, true);
                
                renderConfusionMatrix(d2dDeviceContext, dwFactory);
                renderRoc(d2dDeviceContext, dwFactory);
            };
        }

        private void renderConfusionMatrix(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }
            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var roundedRect = new D2D.RoundedRectangle();
            var h = 20f;
            var w = 20f;
            var xOff = 5;
            var yOff = 25;

            for (int r = 0; r < _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]].Count; r++)
            {
                drawString(d2dDeviceContext, dwFactory, _textFormat, toScreenX((float)r * w + xOff + w / 2.0f), toScreenY(yOff + h * 2) - 15, r+"", false, true, false);
                drawString(d2dDeviceContext, dwFactory, _textFormat, toScreenX(xOff - 1), toScreenY(yOff + h / 2.0f + (1-r) * h), r + "", false, false, true);

                var row = _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]][r];
                var valueSum = (float) row.Sum();
                for (int c = 0; c < row.Count; c++)
                {
                    var value = (float) row[c] / valueSum;
                    var yFrom = toScreenY((float)(1 - r) * w + yOff);
                    var xFrom = toScreenX((float)c * h + xOff);
                    var yTo = toScreenY((float)(1 - r) * w + w + yOff);
                    var xTo = toScreenX((float)c * h + h + xOff);

                    var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float)(value));
                    var binColor = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(lerpColor.R / 255f, lerpColor.G / 255f, lerpColor.B / 255f, 1f));

                    roundedRect.Rect = new RectangleF(
                        xFrom,
                        yTo,
                        xTo - xFrom,
                        yFrom - yTo);
                    roundedRect.RadiusX = roundedRect.RadiusY = 4;
                    d2dDeviceContext.FillRoundedRectangle(roundedRect, binColor);
                    /*if (r == 1 && c == 1)
                    {
                        d2dDeviceContext.FillRoundedRectangle(roundedRect, white);
                    }*/
                    
                    //d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);

                    d2dDeviceContext.DrawRoundedRectangle(roundedRect, white, 0.5f);
                    binColor.Dispose();
                }
            }
            white.Dispose();
        }

        private void renderRoc(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {
            if (_deviceHeight < 0 || _deviceWidth < 0)
            {
                return;
            }

            var white = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(1f, 1f, 1f, 1f));
            var blue = new D2D.SolidColorBrush(d2dDeviceContext, new Color4(41f / 255f, 170f / 255f, 213f / 255f, 255f / 255f));
            var h = 40f;
            var w = 40f;
            var xOff = 55;
            var yOff = 25;
            var xFrom = toScreenX((float)xOff);
            var yFrom = toScreenY((float)yOff);
            var xTo = toScreenX((float)w + xOff);
            var yTo = toScreenY((float)h + yOff);
            var rect = new RectangleF(xFrom,
                        yTo,
                        xTo - xFrom,
                        yFrom - yTo);
            d2dDeviceContext.DrawRectangle(rect, white);
            d2dDeviceContext.DrawLine(new Vector2(xTo, yTo), new Vector2(xFrom, yFrom), white, 0.5f);

            Pt last = new Pt(0,0);
            foreach (var pt in _classfierResultDescriptionModel.RocCurves[_classfierResultDescriptionModel.Labels[_labelIndex]])
            {
                d2dDeviceContext.DrawLine(
                    new Vector2(
                        toScreenX((float)last.X * w + xOff), 
                        toScreenY((float)last.Y * h + yOff)), 
                    new Vector2(
                        toScreenX((float)pt.X * w + xOff),
                        toScreenY((float)pt.Y * h + yOff)), blue, 1f);
                last = pt;
            }

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
