using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.utils
{
    public class StringToVisibilityConverter : SimpleValueConverter<string, Visibility>
    {
        protected override Visibility ConvertBase(string input)
        {
            return (input == null || input == "") ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class BooleanToVisibilityConverter : SimpleValueConverter<bool, Visibility>
    {
        protected override Visibility ConvertBase(bool input)
        {
            return input ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class IsTableVisualizationTypeConverter : SimpleValueConverter<VisualizationType, bool>
    {
        protected override bool ConvertBase(VisualizationType input)
        {
            return input == VisualizationType.table;
        }
    }
    
    public class InverseBooleanToVisibilityConverter : SimpleValueConverter<bool, Visibility>
    {
        protected override Visibility ConvertBase(bool input)
        {
            return input ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class NullToVisibilityConverter : SimpleValueConverter<object, Visibility>
    {
        protected override Visibility ConvertBase(object input)
        {
            return input == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
    

    public class VisualizationTypeToMarginConverter : SimpleValueConverter<VisualizationType, Thickness>
    {
        public VisualizationType TargetVisualizationType { get; set; }
        public Thickness TargetThickness { get; set; }

        protected override Thickness ConvertBase(VisualizationType input)
        {
            return input == TargetVisualizationType ? TargetThickness : new Thickness(0);
        }
    }

    public class VisualizationTypeToVisibilityConverter : SimpleValueConverter<VisualizationType, Visibility>
    {
        public VisualizationType TargetVisualizationType { get; set; }

        protected override Visibility ConvertBase(VisualizationType input)
        {
            return input == TargetVisualizationType ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class TextValueConverter : IValueConverter
    {
        public AttributeTransformationModel AttributeTransformationModel { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (AttributeTransformationModel == null)
            {
                return "";
            }
            if (value != null)
            {
                ResultItemModel model = (value as ResultItemModel);
                /*if (model.AttributeValues.ContainsKey(AttributeTransformationViewModel))
                {
                    return model.AttributeValues[AttributeTransformationViewModel].ShortStringValue;
                }*/
                return ""; 
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class MyUri
    {
        public MyUri(string str) { Uri = new Uri(str); }
        public Uri Uri { get; set; }
    }
    public class ObjectToUriConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MyUri) // data is an image
                return (value as MyUri).Uri;
            throw new NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ObjectToStringConverter : IValueConverter
    {
        static public FrameworkElement LastHit = null;
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            string text = "";
            if (value is Tuple<int, object>)
                text = (value as Tuple<int, object>).Item2.ToString() + " (" + (value as Tuple<int, object>).Item1 + ")";
            else if (value is Tuple<int, double>)
                text = "avg=" + (value as Tuple<int, double>).Item2;
            else text = value.ToString();
            return text;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ObjectToTextAlignmentConverter : IValueConverter
    {
        static public FrameworkElement LastHit = null;
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return value is IDEA_common.range.PreProcessedString || value is string ? TextAlignment.Left : TextAlignment.Right;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ObjectToAlignmentConverter : IValueConverter
    {
        static public FrameworkElement LastHit = null;
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return value is IDEA_common.range.PreProcessedString || value is string ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
