using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PanoramicDataWin8.view.inq
{
    public class InkableScene : InkableCanvas
    {
        private Canvas _elementCanvas = new Canvas();

        public InkableScene()
        {
            Children.Add(_elementCanvas);
        }

        private List<InkStroke> _inkStrokes = new List<InkStroke>();
        public List<InkStroke> InkStrokes
        {
            get
            {
                return _inkStrokes;
            }
        }

        private List<FrameworkElement> _elements = new List<FrameworkElement>();
        public List<FrameworkElement> Elements
        {
            get
            {
                return _elements;
            }
        }

        public void AddToBack(FrameworkElement elem)
        {
            if (!_elements.Contains(elem))
            {
                _elementCanvas.Children.Insert(0, elem);
                _elements.Add(elem);
            }
        }

        public void Add(FrameworkElement elem)
        {
            if (!_elements.Contains(elem))
            {
                _elementCanvas.Children.Add(elem);
               // elem.DataContext = elem.DataContext; // forces propagation of DataContext to children of 'elem' -- apparently not needed when running Debug code from VisStudio
                _elements.Add(elem);
            }
        }

        public void Remove(FrameworkElement elem)
        {
            if (_elements.Contains(elem))
            {
                _elementCanvas.Children.Remove(elem);
                _elements.Remove(elem);
            }
        }
        public void Add(InkStroke s)
        {
            if (!_inkStrokes.Contains(s))
            {
                _elementCanvas.Children.Add(s);
                _inkStrokes.Add(s);
            }
        }

        public void Remove(InkStroke s)
        {
            if (_inkStrokes.Contains(s))
            {
                foreach (var e in _elementCanvas.Children)
                {
                    if (e is InkStrokeElement && (e as InkStrokeElement).InkStroke == s)
                    {
                        _elementCanvas.Children.Remove(e as InkStrokeElement);
                        break;
                    }
                }

                _inkStrokes.Remove(s);
            }
        }

    }
}
