using System;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    public class OperationTypeModelEventArgs : EventArgs
    {
        public OperationTypeModelEventArgs(Rct bounds)
        {
            Bounds = bounds;
        }

        public Rct Bounds { get; set; }
    }
}