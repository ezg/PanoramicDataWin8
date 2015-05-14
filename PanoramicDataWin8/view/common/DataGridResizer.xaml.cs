using PanoramicDataWin8.view.vis;
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
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class DataGridResizer : UserControl
    {
        private Pt _startDrag = new Point(0, 0);
        private Pt _currentFromInkableScene = new Point(0, 0);

        public bool _isResizer = false;
        public bool IsResizer
        {
            get
            {
                return _isResizer;
            }
            set
            {
                _isResizer = value;
                if (!_isResizer)
                {
                    gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                }
            }
        }

        public DataGridResizer()
        {
            this.InitializeComponent();
            this.DataContextChanged += DataGridResizer_DataContextChanged;
        }

        void DataGridResizer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                HeaderObject ho = (DataContext as HeaderObject);
                if (ho.IsLast && _isResizer)
                {
                    gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                }

                if (!_isResizer)
                {
                    gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;

                    if (!ho.IsFirst || (!ho.IsFirst && !ho.IsLast && ho.NrElements > 1))
                    {
                        this.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            e.Handled = true;
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            _startDrag = e.GetCurrentPoint(inkableScene).Position;

            this.PointerMoved += Grid_PointerMoved;
            this.PointerReleased += Grid_PointerReleased;
        }


        void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            Pt fromInkableScene = e.GetCurrentPoint(inkableScene).Position;

            Vec v = _startDrag - fromInkableScene;

            HeaderObject ho = (DataContext as HeaderObject);
            ho.Width -= v.X;
            ho.FireResized();
            _startDrag = fromInkableScene;
        }

        void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            ReleasePointerCapture(e.Pointer);

            this.PointerMoved -= Grid_PointerMoved;
            this.PointerReleased -= Grid_PointerReleased;
        }

        public void Highlight()
        {
            gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush;
        }

        public void UnHighlight()
        {
            if (!_isResizer)
            {
                gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
            }
            else
            {
                HeaderObject ho = (DataContext as HeaderObject);
                if (ho.IsLast)
                {
                    gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                }
                else
                {
                    gapGrid.Background = Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush;
                }
            }
        }
    }
}
