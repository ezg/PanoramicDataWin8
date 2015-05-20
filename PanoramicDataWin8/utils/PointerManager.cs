using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace PanoramicDataWin8.utils
{
    public class PointerManager
    {
        public event EventHandler<PointerManagerEvent> Added;
        public event EventHandler<PointerManagerEvent> Removed;
        public event EventHandler<PointerManagerEvent> Moved;

        private FrameworkElement _frameworkElement = null;

        private TouchCapabilities _supportedContacts = new TouchCapabilities();
        private uint _numActiveContacts;
        private Dictionary<uint, PointerPoint> _currentContacts;
        private Dictionary<uint, PointerPoint> _startContacts;
        private List<Pointer> _currentPointers;


        public uint GetNumActiveContacts()
        {
            return _numActiveContacts;
        }

        public void Attach(FrameworkElement frameworkElement)
        {
            _frameworkElement = frameworkElement;
            _numActiveContacts = 0;
            _currentContacts = new Dictionary<uint, PointerPoint>((int)_supportedContacts.Contacts);
            _startContacts = new Dictionary<uint, PointerPoint>((int)_supportedContacts.Contacts);
            _currentPointers = new List<Pointer>((int)_supportedContacts.Contacts);

            _frameworkElement.PointerPressed += new PointerEventHandler(frameworkElement_PointerPressed);
            _frameworkElement.PointerReleased += new PointerEventHandler(frameworkElement_PointerReleased);
            _frameworkElement.PointerCanceled += new PointerEventHandler(frameworkElement_PointerCanceled);
            _frameworkElement.PointerCaptureLost += new PointerEventHandler(frameworkElement_PointerCaptureLost);
            _frameworkElement.PointerMoved += new PointerEventHandler(frameworkElement_PointerMoved);
        }

        public void Detach()
        {
            _frameworkElement.PointerPressed -= new PointerEventHandler(frameworkElement_PointerPressed);
            _frameworkElement.PointerReleased -= new PointerEventHandler(frameworkElement_PointerReleased);
            _frameworkElement.PointerCanceled -= new PointerEventHandler(frameworkElement_PointerCanceled);
            _frameworkElement.PointerCaptureLost -= new PointerEventHandler(frameworkElement_PointerCaptureLost);
            _frameworkElement.PointerMoved -= new PointerEventHandler(frameworkElement_PointerMoved);
        }

        void frameworkElement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Convert.ToBoolean(_supportedContacts.TouchPresent) && (_numActiveContacts > _supportedContacts.Contacts))
            {
                Debug.WriteLine("Number of contacts exceeds the number supported by the device.");
                return;
            }

            PointerPoint pt = e.GetCurrentPoint(_frameworkElement);

            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen || (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                return;
            }
            if (_currentContacts.ContainsKey(pt.PointerId))
            {
                return;
            }
            _frameworkElement.CapturePointer(e.Pointer);
            if (!_currentPointers.Any(p => p.PointerId == e.Pointer.PointerId))
            {
                _currentPointers.Add(e.Pointer);
            }
            _currentContacts[pt.PointerId] = pt;
            _startContacts[pt.PointerId] = pt;
            ++_numActiveContacts;
            fireAdded(e.Pointer);
            e.Handled = true;
        }

        void frameworkElement_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(_frameworkElement);

            if (_currentContacts.ContainsKey(pt.PointerId))
            {
                _currentPointers.Remove(_currentPointers.First(p => p.PointerId == pt.PointerId));
                _frameworkElement.ReleasePointerCapture(e.Pointer);
                _currentContacts[pt.PointerId] = null;
                _currentContacts.Remove(pt.PointerId);
                _startContacts.Remove(pt.PointerId);
                --_numActiveContacts;
                fireRemoved(e.Pointer);

                e.Handled = true;
            }
        }

        void frameworkElement_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(_frameworkElement);

            if (_currentContacts.ContainsKey(pt.PointerId))
            {
                _frameworkElement.ReleasePointerCapture(e.Pointer);

                e.Handled = true;
            }
        }

        void frameworkElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(_frameworkElement);

            if (_currentContacts.ContainsKey(pt.PointerId))
            {
                _frameworkElement.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }
        }

        void frameworkElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(_frameworkElement);
            if (pt.IsInContact)
            {
                if (_startContacts.ContainsKey(pt.PointerId))
                {
                    _currentContacts[pt.PointerId] = pt;
                    fireMoved(e.Pointer);
                    e.Handled = true;
                }
            }
        }

        PointerManagerEvent createPointerManagerEvent(Pointer triggeringPointer)
        {
            Dictionary<uint, PointerPoint> currentContactsCopy = new Dictionary<uint, PointerPoint>();
            foreach (var k in _currentContacts.Keys)
            {
                currentContactsCopy.Add(k, _currentContacts[k]);
            } 

            Dictionary<uint, PointerPoint> startContactsCopy = new Dictionary<uint, PointerPoint>();
            foreach (var k in _startContacts.Keys)
            {
                startContactsCopy.Add(k, _startContacts[k]);
            }

            List<Pointer> currentPointerCopy = new List<Pointer>();
            foreach (var k in _currentPointers)
            {
                currentPointerCopy.Add(k);
            }

            return new PointerManagerEvent()
            {
                TriggeringPointer = triggeringPointer,
                NumActiveContacts = _numActiveContacts,
                CurrentContacts = currentContactsCopy,
                CurrentPointers = currentPointerCopy,
                StartContacts = startContactsCopy
            };
        }

        void fireAdded(Pointer triggeringPointer)
        {
            if (Added != null)
            {
                Added(this, createPointerManagerEvent(triggeringPointer));
            }
        }

        void fireMoved(Pointer triggeringPointer)
        {
            if (Moved != null)
            {
                Moved(this, createPointerManagerEvent(triggeringPointer));
            }
        }

        void fireRemoved(Pointer triggeringPointer)
        {
            if (Removed != null)
            {
                Removed(this, createPointerManagerEvent(triggeringPointer));
            }
        }
    }
    public class PointerManagerEvent : EventArgs
    {
        public Pointer TriggeringPointer { get; set; }
        public uint NumActiveContacts { get; set; }
        public List<Pointer> CurrentPointers { get; set; }
        public Dictionary<uint, PointerPoint> CurrentContacts { get; set; }
        public Dictionary<uint, PointerPoint> StartContacts { get; set; }
    }
}
