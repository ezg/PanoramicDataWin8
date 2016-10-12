using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using IDEA_common.operations;
using IDEA_common.operations.example;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.view.common;

namespace PanoramicDataWin8.view.vis.render
{
    public class ExampleRendererContentProvider : DXSurfaceContentProvider
    {
        private ExampleOperationModel _exampleOperationModel;

        private ExampleOperationModel _exampleOperationModelClone;

        private ExampleResult _histogramResult;
        private bool _isResultEmpty;

        private Color _textColor;
        private CanvasTextFormat _textFormat;

        public float CompositionScaleX { get; set; }
        public float CompositionScaleY { get; set; }
        
        public void UpdateData(IResult result, ExampleOperationModel exampleOperationModel, ExampleOperationModel exampleOperationModelClone)
        {
            _histogramResult = (ExampleResult) result;
            _exampleOperationModelClone = exampleOperationModelClone;
            _exampleOperationModel = exampleOperationModel;

            if (_histogramResult != null)
                _isResultEmpty = false;
            else
                _isResultEmpty = true;
        }

        public override void Draw(CanvasControl canvas, CanvasDrawEventArgs canvasArgs)
        {
            var mat = Matrix3x2.CreateScale(new Vector2(CompositionScaleX, CompositionScaleY));
            canvasArgs.DrawingSession.Transform = mat;

            if (_histogramResult != null)
            {
                var leftOffset = 10;
                var rightOffset = 20;
                var topOffset = 20;
                var bottomtOffset = 45;

                var deviceWidth = (float) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (float) (canvas.ActualHeight/CompositionScaleY - topOffset - bottomtOffset);
                DrawString(canvasArgs, _textFormat, deviceWidth/2.0f + leftOffset, deviceHeight/2.0f + topOffset, "yay", _textColor, true, true, false);
            }
            if (_isResultEmpty)
            {
                var leftOffset = 10;
                var rightOffset = 20;
                var topOffset = 20;
                var bottomtOffset = 45;

                var deviceWidth = (float) (canvas.ActualWidth/CompositionScaleX - leftOffset - rightOffset);
                var deviceHeight = (float) (canvas.ActualHeight/CompositionScaleY - topOffset - bottomtOffset);
                DrawString(canvasArgs, _textFormat, deviceWidth/2.0f + leftOffset, deviceHeight/2.0f + topOffset, "no datapoints", _textColor, true, true, false);
            }
        }

        public override void Load(CanvasControl canvas, CanvasCreateResourcesEventArgs canvasArgs)
        {
            _textFormat = new CanvasTextFormat
            {
                FontSize = 11,
                FontFamily = "/Assets/font/Abel-Regular.ttf#Abel"
            };
            _textColor = Color.FromArgb(255, 17, 17, 17);
        }
    }
}