using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PanoramicDataWin8.controller.data.tuppleware.gateway;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.tilemenu;

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

        private TileMenuItemView _inputMenu = null;
        private TileMenuItemView _visualizationMenu = null;
        private TileMenuItemView _jobMenu = null;

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
                if (e.Key == VirtualKey.T)
                {
                    QueryModel q1 = new QueryModel(MainViewController.Instance.MainModel.SchemaModel, new ResultModel());
                    QueryModel q2 = new QueryModel(MainViewController.Instance.MainModel.SchemaModel, new ResultModel());
                    q1.FilterModels.Add(new FilterModel());
                    LinkModel lm = new LinkModel();
                    lm.FromQueryModel = q1;
                    lm.ToQueryModel = q2;
                    q1.LinkModels.Add(lm);
                    q2.LinkModels.Add(lm);

                    var tt = q1.Clone();

                 }
            }
        }

        void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Button button = (e.OriginalSource as FrameworkElement).GetFirstAncestorOfType<Button>();
            var ancestors = (e.OriginalSource as FrameworkElement).GetAncestors();
            if (!ancestors.Contains(addInputButton) && !ancestors.Contains(menuGrid))
            {
                if (_inputMenu != null)
                {
                    ((TileMenuItemViewModel) _inputMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel) _inputMenu.DataContext).IsBeingRemoved = true;
                    _inputMenu.Dispose();
                    menuCanvas.Children.Remove(_inputMenu);
                }
            }
            if (!ancestors.Contains(addVisualizationButton) && !ancestors.Contains(menuGrid))
            {
                if (_visualizationMenu != null)
                {
                    ((TileMenuItemViewModel) _visualizationMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel) _visualizationMenu.DataContext).IsBeingRemoved = true;
                    _visualizationMenu.Dispose();
                    menuCanvas.Children.Remove(_visualizationMenu);
                }
            }
            if (!ancestors.Contains(addJobButton) && !ancestors.Contains(menuGrid))
            {
                if (_jobMenu != null)
                {
                    ((TileMenuItemViewModel)_jobMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel)_jobMenu.DataContext).IsBeingRemoved = true;
                    _jobMenu.Dispose();
                    menuCanvas.Children.Remove(_jobMenu);
                }
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
            if (_jobMenu == null || !(_jobMenu.DataContext as TileMenuItemViewModel).AreChildrenExpanded)
            {
                MainModel mainModel = (DataContext as MainModel);
                var buttonBounds = addJobButton.GetBounds(this);
                var taskTypes = MainViewController.Instance.MainModel.Tasks;

                if (_jobMenu != null)
                {
                    ((TileMenuItemViewModel)_jobMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel)_jobMenu.DataContext).IsBeingRemoved = true;
                    _jobMenu.Dispose();
                    menuCanvas.Children.Remove(_jobMenu);
                }

                TileMenuItemViewModel parentModel = new TileMenuItemViewModel(null);
                parentModel.ChildrenNrColumns = (int)Math.Ceiling(taskTypes.Count() / 8.0);
                parentModel.ChildrenNrRows = (int)Math.Min(8.0, taskTypes.Count());
                parentModel.Alignment = Alignment.Center;
                parentModel.AttachPosition = AttachPosition.Right;

                int count = 0;
                foreach (var jobType in taskTypes)
                {
                    TileMenuItemViewModel tileMenuItemViewModel = new TileMenuItemViewModel(parentModel);
                    tileMenuItemViewModel.Alignment = Alignment.Center;
                    tileMenuItemViewModel.AttachPosition = AttachPosition.Right;
                    JobTypeViewModel jobTypeViewModel = new JobTypeViewModel()
                    {
                        TaskType = jobType
                    };
                    tileMenuItemViewModel.TileMenuContentViewModel = new JobTypeViewTileMenuContentViewModel()
                    {
                        Name = jobType.ToString(),
                        JobTypeViewModel = jobTypeViewModel
                    };

                    tileMenuItemViewModel.Row = count;
                    tileMenuItemViewModel.Column = parentModel.ChildrenNrColumns - (int)Math.Floor(parentModel.Children.Count / 8.0) - 1;
                    tileMenuItemViewModel.RowSpan = 1;
                    tileMenuItemViewModel.ColumnSpan = 1;
                    parentModel.Children.Add(tileMenuItemViewModel);
                    count++;
                    if (count == 8.0)
                    {
                        count = 0;
                    }
                }

                _jobMenu = new TileMenuItemView { MenuCanvas = menuCanvas, DataContext = parentModel };
                menuCanvas.Children.Add(_jobMenu);

                parentModel.CurrentPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.TargetPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.Size = new Vec(buttonBounds.Width, buttonBounds.Height);
                parentModel.AreChildrenExpanded = true;
            }
        }

        private void addInputButton_Click(object sender, RoutedEventArgs e)
        {
            MainModel mainModel = (DataContext as MainModel);
            if (mainModel.SchemaModel != null &&(_inputMenu == null || !(_inputMenu.DataContext as TileMenuItemViewModel).AreChildrenExpanded))
            {
                var buttonBounds = addInputButton.GetBounds(this);
                var inputModels =
                    mainModel.SchemaModel.OriginModels.First()
                        .InputModels.Where(am => am.IsDisplayed)/*.OrderBy(am => am.Name)*/;

                if (_inputMenu != null)
                {
                    ((TileMenuItemViewModel) _inputMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel) _inputMenu.DataContext).IsBeingRemoved = true;
                    _inputMenu.Dispose();
                    menuCanvas.Children.Remove(_inputMenu);
                }

                TileMenuItemViewModel parentModel = new TileMenuItemViewModel(null);
                parentModel.ChildrenNrColumns = (int) Math.Ceiling(inputModels.Count()/8.0);
                parentModel.ChildrenNrRows = (int) Math.Min(8.0, inputModels.Count());
                parentModel.Alignment = Alignment.Center;
                parentModel.AttachPosition = AttachPosition.Right;

                int count = 0;
                foreach (var inputModel in inputModels)
                {
                    TileMenuItemViewModel tileMenuItemViewModel = recursiveCreateTileMenu(inputModel, parentModel);
                    tileMenuItemViewModel.Row = count;
                    tileMenuItemViewModel.Column = parentModel.ChildrenNrColumns - (int) Math.Floor(parentModel.Children.Count/8.0) - 1;
                    tileMenuItemViewModel.RowSpan = 1;
                    tileMenuItemViewModel.ColumnSpan = 1;
                    count++;
                    if (count == 8.0)
                    {
                        count = 0;
                    }
                }

                _inputMenu = new TileMenuItemView {MenuCanvas = menuCanvas, DataContext = parentModel};
                menuCanvas.Children.Add(_inputMenu);

                parentModel.CurrentPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.TargetPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.Size = new Vec(buttonBounds.Width, buttonBounds.Height);
                parentModel.AreChildrenExpanded = true;
            }
        }

        private TileMenuItemViewModel recursiveCreateTileMenu(InputModel inputModel, TileMenuItemViewModel parent)
        {
            TileMenuItemViewModel currentTileMenuItemViewModel = null;
            if (inputModel is InputGroupModel)
            {
                var inputGroupModel = inputModel as InputGroupModel;
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                InputGroupViewModel inputGroupViewModel = new InputGroupViewModel(null, inputGroupModel);
                currentTileMenuItemViewModel.TileMenuContentViewModel = new InputGroupViewTileMenuContentViewModel()
                {
                    Name = inputModel.Name,
                    InputGroupViewModel = inputGroupViewModel
                };

                currentTileMenuItemViewModel.ChildrenNrColumns = (int)Math.Ceiling(inputGroupModel.InputModels.Count() / 8.0);
                currentTileMenuItemViewModel.ChildrenNrRows = (int)Math.Min(8.0, inputGroupModel.InputModels.Count());
                currentTileMenuItemViewModel.Alignment = Alignment.Center;
                currentTileMenuItemViewModel.AttachPosition = AttachPosition.Right;

                int count = 0;
                foreach (var childInputModel in inputGroupModel.InputModels/*.OrderBy(am => am.Name)*/)
                {
                    var childTileMenu = recursiveCreateTileMenu(childInputModel, currentTileMenuItemViewModel);
                    childTileMenu.Row = count; // TileMenuItemViewModel.Children.Count;
                    childTileMenu.Column = (currentTileMenuItemViewModel.ChildrenNrColumns - 1) - (int)Math.Floor((currentTileMenuItemViewModel.Children.Count - 1) / 8.0);
                    childTileMenu.RowSpan = 1;
                    childTileMenu.ColumnSpan = 1;
                    //currentTileMenuItemViewModel.Children.Add(childTileMenu);
                    count++;
                    if (count == 8.0)
                    {
                        count = 0;
                    }
                }
            }
            else if (inputModel is InputFieldModel)
            {
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                InputFieldViewModel inputFieldViewModel = new InputFieldViewModel(null, new InputOperationModel(inputModel as InputFieldModel));
                currentTileMenuItemViewModel.TileMenuContentViewModel = new InputFieldViewTileMenuContentViewModel()
                {
                    Name = inputModel.Name,
                    InputFieldViewModel = inputFieldViewModel
                };
            }
            parent.Children.Add(currentTileMenuItemViewModel);
            currentTileMenuItemViewModel.Alignment = Alignment.Center;
            currentTileMenuItemViewModel.AttachPosition = AttachPosition.Right;
            return currentTileMenuItemViewModel;
        }

        private void addVisualizationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_visualizationMenu == null || !(_visualizationMenu.DataContext as TileMenuItemViewModel).AreChildrenExpanded)
            {
                MainModel mainModel = (DataContext as MainModel);
                var buttonBounds = addVisualizationButton.GetBounds(this);
                var visualizationTypes = Enum.GetValues(typeof(VisualizationType)).Cast<VisualizationType>().ToList();

                if (_visualizationMenu != null)
                {
                    ((TileMenuItemViewModel)_visualizationMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel)_visualizationMenu.DataContext).IsBeingRemoved = true;
                    _visualizationMenu.Dispose();
                    menuCanvas.Children.Remove(_visualizationMenu);
                }

                TileMenuItemViewModel parentModel = new TileMenuItemViewModel(null);
                parentModel.ChildrenNrColumns = (int)Math.Ceiling(visualizationTypes.Count() / 8.0);
                parentModel.ChildrenNrRows = (int)Math.Min(8.0, visualizationTypes.Count());
                parentModel.Alignment = Alignment.Center;
                parentModel.AttachPosition = AttachPosition.Right;

                int count = 0;
                foreach (var visualizationType in visualizationTypes)
                {
                    TileMenuItemViewModel tileMenuItemViewModel = new TileMenuItemViewModel(parentModel);
                    tileMenuItemViewModel.Alignment = Alignment.Center;
                    tileMenuItemViewModel.AttachPosition = AttachPosition.Right;
                    VisualizationTypeViewModel visualizationTypeViewModel = new VisualizationTypeViewModel()
                    {
                        VisualizationType = visualizationType
                    };

                    tileMenuItemViewModel.TileMenuContentViewModel = new VisualizationTypeViewTileMenuContentViewModel()
                    {
                        Name = visualizationType.ToString(),
                        VisualizationTypeViewModel = visualizationTypeViewModel
                    };

                    tileMenuItemViewModel.Row = count; 
                    tileMenuItemViewModel.Column = parentModel.ChildrenNrColumns - (int)Math.Floor(parentModel.Children.Count / 8.0) - 1;
                    tileMenuItemViewModel.RowSpan = 1;
                    tileMenuItemViewModel.ColumnSpan = 1;
                    parentModel.Children.Add(tileMenuItemViewModel);
                    count++;
                    if (count == 8.0)
                    {
                        count = 0;
                    }
                }

                _visualizationMenu = new TileMenuItemView { MenuCanvas = menuCanvas, DataContext = parentModel };
                menuCanvas.Children.Add(_visualizationMenu);

                parentModel.CurrentPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.TargetPosition = new Pt(-(buttonBounds.Width), buttonBounds.Top);
                parentModel.Size = new Vec(buttonBounds.Width, buttonBounds.Height);
                parentModel.AreChildrenExpanded = true;
            }
        }

        private void CloseButton_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            codeGrid.Visibility = Visibility.Collapsed;
        }

        public async void FireCodeGeneration(VisualizationViewModel vis)
        {
            codeGrid.Visibility = Visibility.Visible;

            string text = "";
            foreach (var generateCodeUuid in vis.QueryModel.GenerateCodeUuids)
            {
                CodeGenCommand cmd = new CodeGenCommand();
                string response = await cmd.CodeGen((vis.QueryModel.SchemaModel.OriginModels[0] as TuppleWareOriginModel), generateCodeUuid);
                JObject obj = JObject.Parse(response);
                text += obj["code"] + "\n";
            }

            editBox.Document.SetText(TextSetOptions.ApplyRtfDocumentDefaults, text);
        }
    }
}
