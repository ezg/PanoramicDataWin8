using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.view.vis.render
{
    public class TableRenderer : Renderer, AttributeViewModelEventHandler
    {
        private DataGrid _dataGrid = new DataGrid();

        public TableRenderer()
        {
            _dataGrid.CanDrag = false;
            _dataGrid.CanReorder = true;
            _dataGrid.CanResize = true;
            _dataGrid.CanExplore = true;
            this.Content = _dataGrid;
        }

        public override void Dispose()
        {
            base.Dispose();
            _dataGrid.Dispose();
        }
        public void AttributeViewModelMoved(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            AttributeViewModelEventHandler attributeViewModelEventHandler = _dataGrid as AttributeViewModelEventHandler;
            if (attributeViewModelEventHandler != null)
            {
                attributeViewModelEventHandler.AttributeViewModelMoved(sender, e, overElement);
            }
        }

        public void AttributeViewModelDropped(AttributeViewModel sender, AttributeViewModelEventArgs e, bool overElement)
        {
            AttributeViewModelEventHandler attributeViewModelEventHandler = _dataGrid as AttributeViewModelEventHandler;
            if (attributeViewModelEventHandler != null)
            {
                attributeViewModelEventHandler.AttributeViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                AttributeViewModelEventHandler attributeViewModelEventHandler = _dataGrid as AttributeViewModelEventHandler;
                if (attributeViewModelEventHandler != null)
                {
                    return attributeViewModelEventHandler.BoundsGeometry;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
