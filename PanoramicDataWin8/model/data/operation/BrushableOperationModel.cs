﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;
using IDEA_common.operations;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public interface IBrushableOperationModel : IOperationModel
    {
        ObservableCollection<IBrushableOperationModel> BrushOperationModels { get; set; }
        List<Color> BrushColors { get; set; }
    }

    public interface IBrusherOperationModel : IFilterProvider
    {
    }

    public class BrushableOperationModelImpl : ExtendedBindableBase, IBrushableOperationModel
    {
        private ObservableCollection<IBrushableOperationModel> _brushOperationModels = new ObservableCollection<IBrushableOperationModel>();
        private IOperationModel _host;

        public BrushableOperationModelImpl(IOperationModel host)
        {
            _host = host;
            _brushOperationModels.CollectionChanged += BrushOperationModelsCollectionChanged;
        }

        public ObservableCollection<IBrushableOperationModel> BrushOperationModels
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

        public event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;
        public void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            _host.FireOperationModelUpdated(args);
        }

        public IResult Result { get; set; }
        public SchemaModel SchemaModel { get; set; }
        public OperationModel Clone()
        {
            throw new System.NotImplementedException();
        }
    }

    public class BrushOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
    }
}