using System;
using Windows.UI.Xaml;
using IDEA_common.operations;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class TableRenderer : Renderer, InputFieldViewModelEventHandler
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
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated -= QueryModel_QueryModelUpdated;
                ((VisualizationViewModel)DataContext).QueryModel.PropertyChanged -= QueryModel_PropertyChanged;
            }
        }

        void TableRenderer2_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((VisualizationViewModel)DataContext).QueryModel.QueryModelUpdated += QueryModel_QueryModelUpdated;
                ((VisualizationViewModel)DataContext).QueryModel.PropertyChanged += QueryModel_PropertyChanged;
                //mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.VisualizationType.ToString();
                //mainLabel.Text = ((VisualizationViewModel)DataContext).QueryModel.TaskType.Replace("_", " ").ToString();
            }
        }


        private void QueryModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            QueryModel model = (DataContext as VisualizationViewModel).QueryModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Result))
            {
                populateData();
            }
        }

        void QueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
        }

        private void populateData()
        {
            IResult resultModel = (DataContext as VisualizationViewModel).QueryModel.Result;
            if (resultModel != null)
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
