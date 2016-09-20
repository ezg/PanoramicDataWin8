using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class ComparisonViewController
    {
        private Dictionary<OperationViewModel, DateTime> _lastMoved = new Dictionary<OperationViewModel, DateTime>();
        private static ComparisonViewController _instance;

        private DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();

        public ObservableDictionary<StatisticalComparisonOperationViewModel, ComparisonView> StatisticalComparisonViews = new ObservableDictionary<StatisticalComparisonOperationViewModel, ComparisonView>();

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
            if (current.OperationModel is IStatisticallyComparableOperationModel)
            {
                // update last moved time
                _lastMoved[current] = DateTime.Now;

                var allComparableOperationViewModels = MainViewController.Instance.OperationViewModels.Where(c => c.OperationModel is IStatisticallyComparableOperationModel);
                // check if we need to create new inputvisualization views
                foreach (var other in allComparableOperationViewModels)
                {
                    var diff = current.Position - other.Position;

                    bool areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(current, other);
                    if (!areLinked)
                    {
                        // check for comparision views
                        if (Math.Abs(diff.Y) < 300 &&
                            boundHorizontalDistance(current.Bounds, other.Bounds) < 200 &&
                            _lastMoved.ContainsKey(other) &&
                            other != current &&
                            Math.Abs((_lastMoved[other] - _lastMoved[current]).TotalMilliseconds) < 400)
                        {
                            if (!StatisticalComparisonViews.Keys.Any(sov => sov.OperationViewModels.Contains(current) && sov.OperationViewModels.Contains(other)))
                            {
                                StatisticalComparisonOperationViewModel comparisonOperationViewModel = new StatisticalComparisonOperationViewModel(
                                    new StatisticalComparisonOperationModel(current.OperationModel.SchemaModel));
                                comparisonOperationViewModel.OperationViewModels.Add(other);
                                comparisonOperationViewModel.OperationViewModels.Add(current);
                                comparisonOperationViewModel.Position =
                                    (((comparisonOperationViewModel.OperationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()))/2.0) - comparisonOperationViewModel.Size/2.0).GetWindowsPoint();

                                comparisonOperationViewModel.ComparisonViewModelState = ComparisonViewModelState.Opening;
                                comparisonOperationViewModel.DwellStartPosition = current.Position;
                                comparisonOperationViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                ComparisonView view = new ComparisonView();
                                view.DataContext = comparisonOperationViewModel;
                                MainViewController.Instance.InkableScene.Children.Add(view);
                                StatisticalComparisonViews.Add(comparisonOperationViewModel, view);
                            }
                        }
                    }
                }
            }
    }

        private void checkOpenOrCloseComparisionModels(bool dropped = false)
        {
            // views that need to be opened or closed
            foreach (var comparisonViewModel in StatisticalComparisonViews.Keys.ToList())
            {
                var model = comparisonViewModel;

                var diff = comparisonViewModel.OperationViewModels[0].Position - comparisonViewModel.OperationViewModels[1].Position;

                // views to open
                if (Math.Abs(diff.Y) < 300 &&
                    boundHorizontalDistance(comparisonViewModel.OperationViewModels[0].Bounds, comparisonViewModel.OperationViewModels[1].Bounds) < 300 &&
                    (dropped || DateTime.Now.Ticks > TimeSpan.TicksPerSecond*1 + model.TicksSinceDwellStart))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Opened;
                }

                bool areLinked = FilterLinkViewController.Instance.AreOperationViewModelsLinked(comparisonViewModel.OperationViewModels[0], comparisonViewModel.OperationViewModels[1]);


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opening && boundHorizontalDistance(comparisonViewModel.OperationViewModels[0].Bounds, comparisonViewModel.OperationViewModels[1].Bounds) >= 300) ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opened && boundHorizontalDistance(comparisonViewModel.OperationViewModels[0].Bounds, comparisonViewModel.OperationViewModels[1].Bounds) >= 300))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Closing;
                    var view = StatisticalComparisonViews[comparisonViewModel];
                    StatisticalComparisonViews.Remove(comparisonViewModel);

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
        private double boundHorizontalDistance(Rct b1, Rct b2)
        {
            return Math.Min(Math.Abs(b1.Right - b2.Left), Math.Abs(b1.Left - b2.Right));
        }

        private void StatisticalComparisonViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = ((KeyValuePair<StatisticalComparisonOperationViewModel, ComparisonView>)item).Key;
                    current.StatisticalComparisonOperationModel.OperationModelUpdated -= OperationModel_OperationModelUpdated;
                    current.StatisticalComparisonOperationModel.StatisticallyComparableOperationModels.Clear();
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = ((KeyValuePair<StatisticalComparisonOperationViewModel, ComparisonView>)item).Key;
                    current.StatisticalComparisonOperationModel.StatisticallyComparableOperationModels.Add((IStatisticallyComparableOperationModel)current.OperationViewModels[0].OperationModel);
                    current.StatisticalComparisonOperationModel.StatisticallyComparableOperationModels.Add((IStatisticallyComparableOperationModel)current.OperationViewModels[1].OperationModel);
                    current.StatisticalComparisonOperationModel.OperationModelUpdated += OperationModel_OperationModelUpdated;
                    current.StatisticalComparisonOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
            }
        }

        private void OperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            OperationModel model = (OperationModel) sender;
            model.SchemaModel.QueryExecuter.ExecuteOperationModel(model);
        }
    }
}
