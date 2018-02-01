using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using GeoAPI.Geometries;
using IDEA_common.operations;
using IDEA_common.operations.example;
using IDEA_common.operations.ml.optimizer;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;

namespace PanoramicDataWin8.view.vis.render
{
    public class PredictorRendererContentProvider : DXSurfaceContentProvider
    {
        private PredictorOperationModel _predictorOperationModel;

        private PredictorOperationModel _predictorOperationModelClone;

        private OptimizerResult _optimizerResult;
        private bool _isResultEmpty;

        private Color _textColor;
        private CanvasTextFormat _textFormatBig;
        private CanvasTextFormat _textFormatSubmit;
        private CanvasTextFormat _textFormatSmall;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }

        public IGeometry SubmitHitTarget { get; set; }
        public IGeometry SpecifyProblemHitTarget { get; set; }

        public void UpdateData(IResult result, PredictorOperationModel predictorOperationModel, PredictorOperationModel predictorOperationModelClone)
        {
            _optimizerResult = (OptimizerResult) result;
            _predictorOperationModelClone = predictorOperationModelClone;
            _predictorOperationModel = predictorOperationModel;

            if (_optimizerResult != null)
                _isResultEmpty = false;
            else
                _isResultEmpty = true;
        }

        public override void Draw(CanvasControl canvas, CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_optimizerResult != null)
            {
                var leftOffset = 10;
                var rightOffset = 10;
                var topOffset = 10;
                var bottomtOffset = 45;

                var deviceWidth = (float) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (float) (canvas.ActualHeight/CompositionScaleY - topOffset - bottomtOffset);

                var metric = _optimizerResult.Metrics;

                if (MainViewController.Instance.MainModel.IsDarpaSubmissionMode)
                {
                    var blue = Color.FromArgb(255, 41, 170, 213);
                    DrawString(canvasArgs, _textFormatSubmit, deviceWidth + leftOffset, topOffset, "submit\nsolution?", blue, false, false, false);

                    var layout = new CanvasTextLayout(canvas, "submit\nsolution?", _textFormatSubmit, 1000f, 1000f);
                    var layoutBounds = layout.DrawBounds;
                    layout.Dispose();
                    var rct = new Rct(new Pt(deviceWidth + leftOffset - layoutBounds.Width - 5, topOffset),
                        new Vec(layoutBounds.Width + 10, layoutBounds.Height + 10));
                    //canvasArgs.DrawingSession.DrawRectangle(rct, blue);
                    SubmitHitTarget = rct.GetPolygon();

                    
                    DrawString(canvasArgs, _textFormatSubmit, leftOffset, topOffset, "specify\nproblem?", blue, true, false, false);

                    layout = new CanvasTextLayout(canvas, "specify\nproblem?", _textFormatSubmit, 1000f, 1000f);
                    layoutBounds = layout.DrawBounds;
                    layout.Dispose();
                    rct = new Rct(new Pt(leftOffset - 5, topOffset),
                        new Vec(layoutBounds.Width + 10, layoutBounds.Height + 10));
                    //canvasArgs.DrawingSession.DrawRectangle(rct, blue);
                    SpecifyProblemHitTarget = rct.GetPolygon();

                }



                if (metric.AverageAccuracy.HasValue && metric.AverageAccuracy.Value != 0)
                {
                    renderGauge(canvas, canvasArgs,
                        leftOffset,
                        topOffset + 30,
                        deviceWidth,
                        deviceHeight - 30,
                        (float) metric.AverageAccuracy.Value,
                        "accuracy");
                }
                else if (metric.AverageRSquared.HasValue && metric.AverageRSquared.Value != 0)
                {
                    renderGauge(canvas, canvasArgs,
                        leftOffset,
                        topOffset + 30,
                        deviceWidth,
                        deviceHeight - 30,
                        (float)metric.AverageRSquared.Value,
                        "r squared");
                }
            }
            if (_isResultEmpty)
            {
                var leftOffset = 10;
                var rightOffset = 10;
                var topOffset = 10;
                var bottomtOffset = 45;

                var deviceWidth = (float) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (float) (canvas.ActualHeight/CompositionScaleY - topOffset - bottomtOffset);
                DrawString(canvasArgs, _textFormatBig, deviceWidth/2.0f + leftOffset, deviceHeight/2.0f + topOffset, "no datapoints", _textColor, true, true, false);
            }
        }

        private void renderGauge(CanvasControl canvas, CanvasDrawEventArgs canvasArgs,
            float xStart, float yStart, float width, float height, float value, string name)
        {
            value = (float)Math.Min(0.9999, Math.Max(0.0, value));
            var white = Color.FromArgb(255, 255, 255, 255);
            var blue = Color.FromArgb(255, 41, 170, 213);

            var rect = new Rect(xStart,
                yStart,
                width,
                height);
            //d2dDeviceContext.DrawRectangle(rect, white);
            height -= 25;
            var radius = Math.Min(width / 2.0f, height / 2.0f) - 4;
            DrawString(canvasArgs, _textFormatSmall, width / 2.0f + xStart, (height / 2.0f) + radius + yStart + 10, name, _textColor, false, true, false);

            if (width > 0 && height > 0 && radius > 15)
            {
                canvasArgs.DrawingSession.DrawEllipse(new Vector2(
                    (width / 2.0f) + xStart,
                    (height / 2.0f) + yStart), radius, radius, white, 4);

                var thickness = 4.0f;

                float angle = 2.0f * (float)Math.PI * value - (float)Math.PI / 2.0f;
                float x = (width / 2.0f) + xStart;
                float y = (height / 2.0f) + yStart;

                CanvasPathBuilder pathBuilder = new CanvasPathBuilder(canvas);
                pathBuilder.BeginFigure(new Vector2((width / 2.0f) + xStart, (height / 2.0f) + yStart - (radius)), CanvasFigureFill.DoesNotAffectFills);
                pathBuilder.AddArc(new Vector2(
                    (float)Math.Cos(angle) * (radius) + x,
                    (float)Math.Sin(angle) * (radius) + y), radius, radius, 0, CanvasSweepDirection.Clockwise, value > 0.5 ? CanvasArcSize.Large : CanvasArcSize.Small);
                pathBuilder.EndFigure(CanvasFigureLoop.Open);

                CanvasGeometry pathGeometry = CanvasGeometry.CreatePath(pathBuilder);
                canvasArgs.DrawingSession.DrawGeometry(pathGeometry, blue, 4);

                pathBuilder.Dispose();
            }
            DrawString(canvasArgs, _textFormatSmall,
                (width / 2.0f) + xStart,
                (height / 2.0f) + yStart,
                (value * 100.0).ToString("F1") + "%", _textColor, false, true, true);
        }

        public override void Load(CanvasControl canvas, CanvasCreateResourcesEventArgs canvasArgs)
        {
            _textFormatBig = new CanvasTextFormat
            {
                FontSize = 13,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textFormatSubmit = new CanvasTextFormat
            {
                FontSize = 18,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textFormatSmall = new CanvasTextFormat
            {
                FontSize = 11,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textColor = Color.FromArgb(255, 17, 17, 17);
        }
    }
}