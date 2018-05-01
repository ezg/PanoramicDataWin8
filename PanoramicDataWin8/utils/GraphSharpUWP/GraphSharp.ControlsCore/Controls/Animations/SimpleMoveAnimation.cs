using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace GraphSharp.Controls.Animations
{
    public class SimpleMoveAnimation : IAnimation
    {
        #region IAnimation Members

        public void Animate( IAnimationContext context, Control control, double x, double y, TimeSpan duration )
        {
            if ( !double.IsNaN( x ) )
            {
                double from = GraphCanvas.GetX( control );
                from = double.IsNaN( from ) ? 0.0 : from;

                //create the animation for the horizontal position
                var animationX = new DoubleAnimation() { From = from, To = x, Duration = duration, FillBehavior = FillBehavior.HoldEnd };
                    //from,                    / bcz:
                    //x,
                    //duration,
                    //FillBehavior.HoldEnd );
                // bcz:
                    var story = new Storyboard();
                    story.Children.Add(animationX);
                    Storyboard.SetTarget(animationX, control);
                    Storyboard.SetTargetProperty(animationX, "(Canvas.Left)"); //  GraphCanvas.XProperty.ToString());  // bcz:
                
                    story.Completed += (s,e) =>
                    {
                        control.SetValue(Canvas.LeftProperty, x); // bcz:  GraphCanvas.XProperty, x);
                    };
                    story.Begin();
                // bcz:
                //animationX.Completed += ( s, e ) =>
                //{
                //    control.BeginAnimation( GraphCanvas.XProperty, null );
                //    control.SetValue( GraphCanvas.XProperty, x );
                //};
                //control.BeginAnimation( GraphCanvas.XProperty, animationX, HandoffBehavior.Compose );
            }
            if ( !double.IsNaN( y ) )
            {
                double from = GraphCanvas.GetY( control );
                from = ( double.IsNaN( from ) ? 0.0 : from );

                //create an animation for the vertical position
                var animationY = new DoubleAnimation() { From = from, To = y, Duration = duration, FillBehavior = FillBehavior.HoldEnd };
                //from, y,   //bcz:
                //duration,
                //FillBehavior.HoldEnd );
                // bcz:
                    var story = new Storyboard();
                    story.Children.Add(animationY);
                    Storyboard.SetTarget(animationY, control);
                    Storyboard.SetTargetProperty(animationY, "(Canvas.Top)"); // bcz: GraphCanvas.YProperty.ToString());
                    story.Completed += (s,e) =>
                    {
                        control.SetValue(Canvas.TopProperty, y); // bcz:  GraphCanvas.YProperty, y);
                    };
                    story.Begin();
                // bcz:
                //animationY.Completed += ( s, e ) =>
                //{
                //    control.BeginAnimation( GraphCanvas.YProperty, null );
                //    control.SetValue( GraphCanvas.YProperty, y );
                //};
                //control.BeginAnimation( GraphCanvas.YProperty, animationY, HandoffBehavior.Compose );
            }
        }

        #endregion
    }
}