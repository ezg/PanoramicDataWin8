using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Dynamic;
using System.Diagnostics;
using PanoramicDataWin8.utils;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicData.controller.view;
using PanoramicData.controller.data.sim;

namespace PanoramicDataWin8.controller.data.sim
{
    public class SimDataProvider
    {
        private QueryModel _queryModel = null;
        private List<Dictionary<AttributeModel, object>> _data = null;
        private int _nrProcessedSamples = 0;
        private int _nrSamplesToCheck = -1;

        private Dictionary<GroupingObject, IterativeCalculationObject> _iterativeCaluclationObjects = new Dictionary<GroupingObject, IterativeCalculationObject>();

        public SimDataProvider(QueryModel queryModel, List<Dictionary<AttributeModel, object>> data, int nrSamplesToCheck = -1)
        {
            _queryModel = queryModel;
            _data = data;
            _nrSamplesToCheck = nrSamplesToCheck;
        }

        public void StartSampling()
        {
            _nrProcessedSamples = 0;
            _iterativeCaluclationObjects.Clear();
        }

        public List<QueryResultItemModel> GetSampleQueryResultItemModels(int sampleSize)
        {
            if (_nrProcessedSamples < GetNrTotalSamples())
            {
                List<QueryResultItemModel> returnList = new List<QueryResultItemModel>();

                foreach (Dictionary<AttributeModel, object> row in _data.Skip(_nrProcessedSamples).Take(sampleSize))
                {
                    GroupingObject groupingObject = getGroupingObject(row, _queryModel, row[(_queryModel.SchemaModel.OriginModels[0] as SimOriginModel).IdAttributeModel]);
                    if (!_iterativeCaluclationObjects.ContainsKey(groupingObject))
                    {
                        _iterativeCaluclationObjects.Add(groupingObject, new IterativeCalculationObject());
                    }
                    IterativeCalculationObject iterativeCalculation = _iterativeCaluclationObjects[groupingObject];
                    iterativeCalculation.Update(row, _queryModel);
                }

                foreach (GroupingObject groupingObject in _iterativeCaluclationObjects.Keys)
                {
                    var attributeOperationModels = _queryModel.AttributeOperationModels;
                    var iterativeCalculation = _iterativeCaluclationObjects[groupingObject];
                    QueryResultItemModel item = new QueryResultItemModel()
                    {
                        GroupingObject = groupingObject
                    };
                    foreach (var attributeOperationModel in attributeOperationModels)
                    {
                        if (iterativeCalculation.AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            QueryResultItemValueModel valueModel = fromRaw(
                                attributeOperationModel,
                                iterativeCalculation.AggregateValues[attributeOperationModel],
                                iterativeCalculation.IsBinned[attributeOperationModel],
                                iterativeCalculation.BinSize[attributeOperationModel]);
                            if (!item.AttributeValues.ContainsKey(attributeOperationModel))
                            {
                                item.AttributeValues.Add(attributeOperationModel, valueModel);
                            }
                        }
                    }
                    returnList.Add(item);
                }
                _nrProcessedSamples += sampleSize;
                var ordered = returnList.OrderBy(item => item, new ItemComparer(_queryModel));
                return ordered.ToList();
            }
            else
            {
                return null;
            }
        }

        public int GetNrTotalSamples()
        {
            if (_nrSamplesToCheck == -1)
            {
                return _data.Count;
            }
            else
            {
                return _nrSamplesToCheck;
            }
        }

        private GroupingObject getGroupingObject(Dictionary<AttributeModel, object> item, QueryModel queryModel, object idValue)
        {
            var groupers = queryModel.AttributeOperationModels.Where(aom => aom.IsGrouped || aom.IsBinned).ToList();
            GroupingObject groupingObject = new GroupingObject(
                groupers.Count() > 0,
                queryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None),
                idValue);
            int count = 0;
            foreach (var attributeModel in item.Keys)
            {
                if (groupers.Count(avo => avo.IsGrouped && avo.AttributeModel.Equals(attributeModel)) > 0)
                {
                    groupingObject.Add(count++, item[attributeModel]);
                }
                else if (groupers.Count(avo => avo.IsBinned && avo.AttributeModel.Equals(attributeModel)) > 0)
                {
                    AttributeOperationModel bin = groupers.Where(avo => avo.IsBinned && avo.AttributeModel.Equals(attributeModel)).First();
                    if (item[attributeModel] == null)
                    {
                        groupingObject.Add(count++, item[attributeModel]);
                    }
                    else
                    {
                        double d = double.Parse(item[attributeModel].ToString());
                        groupingObject.Add(count++, Math.Floor(d / bin.BinSize) * bin.BinSize);
                    }
                }
            }
            return groupingObject;
        }

        private QueryResultItemValueModel fromRaw(AttributeOperationModel attributeOperationModel, object value, bool binned, double binSize)
        {
            QueryResultItemValueModel valueModel = new QueryResultItemValueModel();
            if (value == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = value;
                if (double.TryParse(value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                    if (binned)
                    {
                        valueModel.StringValue = d + " - " + (d + binSize);
                    }
                    else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.BIT)
                    {
                        if (d == 1.0)
                        {
                            valueModel.StringValue = "True";
                        }
                        else if (d == 0.0)
                        {
                            valueModel.StringValue = "False";
                        }
                    }
                }
                else
                {
                    valueModel.StringValue = valueModel.Value.ToString();
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).ToString();
                    }
                }
                if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.GEOGRAPHY)
                {
                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else
                {
                    valueModel.ShortStringValue = valueModel.StringValue.TrimTo(300);
                }
            }
            return valueModel;
        }
    }
}
