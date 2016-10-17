using System;
using IDEA_common.operations;

namespace PanoramicDataWin8.controller.data
{
    public class JobEventArgs : EventArgs
    {
        public IResult Result { get; set; }
    }
}