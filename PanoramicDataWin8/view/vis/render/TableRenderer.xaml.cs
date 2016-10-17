using System;
using Windows.UI.Xaml;
using IDEA_common.operations;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class TableRenderer : Renderer, AttributeTransformationViewModelEventHandler
    {
        private DataGrid _dataGrid = new DataGrid();
        public TableRenderer()
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
                ((HistogramOperationViewModel)DataContext).HistogramOperationModel.PropertyChanged -= QueryModel_PropertyChanged;
            }
        }
        void TableRenderer2_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((HistogramOperationViewModel)DataContext).HistogramOperationModel.PropertyChanged += QueryModel_PropertyChanged;
                //mainLabel.Text = ((OperationViewModel)DataContext).OperationModel.VisualizationType.ToString();
                //mainLabel.Text = ((OperationViewModel)DataContext).OperationModel.TaskType.Replace("_", " ").ToString();
            }
        }


        private void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel model = (DataContext as HistogramOperationViewModel).HistogramOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                populateData();
            }
        }
        

        private void populateData()
        {
            IResult resultModel = (DataContext as HistogramOperationViewModel).HistogramOperationModel.Result;
            if (resultModel != null)
            {
                mainGrid.Opacity = 1;
                mainLabel.Opacity = 0;
            }
        }
        public void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            AttributeTransformationViewModelEventHandler inputModelEventHandler = _dataGrid as AttributeTransformationViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.AttributeTransformationViewModelMoved(sender, e, overElement);
            }
        }

        public void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            AttributeTransformationViewModelEventHandler inputModelEventHandler = _dataGrid as AttributeTransformationViewModelEventHandler;
            if (inputModelEventHandler != null)
            {
                inputModelEventHandler.AttributeTransformationViewModelDropped(sender, e, overElement);
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                AttributeTransformationViewModelEventHandler inputModelEventHandler = _dataGrid as AttributeTransformationViewModelEventHandler;
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
