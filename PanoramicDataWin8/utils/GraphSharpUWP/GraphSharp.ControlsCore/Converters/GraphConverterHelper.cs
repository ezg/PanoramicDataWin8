using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Windows.Foundation;

namespace GraphSharp.Converters
{
	static class GraphConverterHelper
	{
		public static Point CalculateAttachPoint( Point s, Vector sourceSize, Point t )
		{
			double[] sides = new double[4];
			sides[0] = ( s.X - sourceSize.X / 2.0 - t.X ) / ( s.X - t.X );
			sides[1] = ( s.Y - sourceSize.Y / 2.0 - t.Y ) / ( s.Y - t.Y );
			sides[2] = ( s.X + sourceSize.X / 2.0 - t.X ) / ( s.X - t.X );
			sides[3] = ( s.Y + sourceSize.Y / 2.0 - t.Y ) / ( s.Y - t.Y );

			double fi = 0;
			for ( int i = 0; i < 4; i++ )
			{
				if ( sides[i] <= 1 )
					fi = Math.Max( fi, sides[i] );
			}

			return t + fi * ( s - t );
		}
	}
}