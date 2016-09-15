using NetTopologySuite.Geometries;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using PanoramicDataWin8.utils;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;
using IDEA_common.operations;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.view.inq;
using Point = Windows.Foundation.Point;

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
        private CanvasTextFormat _textFormatLarge;
        private List<List<Windows.Foundation.Point>> _strokes = new List<List<Point>>();
        private List<object> _recognizedText = new List<object>();

        private IResult _result = null;
        private ClassfierResultDescriptionModel _classfierResultDescriptionModel = null;

        private ClassificationOperationModel _classificationOperationModelClone = null;
        private ClassificationOperationModel _classificationOperationModel = null;

        private int _viewIndex = 0;

        public int ViewIndex
        {
            get { return _viewIndex; }
            set { _viewIndex = value; }
        }

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        

        public ClassifierRendererContentProvider()
        {
        }
        
        public void UpdateData(IResult result, ClassificationOperationModel classificationOperationModel, ClassificationOperationModel classificationOperationModelClone, int viewIndex )
        {
            _result = result;
            _classificationOperationModelClone = classificationOperationModelClone;
            _classificationOperationModel = classificationOperationModel;
            _viewIndex = viewIndex;

            //_classfierResultDescriptionModel = _result.ResultDescriptionModel as ClassfierResultDescriptionModel;
        }

        private bool? _testResult = null;
        public async void ProcessStroke(List<Windows.Foundation.Point> stroke, bool isErase)
        {
            if (stroke.Count > 5)
            {
                if (isErase)
                {
                    ILineString inputLineString = stroke.GetLineString();
                    foreach (var stroke1 in _strokes.ToArray())
                    {
                        ILineString currenLineString = stroke1.GetLineString();
                        if (inputLineString.Intersects(currenLineString))
                        {
                            _strokes.Remove(stroke1);
                        }
                    }
                }
                else
                {
                    _strokes.Add(stroke);
                }

                var psm = (_classificationOperationModelClone.SchemaModel as ProgressiveSchemaModel);

                // get text for each feature
                _recognizedText.Clear();
                var h = _deviceHeight/(float)_classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;
                var x = _leftOffset + 20;
                var y = _topOffset;

                float count = 0;
                foreach (var feature in _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature))
                {
                    var rect = new Rect(x, y + h*count, _deviceWidth, h);
                    var geom = rect.GetPolygon();
                    var inter = _strokes.Where(s => s.GetLineString().Intersects(geom));
                    List<string> recog = await inkToText(inter);
                    if (recog.Any())
                    {
                        var t = recog.First();
                        double d = 0;
                        if (double.TryParse(t, out d))
                        {
                            _recognizedText.Add(d);
                        }
                        else
                        {
                            _recognizedText.Add(t);
                        }
                    }
                    else
                    {
                        _recognizedText.Add("");
                    }
                    count++;
                }


                if (_recognizedText.All(g => g.ToString() != ""))
                {
                    JObject f = new JObject();

                    //_classfierResultDescriptionModel.Query
                    var c = 0;
                    foreach (var feature in _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature))
                    {
                        f.Add(new JProperty(feature.AttributeModel.RawName, new JArray(_recognizedText[c] is string ? 0 : _recognizedText[c])));
                        c++;
                    }

                    JObject request = new JObject(
                        new JProperty("type", "test"),
                        new JProperty("dataset", psm.RootOriginModel.DatasetConfiguration.Schema.RawName),
                        new JProperty("uuid", ""),
                        new JProperty("features", f.ToString()),
                        new JProperty("feature_dimensions", _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).Select(fi => fi.AttributeModel.RawName).ToList()));
                    request["uuid"] = _classfierResultDescriptionModel.Uuid;

                    string message = await ProgressiveGateway.Request(request, "test");
                    JToken jToken = JToken.Parse(message);
                    if (jToken["result"] != null && (double)jToken["result"] == 0)
                    {
                        _testResult = false;
                    }
                    else
                    {
                        _testResult = true;
                    }
                    //_histogramOperationModel.FireRequestRender();
                }
                else
                {
                    _testResult = false;
                    //_histogramOperationModel.FireRequestRender();
                }
            }
        }

        private async Task<List<string>> inkToText(IEnumerable<List<Windows.Foundation.Point>> strokes)
        {
            if (!strokes.Any())
            {
                return new List<string>();
            }
            var im = new InkManager();
            var b = new InkStrokeBuilder();

            foreach (var inStroke in strokes)
            {
                var pc = new PointCollection() ;
                foreach (var pt in inStroke)
                {
                    pc.Add(pt);
                }
                var stroke = b.CreateStroke(pc);
                im.AddStroke(stroke);
            }
            var result = await im.RecognizeAsync(InkRecognitionTarget.All);
            return result[0].GetTextCandidates().ToList();
        }


        public override void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;
            if (_result != null && _classfierResultDescriptionModel != null)
            {
                computeSizes(canvas, canvasArgs);

                if (_deviceHeight < 0 || _deviceWidth < 0)
                {
                    return;
                }
                var centerX =  _deviceWidth/2.0f + _leftOffset;
                var centerY =  _deviceHeight/2.0f + _topOffset;

                int maxIndex = 3 + _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;

                if (_viewIndex == 0)
                {
                    string label = (_classfierResultDescriptionModel.F1s.Last() * 100.0).ToString("F1");
                    var layoutL = new CanvasTextLayout(canvas, label, _textFormatLarge, 1000f, 1000f);
                    var metrics = layoutL.DrawBounds;

                    var layoutP = new CanvasTextLayout(canvas, "%", _textFormatBig, 1000f, 1000f);
                    var metricsPercentage = layoutP.DrawBounds;

                    var totalX = metrics.Width + metricsPercentage.Width + 5;

                    var blue = Color.FromArgb(255, 41, 170, 213);
                    var white = Color.FromArgb(255, 255, 255, 255);
                    DrawString(canvasArgs, _textFormatLarge, centerX - (float) totalX/2.0f, _topOffset, label, blue, true, false, false);
                    DrawString(canvasArgs, _textFormatBig, centerX - (float)totalX / 2.0f + (float)metrics.Width +5, _topOffset + (float) metrics.Height +(float) metrics.Top - (float)metricsPercentage.Height - (float) metricsPercentage.Y, "%", blue, true, false, false);


                    float width = _deviceWidth/1.7f;
                    float height = _deviceHeight / 1.7f;

                    float xStart = centerX - width/2.0f;
                    float yStart = _topOffset + (float) (metrics.Height + metrics.Y) + 20;
                    
                    DrawString(canvasArgs, _textFormat, xStart + width / 2.0f, yStart + height + 5, "progress", _textColor, false, true, false);

                    var oldTransform = canvasArgs.DrawingSession.Transform;
                    mat = Matrix3x2.CreateRotation((-90f * (float)Math.PI) / 180.0f, new Vector2(xStart, yStart + height / 2.0f));
                    canvasArgs.DrawingSession.Transform = mat * oldTransform;
                    DrawString(canvasArgs, _textFormat, xStart, yStart + height / 2.0f - 20, "f1", _textColor, false, true, false);
                    canvasArgs.DrawingSession.Transform = oldTransform;


                    var rect = new Rect(xStart,
                                yStart,
                                width,
                                height);
                    canvasArgs.DrawingSession.DrawRectangle(rect, white);

                    
                    var index = 0;
                    List<Pt> points = new List<Pt>();
                    foreach (var f1 in _classfierResultDescriptionModel.F1s)
                    {
                        points.Add(new Pt(_classfierResultDescriptionModel.Progresses[index], f1));
                        index++;
                    }

                    Pt last = new Pt(0, 0);
                    foreach (var pt in points)
                    {
                        canvasArgs.DrawingSession.DrawLine(
                            new Vector2(
                                (float)(last.X * width + xStart),
                                (float)((1.0 - last.Y) * height + yStart)),
                            new Vector2(
                                (float)pt.X * width + xStart,
                                (float)(1.0 - pt.Y) * height + yStart), blue, 1f);
                        last = pt;
                        index++;
                    }
                }
                else if (_viewIndex == 1)
                {
                    string label = "details"; //_viewIndex != -1 ? _classfierResultDescriptionModel.Labels[_viewIndex].RawName : "avg across labels";
                    DrawString(canvasArgs, _textFormatBig, centerX, _topOffset, label, _textColor, false, true, false);

                    var w = (_deviceWidth - 20)/3.0f;
                    var h = Math.Max(0, _deviceHeight/2.0f - 50);
                    var yOff = 40f;

                    if ((_deviceHeight <= 200 || _deviceWidth <= 180) || _viewIndex == -1)
                    {
                        //yOff = centerY - h /2.0f;
                        h = Math.Max(0, _deviceHeight - 50);
                    }

                    renderGauge(canvas, canvasArgs,
                        _leftOffset,
                        yOff,
                        w,
                        h,
                        (float) _classfierResultDescriptionModel.Precision,
                        "precision");

                    renderGauge(canvas, canvasArgs,
                        _leftOffset + w + 10,
                        yOff,
                        w,
                        h,
                        (float) _classfierResultDescriptionModel.Recall,
                        "recall");

                    renderGauge(canvas, canvasArgs,
                        _leftOffset + (w + 10)*2,
                        yOff,
                        w,
                        h,
                        (float) _classfierResultDescriptionModel.F1s.Last(),
                        "f1");

                    if (_deviceHeight > 200 && _deviceWidth > 180 && _viewIndex != -1)
                    {
                        renderConfusionMatrix(canvas, canvasArgs,
                            _leftOffset, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                        renderRoc(canvas, canvasArgs,
                            centerX + 10, centerY, _deviceWidth/2.0f - 10, _deviceHeight/2.0f);
                    }
                }
                else if (_viewIndex == maxIndex - 1)
                {
                    /*string label = "query panel";
                    var layoutL = new CanvasTextLayout(canvas, label, _textFormatLarge, 1000f, 1000f);
                    var metrics = layoutL.DrawBounds;

                    var totalX = metrics.Width;
                                        
                    var white = Color.FromArgb(255, 255, 255, 255);
                    DrawString(canvasArgs, _textFormatLarge, centerX - (float) totalX/2.0f, _topOffset, label, blue, true, false, false);*/

                    var blue = Color.FromArgb(255, 41, 170, 213);
                    var brush = Color.FromArgb(255, 178, 77, 148);
                    var h = _deviceHeight / (float)_classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature).Count;
                    var x = _leftOffset + 20;
                    var y = _topOffset;

                    float count = 0;
                    foreach (var feature in _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature))
                    {
                        var oldTransform = canvasArgs.DrawingSession.Transform;
                        mat = Matrix3x2.CreateRotation((-90f*(float) Math.PI)/180.0f, new Vector2(x, y + (h/2.0f) + count * h));
                        canvasArgs.DrawingSession.Transform = mat*oldTransform;
                        DrawString(canvasArgs, _textFormatBig, x, y + (h / 2.0f) + count * h, feature.AttributeModel.RawName, _textColor, false, true, false);
                        canvasArgs.DrawingSession.Transform = oldTransform;

                        if (_recognizedText.Count > count)
                        {
                            var text = _recognizedText[(int) count];
                            DrawString(canvasArgs, _textFormat, _leftOffset + _deviceWidth - 30, y + (h) + count*h, text.ToString(), _textColor, false, false, false);
                        }
                        count++;
                    }

                    // render strokes
                    foreach (var stroke in _strokes.Where(s=> s.Count > 1))
                    {
                        Pt last = stroke[0];
                        foreach (var pt in stroke.Skip(1))
                        {
                            canvasArgs.DrawingSession.DrawLine(
                                new Vector2(
                                    (float)last.X, (float) last.Y),
                                new Vector2(
                                    (float)pt.X, (float)pt.Y), !_testResult.HasValue ? _textColor : (_testResult.Value ? brush : blue), 3f);
                            last = pt;
                        }
                    }

                    /*
                    InkRecognizerContainer container = new InkRecognizerContainer();
                    InkRecognizer inkrecog = new InkAna;
                    container..
                    var result = await container.RecognizeAsync(ink.InkPresenter.StrokeContainer, InkRecognitionTarget.);
                    string s = result[0].GetTextCandidates()[0];*/
                }
                else if (_viewIndex < maxIndex - 1)
                {
                    int histogramIndex = _viewIndex - 2;
                    var feat = _classificationOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Feature)[histogramIndex];

                    string label = feat.AttributeModel.RawName.Replace("_", " "); //_viewIndex != -1 ? _classfierResultDescriptionModel.Labels[_viewIndex].RawName : "avg across labels";
                    DrawString(canvasArgs, _textFormatBig, centerX, _topOffset, label, _textColor, false, true, false);

                    var rr = new ClassifierRendererPlotContentProvider() {CompositionScaleX = CompositionScaleX, CompositionScaleY = CompositionScaleY};
                    var xIom = new AttributeTransformationModel(feat.AttributeModel) {AggregateFunction = AggregateFunction.None};
                    var yIom = new AttributeTransformationModel(feat.AttributeModel) { AggregateFunction = AggregateFunction.Count };
                    var vIom = new AttributeTransformationModel(feat.AttributeModel) { AggregateFunction = AggregateFunction.Count };
                    rr.UpdateData(_classfierResultDescriptionModel.VisualizationResultModel[histogramIndex], xIom, yIom, vIom);
                    rr.render(canvas, canvasArgs, 40, 20, 40, 45, _deviceWidth, _deviceHeight);
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

            for (int r = 0; r < _classfierResultDescriptionModel.ConfusionMatrices.Count; r++)
            {
                DrawString(canvasArgs, _textFormat, xOff + w / 2.0f + (r * w), yOff - 20, (1 - r) + "", _textColor, false, true, false);
                DrawString(canvasArgs, _textFormat, xOff - 10, yOff + h / 2.0f + (r * h), (1 - r) + "", _textColor, false, true, true);

                var row = _classfierResultDescriptionModel.ConfusionMatrices[r];
                var total = _classfierResultDescriptionModel.ConfusionMatrices.SelectMany(t => t).Sum();
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
                    
                    var binColor = Color.FromArgb(255, 178, 77, 148);

                    if (r == 0 && c == 0)
                    {
                        binColor = Color.FromArgb(255, 178, 77, 148);
                    }
                    else if (r == 1 && c == 0)
                    {
                        binColor = Color.FromArgb(125, 41, 170, 213);
                    }
                    else if (r == 1 && c == 1)
                    {
                        binColor = Color.FromArgb(255, 41, 170, 213);
                    }
                    else if (r == 0 && c == 1)
                    {
                        binColor = Color.FromArgb(125, 178, 77, 148);
                    }

                    canvasArgs.DrawingSession.FillRoundedRectangle(roundedRect, 4, 4, binColor);
                    
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
            foreach (var pt in _classfierResultDescriptionModel.RocCurve)
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

            var auc = _classfierResultDescriptionModel.AUC;
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

            _textFormatLarge = new CanvasTextFormat()
            {
                FontSize = 48,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };

            _textColor = Color.FromArgb(255, 17, 17, 17);
        }
    }
}
