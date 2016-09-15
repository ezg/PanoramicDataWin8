using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.AllJoyn;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class BrushableViewController
    {
        private static BrushableViewController _instance;

        private DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();

        public ObservableDictionary<BrushViewModel, BrushView> BrushViews = new ObservableDictionary<BrushViewModel, BrushView>();

        public static void CreateInstance(ObservableCollection<OperationViewModel> operationViewModel)
        {
            _instance = new BrushableViewController(operationViewModel);
        }

        private BrushableViewController(ObservableCollection<OperationViewModel> operationViewModel)
        {
            BrushViews.CollectionChanged += BrushViewsCollectionChanged;
            operationViewModel.CollectionChanged += OperationViewModels_CollectionChanged;

            _operationViewMovingTimer.Interval = TimeSpan.FromMilliseconds(20);
            _operationViewMovingTimer.Tick += operationViewMovingTimer_Tick;
            _operationViewMovingTimer.Start();
        }

        private void operationViewMovingTimer_Tick(object sender, object e)
        {
            checkOpenOrCloseInputVisualizationModels();
        }
        private void checkOpenOrCloseInputVisualizationModels(bool dropped = false)
        {
            // views that need to be opened or closed
            foreach (var brushViewModel in BrushViews.Keys.ToList())
            {
                var diff = brushViewModel.OperationViewModels[0].Position - brushViewModel.OperationViewModels[1].Position;

                // views to open
                if (Math.Abs(diff.Y) < 300 &&
                    boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) < 50 &&
                    (dropped || DateTime.Now.Ticks > TimeSpan.TicksPerSecond * 1 + brushViewModel.TicksSinceDwellStart))
                {
                    brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opened;
                }

                bool areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(brushViewModel.OperationViewModels[0], brushViewModel.OperationViewModels[1]);


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (brushViewModel.BrushableOperationViewModelState == BrushableOperationViewModelState.Opening && boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) >= 50) ||
                    (brushViewModel.BrushableOperationViewModelState == BrushableOperationViewModelState.Opened && boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) >= 50) //||
                    )//brushViewModel.OperationViewModels.Any(c => !BrushViews.Contains(c as Brush)))
                {
                    brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Closing;
                    var view = BrushViews[brushViewModel];
                    BrushViews.Remove(brushViewModel);

                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        MainViewController.Instance.InkableScene.Children.Remove(view);
                    });

                }
            }
        }

        private void OperationViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var opViewModel in e.OldItems.OfType<OperationViewModel>())
                {
                    if (opViewModel.OperationModel is IBrushableOperationModel)
                    {
                        opViewModel.PropertyChanged -= OperationViewModel_PropertyChanged;
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var opViewModel in e.NewItems.OfType<OperationViewModel>())
                {
                    if (opViewModel.OperationModel is IBrushableOperationModel)
                    {
                        opViewModel.PropertyChanged += OperationViewModel_PropertyChanged;
                    }
                }
            }
        }

        private double boundHorizontalDistance(Rct b1, Rct b2)
        {
            return Math.Min(Math.Abs(b1.Right - b2.Left), Math.Abs(b1.Left - b2.Right));
        }

        
        private Dictionary<OperationViewModel, DateTime> _lastMoved = new Dictionary<OperationViewModel, DateTime>();
        private void OperationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var current = sender as OperationViewModel;
            if (e.PropertyName == current.GetPropertyName(() => current.Position))
            {

                // update last moved time
                _lastMoved[current] = DateTime.Now;

                var allBrushableOperationViewModels = MainViewController.Instance.OperationViewModels.Where(c => c.OperationModel is IBrushableOperationModel);
                // check if we need to create new inputvisualization views
                foreach (var other in allBrushableOperationViewModels)
                {
                    var diff = current.Position - other.Position;

                    bool areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(current, other);
                    if (!areLinked)
                    {
                        if (Math.Abs(diff.Y) < 300 &&
                            boundHorizontalDistance(current.Bounds, other.Bounds) < 50)
                        {
                            if (!BrushViews.Keys.Any(sov => sov.OperationViewModels.Contains(current) && sov.OperationViewModels.Contains(other)))
                            {
                                List<BrushViewModel> inputCohorts = BrushViews.Keys.Where(icv => icv.To == other).ToList();

                                var allColorIndex = Enumerable.Range(0, BrushViewModel.ColorScheme1.Count);
                                allColorIndex = allColorIndex.Except(inputCohorts.Select(c => c.ColorIndex));
                                var colorIndex = inputCohorts.Count % BrushViewModel.ColorScheme1.Count;
                                if (allColorIndex.Any())
                                {
                                    colorIndex = allColorIndex.First();
                                }

                                BrushViewModel brushViewModel = new BrushViewModel();
                                brushViewModel.ColorIndex = colorIndex;
                                brushViewModel.Color = BrushViewModel.ColorScheme1[colorIndex];
                                brushViewModel.From = current;
                                brushViewModel.To = other;
                                brushViewModel.Position =
                                    (((brushViewModel.OperationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec())) / 2.0) - brushViewModel.Size / 2.0).GetWindowsPoint();

                                brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opening;
                                brushViewModel.DwellStartPosition = current.Position;
                                brushViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                BrushView view = new BrushView();
                                view.DataContext = brushViewModel;
                                MainViewController.Instance.InkableScene.Children.Add(view);
                                BrushViews.Add(brushViewModel, view);
                            }
                            else
                            {
                                var inputModel = BrushViews.Keys.First(sov => sov.OperationViewModels.Contains(current) && sov.OperationViewModels.Contains(other));
                                inputModel.From = current;
                            }
                        }
                    }
                }
            }
        }

        public static BrushableViewController Instance
        {
            get
            {
                return _instance;
            }
        }

        private void BrushViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = ((KeyValuePair<BrushViewModel, BrushView>) item).Key;
                    ((IBrushableOperationModel) current.To.OperationModel).BrushOperationModels.Remove(current.From.OperationModel);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = ((KeyValuePair<BrushViewModel, BrushView>) item).Key;
                    ((IBrushableOperationModel) current.To.OperationModel).BrushOperationModels.Add(current.From.OperationModel);
                }
            }
        }
    }
}