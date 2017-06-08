﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.tilemenu;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.view.vis.menu;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Dictionary<uint, FrameworkElement> _deviceRenderings = new Dictionary<uint, FrameworkElement>();
        private readonly PointerManager _mainPointerManager = new PointerManager();
        private readonly DispatcherTimer _messageTimer = new DispatcherTimer();

        private TileMenuItemView _attributeMenu;

        private MenuView _hypothesisMenuView;
        private MenuViewModel _hypothesisMenuViewModel;
        private double _length = 1;
        private Point _mainPointerManagerPreviousPoint;
        private TileMenuItemView _operationMenu;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            DataContextChanged += MainPage_DataContextChanged;
            AddHandler(PointerPressedEvent, new PointerEventHandler(MainPage_PointerPressed), true);
            KeyDown += MainPage_KeyDown;

            _messageTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _messageTimer.Tick += _messageTimer_Tick;
        }

        private void _messageTimer_Tick(object sender, object e)
        {
            msgTextBlock.Opacity = 0;
            _messageTimer.Stop();
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                if (e.Key == VirtualKey.Q)
                {
                    MainViewController.Instance.MainModel.SampleSize =
                        MainViewController.Instance.MainModel.SampleSize + 100;
                    Debug.WriteLine("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);

                    msgTextBlock.Text = "SampleSize : " + MainViewController.Instance.MainModel.SampleSize;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.A)
                {
                    MainViewController.Instance.MainModel.SampleSize =
                        Math.Max(MainViewController.Instance.MainModel.SampleSize - 100, 1.0);
                    Debug.WriteLine("SampleSize : " + MainViewController.Instance.MainModel.SampleSize);

                    msgTextBlock.Text = "SampleSize : " + MainViewController.Instance.MainModel.SampleSize;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.W)
                {
                    MainViewController.Instance.MainModel.ThrottleInMillis =
                        MainViewController.Instance.MainModel.ThrottleInMillis + 300.0;
                    Debug.WriteLine("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);

                    msgTextBlock.Text = "Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.S)
                {
                    MainViewController.Instance.MainModel.ThrottleInMillis =
                        Math.Max(MainViewController.Instance.MainModel.ThrottleInMillis - 300.0, 0.0);
                    Debug.WriteLine("Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis);

                    msgTextBlock.Text = "Throttle : " + MainViewController.Instance.MainModel.ThrottleInMillis;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.E)
                {
                    MainViewController.Instance.MainModel.NrOfXBins = MainViewController.Instance.MainModel.NrOfXBins +
                                                                      1;
                    Debug.WriteLine("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);

                    msgTextBlock.Text = "NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.D)
                {
                    MainViewController.Instance.MainModel.NrOfXBins =
                        Math.Max(MainViewController.Instance.MainModel.NrOfXBins - 1, 1.0);
                    Debug.WriteLine("NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins);

                    msgTextBlock.Text = "NrOfXBins : " + MainViewController.Instance.MainModel.NrOfXBins;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.R)
                {
                    MainViewController.Instance.MainModel.NrOfYBins = MainViewController.Instance.MainModel.NrOfYBins +
                                                                      1;
                    Debug.WriteLine("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);

                    msgTextBlock.Text = "NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.F)
                {
                    MainViewController.Instance.MainModel.NrOfYBins =
                        Math.Max(MainViewController.Instance.MainModel.NrOfYBins - 1, 1.0);
                    Debug.WriteLine("NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins);

                    msgTextBlock.Text = "NrOfYBins : " + MainViewController.Instance.MainModel.NrOfYBins;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.L)
                {
                    MainViewController.Instance.LoadCatalog();
                }
                if (e.Key == VirtualKey.Number1)
                {
                    MainViewController.Instance.MainModel.GraphRenderOption = GraphRenderOptions.Grid;
                    Debug.WriteLine("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption);

                    msgTextBlock.Text = "GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.Number2)
                {
                    MainViewController.Instance.MainModel.GraphRenderOption = GraphRenderOptions.Cell;
                    Debug.WriteLine("GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption);

                    msgTextBlock.Text = "GraphRenderOption : " + MainViewController.Instance.MainModel.GraphRenderOption;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.V)
                {
                    MainViewController.Instance.MainModel.Verbose = !MainViewController.Instance.MainModel.Verbose;
                    Debug.WriteLine("Verbose : " + MainViewController.Instance.MainModel.Verbose);

                    msgTextBlock.Text = "Verbose : " + MainViewController.Instance.MainModel.Verbose;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.H)
                {
                    MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled = !MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled;
                    Debug.WriteLine("IsDefaultHypothesisEnabled : " + MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled);

                    msgTextBlock.Text = "IsDefaultHypothesisEnabled : " + MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                else if (e.Key == VirtualKey.U)
                {
                    MainViewController.Instance.MainModel.IsUnknownUnknownEnabled = !MainViewController.Instance.MainModel.IsUnknownUnknownEnabled;
                    Debug.WriteLine("IsUnknownUnknownEnabled : " + MainViewController.Instance.MainModel.IsUnknownUnknownEnabled);

                    msgTextBlock.Text = "IsUnknownUnknownEnabled : " + MainViewController.Instance.MainModel.IsUnknownUnknownEnabled;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
                if (e.Key == VirtualKey.T)
                {
                    var q1 = new HistogramOperationModel(MainViewController.Instance.MainModel.SchemaModel);
                    var q2 = new HistogramOperationModel(MainViewController.Instance.MainModel.SchemaModel);
                    q1.FilterModels.Add(new FilterModel());
                    var lm = new FilterLinkModel();
                    lm.FromOperationModel = q1;
                    lm.ToOperationModel = q2;
                    q1.ConsumerLinkModels.Add(lm);
                    q2.ConsumerLinkModels.Add(lm);

                    var tt = q1.Clone();
                }
                

                if (e.Key == VirtualKey.P)
                {
                    Debug.WriteLine("Render Fingers / Pen : " +
                                    MainViewController.Instance.MainModel.RenderFingersAndPen);
                    MainViewController.Instance.MainModel.RenderFingersAndPen =
                        !MainViewController.Instance.MainModel.RenderFingersAndPen;
                    msgTextBlock.Text = "Fingers / Pen : " + MainViewController.Instance.MainModel.RenderFingersAndPen;
                    msgTextBlock.Opacity = 1;
                    _messageTimer.Start();
                }
            }
        }

        private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ancestors = (e.OriginalSource as FrameworkElement).GetAncestors();
            if (!ancestors.Contains(addAttributeButton) && !ancestors.Contains(menuGrid))
            {
                if (_attributeMenu != null)
                {
                    ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded = false;
                }
            }
            if (!ancestors.Contains(addOperationButton) && !ancestors.Contains(menuGrid))
            {
                if (_operationMenu != null)
                {
                    ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded = false;
                }
            }

            if (!ancestors.Contains(hypothesisButton) && !ancestors.Contains(_hypothesisMenuView))
            {
                if (_hypothesisMenuView != null)
                {
                    _hypothesisMenuViewModel.IsDisplayed = false;
                }
            }
            if (!ancestors.Contains(hypothesisButton) && !ancestors.Contains(addOperationButton) && !ancestors.Contains(addAttributeButton) &&
                !(e.OriginalSource is TextBlock))
            {
                foreach (var a in ancestors)
                    if (a is TextBox)
                        return;
                var f  = FocusManager.GetFocusedElement();
                addAttributeButton.Focus(FocusState.Pointer);
                //FocusSink.Focus(FocusState.Keyboard);
            }
        }

        private void MainPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (args.NewValue as MainModel).DatasetConfigurations.CollectionChanged +=
                    DatasetConfigurations_CollectionChanged;
            }
        }

        private void DatasetConfigurations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            commandBar.SecondaryCommands.Clear();
            foreach (var datasetConfiguration in (DataContext as MainModel).DatasetConfigurations)
            {
                var b = new AppBarButton();
                b.Style = Application.Current.Resources.MergedDictionaries[0]["AppBarButtonStyle1"] as Style;
                b.Label = datasetConfiguration.Schema.DisplayName;
                b.Icon = new SymbolIcon(Symbol.Library);
                b.DataContext = datasetConfiguration;
                b.Click += appBarButton_Click;
                commandBar.SecondaryCommands.Add(b);
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainViewController.CreateInstance(inkableScene, this);
            DataContext = MainViewController.Instance.MainModel;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(MainViewController.Instance.InkableScene);

            HypothesesViewController.CreateInstance(MainViewController.Instance.MainModel, MainViewController.Instance.OperationViewModels);
            var hypothesesView = new HypothesesView();
            hypothesesView.DataContext = HypothesesViewController.Instance.HypothesesViewModel;
            hypothesisGrid.Children.Add(hypothesesView);

            RecommenderViewController.CreateInstance(MainViewController.Instance.MainModel, MainViewController.Instance.OperationViewModels);

            AddHandler(PointerPressedEvent, new PointerEventHandler(InkableScene_PointerPressed), true);
            AddHandler(PointerReleasedEvent, new PointerEventHandler(InkableScene_PointerReleased), true);
            AddHandler(PointerMovedEvent, new PointerEventHandler(InkableScene_PointerMoved), true);

            hypothesisButton.DataContext = new BudgetViewModel();
        }
        public bool LastTouchWasMouse = false;
        private void InkableScene_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            LastTouchWasMouse = e.Pointer.PointerDeviceType == PointerDeviceType.Mouse;
            if (MainViewController.Instance.MainModel.RenderFingersAndPen)
            {
                FrameworkElement cnv = null;
                if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
                {
                    cnv = createPen();
                }
                else
                {
                    cnv = createFinger();
                }
                _deviceRenderings.Add(e.Pointer.PointerId, cnv);
                var pos = e.GetCurrentPoint(fingerAndPenCanvas).Position;
                (cnv.RenderTransform as TranslateTransform).X = pos.X;
                (cnv.RenderTransform as TranslateTransform).Y = pos.Y;
                fingerAndPenCanvas.Children.Add(cnv);
            }
        }

        private void InkableScene_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (MainViewController.Instance.MainModel.RenderFingersAndPen)
            {
                fingerAndPenCanvas.Children.Remove(_deviceRenderings[e.Pointer.PointerId]);
                _deviceRenderings.Remove(e.Pointer.PointerId);
            }

            MainViewController.Instance.UpdateJobStatus();
        }

        private void InkableScene_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (MainViewController.Instance.MainModel.RenderFingersAndPen)
            {
                if (_deviceRenderings.ContainsKey(e.Pointer.PointerId))
                {
                    var ell = _deviceRenderings[e.Pointer.PointerId];
                    var pos = e.GetCurrentPoint(fingerAndPenCanvas).Position;
                    (ell.RenderTransform as TranslateTransform).X = pos.X;
                    (ell.RenderTransform as TranslateTransform).Y = pos.Y;
                }
            }
        }

        private FrameworkElement createFinger()
        {
            var cnv = new Canvas();
            var p = new Path();
            var b = new Binding
            {
                Source =
                    "m 0,0 c 0,0 0.804,0.702 -0.604,1.908 0,0 -2.958,1.423 -5.141,4.927 0,0 -1.792,1.835 -1.73,2.506 -0.403,2.921 -0.88,2.841 -0.752,3.267 -1.14,1.398 0.181,4.944 2.25,1.956 0.044,0 1.169,-2.459 1.2,-2.426 0,0 0.871,-2.064 1.654,-2.527 1.366,0.05 0.633,6.248 0.81,6.673 0,0 -0.236,3.706 -0.083,10.864 0.344,3.79 2.509,1.769 2.734,0.728 0.274,-0.044 0.803,-9.988 1.126,-10.084 0,0 2.614,2.01 3.92,-0.905 0,0 2.615,1.508 3.619,-1.408 0,0 3.519,-0.301 3.217,-3.519 0,0 0.738,-6.031 -1.473,-10.252 L 10.21,-0.102 0,0 z"
            };
            BindingOperations.SetBinding(p, Path.DataProperty, b);

            p.Fill = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"));
            p.Stroke = new SolidColorBrush(Helpers.GetColorFromString("#ffffff"));
            p.StrokeThickness = 1;
            p.RenderTransform = new MatrixTransform
            {
                Matrix =
                    new Matrix(_deviceRenderings.Count > 0 ? -2.1443171 : 2.1443171, 0, 0, -2.1443171, 2.7481961,
                        62.730957)
            };

            cnv.Children.Add(p);
            cnv.RenderTransform = new TranslateTransform();
            cnv.IsHitTestVisible = false;

            return cnv;
        }

        private FrameworkElement createPen()
        {
            var cnv = new Canvas();
            var p = new Path();
            var b = new Binding
            {
                Source =
                    "M 0.03571429,0.57142857 -1.025,0.576 l -0.943,-2.875 2.97,0 z M -6.570801,-11.634639 l 1.248,-22.444 c 5.34699995,-2.227 8.843,0 8.843,0 l 1.634,22.456 -11.725,-0.012 z m 7.7812978,8.848561 -3.422,-0.021 -4.325,-7.304 11.582,0.003 -3.835,7.322 z"
            };
            BindingOperations.SetBinding(p, Path.DataProperty, b);


            var brush = (SolidColorBrush) Application.Current.Resources["highlightBrush"];
            p.Fill = brush;
            brush = (SolidColorBrush) Application.Current.Resources["backgroundBrush"];
            p.Stroke = brush;
            p.StrokeThickness = 1;
            p.RenderTransform = new MatrixTransform
            {
                Matrix = new Matrix(1.8108176, -0.86759842, -0.98659698, -2.0591868, 2.4069217, 0.87161333)
            };

            cnv.Children.Add(p);
            cnv.RenderTransform = new TranslateTransform();
            cnv.IsHitTestVisible = false;

            return cnv;
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            clearAndDisposeMenus();
            var ds = (sender as AppBarButton).DataContext as DatasetConfiguration;
            MainViewController.Instance.LoadData(ds);
        }


        private void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                _mainPointerManagerPreviousPoint =
                    gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);
            }
            else if (e.NumActiveContacts == 2)
            {
                var gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                var p1 = gt.TransformPoint(e.CurrentContacts.Values.ToList()[0].Position);
                var p2 = gt.TransformPoint(e.CurrentContacts.Values.ToList()[1].Position);
                _mainPointerManagerPreviousPoint = ((p1.GetVec() + p2.GetVec())/2.0).GetWindowsPoint();
                _length = (p1.GetVec() - p2.GetVec()).Length;
            }
        }

        private void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                var currentPoint = gt.TransformPoint(e.CurrentContacts[e.TriggeringPointer.PointerId].Position);

                var delta = _mainPointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();

                var xform = MainViewController.Instance.InkableScene.RenderTransform as MatrixTransform;
                Mat matrix = xform.Matrix;
                //Point center = e.Position;
                //matrix = Mat.Translate(-center.X, -center.Y) * matrix;
                //matrix = Mat.Scale(delta.Scale, delta.Scale) * matrix;
                //matrix = Mat.Translate(+center.X, +center.Y) * matrix;
                matrix = Mat.Translate(-delta.X, -delta.Y)*matrix;
                MainViewController.Instance.InkableScene.RenderTransform = new MatrixTransform
                {
                    Matrix = matrix
                };

                _mainPointerManagerPreviousPoint = currentPoint;
            }
            else if (e.NumActiveContacts == 2)
            {
                var gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                var p1 = gt.TransformPoint(e.CurrentContacts.Values.ToList()[0].Position);
                var p2 = gt.TransformPoint(e.CurrentContacts.Values.ToList()[1].Position);

                var currentPoint = ((p1.GetVec() + p2.GetVec())/2.0).GetWindowsPoint();
                var currentLength = (p1.GetVec() - p2.GetVec()).Length;

                var s = currentLength/_length;

                var delta = _mainPointerManagerPreviousPoint.GetVec() - currentPoint.GetVec();

                var xform = MainViewController.Instance.InkableScene.RenderTransform as MatrixTransform;
                Mat matrix = xform.Matrix;
                matrix =
                    Mat.Translate(currentPoint.X, currentPoint.Y)*
                    Mat.Scale(s, s)*
                    Mat.Translate(-currentPoint.X, -currentPoint.Y)*
                    matrix;
                MainViewController.Instance.InkableScene.RenderTransform = new MatrixTransform
                {
                    Matrix = matrix
                };

                _mainPointerManagerPreviousPoint = currentPoint;
                _length = currentLength;
            }
        }

        private void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var gt = MainViewController.Instance.InkableScene.TransformToVisual(this);
                _mainPointerManagerPreviousPoint = gt.TransformPoint(e.CurrentContacts.Values.ToList()[0].Position);
            }
        }

        private void notesButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (notesBox.Visibility == Visibility.Collapsed)
            {
                notesBox.Visibility = Visibility.Visible;
            }
            else
            {
                notesBox.Visibility = Visibility.Collapsed;
            }*/
            MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled = !MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled;
            Debug.WriteLine("IsDefaultHypothesisEnabled : " + MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled);

            msgTextBlock.Text = "IsDefaultHypothesisEnabled : " + MainViewController.Instance.MainModel.IsDefaultHypothesisEnabled;
            msgTextBlock.Opacity = 1;
            _messageTimer.Start();
        }


        private void hypothesisButton_Click(object sender, RoutedEventArgs e)
        {
            if (_hypothesisMenuView == null)
            {
                _hypothesisMenuViewModel = new MenuViewModel
                {
                    AttachmentOrientation = AttachmentOrientation.Left,
                    NrColumns = 3,
                    NrRows = 1,
                    MoveOnHide = true
                };

                var sliderItem = new MenuItemViewModel
                {
                    MenuViewModel = _hypothesisMenuViewModel,
                    Row = 0,
                    ColumnSpan = 1,
                    RowSpan = 1,
                    Column = 0,
                    Position = new Pt(menuHypothesisGrid.ActualWidth, 0),
                    Size = new Vec(100, 50),
                    TargetSize = new Vec(100, 50),
                    IsAlwaysDisplayed = false,
                    IsWidthBoundToParent = false,
                    IsHeightBoundToParent = false
                };

                var attr1 = new SliderMenuItemComponentViewModel
                {
                    Label = "alpha",
                    Value = 500,
                    MinValue = 1,
                    MaxValue = 1000,
                    Formatter = d => { return Math.Round(d/100.0, 1).ToString("F2") + "%"; }
                };
                attr1.PropertyChanged += (sender2, args) =>
                {
                    var model = sender2 as SliderMenuItemComponentViewModel;
                    if (args.PropertyName == model.GetPropertyName(() => model.FinalValue))
                    {
                        var tt = Math.Round(model.FinalValue/100.0, 1)*100.0;
                        //exampleOperationViewModel.ExampleOperationModel.DummyValue = model.FinalValue;
                    }
                };

                sliderItem.MenuItemComponentViewModel = attr1;
                _hypothesisMenuViewModel.MenuItemViewModels.Add(sliderItem);


                // FDR
                var toggles = new List<ToggleMenuItemComponentViewModel>();
                var items = new List<MenuItemViewModel>();

                var count = 1;
                foreach (var riskCtrlType in HypothesesViewController.SupportedRiskControlTypes)
                {
                    var toggleMenuItem = new MenuItemViewModel
                    {
                        MenuViewModel = _hypothesisMenuViewModel,
                        Row = 0,
                        RowSpan = 0,
                        Position = new Pt(menuHypothesisGrid.ActualWidth, 0),
                        Column = count,
                        Size = new Vec(75, 50),
                        TargetSize = new Vec(75, 50)
                    };
                    //toggleMenuItem.Position = attachmentItemViewModel.Position;
                    var toggle = new ToggleMenuItemComponentViewModel
                    {
                        Label = riskCtrlType.ToString(),
                        IsChecked = HypothesesViewController.Instance.RiskOperationModel.RiskControlType == riskCtrlType
                    };
                    toggles.Add(toggle);
                    toggleMenuItem.MenuItemComponentViewModel = toggle;
                    toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                    {
                        var model = sender2 as ToggleMenuItemComponentViewModel;
                        if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        {
                            if (model.IsChecked)
                            {
                                HypothesesViewController.Instance.RiskOperationModel.RiskControlType = riskCtrlType;
                                foreach (var tg in model.OtherToggles)
                                {
                                    tg.IsChecked = false;
                                }
                            }
                        }
                    };
                    _hypothesisMenuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                    items.Add(toggleMenuItem);
                    count++;
                }

                foreach (var mi in items)
                {
                    (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                }

                if (_hypothesisMenuView != null)
                {
                    menuHypothesisGrid.Children.Remove(_hypothesisMenuView);
                }

                _hypothesisMenuView = new MenuView
                {
                    DataContext = _hypothesisMenuViewModel
                };
                _hypothesisMenuViewModel.AnkerPosition = new Pt(menuHypothesisGrid.ActualWidth, -((_hypothesisMenuViewModel.NrRows - 1)*50 + (_hypothesisMenuViewModel.NrRows - 1)*4));
                _hypothesisMenuViewModel.HidePosition = new Pt(menuHypothesisGrid.ActualWidth, 0);
                menuHypothesisGrid.Children.Add(_hypothesisMenuView);
            }

            _hypothesisMenuViewModel.IsDisplayed = !_hypothesisMenuViewModel.IsDisplayed;
        }

        private void addAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainModel = DataContext as MainModel;
            if ((_attributeMenu != null) && ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded)
            {
                ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded = false;
            }
            else if ((_attributeMenu != null) && !((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded)
            {
                ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded = true;
            }
            else if ((_attributeMenu == null) && (mainModel.SchemaModel != null))
            {
                var buttonBounds = addAttributeButton.GetBounds(this);
                var inputModels =
                    mainModel.SchemaModel.OriginModels.First()
                        .InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;

                if (_attributeMenu != null)
                {
                    ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel) _attributeMenu.DataContext).IsBeingRemoved = true;
                    _attributeMenu.Dispose();
                    menuCanvas.Children.Remove(_attributeMenu);
                }

                var parentModel = new TileMenuItemViewModel(null);
                parentModel.ChildrenNrColumns = (int) Math.Ceiling(inputModels.Count()/10.0);
                parentModel.ChildrenNrRows = (int) Math.Min(10.0, inputModels.Count());
                parentModel.Alignment = Alignment.Center;
                parentModel.AttachPosition = AttachPosition.Right;

                var count = 0;
                foreach (var inputModel in inputModels)
                {
                    var tileMenuItemViewModel = recursiveCreateTileMenu(inputModel, parentModel);
                    tileMenuItemViewModel.Row = count;
                    tileMenuItemViewModel.Column = parentModel.ChildrenNrColumns -
                                                   (int) Math.Floor((parentModel.Children.Count - 1)/10.0) - 1;
                    tileMenuItemViewModel.RowSpan = 1;
                    tileMenuItemViewModel.ColumnSpan = 1;
                   
                    count++;
                    if (count == 10.0)
                    {
                        count = 0;
                    }
                }

                _attributeMenu = new TileMenuItemView {MenuCanvas = menuCanvas, DataContext = parentModel};
                menuCanvas.Children.Add(_attributeMenu);

                parentModel.CurrentPosition = new Pt(-buttonBounds.Width, buttonBounds.Top);
                parentModel.TargetPosition = new Pt(-buttonBounds.Width, buttonBounds.Top);
                parentModel.Size = new Vec(buttonBounds.Width, buttonBounds.Height);
                parentModel.AreChildrenExpanded = true;
            }
        }

        private TileMenuItemViewModel recursiveCreateTileMenu(object inputModel, TileMenuItemViewModel parent)
        {
            TileMenuItemViewModel currentTileMenuItemViewModel = null;
            if (inputModel is AttributeGroupModel)
            {
                var inputGroupModel = inputModel as AttributeGroupModel;
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                var inputGroupViewModel = new InputGroupViewModel(null, inputGroupModel);
                currentTileMenuItemViewModel.TileMenuContentViewModel = new InputGroupViewTileMenuContentViewModel
                {
                    Name = inputGroupModel.DisplayName,
                    InputGroupViewModel = inputGroupViewModel
                };

                currentTileMenuItemViewModel.ChildrenNrColumns =
                    (int) Math.Ceiling(inputGroupModel.InputModels.Count()/10.0);
                currentTileMenuItemViewModel.ChildrenNrRows = (int) Math.Min(10.0, inputGroupModel.InputModels.Count());
                currentTileMenuItemViewModel.Alignment = Alignment.Center;
                currentTileMenuItemViewModel.AttachPosition = AttachPosition.Right;

                var count = 0;
                foreach (var childInputModel in inputGroupModel.InputModels /*.OrderBy(am => am.RawName)*/)
                {
                    var childTileMenu = recursiveCreateTileMenu(childInputModel, currentTileMenuItemViewModel);
                    childTileMenu.Row = count; // TileMenuItemViewModel.Children.Count;
                    childTileMenu.Column = currentTileMenuItemViewModel.ChildrenNrColumns - 1 -
                                           (int) Math.Floor((currentTileMenuItemViewModel.Children.Count - 1)/10.0);
                    childTileMenu.RowSpan = 1;
                    childTileMenu.ColumnSpan = 1;
                    //currentTileMenuItemViewModel.Children.Add(childTileMenu);
                    count++;
                    if (count == 10.0)
                    {
                        count = 0;
                    }
                }
            }
            else if (inputModel is AttributeFieldModel)
            {
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                var attributeTransformationViewModel = new AttributeTransformationViewModel(null,
                    new AttributeTransformationModel(inputModel as AttributeFieldModel));
                currentTileMenuItemViewModel.TileMenuContentViewModel = new InputFieldViewTileMenuContentViewModel
                {
                    Name = (inputModel as AttributeFieldModel).RawName,
                    AttributeTransformationViewModel = attributeTransformationViewModel
                };
            }
            else if (inputModel is OperationTypeGroupModel)
            {
                var taskGroupModel = inputModel as OperationTypeGroupModel;
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                currentTileMenuItemViewModel.TileMenuContentViewModel = new OperationTypeGroupTileMenuContentViewModel
                {
                    Name = taskGroupModel.Name,
                    OperationTypeGroupModel = taskGroupModel
                };

                currentTileMenuItemViewModel.ChildrenNrColumns =
                    (int) Math.Ceiling(taskGroupModel.OperationTypeModels.Count()/10.0);
                currentTileMenuItemViewModel.ChildrenNrRows = (int) Math.Min(10.0, taskGroupModel.OperationTypeModels.Count());
                currentTileMenuItemViewModel.Alignment = Alignment.Center;
                currentTileMenuItemViewModel.AttachPosition = AttachPosition.Right;

                var count = 0;
                foreach (var childInputModel in taskGroupModel.OperationTypeModels /*.OrderBy(am => am.RawName)*/)
                {
                    var childTileMenu = recursiveCreateTileMenu(childInputModel, currentTileMenuItemViewModel);
                    childTileMenu.Row = count; // TileMenuItemViewModel.Children.Count;
                    childTileMenu.Column = currentTileMenuItemViewModel.ChildrenNrColumns - 1 -
                                           (int) Math.Floor((currentTileMenuItemViewModel.Children.Count - 1)/10.0);
                    childTileMenu.RowSpan = 1;
                    childTileMenu.ColumnSpan = 1;
                    //currentTileMenuItemViewModel.Children.Add(childTileMenu);
                    count++;
                    if (count == 10.0)
                    {
                        count = 0;
                    }
                }
            }
            else if (inputModel is OperationTypeModel)
            {
                currentTileMenuItemViewModel = new TileMenuItemViewModel(parent);
                currentTileMenuItemViewModel.TileMenuContentViewModel = new OperationTypeTileMenuContentViewModel
                {
                    Name = (inputModel as OperationTypeModel).Name,
                    OperationTypeModel = inputModel as OperationTypeModel
                };
            }
            parent.Children.Add(currentTileMenuItemViewModel);
            currentTileMenuItemViewModel.Alignment = Alignment.Center;
            currentTileMenuItemViewModel.AttachPosition = AttachPosition.Right;
            return currentTileMenuItemViewModel;
        }

        private void addOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_operationMenu != null) && ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded)
            {
                ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded = false;
            }
            else if ((_operationMenu != null) && !((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded)
            {
                ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded = true;
            }
            else if (_operationMenu == null)
            {
                var mainModel = DataContext as MainModel;
                var buttonBounds = addOperationButton.GetBounds(this);
                var taskModels =
                    mainModel.OperationTypeModels;

                if (_operationMenu != null)
                {
                    ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded = false;
                    ((TileMenuItemViewModel) _operationMenu.DataContext).IsBeingRemoved = true;
                    _operationMenu.Dispose();
                    menuCanvas.Children.Remove(_operationMenu);
                }

                var parentModel = new TileMenuItemViewModel(null);
                parentModel.ChildrenNrColumns = (int) Math.Ceiling(taskModels.Count()/10.0);
                parentModel.ChildrenNrRows = (int) Math.Min(10.0, taskModels.Count());
                parentModel.Alignment = Alignment.Center;
                parentModel.AttachPosition = AttachPosition.Right;

                var count = 0;
                foreach (var inputModel in taskModels)
                {
                    var tileMenuItemViewModel = recursiveCreateTileMenu(inputModel, parentModel);
                    tileMenuItemViewModel.Row = count;
                    tileMenuItemViewModel.Column = parentModel.ChildrenNrColumns - (int) Math.Floor((parentModel.Children.Count - 1)/10.0) - 1;
                    tileMenuItemViewModel.RowSpan = 1;
                    tileMenuItemViewModel.ColumnSpan = 1;
                    Debug.WriteLine(inputModel.Name + " c: " + tileMenuItemViewModel.Column + " r : " + tileMenuItemViewModel.Row);
                    count++;
                    if (count == 10.0)
                    {
                        count = 0;
                    }
                }

                _operationMenu = new TileMenuItemView {MenuCanvas = menuCanvas, DataContext = parentModel};
                menuCanvas.Children.Add(_operationMenu);

                parentModel.CurrentPosition = new Pt(-buttonBounds.Width, buttonBounds.Top);
                parentModel.TargetPosition = new Pt(-buttonBounds.Width, buttonBounds.Top);
                parentModel.Size = new Vec(buttonBounds.Width, buttonBounds.Height);
                parentModel.AreChildrenExpanded = true;
            }
        }

        private void CloseButton_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            codeGrid.Visibility = Visibility.Collapsed;
        }

        private void clearAndDisposeMenus()
        {
            if (_operationMenu != null)
            {
                ((TileMenuItemViewModel) _operationMenu.DataContext).AreChildrenExpanded = false;
                ((TileMenuItemViewModel) _operationMenu.DataContext).IsBeingRemoved = true;
                _operationMenu.Dispose();
                menuCanvas.Children.Remove(_operationMenu);
                _operationMenu = null;
            }
            if (_attributeMenu != null)
            {
                ((TileMenuItemViewModel) _attributeMenu.DataContext).AreChildrenExpanded = false;
                ((TileMenuItemViewModel) _attributeMenu.DataContext).IsBeingRemoved = true;
                _attributeMenu.Dispose();
                menuCanvas.Children.Remove(_attributeMenu);
                _attributeMenu = null;
            }
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            clearAndDisposeMenus();
            MainViewController.Instance.LoadConfig();
        }
    }
}