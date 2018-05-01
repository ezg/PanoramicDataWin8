using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace GraphSharp.Controls
{
	public class HighlightTriggeredEventArgs : RoutedEventArgs
	{
		public bool Cancel { get; set; }
		public bool IsPositiveTrigger { get; private set; }

		public HighlightTriggeredEventArgs( RoutedEvent evt, object source, bool isPositiveTrigger )
			// : base( evt, source )  // bcz:
		{
			Cancel = false;
			IsPositiveTrigger = isPositiveTrigger;
		}
	}

	public class HighlightInfoChangedEventArgs : RoutedEventArgs
	{
		public object OldHighlightInfo { get; private set; }
		public object NewHighlightInfo { get; private set; }

		public HighlightInfoChangedEventArgs( RoutedEvent evt, object source, object oldHighlightInfo, object newHighlightInfo )
			// : base( evt, source ) // bcz:
		{
			OldHighlightInfo = oldHighlightInfo;
			NewHighlightInfo = newHighlightInfo;
		}
	}

	public delegate void HighlightInfoChangedEventHandler( object sender, HighlightInfoChangedEventArgs args );

	public delegate void HighlightTriggerEventHandler( object sender, HighlightTriggeredEventArgs args );

	public static class GraphElementBehaviour
	{
        // bcz:
		//public static readonly RoutedEvent HighlightEvent = EventManager.RegisterRoutedEvent( "Highlight", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( GraphElementBehaviour ) );
		//public static void AddHighlightHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.AddHandler( HighlightEvent, handler, false /*bcz: */);
		//}

		//public static void RemoveHighlightHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.RemoveHandler( HighlightEvent, handler );
		//}

		//public static readonly RoutedEvent UnhighlightEvent = EventManager.RegisterRoutedEvent( "Unhighlight", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( GraphElementBehaviour ) );
		//public static void AddUnhighlightHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.AddHandler( UnhighlightEvent, handler, false /*bcz: */);
		//}

		//public static void RemoveUnhighlightHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.RemoveHandler( UnhighlightEvent, handler );
		//}

		//public static readonly RoutedEvent HighlightInfoChangedEvent = EventManager.RegisterRoutedEvent( "HighlightInfoChanged", RoutingStrategy.Bubble, typeof( HighlightInfoChangedEventHandler ), typeof( GraphElementBehaviour ) );
		//public static void AddHighlightInfoChangedHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.AddHandler( HighlightInfoChangedEvent, handler, false /*bcz: */);
		//}

		//public static void RemoveHighlightInfoChangedHandler( DependencyObject d, RoutedEventHandler handler )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e != null )
		//		e.RemoveHandler( HighlightInfoChangedEvent, handler );
  //      }


            // bcz:  this fixes compile errors but loses highlighting
        //internal static readonly RoutedEvent HighlightTriggeredEvent = EventManager.RegisterRoutedEvent("HighlightTriggered", RoutingStrategy.Bubble, typeof(HighlightTriggerEventHandler), typeof(GraphElementBehaviour));
        //public static void AddHighlightTriggeredHandler(DependencyObject d, RoutedEventHandler handler)
        //{
        //    (d as UIElement)?.AddHandler(HighlightTriggeredEvent, handler, false); // bcz:
        //    //UIElement e = d as UIElement;
        //    //if (e != null)
        //    //    e.AddHandler(HighlightTriggeredEvent, handler, false /*bcz: */);
        //}

        //public static void RemoveHighlightTriggeredHandler(DependencyObject d, RoutedEventHandler handler)
        //{
        //    (d as UIElement)?.RemoveHandler(HighlightTriggeredEvent, handler); // bcz:
        //    //UIElement e = d as UIElement;
        //    //if (e != null)
        //    //    e.RemoveHandler(HighlightTriggeredEvent, handler);
        //}


            //bcz: won't compile
  //      public static bool GetHighlightTrigger( DependencyObject obj )
		//{
		//	return (bool)obj.GetValue( HighlightTriggerProperty );
		//}

		//public static void SetHighlightTrigger( DependencyObject obj, bool value )
		//{
		//	obj.SetValue( HighlightTriggerProperty, value );
		//}

		// Using a DependencyProperty as the backing store for HighlightTrigger.  This enables animation, styling, binding, etc...
        // bcz: won't compile
		//public static readonly DependencyProperty HighlightTriggerProperty =
		//	DependencyProperty.RegisterAttached( "HighlightTrigger", typeof( bool ), typeof( GraphElementBehaviour ), new PropertyMetadata( false, null, HighlightTrigger_Coerce ) ); // bcz: UIPropertyMetadata
        //won't compile
		//private static object HighlightTrigger_Coerce( DependencyObject d, object baseValue )
		//{
		//	UIElement e = d as UIElement;
		//	if ( e == null )
		//		return baseValue;

		//	if ( (bool)baseValue == GetHighlightTrigger( d ) )
		//		return baseValue;

		//	var args = new HighlightTriggeredEventArgs( HighlightTriggeredEvent, d, (bool)baseValue );
  //          try
  //          {
  //              e.RaiseEvent(args);
  //          }
  //          catch (Exception ex)
  //          {
  //              Debug.WriteLine("Exception during HighlightTrigger_Coerce - likely the graph is still animating: " + ex);
  //          }

		//	return args.Cancel ? GetHighlightTrigger( d ) : baseValue;
		//}

		public static readonly DependencyProperty IsHighlightedProperty;
		private static readonly DependencyProperty IsHighlightedPropertyKey = // bcz: DependencyPropertyKey
			DependencyProperty.RegisterAttached( "IsHighlighted", typeof( bool ), typeof( GraphElementBehaviour ), new PropertyMetadata( false, IsHighlighted_PropertyChanged ) ); // bcz: RegisterAttachedReadOnly  UIPropertyMetadata



		public static bool GetIsHighlighted( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsHighlightedProperty );
		}

		internal static void SetIsHighlighted( DependencyObject obj, bool value )
		{
			obj.SetValue( IsHighlightedPropertyKey, value );
		}

		// When the IsHighlighted Property changes we should raise the 
		// Highlight and Unhighlight RoutedEvents.
		private static void IsHighlighted_PropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UIElement control = d as UIElement;
			if ( control == null )
				return;

            //bcz:
			//if ( (bool)e.NewValue )
			//	control.RaiseEvent( new RoutedEventArgs( HighlightEvent, d ) );
			//else
			//	control.RaiseEvent( new RoutedEventArgs( UnhighlightEvent, d ) );
		}

		public static readonly DependencyProperty HighlightInfoProperty;
		private static readonly DependencyProperty HighlightInfoPropertyKey =// bcz: DependencyPropertyKey
            DependencyProperty.RegisterAttached( "HighlightInfo", typeof( object ), typeof( GraphElementBehaviour ),// bcz: RegisterAttachedReadOnly
                                                 new PropertyMetadata( null, HighlightInfo_PropertyChanged ) );



		public static object GetHighlightInfo( DependencyObject obj )
		{
			return (object)obj.GetValue( HighlightInfoProperty );
		}

		internal static void SetHighlightInfo( DependencyObject obj, object value )
		{
			obj.SetValue( HighlightInfoPropertyKey, value );
		}

		private static void HighlightInfo_PropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UIElement control = d as UIElement;
			if ( control == null )
				return;

            // bcz:
			// control.RaiseEvent( new HighlightInfoChangedEventArgs( HighlightInfoChangedEvent, d, e.OldValue, e.NewValue ) );
		}

		public static readonly DependencyProperty IsSemiHighlightedProperty;
		private static readonly DependencyProperty  IsSemiHighlightedPropertyKey =// bcz: DependencyPropertyKey
            DependencyProperty.RegisterAttached( "IsSemiHighlighted", typeof( bool ), typeof( GraphElementBehaviour ),// bcz: RegisterAttachedReadOnly
                                                 new PropertyMetadata( false ) );



		public static bool GetIsSemiHighlighted( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsSemiHighlightedProperty );
		}

		internal static void SetIsSemiHighlighted( DependencyObject obj, bool value )
		{
			obj.SetValue( IsSemiHighlightedPropertyKey, value );
		}

		public static readonly DependencyProperty SemiHighlightInfoProperty;
		private static readonly DependencyProperty SemiHighlightInfoPropertyKey =// bcz: DependencyPropertyKey
            DependencyProperty.RegisterAttached( "SemiHighlightInfo", typeof( object ), typeof( GraphElementBehaviour ), new PropertyMetadata( null ) );// bcz: RegisterAttachedReadOnly



        public static object GetSemiHighlightInfo( DependencyObject obj )
		{
			return (object)obj.GetValue( SemiHighlightInfoProperty );
		}

		internal static void SetSemiHighlightInfo( DependencyObject obj, object value )
		{
			obj.SetValue( SemiHighlightInfoPropertyKey, value );
		}

        public static readonly DependencyProperty LayoutInfoProperty =
            DependencyProperty.RegisterAttached("LayoutInfo", typeof(object), typeof(GraphElementBehaviour), new PropertyMetadata(null)); //  new UIPropertyMetadata( null ) );

		public static object GetLayoutInfo( DependencyObject obj )
		{
			return (object)obj.GetValue( LayoutInfoProperty );
		}

		public static void SetLayoutInfo( DependencyObject obj, object value )
		{
			obj.SetValue( LayoutInfoProperty, value );
		}

		static GraphElementBehaviour()
		{
			IsSemiHighlightedProperty = IsSemiHighlightedPropertyKey; // bcz: 
			SemiHighlightInfoProperty = SemiHighlightInfoPropertyKey;
			HighlightInfoProperty = HighlightInfoPropertyKey;
			IsHighlightedProperty = IsHighlightedPropertyKey;
		}
	}
}