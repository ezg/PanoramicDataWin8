using System;
using System.Linq;
using GraphSharp.Converters;
using GraphSharp.Helpers;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace GraphSharp.Controls
{
	public class EdgeControl : Control, IPoolObject, IDisposable
	{
		#region Dependency Properties

		public static readonly DependencyProperty SourceProperty = DependencyProperty.Register( "Source",
																							   typeof( VertexControl ),
																							   typeof( EdgeControl ),
																							   new PropertyMetadata( null, SourcePropertyChanged ) ); // bcz: new UIPropertyMetadata( null ) );

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register( "Target",
																							   typeof( VertexControl ),
																							   typeof( EdgeControl ),
                                                                                               new PropertyMetadata(null, TargetPropertyChanged)); // bcz: new UIPropertyMetadata( null ) );

        public static readonly DependencyProperty RoutePointsProperty = DependencyProperty.Register( "RoutePoints",
																									typeof( Point[] ),
																									typeof( EdgeControl ),
                                                                                               new PropertyMetadata(null, RoutedPointsPropertyChanged)); // bcz: new UIPropertyMetadata( null ) );

        public static readonly DependencyProperty EdgeProperty = DependencyProperty.Register( "Edge", 
                                                                                             typeof( object ),
																							 typeof( EdgeControl ),
																							 new PropertyMetadata( null ) );

        public static readonly DependencyProperty PathGeometryProperty = DependencyProperty.Register("PathGeometry",
                                                                                             typeof(Geometry),
                                                                                             typeof(EdgeControl),
                                                                                             new PropertyMetadata(null));

        //bcz:
        //public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner( typeof(EdgeControl),
        //                                                                                       new PropertyMetadata(2.0)); // bcz: new UIPropertyMetadata(2.0 ) );
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double),
                                                                                             typeof(EdgeControl),
                                                                                             new PropertyMetadata(5.0));


        private void positionChangedCallback(DependencyObject d, DependencyProperty e)
        {
            var s = GetValue(SourceProperty) as VertexControl;
            var t = GetValue(TargetProperty) as VertexControl;
            var r = GetValue(RoutePointsProperty) as Point[];

            if (s != null && t != null)
            {
                var f = updateRoutePoints(s, t, r);
                SetValue(PathGeometryProperty, f);
            }
        }
        private static void RoutedPointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = d.GetValue(SourceProperty) as VertexControl;
            var t = d.GetValue(TargetProperty) as VertexControl;
            var r = d.GetValue(RoutePointsProperty) as Point[];

            if (s != null && t != null)
            {
                var f = updateRoutePoints(s, t, r);
                d.SetValue(PathGeometryProperty, f);
            }
        }
        private static void TargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var edge = d as EdgeControl;
            var s = d.GetValue(SourceProperty) as VertexControl;
            var t = d.GetValue(TargetProperty) as VertexControl;
            var r = d.GetValue(RoutePointsProperty) as Point[];
            
            t.RegisterPropertyChangedCallback(Canvas.LeftProperty, edge.positionChangedCallback);
            t.RegisterPropertyChangedCallback(Canvas.TopProperty, edge.positionChangedCallback);

            if (s != null && t != null)
            {
                var f = updateRoutePoints(s, t, r);
                d.SetValue(PathGeometryProperty, f);
            }
        }
        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var edge = d as EdgeControl;
            var s = d.GetValue(SourceProperty) as VertexControl;
            var t = d.GetValue(TargetProperty) as VertexControl;
            var r = d.GetValue(RoutePointsProperty) as Point[];

            s.RegisterPropertyChangedCallback(Canvas.LeftProperty, edge.positionChangedCallback);
            s.RegisterPropertyChangedCallback(Canvas.TopProperty, edge.positionChangedCallback);

            if (s != null && t != null)
            {
                var f = updateRoutePoints(s, t, r);
                d.SetValue(PathGeometryProperty, f);
            }
        }

        private static Geometry updateRoutePoints(VertexControl s, VertexControl t, Point[] routeInformation)
        {
            var sourceSize = new Vector(s.ActualWidth, s.ActualHeight);
            var targetSize = new Vector(t.ActualWidth, t.ActualHeight);
            var sourcePos = new Point(Canvas.GetLeft(s), Canvas.GetTop(s)) + sourceSize/2;
            var targetPos = new Point(Canvas.GetLeft(t), Canvas.GetTop(t)) + targetSize / 2;

            //get the route informations
            bool hasRouteInfo = routeInformation != null && routeInformation.Count() > 0 ;

            //
            // Create the path
            //
            var p1 = GraphConverterHelper.CalculateAttachPoint(sourcePos, sourceSize, (hasRouteInfo ? routeInformation[0] : targetPos));
            var p2 = GraphConverterHelper.CalculateAttachPoint(targetPos, targetSize, (hasRouteInfo ? routeInformation[routeInformation.Length - 1] : sourcePos));


           var segments = new PathSegment[1 + (hasRouteInfo ? routeInformation.Length : 0)];
            if (hasRouteInfo)
                //append route points
                for (int i = 0; i < routeInformation.Length; i++)
                    segments[i] = new LineSegment() { Point = routeInformation[i] }; // bcz: new LineSegment( routeInformation[i], true );

            Point pLast = (hasRouteInfo ? routeInformation[routeInformation.Length - 1] : p1);
            Vector v = pLast - p2;
            v = v / v.Length * 5;
            Vector n = new Vector(-v.Y, v.X) * 0.3;

            segments[segments.Length - 1] = new LineSegment() { Point = p2 + v }; //  new LineSegment( p2 + v, true );

            var pfc = new PathFigureCollection(); // bcz: (2);
            var segCollection = new PathSegmentCollection(); // bcz:
            segments.ToList().ForEach((ss) => segCollection.Add(ss));  // bcz :
            pfc.Add(new PathFigure() { StartPoint = p1, Segments = segCollection, IsClosed = false }); // new PathFigure( p1, segments, false ) );
            var segCollection2 = new PathSegmentCollection();  // bcz:
            segCollection2.Add(new LineSegment() { Point = p2 + v - n }); // bcz:
            segCollection2.Add(new LineSegment() { Point = p2 + v + n }); //bcz:
            pfc.Add(new PathFigure() { StartPoint = p2, Segments = segCollection2, IsClosed = true });
            var pg = new PathGeometry();
            pg.Figures = pfc;
            return pg;
        }

        #endregion

        #region Properties
        public VertexControl Source
		{
			get { return (VertexControl)GetValue( SourceProperty ); }
			internal set { SetValue( SourceProperty, value ); }
		}

		public VertexControl Target
		{
			get { return (VertexControl)GetValue( TargetProperty ); }
			internal set { SetValue( TargetProperty, value ); }
		}

		public Point[] RoutePoints
		{
			get { return (Point[])GetValue( RoutePointsProperty ); }
			set { SetValue( RoutePointsProperty, value ); }
		}

		public object Edge
		{
			get { return GetValue( EdgeProperty ); }
			set { SetValue( EdgeProperty, value ); }
        }
        public object PathGeometry
        {
            get { return GetValue(PathGeometryProperty); }
            set { SetValue(PathGeometryProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion
        
		static EdgeControl()
		{
			//override the StyleKey
            //bcz:
			//DefaultStyleKeyProperty.OverrideMetadata( typeof( EdgeControl ), new FrameworkPropertyMetadata( typeof( EdgeControl ) ) );
		}

		#region IPoolObject Members

		public void Reset()
		{
			Edge = null;
			RoutePoints = null;
			Source = null;
			Target = null;
		}

		public void Terminate()
		{
			//nothing to do, there are no unmanaged resources
		}

		public event DisposingHandler Disposing;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if ( Disposing != null )
				Disposing( this );
		}

		#endregion
	}
}