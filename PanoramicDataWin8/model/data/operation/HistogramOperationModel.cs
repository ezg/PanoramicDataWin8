using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.data.progressive;
using IDEA_common.operations;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using IDEA_common.aggregates;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HistogramOperationModel : BaseVisualizationOperationModel, IStatisticallyComparableOperationModel
    {
        private readonly StatisticallyComparableOperationModelImpl _statisticallyComparableOperationModelImpl;
        private StatisticalComparisonOperationModel _statisticalComparisonOperationModel;

        private Dictionary<AttributeUsage, ObservableCollection<AttributeTransformationModel>>
            _attributeUsageTransformationModels =
                new Dictionary<AttributeUsage, ObservableCollection<AttributeTransformationModel>>();

        public HistogramOperationModel(SchemaModel schemaModel) : base(schemaModel) {

            foreach (var attributeUsage in Enum.GetValues(typeof(AttributeUsage)).Cast<AttributeUsage>())
            {
                _attributeUsageTransformationModels.Add(attributeUsage,
                    new ObservableCollection<AttributeTransformationModel>());
                _attributeUsageTransformationModels[attributeUsage].CollectionChanged +=
                    _attributeUsageTransformationModels_CollectionChanged;
            }

            _statisticallyComparableOperationModelImpl = new StatisticallyComparableOperationModelImpl(this);
            IDEAAttributeModel.CodeDefinitionChangedEvent += TestForRefresh;
        }
        private void TestForRefresh(object sender)
        {
            var attributeChanged = sender as IDEAAttributeModel;
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<string> brushes;
            var aggregates = this.AttributeTransformationModelParameters.Concat(
                GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(
                GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).Concat(
                GetAttributeUsageTransformationModel(AttributeUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                GetAttributeUsageTransformationModel(AttributeUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None))).ToList();
            IDEAHelpers.GetBaseOperationParameters(this, out attributeCodeParameters, out brushes, BrushOperationModels.Select((m) => (object)m).ToList(), aggregates);

            foreach (var attr in attributeCodeParameters.OfType<AttributeCodeParameters>())
            {
                if (attr.RawName == attributeChanged.RawName || attr.Code.Contains(attributeChanged.RawName))
                {
                    FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    break;
                }
            }
        }
        public override void Dispose() { 
            IDEAAttributeModel.CodeDefinitionChangedEvent -= TestForRefresh;
            ResultCauserClone?.Dispose();
        }

        public bool IncludeDistribution
        {
            get { return _statisticallyComparableOperationModelImpl.IncludeDistribution; }
            set { _statisticallyComparableOperationModelImpl.IncludeDistribution = value; }
        }
        public StatisticalComparisonOperationModel StatisticalComparisonOperationModel
        {
            get { return _statisticalComparisonOperationModel; }
            set { SetProperty(ref _statisticalComparisonOperationModel, value); }
        }
        private void AttributeTransformationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClearFilterModels();
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((AttributeTransformationModel)item).PropertyChanged -=
                        AttributeTransformationModel_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    ((AttributeTransformationModel)item).OperationModel = this;
                    ((AttributeTransformationModel)item).PropertyChanged +=
                        AttributeTransformationModel_PropertyChanged;
                }
            }
            ClearFilterModels();
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
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