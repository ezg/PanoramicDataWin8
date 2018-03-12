using GeoAPI.Geometries;
using IDEA_common.catalog;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using IDEA_common.operations.rawdata;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.operation.computational;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using static PanoramicDataWin8.view.vis.render.RawDataColumn;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class AttributeRenderer : Renderer, IScribbable
    {
        public ObservableCollection<RawDataColumnModel> Records { get; set; } = new ObservableCollection<RawDataColumnModel>();

        DispatcherTimer _keyboardTimer = new DispatcherTimer();
        public AttributeRenderer()
        {
            this.DataContextChanged += dataContextChanged;
            this.InitializeComponent();
            Loaded += AttributeRenderer_Loaded;
        }
        AttributeOperationViewModel Model => (DataContext as AttributeOperationViewModel);
        void AttributeRenderer_Loaded(object sender, RoutedEventArgs args)
        {
            var cp = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollBar>(this);
            if (cp != null)
            {
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.Background = new SolidColorBrush(Colors.DarkGray);
                cp.Margin = new Thickness(0, 0, 2, 0);
                Grid.SetColumn(cp, 0);
                var cp2 = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollContentPresenter>(this);
                Grid.SetColumn(cp2, 1);
            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Model != null && e.PropertyName == Model.OperationModel.GetPropertyName(() => Model.OperationModel.Result))
            {
                loadRawDataResult();
            }
        }
        void dataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Model.OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                Model.OperationModel.PropertyChanged += OperationModel_PropertyChanged;
                
                loadRawDataResult();
                MainViewController.Instance.MainPage.clearAndDisposeMenus();
            }
        }

        void loadRawDataResult()
        {
            var result = Model.AttributeOperationModel.Result;
            if (result != null)
            {
                var attributeModel = this.Model.AttributeOperationModel.GetAttributeModel();
              //  ((attributeModel.FuncModel as AttributeFuncModel.AttributeCodeFuncModel).Data as Dictionary<List<object>,object>).Clear();
                Records = new ObservableCollection<RawDataColumn.RawDataColumnModel>();

                var primaryKeyData = new List<List<object>>();
                foreach (var atm in Model.AttributeOperationModel.AttributeTransformationModelParameters)
                {
                    if (atm.AttributeModel != attributeModel)
                    {
                        var samples = (result as RawDataResult).Samples[atm.AttributeModel.RawName];
                        primaryKeyData.Add(samples);
                    }
                }
                if (primaryKeyData.Count > 0)
                {
                    for (int sampleIndex = 0; sampleIndex < (result as RawDataResult).Samples.Count(); sampleIndex++)
                    {
                        var s = (result as RawDataResult).Samples[Model.AttributeOperationModel.AttributeTransformationModelParameters[sampleIndex].AttributeModel.RawName];
                        if (s.Count > 0)
                            loadRecordsAsync(
                                s,
                                Model.AttributeOperationModel.AttributeTransformationModelParameters[sampleIndex],
                                sampleIndex + 1 == (result as RawDataResult).Samples.Count(),
                                Model.AttributeOperationModel.AttributeTransformationModelParameters.Select((atm) => atm.AttributeModel).Where((am) => am != attributeModel).ToList(),
                                primaryKeyData);
                    }
                }
                else this.xListView.Children.Clear();

                setupListView(Records);
            }
        }

        private void setupListView(ObservableCollection<RawDataColumnModel> newRecords)
        {
            Records = newRecords;
            this.xListView.Children.Clear();
            this.xListView.ColumnDefinitions.Clear();
            foreach (var n in newRecords)
            {
                var rawColumn = new RawDataColumn()
                {
                    DataContext = n,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                this.xListView.Children.Add(rawColumn);
                Grid.SetColumn(xListView.Children.Last() as FrameworkElement, xListView.ColumnDefinitions.Count);
                xListView.ColumnDefinitions.Add(
                    new ColumnDefinition()
                    {
                        Width = new GridLength(n.AttributeTranformationModel.AttributeModel.DataType == IDEA_common.catalog.DataType.String ? 2 : 1, GridUnitType.Star)
                    }
                );
            }
        }
    

        void loadRecordsAsync(List<object> records, AttributeTransformationModel model, bool showScroll, List<AttributeModel> primaryKeys, List<List<object>> primaryKeyValues)
        {
            var acollection = new RawDataColumn.RawDataColumnModel
            {
                Alignment = records.First() is string || records.First() is IDEA_common.range.PreProcessedString ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                AttributeTranformationModel = model,
                PrimaryKeys = primaryKeys,
                IsEditable = model.AttributeModel.FuncModel is AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel,
                PrimaryValues = primaryKeyValues,
                RendererListView = xListView,
                ColumnWidth = records.First() is string || records.First() is IDEA_common.range.PreProcessedString ? 200 : 85,
                ShowScroll = showScroll
            };
            Records.Add(acollection);

            if (records != null)
                if (model.AttributeModel.VisualizationHints.FirstOrDefault() == IDEA_common.catalog.VisualizationHint.Image)
                {
                    var dataset = model.AttributeModel.OriginModel.Name;
                    var hostname = MainViewController.Instance.MainModel.Hostname;
                    string prepend = hostname + "/api/rawdata/" + dataset + "/" + model.AttributeModel.RawName + "/";
#pragma warning disable CS4014
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => {
                        foreach (var val in records)
                        {
                            acollection.Data.Add(new MyUri(prepend + val.ToString()));
                        }
                    });
#pragma warning restore CS4014

                }
                else if (model.AttributeModel.FuncModel is AttributeModel.AttributeFuncModel.AttributeAssignedValueFuncModel)
                {
#pragma warning disable CS4014
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => {
                        for (int i = 0; i < records.Count; i++)
                            acollection.Data.Add(new Tuple<int, object>(i, records[i]));
                    });
#pragma warning restore CS4014
                }
                else
                {
#pragma warning disable CS4014
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                        () => records.ForEach((r) => acollection.Data.Add(r)));
#pragma warning restore CS4014
                }
        }
        
        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as AttributeOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        private void AttributeRenderer_OperationViewModelTapped(PointerRoutedEventArgs e)
        {
            var attributeOperationViewModel = DataContext as AttributeOperationViewModel;
        }

        private void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }
        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }


        private void NameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {

                e.Handled = true;
            }
            else
                _keyboardTimer.Start();
        }
    }
}
