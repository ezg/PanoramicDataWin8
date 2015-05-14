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
using Windows.Storage;
using System.Diagnostics;
using System.ComponentModel;
using PanoramicDataWin8.view.vis;
using Windows.UI.Input;
using MathNet.Numerics.LinearAlgebra;
using PanoramicDataWin8.view;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.utils;
using Windows.UI.Notifications;
using Windows.UI.Core;
using Windows.System;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.model.view;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();
        private DispatcherTimer _messageTimer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.DataContextChanged += MainPage_DataContextChanged;
            this.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(MainPage_PointerPressed), true);
            //this.KeyUp += MainPage_KeyUp;
            this.KeyDown += MainPage_KeyDown;

            _messageTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _messageTimer.Tick += _messageTimer_Tick;
        }

        void _messageTimer_Tick(object sender, object e)
        {
            msgTextBlock.Opacity = 0;
            _messageTimer.Stop();
        }

        void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {

                if (e.Key == Windows.System.VirtualKey.Q)
                {
                    MainViewController.Instance.MainModel.SampleSize = MainViewController.Instance.MainModel.SampleSize + 100;
                    Debug.WriteLine("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);

                    msgTextBlock.Text = ("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == Windows.System.VirtualKey.A)
                {
                    MainViewController.Instance.MainModel.SampleSize = Math.Max(MainViewController.Instance.MainModel.SampleSize - 100, 1.0);
                    Debug.WriteLine("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);

                    msgTextBlock.Text = ("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.W)
                {
                    MainViewController.Instance.MainModel.ThrottleInMillis = MainViewController.Instance.MainModel.ThrottleInMillis + 300.0;
                    Debug.WriteLine("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);

                    msgTextBlock.Text = ("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == Windows.System.VirtualKey.S)
                {
                    MainViewController.Instance.MainModel.ThrottleInMillis = Math.Max(MainViewController.Instance.MainModel.ThrottleInMillis - 300.0, 0.0);
                    Debug.WriteLine("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);

                    msgTextBlock.Text = ("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.E)
                {
                    MainViewController.Instance.MainModel.NrOfXBins = MainViewController.Instance.MainModel.NrOfXBins + 1;
                    Debug.WriteLine("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);

                    msgTextBlock.Text = ("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == Windows.System.VirtualKey.D)
                {
                    MainViewController.Instance.MainModel.NrOfXBins = Math.Max(MainViewController.Instance.MainModel.NrOfXBins - 1, 1.0);
                    Debug.WriteLine("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);

                    msgTextBlock.Text = ("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.R)
                {
                    MainViewController.Instance.MainModel.NrOfYBins = MainViewController.Instance.MainModel.NrOfYBins + 1;
                    Debug.WriteLine("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);

                    msgTextBlock.Text = ("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == Windows.System.VirtualKey.F)
                {
                    MainViewController.Instance.MainModel.NrOfYBins = Math.Max(MainViewController.Instance.MainModel.NrOfYBins - 1, 1.0);
                    Debug.WriteLine("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);

                    msgTextBlock.Text = ("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.Number1)
                {
                    MainViewController.Instance.MainModel.GraphRenderOption = GraphRenderOptions.Grid;
                    Debug.WriteLine("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption.ToString());

                    msgTextBlock.Text = ("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption.ToString());
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.Number2)
                {
                    MainViewController.Instance.MainModel.GraphRenderOption = GraphRenderOptions.Cell;
                    Debug.WriteLine("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption.ToString());

                    msgTextBlock.Text = ("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption.ToString());
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == Windows.System.VirtualKey.V)
                {
                    MainViewController.Instance.MainModel.Verbose = !MainViewController.Instance.MainModel.Verbose;
                    Debug.WriteLine("Verbose : " + MainViewController.Instance.MainModel.Verbose.ToString());

                    msgTextBlock.Text = ("Verbose : " + MainViewController.Instance.MainModel.Verbose.ToString());
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
            }
        }

        void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Button button = (e.OriginalSource as FrameworkElement).GetFirstAncestorOfType<Button>();
            var ancestors = (e.OriginalSource as FrameworkElement).GetAncestors();
            if (!ancestors.Contains(addAttributeButton) && !ancestors.Contains(attributeGrid))
            {
                attributeGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
                
        void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (args.NewValue as MainModel).PropertyChanged += MainPage_PropertyChanged;
                (args.NewValue as MainModel).DatasetConfigurations.CollectionChanged += DatasetConfigurations_CollectionChanged;
            }
        }

        void MainPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = DataContext as MainModel;
            if (model.SchemaModel != null)
            {
                if (model.SchemaModel != null && model.SchemaModel is TuppleWareSchemaModel)
                {
                    addJobButton.Visibility = Visibility.Visible;
                }
                else
                {
                    addJobButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        void DatasetConfigurations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            commandBar.SecondaryCommands.Clear();
            foreach (var datasetConfiguration in (DataContext as MainModel).DatasetConfigurations)
            {
                AppBarButton b = new AppBarButton();
                b.Style =  Application.Current.Resources.MergedDictionaries[0]["AppBarButtonStyle1"] as Style;
                b.Label = datasetConfiguration.Name;
                b.Icon = new SymbolIcon(Symbol.Library);
                b.DataContext = datasetConfiguration;
                b.Click += appBarButton_Click;
                commandBar.SecondaryCommands.Add(b);
            }

        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainViewController.CreateInstance(inkableScene, this);
            DataContext = MainViewController.Instance.MainModel;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(MainViewController.Instance.InkableScene);
        }

        void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            DatasetConfiguration ds = (sender as AppBarButton).DataContext as DatasetConfiguration;
            MainViewController.Instance.LoadData(ds);
        }
        

        void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                GeneralTransform gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                Point currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                Vec delta = _mainPointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();

                MatrixTransform xform = MainViewController.Instance.InkableScene.RenderTransform as MatrixTransform;
                Mat matrix = xform.Matrix;
                //Point center = e.Position;
                //matrix = Mat.Translate(-center.X, -center.Y) * matrix;
                //matrix = Mat.Scale(delta.Scale, delta.Scale) * matrix;
                //matrix = Mat.Translate(+center.X, +center.Y) * matrix;
                matrix = Mat.Translate(-delta.X, -delta.Y) * matrix;
                MainViewController.Instance.InkableScene.RenderTransform = new MatrixTransform()
                {
                    Matrix = matrix
                };

                _mainPointerManagerPreviousPoint = currentPoint;
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
        }

        private void addJobButton_Click(object sender, RoutedEventArgs e)
        {
            MainModel mainModel = (DataContext as MainModel);
            var buttonBounds = addJobButton.GetBounds(this);
            var jobTypes = Enum.GetValues(typeof(JobType)).Cast<JobType>().Where(jt => jt != JobType.DB).ToList();

            attributeCanvas.Children.Clear();
            double perColumn = Math.Ceiling(jobTypes.Count / 2.0);
            double height = perColumn * 50 + (perColumn - 1) * 4;
            double startY = buttonBounds.Center.Y - height / 2.0;

            int countPerColumn = 0;
            int column = 0;
            foreach (var jobType in jobTypes)
            {
                JobTypeViewModel jobTypeViewModel = new JobTypeViewModel()
                {
                    JobType = jobType
                };
                JobTypeView jobTypeView = new JobTypeView();
                jobTypeView.DataContext = jobTypeViewModel;
                attributeCanvas.Children.Add(jobTypeView);
                jobTypeView.RenderTransform = new TranslateTransform()
                {
                    X = column * 54,
                    Y = startY + countPerColumn * 54
                };

                countPerColumn++;
                if (countPerColumn >= perColumn)
                {
                    column++;
                    countPerColumn = 0;
                }
            }

            attributeGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void addAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            MainModel mainModel = (DataContext as MainModel);
            var buttonBounds = addAttributeButton.GetBounds(this);
            var attributeModels = mainModel.SchemaModel.OriginModels.First().AttributeModels.Where(am => am.IsDisplayed).OrderBy(am => am.Name);

            attributeCanvas.Children.Clear();
            double perColumn = Math.Ceiling(attributeModels.Count() / 2.0);
            double height = perColumn * 50 + (perColumn - 1) * 4;
            double startY = buttonBounds.Center.Y - height / 2.0;

            int countPerColumn = 0;
            int column = 0;
            foreach (var attributeModel in attributeModels)
            {
                AttributeViewModel attributeViewModel = new AttributeViewModel(null, new AttributeOperationModel(attributeModel))
                {
                    IsNoChrome = true,
                    IsMenuEnabled = false
                };
                AttributeView attributeView = new AttributeView();
                attributeView.DataContext = attributeViewModel;
                attributeCanvas.Children.Add(attributeView);
                attributeView.RenderTransform = new TranslateTransform()
                {
                    X = column * 54,
                    Y = startY + countPerColumn * 54
                };

                countPerColumn++;
                if (countPerColumn >= perColumn)
                {
                    column++;
                    countPerColumn = 0;
                }
            }

            attributeGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void addVisualizationButton_Click(object sender, RoutedEventArgs e)
        {
            MainModel mainModel = (DataContext as MainModel);
            var buttonBounds = addVisualizationButton.GetBounds(this);
            var visualizationTypes = Enum.GetValues(typeof(VisualizationType)).Cast<VisualizationType>().ToList();

            attributeCanvas.Children.Clear();
            double perColumn = Math.Ceiling(visualizationTypes.Count() / 2.0);
            double height = perColumn * 50 + (perColumn - 1) * 4;
            double startY = buttonBounds.Center.Y - height / 2.0;

            int countPerColumn = 0;
            int column = 0;
            foreach (var visualizationType in visualizationTypes)
            {
                VisualizationTypeViewModel visualizationTypeViewModel = new VisualizationTypeViewModel()
                {
                    VisualizationType = visualizationType
                };
                VisualizationTypeView visualizationTypeView = new VisualizationTypeView();
                visualizationTypeView.DataContext = visualizationTypeViewModel;
                attributeCanvas.Children.Add(visualizationTypeView);
                visualizationTypeView.RenderTransform = new TranslateTransform()
                {
                    X = column * 54,
                    Y = startY + countPerColumn * 54
                };

                countPerColumn++;
                if (countPerColumn >= perColumn)
                {
                    column++;
                    countPerColumn = 0;
                }
            }

            attributeGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
