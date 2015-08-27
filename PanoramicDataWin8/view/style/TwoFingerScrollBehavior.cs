using Microsoft.Xaml.Interactivity;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.view.style
{
    public class TwoFingerScrollBehavior : DependencyObject, IBehavior
    {
        private FrameworkElement _frameworkElement = null;
        private ScrollViewer _scrollViewer = null;
        private Point _previousPoint = new Point();
        private PointerManager _pointerManager = null;
        private InertiaHandler _inertiaHandler = null;
        private Pointer scrollPointer = null;
        private Point _startPoint = new Point();
        private double _startVerticalOffset = 0;
        private IOneFingerListener _oneFingerListener = null;

        public DependencyObject AssociatedObject
        {
            get { return _frameworkElement; }
        }

        public void Detach()
        {
            if (_frameworkElement != null && _pointerManager != null)
            {
                _pointerManager.Detach();
                _inertiaHandler.Dispose();
            }
        }

        public void Attach(DependencyObject associatedObject)
        {
            if (associatedObject is FrameworkElement)
            {
                _frameworkElement = (associatedObject as FrameworkElement);
                _frameworkElement.ManipulationMode = ManipulationModes.None;
                _frameworkElement.Loaded += _frameworkElement_Loaded;
                _pointerManager = new PointerManager();
                _pointerManager.Added += _pointerManager_Added;
                _pointerManager.Moved += _pointerManager_Moved;
                _pointerManager.Removed += _pointerManager_Removed;
                _pointerManager.Attach(_frameworkElement);
            }
        }

        void _frameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = _frameworkElement.GetFirstAncestorOfType<ScrollViewer>();
            _inertiaHandler = new InertiaHandler(_scrollViewer);
            _oneFingerListener = _frameworkElement.GetAncestors().FirstOrDefault(a => a is IOneFingerListener) as IOneFingerListener;
        }

        void _pointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1 && _oneFingerListener != null)
            {
                var tt = e.CurrentContacts[e.TriggeringPointer.PointerId].Position;
                _oneFingerListener.Pressed(_frameworkElement, e);
            }
            else if (e.NumActiveContacts == 2 && _scrollViewer != null)
            {
                scrollPointer = e.TriggeringPointer;
                _startPoint = e.CurrentContacts[scrollPointer.PointerId].Position;
                _previousPoint = _startPoint;
                _startVerticalOffset = _scrollViewer.VerticalOffset;
                _inertiaHandler.InertiaActive = false;
            }
        }

        void _pointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1 && _oneFingerListener != null)
            {
                _oneFingerListener.Moved(_frameworkElement, e);
            }
            else if (_scrollViewer != null && e.NumActiveContacts == 2 && scrollPointer != null && e.CurrentPointers.Contains(scrollPointer) && e.TriggeringPointer.PointerId == scrollPointer.PointerId)
            {
                Point currentPoint = e.CurrentContacts[scrollPointer.PointerId].Position;
                double yDelta = _startPoint.Y - currentPoint.Y;
                _scrollViewer.ChangeView(0, yDelta + _startVerticalOffset, 1);

                _inertiaHandler.InertiaActive = false;
                _inertiaHandler.ScrollTarget = new Point(0, yDelta + _startVerticalOffset);
                if (_previousPoint.Y - currentPoint.Y != 0)
                {
                    _inertiaHandler.Velocity = new Vec(0, _previousPoint.Y - currentPoint.Y);
                }
                
                _previousPoint = currentPoint;
            }
        }

        void _pointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts < 2)
            {
                _inertiaHandler.InertiaActive = true;
                if (e.NumActiveContacts < 1 && _oneFingerListener != null)
                {
                    _oneFingerListener.Released(_frameworkElement, e);
                }
            }
            if (scrollPointer != null && e.TriggeringPointer.PointerId == scrollPointer.PointerId)
            {
                scrollPointer = null;
            }
        }
    }

    class InertiaHandler : IDisposable
    {
        private ScrollViewer scroller;
        private DispatcherTimer animationTimer;

        private Point scrollTarget;
        public Point ScrollTarget
        {
            get { return scrollTarget; }
            set { scrollTarget = value; }
        }

        private bool inertiaActive;
        public bool InertiaActive
        {
            get { return inertiaActive; }
            set { inertiaActive = value; }
        }

        private Vec velocity;
        public Vec Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public InertiaHandler(ScrollViewer scroller)
        {
            this.scroller = scroller;
            this.InertiaActive = false;
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            animationTimer.Tick += HandleWorldTimerTick;
            animationTimer.Start();
        }
        private void HandleWorldTimerTick(object sender, object e)
        {
            if (InertiaActive)
            {
                if (velocity.Length > 1)
                {
                    
                    scroller.ChangeView(0, ScrollTarget.Y, 1);
                    scrollTarget.X += velocity.X;
                    scrollTarget.Y += velocity.Y;
                    velocity *= 0.95;
                }
            }
        }

        public void Dispose()
        {
            animationTimer.Stop();
        }
    }

    public interface IOneFingerListener
    {
        void Pressed(FrameworkElement sender, PointerManagerEvent e);
        void Moved(FrameworkElement sender, PointerManagerEvent e);
        void Released(FrameworkElement sender, PointerManagerEvent e);
    }
}
