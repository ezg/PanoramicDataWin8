using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using PanoramicDataWin8.model.view.operation;


namespace PanoramicDataWin8.view.vis.render
{
    public class Renderer : UserControl 
    {
        public virtual void Dispose() { }
        public virtual void StartSelection(Point point) { }
        public virtual void MoveSelection(Point point) { }
        public virtual bool EndSelection()
        {
            return false;
        }

        public virtual void Refactor(string oldName, string newName)
        {

        }
        public Renderer()
        {
            MinWidth  = OperationViewModel.MIN_WIDTH;
            MinHeight = OperationViewModel.MIN_HEIGHT;
        }
    }
}