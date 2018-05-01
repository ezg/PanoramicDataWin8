using System;
using System.Text;
using Windows.UI.Xaml.Data;

namespace GraphSharp.Converters
{
	public class PointArrayToString : IValueConverter
	{
		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, string language)
		{
			var points = value as Point[];

			if ( points == null )
				return string.Empty;
			var sb = new StringBuilder();
			foreach ( var point in points )
			{
				sb.AppendLine( point.ToString() );
			}
			return sb;
		}

		public object ConvertBack( object value, Type targetType, object parameter, string language )
		{
			throw new NotImplementedException();
		}

        #endregion
    }
}