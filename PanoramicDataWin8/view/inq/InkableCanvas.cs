using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.view.inq
{
    public class InkableCanvas : Canvas
    {
        public delegate void InkCollectedEventHandler(object sender, InkCollectedEventArgs e);
        public event InkCollectedEventHandler InkCollectedEvent;

        private List<DateTime> _currentInkDelays;
        private InkStroke _currentInkStroke;
        private bool _isPointerPressed;

        public InkableCanvas()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            this.PointerPressed += InkableCanvas_PointerPressed;
            this.PointerMoved += InkableCanvas_PointerMoved;
            this.PointerReleased += InkableCanvas_PointerReleased;
        }

        void InkableCanvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen || (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down) 
            {
                handleDown(e);
                e.Handled = true;
            }
        }

        private void handleDown(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _isPointerPressed = true;
            CapturePointer(e.Pointer);
            List<Point> pts = new List<Point>();
            pts.Add(e.GetCurrentPoint(this).Position);
            _currentInkStroke = new InkStroke(pts);
            _currentInkDelays = new List<DateTime>();
            _currentInkDelays.Add(DateTime.Now);

            var leftControl = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            var leftShift = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftShift);

            _currentInkStroke.IsPause = false;
            _currentInkStroke.IsErase = e.GetCurrentPoint(this).Properties.IsEraser || e.GetCurrentPoint(this).Properties.IsRightButtonPressed || (
                (leftControl & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down && (leftShift & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down);
            addDrawingInkStroke(_currentInkStroke);

        }

        void InkableCanvas_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            if (_isPointerPressed)
            {
                handleMove(e.GetCurrentPoint(this).Position);
                e.Handled = true;
            }
        }


        private void handleMove(Point pt)
        {
            _currentInkDelays.Add(DateTime.Now);
            _currentInkStroke.Add(new Point(pt.X, pt.Y));
            _currentInkStroke.IsPause = _currentInkDelays.Count > 5 &&
               ((utils.Pt)_currentInkStroke.Points[_currentInkStroke.Points.Count-1] - (utils.Pt)_currentInkStroke.Points[_currentInkStroke.Points.Count - 5]).Length < 10;
        }

        void InkableCanvas_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            if (_isPointerPressed)
            {
                handleUp(e);
                e.Handled = true;
            }
        }

        private void handleUp(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var tms = DateTime.Now.Subtract(_currentInkDelays[_currentInkDelays.Count - 1]).TotalMilliseconds;
            _currentInkStroke.IsPause |= tms > 100;
            ReleasePointerCapture(e.Pointer);
            _currentInkStroke.Add(new Point(e.GetCurrentPoint(this).Position.X, e.GetCurrentPoint(this).Position.Y));
            removeDrawingInkStroke(_currentInkStroke);
            fireInkCollected(_currentInkStroke);
            _currentInkStroke = null;
            _isPointerPressed = false;

        }

        private void addDrawingInkStroke(InkStroke s)
        {
            Children.Add(s);
        }

        private void removeDrawingInkStroke(InkStroke s)
        {
            foreach (var c in Children)
            {
                if (c is InkStrokeElement && (c as InkStrokeElement).InkStroke == s)
                {
                    Children.Remove(c as InkStrokeElement);
                    break;
                }
            }
        }

        private void fireInkCollected(InkStroke s)
        {
            if (InkCollectedEvent != null)
            {
                InkCollectedEvent(this, new InkCollectedEventArgs(s));
            }
        }
        
    }

    public class InkCollectedEventArgs : RoutedEventArgs
    {
        public InkStroke InkStroke { get; private set; }

        public InkCollectedEventArgs(InkStroke s)
        {
            InkStroke = s;
        }
    }
}
