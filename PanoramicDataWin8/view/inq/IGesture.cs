using System;
using System.Collections.Generic;
namespace PanoramicDataWin8.view.inq
{
    public interface IGesture
    {
        bool Recognize(InkStroke inkStroke);
    }

    public abstract class HitGesture : IGesture
    {
        protected IList<IScribbable> _hitScribbables;
        public IList<IScribbable> HitScribbables
        {
            get { return _hitScribbables; }
        }

        public virtual bool Recognize(InkStroke inkStroke)
        {
            throw new System.NotImplementedException();
        }
    }
}
