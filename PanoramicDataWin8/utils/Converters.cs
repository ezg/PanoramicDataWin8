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
}
