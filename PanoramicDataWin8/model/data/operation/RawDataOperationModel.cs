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
    public class RawDataOperationModel : BaseVisualizationOperationModel
    {
        public RawDataOperationModel(OriginModel schemaModel) : base(schemaModel)
        {
            ColumnHeaderAttributeUsageModels.CollectionChanged += columnHeaderAttributeUsageModels_CollectionChanged;

            IDEAAttributeModel.CodeDefinitionChangedEvent += TestForRefresh;
        }

        public override void Dispose()
        {
            IDEAAttributeModel.CodeDefinitionChangedEvent -= TestForRefresh;
            ResultCauserClone?.Dispose();
        }

        public void SetOrderingFunction(AttributeTransformationModel atm, OrderingFunction orderingFunction)
        {
            atm.OrderingFunction = orderingFunction;
            foreach (var a in AttributeTransformationModelParameters)
                if (a != atm)
                    a.OrderingFunction = OrderingFunction.None;
        }

        public bool SetAggregationForModel(AttributeTransformationModel atm, AggregateFunction newAgg)
        {
            if (atm.AggregateFunction == newAgg)
                return false;
            else
            {
                if (atm.GroupBy)
                {
                    atm.GroupBy = false;
                }
                atm.AggregateFunction = newAgg;
            }
            return true;
        }

        private void TestForRefresh(object sender)
        {
            var attributeChanged = sender as IDEAAttributeModel;
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<string> brushes;
            var aggregates = this.AttributeTransformationModelParameters.ToList();
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
        public ObservableCollection<AttributeModel> ColumnHeaderAttributeUsageModels { get; } = new ObservableCollection<AttributeModel>();

        private void columnHeaderAttributeUsageModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}