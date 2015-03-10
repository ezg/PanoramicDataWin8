using PanoramicData.model.data;
using PanoramicData.controller.data;
using PanoramicDataWin8.view.vis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Runtime.InteropServices;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class DataGridCell : UserControl
    {
        public static readonly DependencyProperty HeaderObjectProperty =
            DependencyProperty.Register(
                "HeaderObject", 
                typeof(HeaderObject),
                typeof(DataGridCell), 
                new PropertyMetadata(
                     null, 
                     new PropertyChangedCallback(OnHeaderObjectPropertyChanged))
            );

        private static void OnHeaderObjectPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as DataGridCell).update();
        }

        public HeaderObject HeaderObject
        {
            get { return (HeaderObject)GetValue(HeaderObjectProperty); }
            set { SetValue(HeaderObjectProperty, value); }
        }

        public DataGridCell()
        {
            this.InitializeComponent();
            this.DataContextChanged += DataGridCell_DataContextChanged;
        }
        void DataGridCell_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            update();
        }

        private void update()
        {
            try
            {
                if (DataContext != null)
                {
                    DataWrapper<QueryResultItemModel> dataWrapper = DataContext as DataWrapper<QueryResultItemModel>;
                    dataWrapper.PropertyChanged += dataWrapper_PropertyChanged;
                }
                updateValue();

                if (HeaderObject != null)
                {
                    HeaderObject.PropertyChanged += HeaderObject_PropertyChanged;
                    updateWidth();
                }
            }
            catch (COMException e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        void HeaderObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateWidth();
        }

        void dataWrapper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateValue();
        }

        private void updateValue()
        {
            DataWrapper<QueryResultItemModel> dataWrapper = DataContext as DataWrapper<QueryResultItemModel>;
            if (dataWrapper!= null && dataWrapper.Data != null && HeaderObject != null && HeaderObject.AttributeViewModel != null)
            {
                QueryResultItemModel model = dataWrapper.Data;
                if (model.AttributeValues.ContainsKey(HeaderObject.AttributeViewModel.AttributeOperationModel))
                {
                    textBlock.Text = model.AttributeValues[HeaderObject.AttributeViewModel.AttributeOperationModel].ShortStringValue;
                    return;
                }
            }
            textBlock.Text = "";
        }
        private void updateWidth()
        {
            this.Width = HeaderObject.Width + 4;
        }
    }
}
