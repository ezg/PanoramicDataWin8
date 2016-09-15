using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IBrushableOperationModel
    {
        ObservableCollection<OperationModel> BrushOperationModels { get; set; }
        List<Color> BrushColors { get; set; }
    }

    public interface IBrusherOperationModel
    {
    }

    public class BrushableOperationModelImpl : ExtendedBindableBase, IBrushableOperationModel
    {
        private ObservableCollection<OperationModel> _brushOperationModels = new ObservableCollection<OperationModel>();
        private OperationModel _host;

        public BrushableOperationModelImpl(OperationModel host)
        {
            _host = host;
            _brushOperationModels.CollectionChanged += BrushOperationModelsCollectionChanged;
        }

        public ObservableCollection<OperationModel> BrushOperationModels
        {
            get { return _brushOperationModels; }
            set { SetProperty(ref _brushOperationModels, value); }
        }

        public List<Color> BrushColors { get; set; }

        private void BrushOperationModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = (OperationModel) item;
                    current.OperationModelUpdated -= Current_OperationModelUpdated;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = (OperationModel) item;
                    current.OperationModelUpdated += Current_OperationModelUpdated;
                }
            }
            _host.FireOperationModelUpdated(new BrushOperationModelUpdatedEventArgs());
        }

        private void Current_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            _host.FireOperationModelUpdated(new BrushOperationModelUpdatedEventArgs());
        }
    }

    public class BrushOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
    }
}