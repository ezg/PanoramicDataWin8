using NetTopologySuite.Geometries;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

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

        private Color _textColor;
        private CanvasTextFormat _textFormat;
        private CanvasTextFormat _textFormatBig;
        
        private ResultModel _resultModel = null;
        private ClassfierResultDescriptionModel _classfierResultDescriptionModel = null;

        private QueryModel _queryModel = null;
        
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

        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;
            if (_resultModel != null && _classfierResultDescriptionModel != null)
            {
                computeSizes(canvas, canvasArgs);

                if (_deviceHeight < 0 || _deviceWidth < 0)
                {
                    return;
                }
                var centerX =  _deviceWidth/2.0f + _leftOffset;
                var centerY =  _deviceHeight/2.0f + _topOffset;

                string label = _labelIndex != -1 ? _classfierResultDescriptionModel.Labels[_labelIndex].Name : "avg across labels";
                DrawString(canvasArgs, _textFormatBig, centerX, _topOffset, label, _textColor, false, true, false);

                var w = (_deviceWidth - 20)/3.0f;
                var h = Math.Max(0, _deviceHeight/2.0f - 50);
                var yOff = 40f;

                if ((_deviceHeight <= 200 || _deviceWidth <= 180) || _labelIndex == -1)
                {
                    //yOff = centerY - h /2.0f;
                    h = Math.Max(0, _deviceHeight - 50);
                }

                renderGauge(canvas, canvasArgs,
                    _leftOffset,
                    yOff,
                    w,
                    h,
                    _labelIndex != -1 ? (float)_classfierResultDescriptionModel.Precisions[_classfierResultDescriptionModel.Labels[_labelIndex]] :(float) _classfierResultDescriptionModel.AvgPrecision, 
                    "precision");

                renderGauge(canvas, canvasArgs,
                   _leftOffset + w + 10,
                   yOff,
                   w,
                   h,
                   _labelIndex != -1 ? (float)_classfierResultDescriptionModel.Recalls[_classfierResultDescriptionModel.Labels[_labelIndex]] :(float) _classfierResultDescriptionModel.AvRecall, 
                   "recall");

                renderGauge(canvas, canvasArgs,
                   _leftOffset + (w + 10)  * 2,
                   yOff,
                   w,
                   h,
                   _labelIndex != -1 ? (float)_classfierResultDescriptionModel.F1s[_classfierResultDescriptionModel.Labels[_labelIndex]] : (float)_classfierResultDescriptionModel.AvgF1, 
                   "f1");

                if (_deviceHeight > 200 && _deviceWidth > 180 && _labelIndex != -1)
                {
                    renderConfusionMatrix(canvas, canvasArgs,
                        _leftOffset, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                    renderRoc(canvas, canvasArgs,
                        centerX + 10, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                }
            };
        }

        private void renderGauge(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs,
            float xStart, float yStart, float width, float height, float value, string name)
        {
            value = (float) Math.Min(0.9999, Math.Max(0.0, value));
            var white = Color.FromArgb(255, 255, 255, 255);
            var blue = Color.FromArgb(255, 41, 170, 213); 
            
            var rect = new Rect(xStart,
                yStart,
                width,
                height);
            //d2dDeviceContext.DrawRectangle(rect, white);
                  height -= 25;
            var radius = Math.Min(width / 2.0f, height / 2.0f) - 4;
            DrawString(canvasArgs, _textFormat, width / 2.0f + xStart, (height / 2.0f) + radius+ yStart + 10, name, _textColor, false, true, false);
            
            if (width > 0 && height > 0 && radius > 15)
            {
                canvasArgs.DrawingSession.DrawEllipse(new Vector2(
                    (width / 2.0f) + xStart,
                    (height / 2.0f) + yStart), radius, radius, white, 4);

                var thickness = 4.0f;

                float angle = 2.0f * (float)Math.PI * value - (float) Math.PI / 2.0f;
                float x = (width / 2.0f) + xStart;
                float y = (height / 2.0f) + yStart;

                CanvasPathBuilder pathBuilder = new CanvasPathBuilder(canvas);
                pathBuilder.BeginFigure(new Vector2((width / 2.0f) + xStart, (height / 2.0f) + yStart - (radius)), CanvasFigureFill.DoesNotAffectFills);
                pathBuilder.AddArc(new Vector2(
                    (float) Math.Cos(angle)*(radius) + x,
                    (float) Math.Sin(angle)*(radius) + y), radius, radius, 0, CanvasSweepDirection.Clockwise, value > 0.5 ? CanvasArcSize.Large : CanvasArcSize.Small);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);

                CanvasGeometry pathGeometry = CanvasGeometry.CreatePath(pathBuilder);
                canvasArgs.DrawingSession.DrawGeometry(pathGeometry, blue, 4);

                pathBuilder.Dispose();
            }
            DrawString(canvasArgs, _textFormat,
                 (width / 2.0f) + xStart,
                 (height / 2.0f) + yStart,
                 (value * 100.0).ToString("F1") + "%", _textColor, false, true, true);
        }


        private void renderConfusionMatrix(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs,
            float xStart, float yStart, float width, float height)
        {
           // width -= 20;
            height -= 20;
            var white = Color.FromArgb(255, 255, 255, 255);
            
            var rect = new Rect(xStart,
                       yStart,
                       width,
                       height);
            //d2dDeviceContext.DrawRectangle(rect, white);

            DrawString(canvasArgs, _textFormatBig, xStart + width / 2.0f + 20, yStart, "confusion matrix", _textColor, false, true, false);

            yStart += 20;
            DrawString(canvasArgs, _textFormat, xStart + width / 2.0f + 20, yStart, "predicted", _textColor, false, true, false);

            var oldTransform = canvasArgs.DrawingSession.Transform;
            var mat = Matrix3x2.CreateRotation((-90f * (float)Math.PI) / 180.0f, new Vector2(xStart, yStart + height / 2.0f + 10f));
            canvasArgs.DrawingSession.Transform = mat * oldTransform;
            DrawString(canvasArgs, _textFormat, xStart, yStart + height / 2.0f + 10, "actual", _textColor, false, true, false);
            canvasArgs.DrawingSession.Transform = oldTransform;

            var xOff = xStart + 40;
            var yOff = yStart + 40;
            var h = (height - 60) / 2.0f;
            var w = (width - 40) / 2.0f;

            for (int r = 0; r < _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]].Count; r++)
            {
                DrawString(canvasArgs, _textFormat, xOff + w / 2.0f + (r * w), yOff - 20, (1 - r) + "", _textColor, false, true, false);
                DrawString(canvasArgs, _textFormat, xOff - 10, yOff + h / 2.0f + (r * h), (1 - r) + "", _textColor, false, true, true);

                var row = _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]][r];
                var total = _classfierResultDescriptionModel.ConfusionMatrices[_classfierResultDescriptionModel.Labels[_labelIndex]].SelectMany(t => t).Sum();
                var valueSum = (float) row.Sum();
                for (int c = 0; c < row.Count; c++)
                {
                    var value = total == 0.0 ? 0.0 : (float)row[c] / total;
                    var yFrom = ((float)r * h + yOff);
                    var xFrom = (float)c * w + xOff;
                    
                    var roundedRect = new Rect(
                        xFrom,
                        yFrom,
                        w,
                        h);

                    if (value > 0)
                    {
                        var lerpColor = LABColor.Lerp(Windows.UI.Color.FromArgb(255, 222, 227, 229), Windows.UI.Color.FromArgb(255, 40, 170, 213), (float) (value));
                        var binColor = Color.FromArgb(255, lerpColor.R, lerpColor.G, lerpColor.B);

                        canvasArgs.DrawingSession.FillRoundedRectangle(roundedRect, 4, 4, binColor);
                    }
                    DrawString(canvasArgs, _textFormat, xFrom + w / 2.0f, yFrom + h / 2.0f, row[c].ToString("N0"), _textColor, false, true, true);

                    canvasArgs.DrawingSession.DrawRoundedRectangle(roundedRect, 4, 4, white, 0.5f);
                }
            }
        }

        private void renderRoc(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs,
            float xStart, float yStart, float width, float height)
        {
            var white = Color.FromArgb(255, 255, 255, 255);
            var blue = Color.FromArgb(255, 41, 170, 213);

            DrawString(canvasArgs, _textFormatBig, xStart + width / 2.0f + 5, yStart, "roc curve", _textColor, false, true, false);

            yStart += 60;

            float xOff = xStart + 20f;
            float yOff = yStart;
            float h = (height - 80f);
            float w = (width - 20f);
            DrawString(canvasArgs, _textFormat, xOff + w / 2.0f, yStart + h + 5, "fpr", _textColor, false, true, false);

            var oldTransform = canvasArgs.DrawingSession.Transform;
            var mat = Matrix3x2.CreateRotation((-90f * (float)Math.PI) / 180.0f, new Vector2(xStart + 5, yStart + h / 2.0f));
            canvasArgs.DrawingSession.Transform = mat * oldTransform;
            DrawString(canvasArgs, _textFormat, xStart + 5, yStart + h / 2.0f, "tpr", _textColor, false, true, false);
            canvasArgs.DrawingSession.Transform = oldTransform;


            var rect = new Rect(xOff,
                        yOff,
                        w,
                        h);
            canvasArgs.DrawingSession.DrawRectangle(rect, white);
            canvasArgs.DrawingSession.DrawLine(new Vector2(xOff, yOff + h), new Vector2(xOff + w, yOff), white, 0.5f);

            Pt last = new Pt(0,0);
            foreach (var pt in _classfierResultDescriptionModel.RocCurves[_classfierResultDescriptionModel.Labels[_labelIndex]])
            {
                canvasArgs.DrawingSession.DrawLine(
                    new Vector2(
                        (float) (last.X*w + xOff),
                        (float) ((1.0 - last.Y) * h + yOff)), 
                    new Vector2(
                        (float)pt.X * w + xOff,
                        (float)(1.0 - pt.Y) * h + yOff), blue, 1f);
                last = pt;
            }

            var auc = _classfierResultDescriptionModel.AUCs[_classfierResultDescriptionModel.Labels[_labelIndex]];
            DrawString(canvasArgs, _textFormat, xOff + w - 4, yOff + h - 15, "auc: " + auc.ToString("F2"), _textColor, false, false, false);
        }

        private void computeSizes(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            _deviceWidth = (float)(canvas.ActualWidth / CompositionScaleX - _leftOffset - _rightOffset);
            _deviceHeight = (float)(canvas.ActualHeight / CompositionScaleY - _topOffset - _bottomtOffset);

            _minX = (float) 0;
            _minY = (float) 0;
            _maxX = (float) 100;
            _maxY = (float) 100;

            _xScale = _maxX - _minX;
            _yScale = _maxY - _minY;
        }

        public override void Load(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs canvasArgs)
        {
            _textFormat = new CanvasTextFormat()
            {
                FontSize = 11,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };

            _textFormatBig = new CanvasTextFormat()
            {
                FontSize = 16,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            
            _textColor = Color.FromArgb(255, 17, 17, 17);
        }
    }
}
