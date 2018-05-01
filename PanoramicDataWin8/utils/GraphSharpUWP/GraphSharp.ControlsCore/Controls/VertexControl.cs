using System.Windows;
using GraphSharp.Helpers;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphSharp.Controls
{
	/// <summary>
	/// Logical representation of a vertex.
	/// </summary>
	public class VertexControl : Control, IPoolObject, IDisposable
	{
		public object Vertex
		{
			get { return GetValue( VertexProperty ); }
			set { SetValue( VertexProperty, value ); }
		}

		public static readonly DependencyProperty VertexProperty =
			DependencyProperty.Register( "Vertex", typeof( object ), typeof( VertexControl ), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );


        public static readonly DependencyProperty VBrushProperty =
            DependencyProperty.Register("VBrush", typeof(Brush), typeof(VertexControl), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public static readonly DependencyProperty VThicknessProperty =
            DependencyProperty.Register("VThickness", typeof(Thickness), typeof(VertexControl), new PropertyMetadata(null));// bcz: new UIPropertyMetadata( null ) );
        public GraphCanvas RootCanvas
        {
            get { return (GraphCanvas)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootCanvas", typeof(GraphCanvas), typeof(VertexControl), new PropertyMetadata(null));// bcz: new UIPropertyMetadata(null));

        public Brush VBrush
        {
            get { return (Brush)GetValue(VBrushProperty); }
            set { SetValue(VBrushProperty, value); }
        }
        public Thickness VThickness
        {
            get { return (Thickness)GetValue(VThicknessProperty); }
            set { SetValue(VThicknessProperty, value); }
        }
        static VertexControl()
		{
			//override the StyleKey Property
			//bcz: DefaultStyleKeyProperty.OverrideMetadata( typeof( VertexControl ), new FrameworkPropertyMetadata( typeof( VertexControl ) ) );
		}

		#region IPoolObject Members

		public void Reset()
		{
			Vertex = null;
		}

		public void Terminate()
		{
			//nothing to do, there are no unmanaged resources
		}

		public event DisposingHandler Disposing;

		public void Dispose()
		{
			if ( Disposing != null )
				Disposing( this );
		}

		#endregion
	}
}