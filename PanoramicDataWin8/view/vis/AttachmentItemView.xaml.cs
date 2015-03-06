using PanoramicData.utils;
using PanoramicDataWin8.utils;
using PanoramicData.view.inq;
using PanoramicDataWin8.model.view;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class AttachmentItemView : UserControl, IScribbable
    {
        public AttachmentItemView()
        {
            this.InitializeComponent();
            this.PointerPressed += AttachmentItemView_PointerPressed;
        }

        void AttachmentItemView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsRightButtonPressed)
                {
                    var model = (DataContext as AttachmentItemViewModel);
                    model.AttachmentHeaderViewModel.RemovedTriggered(model);
                    e.Handled = true;
                }
            }
        }

        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                var model = (DataContext as AttachmentItemViewModel);
                return new Rct(model.Position, model.Size).GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get
            {
                return new List<IScribbable>();
            }
        }
    }
}
