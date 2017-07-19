using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class BrushableOperationModelImpl : ExtendedBindableBase, IBrushableOperationModel
    {
        private ObservableCollection<IBrusherOperationModel> _brushOperationModels = new ObservableCollection<IBrusherOperationModel>();
        private readonly IOperationModel _host;

        public void Cleanup() { }

        public BrushableOperationModelImpl(IOperationModel host)
        {
            _host = host;
            _brushOperationModels.CollectionChanged += BrushOperationModelsCollectionChanged;
        }
        public int ExecutionId { get; set; } = 0;
        public ObservableCollection<IBrusherOperationModel> BrushOperationModels
        {
            get { return _brushOperationModels; }
            set { SetProperty(ref _brushOperationModels, value); }
        }
        public List<Color> BrushColors { get; set; }

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;

        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }

        public IResult Result { get; set; }
        public ResultParameters ResultParameters { get; }
        public IOperationModel ResultCauserClone { get; set; }
        public SchemaModel SchemaModel { get; set; }

        public OperationModel Clone()
        {
            throw new NotImplementedException();
        }

        private void BrushOperationModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    var current = (OperationModel) item;
                    current.OperationModelUpdated -= Current_OperationModelUpdated;
                }
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    var current = (OperationModel) item;
                    current.OperationModelUpdated += Current_OperationModelUpdated;
                }
            _host.FireOperationModelUpdated(new BrushOperationModelUpdatedEventArgs());
        }

        private void Current_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            _host.FireOperationModelUpdated(new BrushOperationModelUpdatedEventArgs());
        }
    }
}