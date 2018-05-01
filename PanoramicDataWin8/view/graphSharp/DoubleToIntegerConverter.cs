using System;
using Windows.UI.Xaml.Data;

namespace GraphSharp.Sample
{
	public class IntegerToDoubleConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert( object value, Type targetType, object parameter, string culture )
		{
			double r = (int)value;
			return r;
		}

		public object ConvertBack( object value, Type targetType, object parameter,string culture )
		{
			return Math.Round( (double)value );
		}

		#endregion
	}
}
