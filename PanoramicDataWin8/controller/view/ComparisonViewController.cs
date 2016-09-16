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
        private static ComparisonViewController _instance;

        private DispatcherTimer _operationViewMovingTimer = new DispatcherTimer();

        public ObservableDictionary<ComparisonViewModel, ComparisonView> ComparisonViews = new ObservableDictionary<ComparisonViewModel, ComparisonView>();

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
            ComparisonViews.CollectionChanged += ComparisonViewsCollectionChanged;
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
            checkOpenOrCloseInputVisualizationModels();
        }

        private void ComparisonViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = ((KeyValuePair<ComparisonViewModel, ComparisonView>)item).Key;
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
