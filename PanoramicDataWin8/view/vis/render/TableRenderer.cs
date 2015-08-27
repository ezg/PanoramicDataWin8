using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.style;
using WinRTXamlToolkit.Controls.Extensions;

namespace PanoramicDataWin8.view.vis.render
{
    public class TableRenderer : Renderer, InputFieldViewModelEventHandler
    {
        private DataGrid _dataGrid = new DataGrid();

        public TableRenderer()
        {
            _dataGrid.CanDrag = false;
            _dataGrid.CanReorder = true;
            _dataGrid.CanResize = true;
            _dataGrid.CanExplore = true;
            this.Loaded += TableRenderer_Loaded;
        }

        void TableRenderer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Content = _dataGrid;
        }

        public override void Dispose()
        {
            base.Dispose();
            _dataGrid.Dispose();
        }
        public void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelMoved(sender, e, overElement);
            }
        }

        public void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
                if (inputModelEventHandler != null)
                {
                    return inputModelEventHandler.BoundsGeometry;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
