using System;
using GraphSharp.Algorithms.Layout.Compound;
using Windows.UI.Xaml.Data;

namespace GraphSharp.Sample
{
    public class PocVertexToLayoutModeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var vertex = value as string;
            if (vertex == "2" || vertex == "3")
                return CompoundVertexInnerLayoutType.Fixed;
            else
                return CompoundVertexInnerLayoutType.Automatic;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
