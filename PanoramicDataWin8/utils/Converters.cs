using PanoramicData.model.data;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PanoramicData.utils
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

    public class SortModeAscToVisibilityConverter : SimpleValueConverter<SortMode, Visibility>
    {
        protected override Visibility ConvertBase(SortMode input)
        {
            return input == SortMode.Asc ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class SortModeDescToVisibilityConverter : SimpleValueConverter<SortMode, Visibility>
    {
        protected override Visibility ConvertBase(SortMode input)
        {
            return input == SortMode.Desc ? Visibility.Visible : Visibility.Collapsed;
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
        public AttributeOperationModel AttributeOperationModel { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (AttributeOperationModel == null)
            {
                return "";
            }
            if (value != null)
            {
                QueryResultItemModel model = (value as QueryResultItemModel);
                if (model.AttributeValues.ContainsKey(AttributeOperationModel))
                {
                    return model.AttributeValues[AttributeOperationModel].ShortStringValue;
                }
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
