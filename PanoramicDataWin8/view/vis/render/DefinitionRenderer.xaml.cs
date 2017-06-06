using PanoramicDataWin8.view.inq;
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
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view.operation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.vis.render
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefinitionRenderer : Renderer, IScribbable
    {
        public DefinitionRenderer()
        {
            this.InitializeComponent();
        }

        public List<IScribbable> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as DefinitionOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }
    }
}
