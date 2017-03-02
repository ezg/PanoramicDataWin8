using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.utils
{
    public class ClipToBounds
    {
        public static bool GetClipToBounds(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClipToBoundsProperty);
        }

        public static void SetClipToBounds(DependencyObject obj, bool value)
        {
            obj.SetValue(ClipToBoundsProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClipToBounds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipToBoundsProperty =
            DependencyProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(ClipToBounds), new PropertyMetadata(false, ClipToBoundsChanged));

        private static void ClipToBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //use FrameworkElement because it is the highest abstraction that contains safe size
            //UIElement does not contain save size data
            var element = d as FrameworkElement;
            if (element != null)
            {
                element.Loaded += (s, evt) => ClipElement(element);
                element.SizeChanged += (s, evt) => ClipElement(element);
            }
        }

        private static void ClipElement(FrameworkElement element)
        {
            if (GetClipToBounds(element))
            {
                var clip = new RectangleGeometry { Rect = new Rect(0, 0, element.ActualWidth, element.ActualHeight) };
                element.Clip = clip;
            }
        }
    }
}
