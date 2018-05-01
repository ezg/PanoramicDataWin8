using System;
using Windows.UI.Xaml.Controls;

namespace GraphSharp.Controls
{
    public abstract class TransitionBase : ITransition
    {
        #region ITransition Members

        public void Run( IAnimationContext context, Control control, TimeSpan duration )
        {
            Run( context, control, duration, null );
        }

        public abstract void Run( IAnimationContext context,
            Control control,
            TimeSpan duration,
            Action<Control> endMethod );

        #endregion
    }
}
