using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class ComparisonViewController
    {
        private Dictionary<OperationViewModel, DateTime> _lastMoved = new Dictionary<OperationViewModel, DateTime>();
        private static ComparisonViewController _instance;

        private DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();

        public ObservableDictionary<StatisticalComparisonViewModel, ComparisonView> StatisticalComparisonViews = new ObservableDictionary<StatisticalComparisonViewModel, ComparisonView>();

        public static void CreateInstance(ObservableCollection<OperationViewModel> operationViewModel)
        {
            _instance = new ComparisonViewController(operationViewModel);
        }

        public static ComparisonViewController Instance
        {
            get
            {
                return _instance;
            }
        }

        private ComparisonViewController(ObservableCollection<OperationViewModel> operationViewModel)
        {
            StatisticalComparisonViews.CollectionChanged += StatisticalComparisonViewsCollectionChanged;
            operationViewModel.CollectionChanged += OperationViewModels_CollectionChanged;

            _operationViewMovingTimer.Interval = TimeSpan.FromMilliseconds(20);
            _operationViewMovingTimer.Tick += operationViewMovingTimer_Tick;
            _operationViewMovingTimer.Start();
        }


        private void OperationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var current = sender as OperationViewModel;
            if (e.PropertyName == current.GetPropertyName(() => current.Position))
            {
                operationViewModelUpdated(current);
            }
        }

        private void operationViewModelUpdated(OperationViewModel current)
        {
            if (current.OperationModel is IBrusherOperationModel)
            {
                // update last moved time
                _lastMoved[current] = DateTime.Now;

                // check if we need to create new inputvisualization views
                foreach (var other in OperationViewModels.Select(c => c as HistogramOperationViewModel).Where(c => c != null))
                {
                    var diff = current.Position - other.Position;

                    bool areLinked = false;
                    foreach (var linkModel in current.HistogramOperationModel.LinkModels)
                    {
                        if ((linkModel.FromOperationModel == current.HistogramOperationModel && linkModel.ToOperationModel == other.HistogramOperationModel) ||
                            (linkModel.FromOperationModel == other.HistogramOperationModel && linkModel.ToOperationModel == current.HistogramOperationModel))
                        {
                            areLinked = true;
                        }
                    }
                    if (!areLinked)
                    {
                        // check for comparision views
                        if (Math.Abs(diff.Y) < 300 &&
                            boundHorizontalDistance(current.Bounds, other.Bounds) < 200 &&
                            _lastMoved.ContainsKey(other) &&
                            other != current &&
                            Math.Abs((_lastMoved[other] - _lastMoved[current]).TotalMilliseconds) < 400)
                        {
                            if (!ComparisonViews.Keys.Any(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other)))
                            {
                                ComparisonViewModel comparisonViewModel = new ComparisonViewModel();
                                comparisonViewModel.VisualizationViewModels.Add(other);
                                comparisonViewModel.VisualizationViewModels.Add(current);
                                comparisonViewModel.Position =
                                    (((comparisonViewModel.VisualizationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()))/2.0) - comparisonViewModel.Size/2.0).GetWindowsPoint();

                                comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Opening;
                                comparisonViewModel.DwellStartPosition = current.Position;
                                comparisonViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                ComparisonView view = new ComparisonView();
                                view.DataContext = comparisonViewModel;
                                InkableScene.Children.Add(view);
                                ComparisonViews.Add(comparisonViewModel, view);
                            }
                        }

                        // check for inputvisualization views
                        else if (Math.Abs(diff.Y) < 300 &&
                                 boundHorizontalDistance(current.Bounds, other.Bounds) < 50)
                        {
                            if (!InputVisualizationViews.Keys.Any(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other)))
                            {
                                List<BrushViewModel> inputCohorts = InputVisualizationViews.Keys.Where(icv => icv.To == other).ToList();

                                var allColorIndex = Enumerable.Range(0, BrushViewModel.ColorScheme1.Count);
                                allColorIndex = allColorIndex.Except(inputCohorts.Select(c => c.ColorIndex));
                                var colorIndex = inputCohorts.Count%BrushViewModel.ColorScheme1.Count;
                                if (allColorIndex.Any())
                                {
                                    colorIndex = allColorIndex.First();
                                }

                                BrushViewModel brushViewModel = new BrushViewModel();
                                brushViewModel.ColorIndex = colorIndex;
                                brushViewModel.Color = BrushViewModel.ColorScheme1[colorIndex];
                                brushViewModel.OperationViewModels.Add(other);
                                brushViewModel.OperationViewModels.Add(current);
                                brushViewModel.Position =
                                    (((brushViewModel.OperationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()))/2.0) - brushViewModel.Size/2.0).GetWindowsPoint();

                                brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opening;
                                brushViewModel.DwellStartPosition = current.Position;
                                brushViewModel.From = current;
                                brushViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                BrushView view = new BrushView();
                                view.DataContext = brushViewModel;
                                InkableScene.Children.Add(view);
                                InputVisualizationViews.Add(brushViewModel, view);
                            }
                            else
                            {
                                var inputModel = InputVisualizationViews.Keys.First(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other));
                                inputModel.From = current;
                            }
                        }
                    }
                }
            }
    }

        private void checkOpenOrCloseComparisionModels(bool dropped = false)
        {
            /*// views that need to be opened or closed
            foreach (var comparisonViewModel in ComparisonViews.Keys.ToList())
            {
                var model = comparisonViewModel;

                var diff = comparisonViewModel.VisualizationViewModels[0].Position - comparisonViewModel.VisualizationViewModels[1].Position;

                // views to open
                if (Math.Abs(diff.Y) < 300 &&
                    boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) < 300 &&
                    (dropped || DateTime.Now.Ticks > TimeSpan.TicksPerSecond*1 + model.TicksSinceDwellStart))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Opened;
                }

                bool areLinked = false;
                foreach (var linkModel in comparisonViewModel.VisualizationViewModels.First().HistogramOperationModel.LinkModels)
                {
                    if ((linkModel.FromOperationModel == comparisonViewModel.VisualizationViewModels[0].HistogramOperationModel && linkModel.ToOperationModel == comparisonViewModel.VisualizationViewModels[1].HistogramOperationModel) ||
                        (linkModel.FromOperationModel == comparisonViewModel.VisualizationViewModels[1].HistogramOperationModel && linkModel.ToOperationModel == comparisonViewModel.VisualizationViewModels[0].HistogramOperationModel))
                    {
                        areLinked = true;
                    }
                }


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opening && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opened && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    comparisonViewModel.VisualizationViewModels.Any(c => !OperationViewModels.Contains(c)))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Closing;
                    var view = ComparisonViews[comparisonViewModel];
                    ComparisonViews.Remove(comparisonViewModel);

                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        InkableScene.Children.Remove(view);
                    });

                }
            }*/
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
                        //opViewModel.OperationViewModelTapped -= OpViewModel_OperationViewModelTapped;
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
                        //opViewModel.OperationViewModelTapped += OpViewModel_OperationViewModelTapped;
                    }
                }
            }
        }


        private void operationViewMovingTimer_Tick(object sender, object e)
        {
            checkOpenOrCloseComparisionModels();
        }

        private void StatisticalComparisonViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = ((KeyValuePair<StatisticalComparisonViewModel, ComparisonView>)item).Key;
                    var toModel = (IBrushableOperationModel)current.To.OperationModel;
                    var index = toModel.BrushOperationModels.IndexOf(current.From.OperationModel as IBrushableOperationModel);
                    toModel.BrushColors.RemoveAt(index);
                    toModel.BrushOperationModels.RemoveAt(index);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = ((KeyValuePair<BrushViewModel, BrushView>)item).Key;
                    var toModel = (IBrushableOperationModel)current.To.OperationModel;
                    toModel.BrushColors.Add(current.Color);
                    toModel.BrushOperationModels.Add(current.From.OperationModel as IBrushableOperationModel);
                }
            }
        }
    }
}
