using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI;
using PanoramicDataWin8.controller.view;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace PanoramicDataWin8.view.common
{
    public sealed partial class DXSurface : UserControl
    {
        private DXSurfaceContentProvider _contentProvider = null;
        public DXSurfaceContentProvider ContentProvider
        {
            get
            {
                return _contentProvider;
            }
            set
            {
                _contentProvider = value;
            }
        }

        public DXSurface()
        {
            this.InitializeComponent();
        }

        public void Dispose()
        {
            this.canvasControl.RemoveFromVisualTree();
            this.canvasControl = null;
        }

        public void Redraw()
        {
            canvasControl.Invalidate();
        }

        public float CompositionScaleX
        {
            get
            {
                return 1;
            }
        }

        public float CompositionScaleY
        {
            get
            {
                return 1;
            }
        }

        private void canvasControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (_contentProvider != null)
            {
                _contentProvider.Load(sender, args);
            }
        }

        private void canvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (_contentProvider != null)
            {
                _contentProvider.Draw(sender, args);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (canvasControl != null)
            {
                this.canvasControl.RemoveFromVisualTree();
                this.canvasControl = null;
            }
        }
    }

    public class DXSurfaceContentProvider
    {
        public virtual void Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs)
        {

        }
        public virtual void Load(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl canvas, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs canvasArgs)
        {

        }

        protected void DrawString(Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs canvasArgs, CanvasTextFormat textFormat, float x, float y, string text, Color textColor,
            bool leftAligned,
            bool horizontallyCentered, bool verticallyCentered)
        {
            CanvasTextFormat ctf = new CanvasTextFormat()
            {
                FontSize = textFormat.FontSize,
                FontFamily = textFormat.FontFamily,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top
            };
            
            if (horizontallyCentered)
            {
                ctf.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            }
            if (verticallyCentered)
            {
                ctf.VerticalAlignment = CanvasVerticalAlignment.Center; ;
            }
            if (!leftAligned && !horizontallyCentered)
            {
                ctf.HorizontalAlignment = CanvasHorizontalAlignment.Right;
            }

            canvasArgs.DrawingSession.DrawText(text, new Vector2(x, y), textColor, ctf);
        }
    }
}
