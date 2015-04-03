using PanoramicData.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.sim
{
    public class IterativeCalculation
    {
        private double _n = 0;

        public IterativeCalculation()
        {
            AggregateValues = new Dictionary<AttributeOperationModel, object>();
            BinSize = new Dictionary<AttributeOperationModel, double>();
            IsBinned = new Dictionary<AttributeOperationModel, bool>();
        }
        public Dictionary<AttributeOperationModel, object> AggregateValues { get; set; }
        public Dictionary<AttributeOperationModel, double> BinSize { get; set; }
        public Dictionary<AttributeOperationModel, bool> IsBinned { get; set; }

        public void Update(Dictionary<AttributeModel, object> row, QueryModel queryModel)
        {
            var attributeOperationModels = queryModel.AttributeOperationModels;
            foreach (var attributeOperationModel in attributeOperationModels)
            {
                bool binned = false;
                double binSize = 0;
                object value = row[attributeOperationModel.AttributeModel];
                object rawValue = null;

                if (value != null)
                {
                    if (attributeOperationModel.AggregateFunction == AggregateFunction.Max)
                    {
                        object currentValue = value;
                        if (AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            currentValue = AggregateValues[attributeOperationModel];
                        }
                        rawValue = new object[] { value, currentValue }.Max();
                    }
                    else if (attributeOperationModel.AggregateFunction == AggregateFunction.Min)
                    {
                        object currentValue = value;
                        if (AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            currentValue = AggregateValues[attributeOperationModel];
                        }
                        rawValue = new object[] { value, currentValue }.Min();
                    }
                    else if (attributeOperationModel.AggregateFunction == AggregateFunction.Avg)
                    {
                        object currentValue = 0.0;
                        if (AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            currentValue = AggregateValues[attributeOperationModel];
                        }

                        if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                        {
                            rawValue = (((double)currentValue * _n) + (double)value) / (_n + 1);
                        }
                        else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT)
                        {
                            rawValue = (((double)currentValue * _n) + (int)value) / (_n + 1);
                        }
                    }
                    else if (attributeOperationModel.AggregateFunction == AggregateFunction.Sum)
                    {
                        object currentValue = 0.0;
                        if (AggregateValues.ContainsKey(attributeOperationModel))
                        {
                            currentValue = AggregateValues[attributeOperationModel];
                        }

                        if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.FLOAT)
                        {
                            rawValue = (double)currentValue + (double)value;
                        }
                        else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.INT)
                        {
                            rawValue = (double)currentValue + (int)value;
                        }
                    }
                    else if (attributeOperationModel.AggregateFunction == AggregateFunction.Count)
                    {
                        rawValue = _n + 1;
                    }
                    else if (attributeOperationModel.AggregateFunction == AggregateFunction.None)
                    {
                        if (queryModel.AttributeOperationModels.Any(aom => aom.GroupMode != GroupMode.None))
                        {
                            if (queryModel.AttributeOperationModels.Where(aom => aom.GroupMode != GroupMode.None).Any(aom => aom.AttributeModel.Equals(attributeOperationModel.AttributeModel)))
                            {
                                AttributeOperationModel grouper = queryModel.AttributeOperationModels.Where(aom => aom.GroupMode != GroupMode.None).Where(aom => aom.AttributeModel.Equals(attributeOperationModel.AttributeModel)).First();
                                if (grouper.GroupMode == GroupMode.Distinct)
                                {
                                    rawValue = value;
                                }
                                else if (grouper.GroupMode == GroupMode.Binned)
                                {
                                    if (value != null)
                                    {
                                        double d = double.Parse(value.ToString());
                                        rawValue = Math.Floor(d / grouper.BinSize) * grouper.BinSize;
                                        binned = true;
                                        binSize = grouper.BinSize;
                                    }
                                    else
                                    {
                                        rawValue = null;
                                        binned = true;
                                        binSize = grouper.BinSize;
                                    }
                                }
                                else if (grouper.GroupMode == GroupMode.Year)
                                {
                                    if (value != null)
                                    {
                                        rawValue = ((DateTime)value).Year;
                                    }
                                }
                            }
                            else
                            {
                                rawValue = "...";
                            }
                        }
                        else
                        {
                            if (queryModel.AttributeOperationModels.Any(aom => aom.AggregateFunction != AggregateFunction.None))
                            {
                                rawValue = "...";
                            }
                            else
                            {
                                rawValue = value;
                            }
                        }
                    }
                }
                if (!AggregateValues.ContainsKey(attributeOperationModel))
                {
                    AggregateValues.Add(attributeOperationModel, rawValue);
                    BinSize.Add(attributeOperationModel, binSize);
                    IsBinned.Add(attributeOperationModel, binned);
                }
                else
                {
                    AggregateValues[attributeOperationModel] = rawValue;
                    BinSize[attributeOperationModel] = binSize;
                    IsBinned[attributeOperationModel] = binned;
                }
            }
            _n++;
        }

        public QueryResultItemValueModel GetQueryResultItemValueModel(AttributeOperationModel attributeOperationModel)
        {
            QueryResultItemValueModel valueModel = new QueryResultItemValueModel();
            if (this.AggregateValues[attributeOperationModel] == null)
            {
                valueModel.Value = null;
                valueModel.StringValue = "";
                valueModel.ShortStringValue = "";
            }
            else
            {
                double d = 0.0;
                valueModel.Value = this.AggregateValues[attributeOperationModel];
                valueModel.StringValue = valueModel.Value.ToString();

                if (double.TryParse(valueModel.Value.ToString(), out d))
                {
                    valueModel.StringValue = valueModel.Value.ToString().Contains(".") ? d.ToString("N") : valueModel.Value.ToString();
                    if (this.IsBinned[attributeOperationModel])
                    {
                        valueModel.StringValue = d + " - " + (d + this.BinSize[attributeOperationModel]);
                    }
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.GEOGRAPHY)
                {
                    string toSplit = valueModel.StringValue;
                    if (toSplit.Contains("(") && toSplit.Contains(")"))
                    {
                        toSplit = toSplit.Substring(toSplit.IndexOf("("));
                        toSplit = toSplit.Substring(1, toSplit.IndexOf(")") - 1);
                    }
                    valueModel.ShortStringValue = valueModel.StringValue.Replace("(" + toSplit + ")", "");
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.TIME)
                {
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                        valueModel.ShortStringValue = ((DateTime)valueModel.Value).TimeOfDay.ToString();
                    }
                }
                else if (attributeOperationModel.AttributeModel.AttributeDataType == AttributeDataTypeConstants.DATE)
                {
                    if (valueModel.Value is DateTime)
                    {
                        valueModel.StringValue = ((DateTime)valueModel.Value).ToString("MM/dd/yyyy");
                        valueModel.ShortStringValue = ((DateTime)valueModel.Value).ToString("MM/dd/yyyy");
                    }
                }
                valueModel.ShortStringValue = valueModel.StringValue.TrimTo(300);
            }
            return valueModel;
        }
    }
}
