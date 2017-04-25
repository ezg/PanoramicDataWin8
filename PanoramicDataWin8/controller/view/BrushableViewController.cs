using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class BrushableViewController
    {
        private readonly Dictionary<OperationViewModel, DateTime> _lastMoved = new Dictionary<OperationViewModel, DateTime>();

        private readonly DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();

        public ObservableDictionary<BrushViewModel, BrushView> BrushViews = new ObservableDictionary<BrushViewModel, BrushView>();

        private BrushableViewController(ObservableCollection<OperationViewModel> operationViewModel)
        {
            BrushViews.CollectionChanged += BrushViewsCollectionChanged;
            operationViewModel.CollectionChanged += OperationViewModels_CollectionChanged;

            _operationViewMovingTimer.Interval = TimeSpan.FromMilliseconds(20);
            _operationViewMovingTimer.Tick += operationViewMovingTimer_Tick;
            _operationViewMovingTimer.Start();
        }

        public void Remove(IBrushableOperationModel bmodel)
        {
            foreach (var view in BrushViews.ToArray()) {
                if (view.Key.From.OperationModel == bmodel || view.Key.To.OperationModel == bmodel) {
                    BrushViews.Remove(view);
                    view.Value.DataContext = null;
                    MainViewController.Instance.InkableScene.Children.Remove(view.Value);
                }
            }
        }

        public static BrushableViewController Instance { get; private set; }

        public static void CreateInstance(ObservableCollection<OperationViewModel> operationViewModel)
        {
            Instance = new BrushableViewController(operationViewModel);
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

                var currentRect = RectHelper.FromLocationAndSize(brushViewModel.OperationViewModels[0].Position, 
                                     (Windows.Foundation.Size)brushViewModel.OperationViewModels[0].Size);
                var otherRect = RectHelper.FromLocationAndSize(brushViewModel.OperationViewModels[1].Position, 
                                     (Windows.Foundation.Size)brushViewModel.OperationViewModels[1].Size);

                // views to open
                if (RectYOverlap(currentRect, otherRect) && // (Math.Abs(diff.Y) < 300) &&
                    (boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) < 100) &&
                    (dropped || (DateTime.Now.Ticks > TimeSpan.TicksPerSecond*1 + brushViewModel.TicksSinceDwellStart)))
                    brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opened;

                var areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(brushViewModel.OperationViewModels[0], brushViewModel.OperationViewModels[1]);


                // Views to close
                if (areLinked ||
                    !RectYOverlap(currentRect, otherRect) || //(Math.Abs(diff.Y) >= 300) ||
                    ((brushViewModel.BrushableOperationViewModelState == BrushableOperationViewModelState.Opening) &&
                     (boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) >= 100)) ||
                    ((brushViewModel.BrushableOperationViewModelState == BrushableOperationViewModelState.Opened) &&
                     (boundHorizontalDistance(brushViewModel.OperationViewModels[0].Bounds, brushViewModel.OperationViewModels[1].Bounds) >= 100)) //||
                ) //brushViewModel.OperationViewModels.Any(c => !BrushViews.Contains(c as Brush)))
                {
                    brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Closing;
                    var view = BrushViews[brushViewModel];
                    BrushViews.Remove(brushViewModel);

                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        view.DataContext = null;
                        MainViewController.Instance.InkableScene.Children.Remove(view);
                    });
                }
            }
        }

        private Dictionary<OperationViewModel, IDisposable> _disposables = new Dictionary<OperationViewModel, IDisposable>();
        private async void OperationViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var opViewModel in e.OldItems.OfType<OperationViewModel>())
                {
                    if (opViewModel.OperationModel is IBrushableOperationModel)
                    {
                        //opViewModel.OperationViewModelTapped -= OpViewModel_OperationViewModelTapped;
                        if (_disposables.ContainsKey(opViewModel))
                        {
                            _disposables[opViewModel].Dispose();
                            _disposables.Remove(opViewModel);
                        }

                        Remove((IBrushableOperationModel)opViewModel.OperationModel);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var opViewModel in e.NewItems.OfType<OperationViewModel>())
                {
                    if (opViewModel.OperationModel is IBrushableOperationModel)
                    {
                        //opViewModel.OperationViewModelTapped += OpViewModel_OperationViewModelTapped;
                        opViewModel.PropertyChanged += OperationViewModel_PropertyChanged;

                        IDisposable disposable = Observable.FromEventPattern<PropertyChangedEventArgs>(opViewModel, "PropertyChanged")
                            .Sample(TimeSpan.FromMilliseconds(50))
                            .Subscribe(async arg =>
                            {
                                var dispatcher = MainViewController.Instance.MainPage.Dispatcher;
                                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    OperationViewModel_PropertyChanged(arg.Sender, arg.EventArgs);
                                });
                            });
                        _disposables.Add(opViewModel, disposable);
                    }
                }
            }
        }

        private bool isBrushAllowed(OperationViewModel current, OperationViewModel other)
        {
            var chain = new HashSet<IOperationModel>();
            recursiveCheckForCircularBrushing(other.OperationModel, chain);
            if (chain.Contains(current.OperationModel as IFilterConsumerOperationModel))
            {
                return false;
            }
            
            return true;
        }

        private void recursiveCheckForCircularBrushing(IOperationModel current, HashSet<IOperationModel> chain)
        {            
            if (!chain.Contains(current) && current is IFilterConsumerOperationModel)
            {
                var links = ((IFilterConsumerOperationModel) current).LinkModels;
                foreach (var link in links)
                {
                    chain.Add(link.ToOperationModel);
                    recursiveCheckForCircularBrushing(link.ToOperationModel, chain);
                }
                var brushes = ((IBrushableOperationModel) current).BrushOperationModels;
                foreach (var brush in brushes)
                {
                    chain.Add(brush);
                    recursiveCheckForCircularBrushing(brush, chain);
                }
            }
        }

        private void OpViewModel_OperationViewModelTapped(object sender, EventArgs e)
        {
            operationViewModelUpdated((OperationViewModel) sender);
        }

        private double boundHorizontalDistance(Rct b1, Rct b2)
        {
            return Math.Min(Math.Abs(b1.Right - b2.Left), Math.Abs(b1.Left - b2.Right));
        }

        bool RectYOverlap(Windows.Foundation.Rect r1, Windows.Foundation.Rect r2)
        {
            if (r1.Top > r2.Bottom || r2.Top > r1.Bottom)
                return false;
            return true;
        }
        private void operationViewModelUpdated(OperationViewModel current)
        {
            if (current.OperationModel is IBrusherOperationModel)
            {
                // update last moved time
                _lastMoved[current] = DateTime.Now;

                var allBrushableOperationViewModels = MainViewController.Instance.OperationViewModels.Where(c => c.OperationModel is IBrushableOperationModel);
                // check if we need to create new inputvisualization views
                foreach (var other in allBrushableOperationViewModels)
                {
                    var diff = current.Position - other.Position;
                    var currentRect = RectHelper.FromLocationAndSize(current.Position, (Windows.Foundation.Size)current.Size);
                    var otherRect = RectHelper.FromLocationAndSize(other.Position, (Windows.Foundation.Size)other.Size);

                    var areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(current, other);
                    var isBrushAllowed = this.isBrushAllowed(current, other);
                    if (!areLinked &&
                        (!_lastMoved.ContainsKey(other) || (DateTime.Now - _lastMoved[other] > TimeSpan.FromSeconds(0.5))))
                    {
                        if ((boundHorizontalDistance(current.Bounds, other.Bounds) > 100))
                        {
                            if (BrushViews.Keys.Any(sov => (sov.To == current) && (sov.From == other)))
                            {
                                var oldBrushView = BrushViews.Keys.First(sov => (sov.To == current) && (sov.From == other));
                                oldBrushView.BrushableOperationViewModelState = BrushableOperationViewModelState.Closing;
                                var view = BrushViews[oldBrushView];
                                BrushViews.Remove(oldBrushView);

                                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                                dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                                    view.DataContext = null;
                                    MainViewController.Instance.InkableScene.Children.Remove(view);
                                });
                            }

                        }

                        if (RectYOverlap(currentRect, otherRect) && // (Math.Abs(diff.Y) < 300) &&
                                (boundHorizontalDistance(current.Bounds, other.Bounds) < 100) && isBrushAllowed)
                        {

                            if (!BrushViews.Keys.Any(sov => (sov.To == current) && (sov.From == other)) &&
                                !BrushViews.Keys.Any(sov => (sov.From == current) && (sov.To == other)))
                            {
                                var otherview = other;
                                if (current is HistogramOperationViewModel && otherview is HistogramOperationViewModel &&
                                    (current as HistogramOperationViewModel).HistogramOperationModel.FilterModels.Count == 0 &&
                                    (other as HistogramOperationViewModel).HistogramOperationModel.FilterModels.Count != 0)
                                {
                                    var tmp = current;
                                    current = otherview;
                                    otherview = tmp;
                                }
                                var inputCohorts = BrushViews.Keys.Where(icv => icv.To == otherview).ToList();

                                var allColorIndex = Enumerable.Range(0, BrushViewModel.ColorScheme1.Count);
                                allColorIndex = allColorIndex.Except(inputCohorts.Select(c => c.ColorIndex));
                                var colorIndex = inputCohorts.Count % BrushViewModel.ColorScheme1.Count;
                                if (allColorIndex.Any())
                                    colorIndex = allColorIndex.First();

                                var brushViewModel = new BrushViewModel();
                                brushViewModel.ColorIndex = colorIndex;
                                brushViewModel.Color = BrushViewModel.ColorScheme1[colorIndex];
                                brushViewModel.From = current;
                                brushViewModel.To = otherview;
                                brushViewModel.Position =
                                    (brushViewModel.OperationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()) / 2.0 - brushViewModel.Size / 2.0).GetWindowsPoint();

                                brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opening;
                                brushViewModel.DwellStartPosition = current.Position;
                                brushViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                var view = new BrushView();
                                view.DataContext = brushViewModel;
                                MainViewController.Instance.InkableScene.Children.Add(view);
                                BrushViews.Add(brushViewModel, view);
                            }
                        }
                    }
                }
            }
        }

        private void OperationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var current = sender as OperationViewModel;
            if (e.PropertyName == current.GetPropertyName(() => current.Position))
                operationViewModelUpdated(current);
            else if (e.PropertyName == current.GetPropertyName(() => current.Size))
                operationViewModelUpdated(current);
        }

        private void BrushViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    var current = ((KeyValuePair<BrushViewModel, BrushView>) item).Key;
                    var toModel = (IBrushableOperationModel) current.To.OperationModel;
                    var index = toModel.BrushOperationModels.IndexOf(current.From.OperationModel as IBrushableOperationModel);
                    toModel.BrushColors.RemoveAt(index);
                    toModel.BrushOperationModels.RemoveAt(index);
                }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    var current = ((KeyValuePair<BrushViewModel, BrushView>) item).Key;
                    var toModel = (IBrushableOperationModel) current.To.OperationModel;
                    toModel.BrushColors.Add(current.Color);
                    toModel.BrushOperationModels.Add(current.From.OperationModel as IBrushableOperationModel);
                }
        }
    }
}