using PanoramicData.model.data;
using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataAggregator
    {
        public void AggregateStep(DataBinStructure dataBinStructure, QueryModel queryModel)
        {
            Dictionary<AttributeOperationModel, double> maxValues = new Dictionary<AttributeOperationModel, double>();
            Dictionary<AttributeOperationModel, double> minValues = new Dictionary<AttributeOperationModel, double>();

            double maxCount = double.MinValue;

            var groupers = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group);
            var aggregators = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value);

            // update aggregations and counts
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                bin.Count += bin.Samples.Count;
                maxCount = Math.Max(bin.Count, maxCount);

                if (aggregators.Count > 0)
                {
                    foreach (var aggregator in aggregators)
                    {
                        foreach (var sample in bin.Samples)
                        {
                            GroupingObject sampleGroupingObject = new GroupingObject();
                            groupers.Each((grouper, i) =>
                            {
                                sampleGroupingObject.Add(i, sample.Entries[grouper.AttributeModel]);
                            });
                            update(bin, sample, sampleGroupingObject, aggregator, queryModel);
                        }
                    }
                }
            }

            // update max / min
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                foreach (var groupingObject in bin.Values.Keys)
                {
                    foreach (var aggregator in aggregators)
                    {
                        if (bin.Values[groupingObject][aggregator].HasValue)
                        {
                            if (!maxValues.ContainsKey(aggregator))
                            {
                                maxValues.Add(aggregator, double.MinValue);
                                minValues.Add(aggregator, double.MaxValue);
                            }
                            maxValues[aggregator] = Math.Max(maxValues[aggregator], bin.Values[groupingObject][aggregator].Value);
                            minValues[aggregator] = Math.Min(minValues[aggregator], bin.Values[groupingObject][aggregator].Value);
                        }
                    }
                }
            }

            // noramlize values
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                bin.NormalizedCount = bin.Count / maxCount;
                if (aggregators.Count > 0)
                {
                    foreach (var aggregator in aggregators)
                    {
                        foreach (var groupingObject in bin.Values.Keys)
                        {
                            bin.NormalizedValues[groupingObject][aggregator] =
                                (bin.Values[groupingObject][aggregator] - minValues[aggregator]) / (maxValues[aggregator] - minValues[aggregator]);
                        }
                    }
                }
            }
        }

        private void update(Bin bin, DataRow sample, GroupingObject sampleGroupingObject, AttributeOperationModel aggregator, QueryModel queryModel)
        {
            double? currentValue = null;
            double? sampleValue = null;

            double d = 0;
            if (double.TryParse(sample.Entries[aggregator.AttributeModel].ToString(), out d))
            {
                sampleValue = d;
            }

            if (bin.Values.ContainsKey(sampleGroupingObject) && bin.Values[sampleGroupingObject].ContainsKey(aggregator))
            {
                currentValue = bin.Values[sampleGroupingObject][aggregator];
            }
            else
            {
                currentValue = sampleValue;
            }

            if (!bin.Counts.ContainsKey(sampleGroupingObject)) 
            {
                bin.Counts.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel,double>());
            }
            if (!bin.Counts[sampleGroupingObject].ContainsKey(aggregator)) 
            {
                bin.Counts[sampleGroupingObject].Add(aggregator, 0);
            }

            if (sampleValue.HasValue)
            {
                if (aggregator.AggregateFunction == AggregateFunction.Max)
                {
                    currentValue = new double?[] { currentValue, sampleValue }.Max();
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Min)
                {
                    currentValue = new double?[] { currentValue, sampleValue }.Min();
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Avg)
                {
                    currentValue = ((currentValue * bin.Counts[sampleGroupingObject][aggregator]) + sampleValue) / (bin.Counts[sampleGroupingObject][aggregator] + 1);
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Sum)
                {
                    currentValue = currentValue + sampleValue;
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Count)
                {
                    currentValue = currentValue + 1;
                }
                else
                {
                    currentValue = ((currentValue * bin.Counts[sampleGroupingObject][aggregator]) + sampleValue) / (bin.Counts[sampleGroupingObject][aggregator] + 1);
                    //currentValue = currentValue + 1;
                }
            }


            if (!bin.Values.ContainsKey(sampleGroupingObject))
            {
                bin.Values.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel, double?>());
                bin.NormalizedValues.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel, double?>());
            }
            if (!bin.Values[sampleGroupingObject].ContainsKey(aggregator))
            {
                bin.Values[sampleGroupingObject].Add(aggregator, 0);
                bin.NormalizedValues[sampleGroupingObject].Add(aggregator, 0);
            } 
            bin.Values[sampleGroupingObject][aggregator] = currentValue;
            bin.Counts[sampleGroupingObject][aggregator] += 1;
        }
    }
}
