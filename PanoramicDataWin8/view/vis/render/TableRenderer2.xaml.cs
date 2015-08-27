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
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.vis.render;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class TableRenderer2 : Renderer, InputFieldViewModelEventHandler
    {
        private DataGrid _dataGrid = new DataGrid();
        public TableRenderer2()
        {
            _dataGrid.CanDrag = false;
            _dataGrid.CanReorder = true;
            _dataGrid.CanResize = true;
            _dataGrid.CanExplore = true;
            this.InitializeComponent();
            this.Loaded += TableRenderer2_Loaded;
            this.DataContextChanged += TableRenderer2_DataContextChanged;
        }

        void TableRenderer2_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainGrid.Children.Add(_dataGrid);
        }

        public override void Dispose()
        {
            base.Dispose();
            _dataGrid.Dispose();
            if (DataContext != null)
            {
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated -= QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated -= resultModel_ResultModelUpdated;
            }
        }

        void TableRenderer2_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
                ResultModel resultModel = ((VisualizationViewModel)DataContext).QueryModel.ResultModel;
                resultModel.ResultModelUpdated += resultModel_ResultModelUpdated;
                //mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.VisualizationType.ToString();
                //mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskType.Replace("_", " ").ToString();
            }
        }

        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
        }

        void resultModel_ResultModelUpdated(object sender, EventArgs e)
        {
            populateData();
        }

        private void populateData()
        {
            ResultModel resultModel = (DataContext as VisualizationViewModel).QueryModel.ResultModel;
            if (resultModel.ResultItemModels.Count > 0)
            {
                mainGrid.Opacity = 1;
                mainLabel.Opacity = 0;
            }
        }
        public void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelMoved(sender, e, overElement);
            }
        }

        public void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.InputFieldViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                InputFieldViewModelEventHandler inputModelEventHandler = _dataGrid as InputFieldViewModelEventHandler;
                if (inputModelEventHandler != null)
                {
                    return inputModelEventHandler.BoundsGeometry;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
