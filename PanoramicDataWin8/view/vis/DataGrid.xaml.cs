using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class DataGrid : UserControl, AttributeTransformationViewModelEventHandler
    {
        private IDisposable _observableDisposable = null;


        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        public ObservableCollection<HeaderObject> HeaderObjects { get; set; }
        public bool CanReorder { get; set; }
        public bool CanResize { get; set; }
        public bool CanDrag { get; set; }
        public bool CanExplore { get; set; }

        private DispatcherTimer _activeTimer = new DispatcherTimer();

        public DataGrid()
        {
            HeaderObjects = new ObservableCollection<HeaderObject>();

            this.InitializeComponent();
            this.DataContextChanged += DataGrid_DataContextChanged;
            InputFieldView.InputFieldViewModelTapped += InputFieldViewInputFieldViewModelTapped;
            
            listView.ManipulationMode = ManipulationModes.None;            
            _activeTimer.Interval = TimeSpan.FromMilliseconds(10);
            _activeTimer.Tick += _activeTimer_Tick;
            _activeTimer.Start();
        }

        void _activeTimer_Tick(object sender, object e)
        {
            var model = (DataContext as HistogramOperationViewModel);
            if (model != null)
            {
                if (model.ActiveStopwatch.Elapsed > TimeSpan.FromSeconds(5))
                {
                    removeMenu();
                    model.ActiveStopwatch.Reset();
                }
            }
        }

        public void Dispose()
        {
            if (_observableDisposable != null)
            {
                _observableDisposable.Dispose();
            }
            if (DataContext != null)
            {
                (DataContext as HistogramOperationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                (DataContext as HistogramOperationViewModel).HistogramOperationModel.PropertyChanged -= QueryModel_PropertyChanged;
                removeMenu();
            }
            InputFieldView.InputFieldViewModelTapped -= InputFieldViewInputFieldViewModelTapped;
        }

        ~DataGrid()
        {
           
        }

        void DataGrid_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                if (_observableDisposable != null)
                {
                    _observableDisposable.Dispose();
                }
                _observableDisposable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(
                    (args.NewValue as HistogramOperationViewModel).HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X), "CollectionChanged")
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe((arg) =>
                    {
                        populateTableHeaders();
                    });

                (DataContext as HistogramOperationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                (DataContext as HistogramOperationViewModel).HistogramOperationModel.PropertyChanged += QueryModel_PropertyChanged;
               
                if ((DataContext as HistogramOperationViewModel).HistogramOperationModel != null)
                {
                    populateData();
                }
                populateTableHeaders();
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

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = DataContext as HistogramOperationViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Size))
            {
                updateSize(null);
            }
            else if (e.PropertyName == model.GetPropertyName(() => model.Size) ||
                     e.PropertyName == model.GetPropertyName(() => model.Position))
            {
                setMenuViewModelAnkerPosition();
            }      
        }

        void headerObject_Resized(object sender, EventArgs e)
        {
            updateSize(sender as HeaderObject);
        }

        private void updateSize(HeaderObject exclude)
        {
            double totalHeaderWidth = HeaderObjects.Sum(ho => ho.Width);
            double availableWidth = (DataContext as HistogramOperationViewModel).Size.X - (HeaderObjects.Count) * 8;

            if (exclude != null)
            {
                totalHeaderWidth -= exclude.Width;
                availableWidth -= exclude.Width;

                exclude.Width = Math.Min(exclude.Width, (DataContext as HistogramOperationViewModel).Size.X - (HeaderObjects.Count - 1) * 28);

                double ratio = availableWidth / totalHeaderWidth;
                HeaderObjects.Where(ho => ho != exclude).ToList().ForEach(ho => ho.Value.Width *= ratio);
            }
            else
            {
                double ratio = availableWidth / totalHeaderWidth;
                HeaderObjects.ForEach(ho => ho.Value.Width *= ratio);
            }
            setMenuViewModelAnkerPosition();
        }
        
        private void populateData()
        {
           // ResultModel resultModel = (DataContext as OperationViewModel).OperationModel.Result;
           // listView.ItemsSource = resultModel.ResultItemModels;
        }

        private void populateTableHeaders()
        {
            removeMenu();
            HistogramOperationViewModel model = (DataContext as HistogramOperationViewModel);
            List<AttributeTransformationModel> inputOperationModels = model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).ToList();

            List<HeaderObject> headerObjects = new List<HeaderObject>();

            foreach (var inputOperationModel in inputOperationModels)
            {
                HeaderObject ho = new HeaderObject();
                ho.AttributeTransformationViewModel = new AttributeTransformationViewModel(model, inputOperationModel)
                {
                    AttachmentOrientation = AttachmentOrientation.Top
                };
                ho.Width = 100;
                if (HeaderObjects.Any(hoo => hoo.AttributeTransformationViewModel != null && hoo.AttributeTransformationViewModel.AttributeTransformationModel == ho.AttributeTransformationViewModel.AttributeTransformationModel))
                {
                    var oldHo = HeaderObjects.First(hoo => hoo.AttributeTransformationViewModel.AttributeTransformationModel == ho.AttributeTransformationViewModel.AttributeTransformationModel);
                    ho.Width = oldHo.Width;
                }
                headerObjects.Add(ho);
            }
            if (headerObjects.Count > 0)
            {
                headerObjects.First().IsFirst = true;
                headerObjects.Last().IsLast = true;
            }
            headerItemsControl.ItemsSource = headerObjects;

            // header template
            StringBuilder sb = new StringBuilder();
            sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" >" );
            sb.Append("<Grid>");
            //sb.Append("<Border >");
            sb.Append(" <StackPanel Orientation=\"Horizontal\">");
            sb.Append("     <local:DataGridResizer Margin=\"0,0,0,0\" Width=\"4\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/> ");
            sb.Append("     <Grid Width=\"{Binding Width}\">");
            sb.Append("         <local:InputFieldView DataContext=\"{Binding AttributeTransformationViewModel}\" Width=\"{Binding Width}\" RawName=\"inputFieldView\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
            sb.Append("     </Grid>");
            sb.Append("     <local:DataGridResizer Width=\"4\" xmlns:local=\"using:PanoramicDataWin8.view.common\" IsResizer=\"True\"/> ");
            sb.Append(" </StackPanel>");
            //sb.Append("</Border>");
            sb.Append("</Grid>");
            sb.Append("</DataTemplate>");
            DataTemplate datatemplate = (DataTemplate)XamlReader.Load(sb.ToString());
            headerItemsControl.ItemTemplate = datatemplate;

            foreach (var ho in HeaderObjects)
            {
                ho.Resized -= headerObject_Resized;
            }
            HeaderObjects.Clear();

            foreach (var ho in headerObjects)
            {
                ho.Resized += headerObject_Resized;
                ho.NrElements = headerObjects.Count;
                HeaderObjects.Add(ho);
            }
            updateSize(null);

            // list view template
            sb = new StringBuilder();
            sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" >");
            sb.Append("<StackPanel Orientation=\"Horizontal\" Background=\"{StaticResource lightBrush}\">");
            int count = 0;
            foreach (var inputOperationModel in inputOperationModels)
            {
                sb.Append("     <local:DataGridCell HeaderObject=\"{Binding ElementName=_this, Path=HeaderObjects[" + count + "]}\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
                //sb.Append("     <TextBlock Text=\"asdfasfds}\" xmlns:local=\"using:PanoramicDataWin8.view.common\"/>");
                count++;
            }

            
            sb.Append("</StackPanel>");
            sb.Append("</DataTemplate>");
            datatemplate = (DataTemplate)XamlReader.Load(sb.ToString());
            listView.ItemTemplate = datatemplate;
        }

        public void AttributeTransformationViewModelMoved(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;

            // hide cloumn header reorder drop highlights 
            hideColumnReorderFeedbacks();

            if (overElement)
            {
                Point fromThis = inkableScene.TransformToVisual(this).TransformPoint(e.Bounds.Center);

                if (CanReorder)
                {
                    IEnumerable<DataGridResizer> resizers = headerItemsControl.GetDescendantsOfType<DataGridResizer>();
                    // find closest header reorder drop highlight 
                    DataGridResizer closestHeader = findClosestReorderDataGridResizer(e.Bounds.Center);
                    if (closestHeader != null)
                    {
                        closestHeader.Highlight();
                    }
                }
            }
        }

        private DataGridResizer findClosestReorderDataGridResizer(Point fromInqScene)
        {
            InkableScene inkableScene = MainViewController.Instance.InkableScene;
            IEnumerable<DataGridResizer> resizers = headerItemsControl.GetDescendantsOfType<DataGridResizer>();

            // find closest header reorder drop highlight 
            DataGridResizer clostestDataGridResizer = null;
            double closestXDist = double.MaxValue;
            foreach (var h in resizers)
            {
                if (h.Visibility != Windows.UI.Xaml.Visibility.Collapsed)
                {
                    Point p = inkableScene.TransformToVisual(h).TransformPoint(fromInqScene);
                    if (Math.Abs(p.X) < closestXDist)
                    {
                        closestXDist = Math.Abs(p.X);
                        clostestDataGridResizer = h;
                    }
                }
            }
            return clostestDataGridResizer;
        }

        private void hideColumnReorderFeedbacks()
        {
            IEnumerable<DataGridResizer> resizers = headerItemsControl.GetDescendantsOfType<DataGridResizer>();
            foreach (var r in resizers)
            {
                r.UnHighlight();
            }
        }

        public void AttributeTransformationViewModelDropped(AttributeTransformationViewModel sender, AttributeTransformationViewModelEventArgs e, bool overElement)
        {
            // hide cloumn header reorder drop highlights 
            hideColumnReorderFeedbacks();
            if (overElement)
            {

                HistogramOperationViewModel model = (DataContext as HistogramOperationViewModel);
                if (model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Count == 0)
                {
                    model.HistogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, sender.AttributeTransformationModel);
                    return;
                }

                InkableScene inkableScene = MainViewController.Instance.InkableScene;
                Point fromThis = inkableScene.TransformToVisual(this).TransformPoint(e.Bounds.Center);

                DataGridResizer closestDataGridResizer = findClosestReorderDataGridResizer(e.Bounds.Center);
                HeaderObject headerObject = closestDataGridResizer.DataContext as HeaderObject;

                if ((CanReorder || CanDrag) &&
                    model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Any(aom => object.ReferenceEquals(aom, sender.AttributeTransformationModel)))
                {
                    model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Remove(sender.AttributeTransformationModel);

                }
                AttributeTransformationModel clone = e.AttributeTransformationModel;
                if (headerObject.AttributeTransformationViewModel == null)
                {
                    model.HistogramOperationModel.AddUsageAttributeTransformationModel(InputUsage.X, clone);
                }
                else
                {
                    int index = HeaderObjects.IndexOf(headerObject);
                    if (closestDataGridResizer.IsResizer)
                    {
                        model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Insert(Math.Min(index + 1, model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Count), clone);
                    }
                    else
                    {
                        model.HistogramOperationModel.GetUsageAttributeTransformationModel(InputUsage.X).Insert(index, clone);
                    }
                }
            }
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        void removeMenu()
        {
            
            if (_menuViewModel != null)
            {
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.AttributeTransformationViewModel).FirstOrDefault();
                if (inputFieldView != null)
                {
                    Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                    foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                    {
                        menuItem.TargetPosition = bounds.TopLeft;
                    }
                    _menuViewModel.IsToBeRemoved = true;
                    _menuViewModel.IsDisplayed = false;
                }
            }
        }
        
        void InputFieldViewInputFieldViewModelTapped(object sender, EventArgs e)
        {
            var visModel = (DataContext as HistogramOperationViewModel);
            visModel.ActiveStopwatch.Restart();

            AttributeTransformationViewModel model = (sender as InputFieldView).DataContext as AttributeTransformationViewModel;
            if (HeaderObjects.Any(ho => ho.AttributeTransformationViewModel == model))
            {
                bool createNew = true;
                if (_menuViewModel != null && !_menuViewModel.IsToBeRemoved)
                {
                    createNew = _menuViewModel.AttributeTransformationViewModel != model;
                    removeMenu();
                }

                if (createNew)
                {
                    InputFieldView inputFieldView = sender as InputFieldView;
                    var menuViewModel = model.CreateMenuViewModel(inputFieldView.GetBounds(MainViewController.Instance.InkableScene));
                    if (menuViewModel.MenuItemViewModels.Count > 0)
                    {
                        _menuViewModel = menuViewModel;
                        _menuView = new MenuView()
                        {
                            DataContext = _menuViewModel
                        };
                        setMenuViewModelAnkerPosition();
                        MainViewController.Instance.InkableScene.Add(_menuView);
                        _menuViewModel.IsDisplayed = true;
                    }
                }
            }
        }

        private void setMenuViewModelAnkerPosition()
        {
            if (_menuViewModel != null)
            {
                InputFieldView inputFieldView = this.GetDescendantsOfType<InputFieldView>().Where(av => av.DataContext == _menuViewModel.AttributeTransformationViewModel).FirstOrDefault();

                if (inputFieldView != null)
                {
                    if (_menuViewModel.IsToBeRemoved)
                    {
                        Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                        foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                        {
                            menuItem.TargetPosition = bounds.TopLeft;
                        }
                    }
                    else
                    {
                        Rct bounds = inputFieldView.GetBounds(MainViewController.Instance.InkableScene);
                        _menuViewModel.AnkerPosition = bounds.TopLeft;
                    }
                }
            }
        }

    }

    public class HeaderObject : ExtendedBindableBase
    {
        public event EventHandler<EventArgs> Resized;

        public AttributeTransformationViewModel AttributeTransformationViewModel { get; set; }

        public void FireResized()
        {
            if (Resized != null)
            {
                Resized(this, new EventArgs());
            }
        }


        private int _nrElements = 0;
        public int NrElements
        {
            get
            {
                return _nrElements;
            }
            set
            {
                this.SetProperty(ref _nrElements, value);
            }
        }

        private bool _isLast = false;
        public bool IsLast
        {
            get
            {
                return _isLast;
            }
            set
            {
                this.SetProperty(ref _isLast, value);
            }
        }

        private bool _isFrist = false;
        public bool IsFirst
        {
            get
            {
                return _isFrist;
            }
            set
            {
                this.SetProperty(ref _isFrist, value);
            }
        }

        private double _width = 0;
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                this.SetProperty(ref _width, Math.Max(20, value));
            }
        }
    }
}
