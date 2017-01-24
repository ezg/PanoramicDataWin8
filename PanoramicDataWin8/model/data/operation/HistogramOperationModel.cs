﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HistogramOperationModel : OperationModel, IBrushableOperationModel,
        IBrusherOperationModel, IFilterConsumerOperationModel,
        IStatisticallyComparableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        private readonly FilterConsumerOperationModelImpl _filterConsumerOperationModelImpl;
        private readonly FilterProviderOperationModelImpl _filterProviderOperationModelImpl;
        private readonly StatisticallyComparableOperationModelImpl _statisticallyComparableOperationModelImpl;


        private Dictionary<AttributeUsage, ObservableCollection<AttributeTransformationModel>>
            _attributeUsageTransformationModels =
                new Dictionary<AttributeUsage, ObservableCollection<AttributeTransformationModel>>();

        private StatisticalComparisonOperationModel _statisticalComparisonOperationModel;

        private VisualizationType _visualizationType;

        public HistogramOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _filterProviderOperationModelImpl = new FilterProviderOperationModelImpl(this);
            _filterConsumerOperationModelImpl = new FilterConsumerOperationModelImpl(this);
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
            _statisticallyComparableOperationModelImpl = new StatisticallyComparableOperationModelImpl(this);

            foreach (var attributeUsage in Enum.GetValues(typeof(AttributeUsage)).Cast<AttributeUsage>())
            {
                _attributeUsageTransformationModels.Add(attributeUsage,
                    new ObservableCollection<AttributeTransformationModel>());
                _attributeUsageTransformationModels[attributeUsage].CollectionChanged +=
                    _attributeUsageTransformationModels_CollectionChanged;
            }
        }

        public Dictionary<AttributeUsage, ObservableCollection<AttributeTransformationModel>>
            AttributeUsageTransformationModels
        {
            get { return _attributeUsageTransformationModels; }
            set { SetProperty(ref _attributeUsageTransformationModels, value); }
        }

        public List<AttributeTransformationModel> AttributeTransformationModels
        {
            get
            {
                var retList = new List<AttributeTransformationModel>();
                foreach (var key in _attributeUsageTransformationModels.Keys)
                {
                    retList.AddRange(_attributeUsageTransformationModels[key]);
                }
                return retList;
            }
        }

        public int IExecutionId { get; set; } = 0;

        public StatisticalComparisonOperationModel StatisticalComparisonOperationModel
        {
            get { return _statisticalComparisonOperationModel; }
            set { SetProperty(ref _statisticalComparisonOperationModel, value); }
        }

        public VisualizationType VisualizationType
        {
            get { return _visualizationType; }
            set { SetProperty(ref _visualizationType, value); }
        }

        public ObservableCollection<IBrushableOperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value; }
        }

        public List<Color> BrushColors { get; set; } = new List<Color>();

        public ObservableCollection<FilterModel> FilterModels
        {
            get { return _filterProviderOperationModelImpl.FilterModels; }
        }
        public ObservableCollection<BczBinMapModel> BczBinMapModels
        {
            get { return _filterProviderOperationModelImpl.BczBinMapModels; }
        }

        public void ClearFilterModels()
        {
            _filterProviderOperationModelImpl.ClearFilterModels();
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelImpl.AddFilterModels(filterModels);
        }
        public void AddBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            _filterProviderOperationModelImpl.AddBczBinMapModels(binMapModels);
        }

        public void AddFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelImpl.AddFilterModel(filterModel);
        }
        public void AddBczBinMapModel(BczBinMapModel binMapModel)
        {
            _filterProviderOperationModelImpl.AddBczBinMapModel(binMapModel);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            _filterProviderOperationModelImpl.RemoveFilterModel(filterModel);
        }
        public void RemoveBczBinMapModel(BczBinMapModel binMapModel)
        {
            _filterProviderOperationModelImpl.RemoveBczBinMapModel(binMapModel);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            _filterProviderOperationModelImpl.RemoveFilterModels(filterModels);
        }
        public void RemoveBczBinMapModels(List<BczBinMapModel> binMapModels)
        {
            _filterProviderOperationModelImpl.RemoveBczBinMapModels(binMapModels);
        }

        public FilteringOperation FilteringOperation
        {
            get { return _filterConsumerOperationModelImpl.FilteringOperation; }
            set { _filterConsumerOperationModelImpl.FilteringOperation = value; }
        }

        public ObservableCollection<FilterLinkModel> LinkModels
        {
            get { return _filterConsumerOperationModelImpl.LinkModels; }
            set { _filterConsumerOperationModelImpl.LinkModels = value; }
        }

        public bool IncludeDistribution
        {
            get { return _statisticallyComparableOperationModelImpl.IncludeDistribution; }
            set { _statisticallyComparableOperationModelImpl.IncludeDistribution = value; }
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((AttributeTransformationModel) item).PropertyChanged -=
                        AttributeTransformationModel_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    ((AttributeTransformationModel) item).OperationModel = this;
                    ((AttributeTransformationModel) item).PropertyChanged +=
                        AttributeTransformationModel_PropertyChanged;
                }
            }
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            FireOperationModelUpdated(new FilterOperationModelUpdatedEventArgs(FilterOperationModelUpdatedEventType.ClearFilterModels));
        }

        private void AttributeTransformationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }

        public void AddAttributeUsageTransformationModel(AttributeUsage attributeUsage,
            AttributeTransformationModel attributeTransformationModel)
        {
            _attributeUsageTransformationModels[attributeUsage].Add(attributeTransformationModel);
        }

        public void RemoveAttributeUsageTransformationModel(AttributeUsage attributeUsage,
            AttributeTransformationModel attributeTransformationModel)
        {
            _attributeUsageTransformationModels[attributeUsage].Remove(attributeTransformationModel);
        }

        public void RemoveAttributeTransformationModel(AttributeTransformationModel attributeTransformationModel)
        {
            foreach (var key in _attributeUsageTransformationModels.Keys)
            {
                if (_attributeUsageTransformationModels[key].Any(aom => aom == attributeTransformationModel))
                {
                    RemoveAttributeUsageTransformationModel(key, attributeTransformationModel);
                }
            }
        }

        public ObservableCollection<AttributeTransformationModel> GetAttributeUsageTransformationModel(
            AttributeUsage attributeUsage)
        {
            return _attributeUsageTransformationModels[attributeUsage];
        }
    }
}